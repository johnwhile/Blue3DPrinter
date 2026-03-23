using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using Common;
using Common.Maths;
using Common.IO.Wavefront;
using Common.Tools;

using FbxTool;


namespace Blue3DPrinter
{
    public partial class VoxelizerControl : UserControl
    { 
        public Blue3DPrinterForm main { get; set; }
        
        
        static Dictionary<string, PrefabType> PrefabTypeConv;

        static VoxelizerControl()
        {
            PrefabTypeConv = new Dictionary<string, PrefabType>();
            PrefabTypeConv.Add("Small (SV)", PrefabType.SV);
            PrefabTypeConv.Add("Hover (HV)", PrefabType.HV);
            PrefabTypeConv.Add("Capital (CV)", PrefabType.CV);
            PrefabTypeConv.Add("Base (BA)", PrefabType.BA);
            PrefabTypeConv.Add("AsteroidVoxel (AV)", PrefabType.AV);
            PrefabTypeConv.Add("Unknow (0)", PrefabType.UNKNOWN);
        }

        int fixSize(int size)
        {
            if (size > 250) size = 250;
            if (size < 1) size = 10;
            return size;
        }
        public VoxelizerControl()
        {
            InitializeComponent();
            ObjAlignmentComboBox.SelectedIndex = 1;
            PrefabTypeComboBox.Items.AddRange(PrefabTypeConv.Keys.ToArray());
            PrefabTypeComboBox.SelectedIndex = 3;
        }

        private void OpenAndRun_Click(object sender, EventArgs e)
        {
            //the X size
            if (!int.TryParse(WidthTextBox.Text, out int width)) width = 50;
            //the Y size
            if (!int.TryParse(HeightTextBox.Text, out int height)) height = 50;
            //the Z size
            if (!int.TryParse(LengthTextBox.Text, out int length)) length = 50;

            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = @"";
                openDialog.Filter = "wavefront OBJ (*.obj)|*.obj|fbx (*.fbx)|*.fbx";
                openDialog.RestoreDirectory = true;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = Path.GetFullPath(openDialog.FileName);

                    if (File.Exists(filename))
                    {
                        eAxis mirror = eAxis.None;
                        if (checkBoxForceMirroY.Checked) mirror = eAxis.Y;
                        if (checkBoxForceMirroX.Checked) mirror = eAxis.X;
                        if (checkBoxForceMirroZ.Checked) mirror = eAxis.Z;

                        WavefrontObj file = null;

                        switch (openDialog.FilterIndex)
                        {
                            case 1: file = WavefrontObj.Load(filename); break;
                            case 2: file = ConvertFbxFrom(openDialog.FileName); break;
                            default: throw new NotImplementedException("wrong filedialog file extension selection");
                        }

                        if (file == null) return;

                        BlueprintVoxelizer voxelizer = new BlueprintVoxelizer(width, length, height, ObjAlignmentComboBox.SelectedIndex == 1, mirror);

                        if (!uint.TryParse(BlockIDTextBox.Text, out uint preferedblock)) preferedblock = 403;


                        Blueprint blueprintvox = voxelizer.Voxelize(file, main.config, preferedblock);
                        if (blueprintvox != null)
                        {
                            string selected = PrefabTypeComboBox.SelectedItem.ToString();
                            PrefabType prefabType = PrefabTypeConv[selected];
                            blueprintvox.Header.prefabType = prefabType;
                            blueprintvox.Header.Version = BlueprintVersion.V_1_06;

                            filename = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename)) + "_voxelized_" + prefabType.ToString();
                            blueprintvox.Save(filename);
                            LogMsg.Message("> done", ConsoleColor.Green);
                        }
                    }

                }
            }
        }

        private void SizeTextBox_Leave(object sender, EventArgs e)
        {
            if (sender is TextBox textbox)
            {
                if (!int.TryParse(textbox.Text, out int size)) size = 250;
                size = fixSize(size);
                textbox.Text = size.ToString();
            }
        }

        private void checkBoxForceMirroY_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxForceMirroX.CheckState = CheckState.Unchecked;
            checkBoxForceMirroZ.CheckState = CheckState.Unchecked;
        }

        private void checkBoxForceMirroZ_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxForceMirroX.CheckState = CheckState.Unchecked;
            checkBoxForceMirroY.CheckState = CheckState.Unchecked;
        }

        private void checkBoxForceMirroX_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxForceMirroY.CheckState = CheckState.Unchecked;
            checkBoxForceMirroZ.CheckState = CheckState.Unchecked;
        }

        private void DebugFbxToObj(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = @"";
                openDialog.Filter = "fbx (*.fbx)|*.fbx";
                openDialog.RestoreDirectory = true;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    var file = ConvertFbxFrom(openDialog.FileName);
                    if (file != null) file.Save(openDialog.FileName);
                }
            }
        }

        /// <summary>
        /// Try to convert fbx format to obj
        /// </summary>
        public WavefrontObj ConvertFbxFrom(string filename)
        {
            WavefrontObj file = null;

            filename = Path.GetFullPath(filename);
            
            if (File.Exists(filename))
            {
                LogMsg.Message("> try convert fbx to obj format");

                if (FbxImporter.Validate())
                {
                    var scene = FbxImporter.Import(filename);
                    file = ConvertFrom(scene);
                    if (file == null) { LogMsg.Message("> fail to convert to obj", ConsoleColor.Red); return null; }
                    //file.UpdateBound();
                }
                else LogMsg.Message("> fail to load the fbx library", ConsoleColor.Red);
            }
            return file;
        }

        /// <summary>
        /// Try to convert my mesh format to obj
        /// </summary>
        public WavefrontObj ConvertFrom(SceneTree scene)
        {
            if (scene == null) return null;

            //degenerate all submeshes and nodes to one mesh
            Mesh bigone = new Mesh(Primitive.TriangleList, scene.Name);

            //need to change the scene.root.treehierarchy sequence...
            if (scene.Root.Element is Mesh mesh1)
            {
                bigone.Merge(mesh1);
            }

            foreach (var node in scene.Root.TreeHierarchy)
            {
                if (node.Element is Mesh mesh2)
                {
                    bigone.Merge(mesh2);
                }
            }
            if (bigone.VerticesCount==0 || bigone.IndicesCount ==0)
            {
                LogMsg.Message("> wavefront obj is empty...", ConsoleColor.Red);
                return null;
            }
                
            return bigone.ConvertToWavefront(true);
        }
    }
}
