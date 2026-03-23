
namespace Blue3DPrinter
{
    partial class SettingForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.textBoxGameDirectory = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxFileVersion = new System.Windows.Forms.TextBox();
            this.toolTipFileVersion = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // textBoxGameDirectory
            // 
            this.textBoxGameDirectory.Location = new System.Drawing.Point(96, 11);
            this.textBoxGameDirectory.Name = "textBoxGameDirectory";
            this.textBoxGameDirectory.Size = new System.Drawing.Size(544, 20);
            this.textBoxGameDirectory.TabIndex = 0;
            this.textBoxGameDirectory.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxGameDirectory_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Game directory";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(61, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "File Version";
            // 
            // textBoxFileVersion
            // 
            this.textBoxFileVersion.Location = new System.Drawing.Point(96, 43);
            this.textBoxFileVersion.Name = "textBoxFileVersion";
            this.textBoxFileVersion.Size = new System.Drawing.Size(31, 20);
            this.textBoxFileVersion.TabIndex = 5;
            this.textBoxFileVersion.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxFileVersion_KeyPress);
            // 
            // toolTipFileVersion
            // 
            this.toolTipFileVersion.AutomaticDelay = 0;
            this.toolTipFileVersion.AutoPopDelay = 5000;
            this.toolTipFileVersion.InitialDelay = 0;
            this.toolTipFileVersion.ReshowDelay = 0;
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(652, 83);
            this.Controls.Add(this.textBoxFileVersion);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxGameDirectory);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "SettingForm";
            this.Text = "Setting";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxGameDirectory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxFileVersion;
        private System.Windows.Forms.ToolTip toolTipFileVersion;
    }
}