using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;

using Common;
using Common.Tools;
using Common.Maths;

using UnityTool;

using TriMesh = Common.Maths.TriMesh;

namespace Gui
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            ToolTip bundletip = new ToolTip();
            bundletip.ShowAlways = true;
            bundletip.SetToolTip(buttonBundle, "extract the bundle file resources, usually they are the assets \"CAB-*\" files");

            ToolTip assettip = new ToolTip();
            assettip.ShowAlways = true;
            assettip.SetToolTip(buttonAsset, "extract from assets \"CAB-*\" only and all geometries into a folder");

        }

        private void buttonBundle_Click(object sender, EventArgs e)
        {
            var dialog = OpenDialog("Extract Bundle");

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                Debugg.Info($"open {dialog.FileName}");

                string filename = Path.GetFullPath(dialog.FileName);
                string directory = Path.GetDirectoryName(filename);

                using (UnityFileReader reader = new UnityFileReader(dialog.FileName))
                {
                    if (!reader.LoadFile(out BundleFile bundle)) return;

                    for (int i = 0; i < bundle.FilesCount; i++)
                    {
                        var res = bundle.ExportFileStream(reader, i, directory);
                        Debugg.Info($"Exported resource : {res}");
                    }
                }
                Debugg.Success("done");

            }
        }

        private void buttonAsset_Click(object sender, EventArgs e)
        {
            var dialog = OpenDialog("Extract Assets");

            List<EmpyrionModel> models = new List<EmpyrionModel>();

            if (dialog.ShowDialog() != DialogResult.OK) return;

            Debugg.Info($"open {dialog.FileName}");

            string filename = Path.GetFullPath(dialog.FileName);
            string directory = Path.GetDirectoryName(filename);

            using (UnityFileReader unityreader = new UnityFileReader(filename))
            {
                if (unityreader.LoadFile(out AssetFile asset))
                {
                    var objects = GetMainObjects(unityreader);
                    Debugg.Info($"Find {objects.Count} main gameobjects");

                    foreach (var gameobj in objects)
                    {
                        EmpyrionModel model = EmpyrionModel.FromAsset(gameobj, unityreader);
                        models.Add(model);
                    }
                }
                else
                {
                    Debugg.Error($"can't read asset {filename}");
                }
            }

            directory += "\\" + Path.GetFileNameWithoutExtension(filename) + "_extracted\\";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            foreach (var model in models)
            {
                /*
                var setting = new XmlWriterSettings()
                {
                    Indent = true,
                    ConformanceLevel = ConformanceLevel.Fragment,
                };


                using (var writer = XmlWriter.Create(directory + "\\" + model.Name + ".xml", setting))
                    model.Write(writer, TransformVersion.Decomposed);
                
                            using (var reader = XmlReader.Create(directory + "\\" + model.Name + ".xml"))
                            {
                                ModelTree model2 = new ModelTree(reader);
                            }
                */
                using (var file = File.Create(directory + "\\" + model.Name + ".treemesh"))
                using (var writer = new BinaryWriter(file))
                    model.Write(writer, TransformVersion.Float16);
                /*
                using (var file = File.OpenRead(directory + "\\" + model.Name + ".treemesh"))
                using (var reader = new BinaryReader(file))
                {
                    EmpyrionModel model2 = new EmpyrionModel(reader);
                }*/
            }

            Debugg.Warning("DONE");

        }

        /// <summary>
        /// Get only relevant objects
        /// </summary>
        private List<GameObject> GetMainObjects(UnityFileReader unityreader)
        {
            List<GameObject> MainObjects = new List<GameObject>();
            var asset = unityreader.Asset;
            for (int i = 0; i < asset.ObjectCount; i++)
            {
                var info = asset.ObjectInfos[i];
                switch (info.classID)
                {
                    //i take only GameObjects
                    case ClassIDType.GameObject:
                        var gameobj = (GameObject)asset.GetObjectByIndex(unityreader, i);
                        {
                            //all gameobject must be linked before use transform
                            gameobj.LinkTransforms(unityreader);
                            if (gameobj.Transform != null && gameobj.Transform.Father.IsNull)
                                MainObjects.Add(gameobj);
                        }
                        break;
                }
            }
            return MainObjects;
        }


        public static OpenFileDialog OpenDialog(string title)
        {
            return new OpenFileDialog
            {
                Title = title,
                DefaultExt = "",
                //Filter = "txt files (*.txt)|*.txt",
                //FilterIndex = 2,
                RestoreDirectory = true,
            };
        }

        private void buttonExportScene_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Convert my scene to wavefront obj",
                DefaultExt = "",
                Filter = "my scene files (*.treemesh)|*.treemesh",
                RestoreDirectory = true,
            };

            if (dialog.ShowDialog() != DialogResult.OK) return;

            Debugg.Info($"open {dialog.FileName}");

            using (var file = File.OpenRead(dialog.FileName))
            {
                using (var reader =new  BinaryReader(file))
                {
                    EmpyrionModel model = new EmpyrionModel(reader);
                    string path = Path.GetDirectoryName(dialog.FileName);
                    string filename = Path.GetFileNameWithoutExtension(dialog.FileName);
                    filename = path + "\\" + filename + "_export";


                    TriMesh bigone = TriMesh.MergeAllGeometries(model.Tree.GetElementCollection<TriMesh>());
                    var wave = bigone.ConvertToWavefront();
                    wave.Save(@"C:\Users\johnw\Desktop\export");


                    //TriMesh tmesh = TriMesh.MergeAllGeometries(geometries);
                    //tmesh.Name = model.Name;

                    //var wavefront = tmesh.ConvertToWavefront();
                    //wavefront.Save(filename);
                }
            }

        }
    }
}
