using Common.Tools;
using System;
using System.IO;
using System.Windows.Forms;
using UnityTool;

namespace Blue3DPrinter
{
    public partial class Blue3DDebuggerForm : Form
    {
        Blue3DPrinterForm main;
        BlockDescription description = null;

        public Blue3DDebuggerForm(Blue3DPrinterForm main)
        {
            this.main = main;
            InitializeComponent();
            DescriptionNameSearch.Text = "HullThinLarge";
            ComboBoxChildShape.Text = "404";

            //var storage = main.resourceManager.GetMyFileStorageManager();
            storageComboBox.Items.Clear();

            if (ModelResourceManager.Open())
            //if (storage.OpenAndLoad())
            {
                //foreach (var file in storage.Indexer.FileFilenames)
                foreach (var file in ModelResourceManager.ModelFilenames)
                    storageComboBox.Items.Add(file);
                storageComboBox.SelectedIndex = 1;
            }
            //storage.Close();

            Show(main);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            main.shapeDebugger = null;
        }

        private void DescriptionNameSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                //remove ding
                e.Handled = true;

                description = null;

                if (main.config != null)
                    description = main.config.GetDescription(DescriptionNameSearch.Text.ToLower());

                if (description != null)
                {
                    Console.WriteLine(description.ToString());

                    BlockIdSearch.Text = description.BlockId.ToString();

                    ComboBoxChildShape.Items.Clear();
                    if (description.ChildName != null && description.ChildName.Length > 0)
                        ComboBoxChildShape.Items.AddRange(description.ChildName);

                    fbxfilenameLabel.Text = description.GetFilenameAsset(ComboBoxChildShape.SelectedIndex);
                }
                else
                {
                    Console.WriteLine(">>> description not found");
                    BlockIdSearch.Text = "0";
                }
            }
            DescriptionNameSearch.Update();
            ComboBoxChildShape.Update();
            BlockIdSearch.Update();
            fbxfilenameLabel.Update();
        }

        private void BlockIdSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                //remove ding
                e.Handled = true;

                description = null;

                if (main.config != null)
                {
                    int blockId;
                    if (!int.TryParse(BlockIdSearch.Text, out blockId)) blockId = 403;
                    description = main.config.GetDescription(blockId);
                }
                if (description != null)
                {
                    Console.WriteLine(description.ToString());
                    DescriptionNameSearch.Text = description.Name;
                    ComboBoxChildShape.Items.Clear();
                    if (description.ChildName != null && description.ChildName.Length > 0)
                    {
                        ComboBoxChildShape.Items.AddRange(description.ChildName);
                        ComboBoxChildShape.SelectedIndex = 0;
                    }

                    fbxfilenameLabel.Text = description.GetFilenameAsset(ComboBoxChildShape.SelectedIndex);

                }
                else
                {
                    Console.WriteLine(">>> description not found");
                }
            }
            DescriptionNameSearch.Update();
            ComboBoxChildShape.Update();
            BlockIdSearch.Update();
            fbxfilenameLabel.Update();
        }
        private void CreateShapeObj_Click(object sender, EventArgs e)
        {
            if (description != null)
            {
                int childindex = ComboBoxChildShape.SelectedIndex;

                if (childindex < 0) childindex = 0;

                string shapename = description.GetFilenameAsset(childindex);
                Console.WriteLine("> read file " + shapename);

                ExportModelToWavefont(shapename);
            }
        }
        private void StorageExportButton_Click(object sender, EventArgs e)
        {
            ExportModelToWavefont(storageComboBox.SelectedItem.ToString());
        }
        private void ExportModelToWavefont(string shapename)
        {
            if (string.IsNullOrEmpty(shapename)) return;

            if (ModelResourceManager.TryGetModel(shapename, out SceneTree scene))
            {
                if (scene.ElementsCount > 0)
                {
                    LogMsg.Message("> Export file " + scene.Name);

                    BlockModel model = new BlockModel(scene, description, shapename);

                    model.Mesh.InitializeWavefontFile(out var objfile, out var matfile);
                    WavefrontExporter.WriteToWavefront_Separate(objfile, matfile, model.Mesh, null, true);

                    string filename = shapename;

                    LogMsg.Message("> Save wavefront mat " + filename);
                    matfile.Save(filename);

                    LogMsg.Message("> Save wavefront obj " + filename);
                    objfile.Save(filename);

                    LogMsg.Success("done");
                }
                else
                {
                    LogMsg.Error("Empty file");
                }
            }
        }

        private void ConvertFbxToObj_Click(object sender, EventArgs e)
        {
            /*
            if (FbxTool.FbxImporter.Validate())
            {
                using (OpenFileDialog openDialog = new OpenFileDialog())
                {
                    openDialog.InitialDirectory = Blue3DPrinter.Default.ModelsFolderFBX;
                    openDialog.Filter = "fbx files (*.fbx)|*.fbx";
                    
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        string filename = Path.GetFullPath(openDialog.FileName);
                        SceneFile file = FbxTool.FbxImporter.Import(filename);

                        filename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
                        file.Save(filename);

                        BlockModel model = new BlockModel(file, null, file.Name);

                        int i = 0;
                        foreach (var m_obj in file.m_sceneObjects)
                            if (m_obj is Mesh mesh)
                                mesh.Save(filename + "_mobj_" + i++.ToString());

                        Mesh bigone = model.Mesh;

                        WaveFileObj obj = bigone.ConvertToWavefront();

                        obj.Save(filename + "_bigone");
                    }
                }
            }*/
        }

        private void BtnConvertTreeMesh_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                //openDialog.InitialDirectory = @"";
                openDialog.Filter = "My TreeMesh data structure (*.treemesh)|*.treemesh";
                openDialog.RestoreDirectory = true;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    string folder = Path.GetDirectoryName(openDialog.FileName);
                    string filename = Path.GetFileNameWithoutExtension(openDialog.FileName);
                    string extension = Path.GetExtension(openDialog.FileName);

                    SceneTree scene = null;

                    using (FileStream file = File.OpenRead(openDialog.FileName))
                    using (BinaryReader reader = new BinaryReader(file))
                    {
                        LogMsg.Message("> reading file");
                        var empyrion = new EmpyrionModel();
                        if (!empyrion.Read(reader))
                        {
                            LogMsg.Error($"> error reading {filename}.{extension}");
                            return;
                        }
                        scene = empyrion.Tree;

                    }
                    LogMsg.Message("> converting file");
//                    using (FileStream file = File.OpenWrite(filename))

                   BlockModel model = new BlockModel(scene, null, filename);

                    model.Mesh.InitializeWavefontFile(out var objfile, out var matfile);
                    WavefrontExporter.WriteToWavefront_Merge(objfile, matfile, model.Mesh);


                    LogMsg.Message("> Save wavefront mat " + filename);
                    matfile.Save(Path.Combine(folder, filename));

                    LogMsg.Message("> Save wavefront obj " + filename);
                    objfile.Save(Path.Combine(folder, filename));

                    LogMsg.Success("done");
                }
                else
                {
                    return;
                }
            }
        }


    }
}
