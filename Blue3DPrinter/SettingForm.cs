using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blue3DPrinter
{
    public partial class SettingForm : Form
    {
        Blue3DPrinterForm main;

        public SettingForm(Blue3DPrinterForm main)
        {
            this.main = main;
            InitializeComponent();
            textBoxGameDirectory.Text = Blue3DPrinter.Default.GameDirectory;
            textBoxFileVersion.Text = Blue3DPrinter.Default.FileVersion.ToString();

            toolTipFileVersion.AutomaticDelay = 0;
            toolTipFileVersion.InitialDelay = 0;
            toolTipFileVersion.ReshowDelay = 0;
            toolTipFileVersion.ShowAlways = true;
            toolTipFileVersion.SetToolTip(textBoxFileVersion, "29 for game 1.6.3\n31 for game 1.10");
            Show(main);
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            main.setting = null;
        }

        private void textBoxGameDirectory_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                //remove ding
                e.Handled = true;

                if (File.Exists(textBoxGameDirectory.Text + Program.BlockConfigPath))
                {
                    Blue3DPrinter.Default.GameDirectory = textBoxGameDirectory.Text;
                }
                else
                {
                    MessageBox.Show(this, "the directory not containt the file :\n\n" + textBoxGameDirectory.Text + Program.BlockConfigPath, "warning");
                }
            }
        }

        private void textBoxFileVersion_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                //remove ding
                e.Handled = true;

                int version;
                if (!int.TryParse(textBoxFileVersion.Text, out version)) version = 31;
                Blue3DPrinter.Default.FileVersion = version;

            }
        }
    }
}
