
using Common;
using System;
using System.IO;
using System.Windows.Forms;

namespace Blue3DPrinter
{
    public partial class Blue3DPrinterForm : Form
    {
        internal Blue3DDebuggerForm shapeDebugger;
        internal SettingForm setting;
        internal BlocksConfig config;

        Blueprint blueprint;


        public Blue3DPrinterForm()
        {
            InitializeComponent();

            Text = "Blue3DPrinter v1.12";

            voxelizerControl.main = this;
            converterControl.main = this;


#if DEBUG
            //call the static constuctor to enable the cast of FileBase with AssetTransform and read the SceneNode.Tag
            //AssetStudio.AssetTransform ass = new AssetStudio.AssetTransform();
#endif
        }

        private void Blue3DPrinterForm_Load(object sender, EventArgs e)
        {
            // check and load the BlockConfig.ecf file
            string blockConfigFilename = Path.Combine(AppSetting.GameDirectory, AppSetting.BlockConfigPath);
            if (!File.Exists(blockConfigFilename)) blockConfigFilename = "BlocksConfig.ecf";

            if (!File.Exists(blockConfigFilename))
                MessageBox.Show(this, "BlockConfig.ecf not found, please set the correctly Content folder name in Blue3DPrinter.settings file", "Error", MessageBoxButtons.OK);
            else
                config = BlocksConfig.LoadConfig(blockConfigFilename);

            // check FBX library
            FbxTool.FbxImporter.Validate();


            if (!ModelResourceManager.Open("Models.zip", "Shapes.zip")) Debugg.Error("Models.zip and Shapes.zip not found");
            /*

            // check if you have unpacked the game's bundles files first time
            // Since the process is very long and the AssetsTools.NET library is absurdly complicated,
            // I decided to create my own version of bundle files with all meshes extracted and ready to use

            string storageFilename = Path.Combine(Blue3DPrinter.Default.ModelsFolder, "modelStorage");

            //if (!File.Exists(storageFilename + "*.index") && !File.Exists(storageFilename + "*.data"))
            //storageFilename = Path.GetFullPath(@"..\..\..\..\..\Models\modelStorage");
            resourceManager = ModelResourceManager.Load(this, storageFilename);

            int found = ModelResourceManager.SearchFbxFilsToOverrideVanillaModels(Blue3DPrinter.Default.ModelsFolderFBX);
            */
        }

        private void OpenBlueprintMenu_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                //openDialog.InitialDirectory = @"";
                openDialog.Filter = "epb files (*.epb)|*.epb";
                openDialog.RestoreDirectory = true;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = Path.GetFullPath(openDialog.FileName);
                    toolStripStatusLabel1.Text = filename;
                    blueprint = Blueprint.Open(filename);
                }
            }
        }

        private void convertToEBPXmenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = @"";
                openDialog.Filter = "epb files (*.epb)|*.epb";
                openDialog.RestoreDirectory = true;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {

                    string filename = Path.GetFullPath(openDialog.FileName);

                    toolStripStatusLabel1.Text = filename;

                    Blueprint.ConvertToUncompressed(filename);
                }
            }
        }

        private void convertToTXTmenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openDialog = new OpenFileDialog())
            {
                //openFileDialog.InitialDirectory = @"";
                openDialog.Filter = "epb files (*.epb)|*.epb";
                openDialog.RestoreDirectory = true;

                if (openDialog.ShowDialog() == DialogResult.OK)
                {

                    string filename = Path.GetFullPath(openDialog.FileName);

                    toolStripStatusLabel1.Text = filename;

                    Blueprint.ConvertToText(filename);
                }
            }
        }


        private void DebugShapeMenu_Click(object sender, EventArgs e)
        {
            if (shapeDebugger == null) shapeDebugger = new Blue3DDebuggerForm(this);
        }

        private void settingsMenuItem_Click(object sender, EventArgs e)
        {
            if (setting == null) setting = new SettingForm(this);
        }


    }
}
