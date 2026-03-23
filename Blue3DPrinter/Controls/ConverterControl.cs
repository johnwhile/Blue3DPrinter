using Common;
using System;
using System.IO;
using System.Windows.Forms;

namespace Blue3DPrinter
{
    public partial class ConverterControl : UserControl
    {
        public Blue3DPrinterForm main { get; set; }

        public ConverterControl()
        {
            InitializeComponent();
            checkBoxMergeBlocks.Checked = true;
            checkBoxHideTriangles.Checked = true;
            checkBoxExportColor.Checked = true;

#if DEBUG
            checkBoxMergeBlocks.Enabled = true;
#else
            checkBoxMergeBlocks.Enabled = false;
#endif

        }

        private void ConvertButton_Click(object sender, EventArgs e)
        {
            if (main == null) return;

            if (!ModelResourceManager.Open() && ModelResourceManager.ModelFilenames.Count == 0)
            {
                MessageBox.Show(this, "Models file \"Shapes.zip\" contains no meshes or not exist", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (main.config == null)
            {
                MessageBox.Show(this, "BlockConfig.ecf not loaded", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Blueprint blueprint = null;

            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                //openDialog.InitialDirectory = @"";
                openDialog.Filter = "epb files (*.epb)|*.epb";
                openDialog.RestoreDirectory = true;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = Path.GetFullPath(openDialog.FileName);
                    blueprint = Blueprint.Open(filename);
                }
                else
                {
                    return;
                }
            }

            if (blueprint == null)
            {
                MessageBox.Show(this, "blueprint not loaded", "error", MessageBoxButtons.OK);
                return;
            }


            bool result = true;


            if (blueprint.Blocks.LoadDescriptions(main.config) && blueprint.Blocks.LoadModels())
            {
                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "wavefront OBJ (*.obj)|*.obj|fbx scene (not implemented yet) (*.fbx)|*.fbx";
                    saveDialog.RestoreDirectory = false;
                    saveDialog.InitialDirectory = Path.GetDirectoryName(blueprint.Filename);
                    saveDialog.FileName = Path.GetFileNameWithoutExtension(blueprint.Filename);

                    HideLevel hidelevel = checkBoxHideTriangles.Checked ? HideLevel.Complete : HideLevel.None;
                    bool mergeblock = checkBoxMergeBlocks.Checked;
                    bool exportcolor = checkBoxExportColor.Checked;

                    if (!int.TryParse(textBoxMaxVertsLimit.Text, out int maxvertices)) maxvertices = 0;

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        //SceneGenerator.GenerateWaveFront_Debug(blueprint, saveDialog.FileName);

                        switch (saveDialog.FilterIndex)
                        {
                            case 1: result &= SceneGenerator.GenerateWaveFront(blueprint, saveDialog.FileName, hidelevel, mergeblock, maxvertices, exportcolor); break;
                            case 2: result &= SceneGenerator.GenerateFbxScene(blueprint, saveDialog.FileName, hidelevel); break;
                            default: result = false; break;
                        }

                        if (result)
                        {
                            LogMsg.Message("> GenerateScene: OK", ConsoleColor.Green);
                        }
                        else
                            LogMsg.Message("> GenerateScene: FAIL", ConsoleColor.Red);
                    }
                    else
                        LogMsg.Message("> GenerateScene: ABORT");
                }
            }
            else
            {
                LogMsg.Message("> blueprint.Blocks.LoadDescrAndModels = false", ConsoleColor.Red);
            }

            ModelResourceManager.Close();
        }
    }
}
