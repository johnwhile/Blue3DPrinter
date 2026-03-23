
namespace Blue3DPrinter
{
    partial class ConverterControl
    {
        /// <summary> 
        /// Variabile di progettazione necessaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Pulire le risorse in uso.
        /// </summary>
        /// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Codice generato da Progettazione componenti

        /// <summary> 
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare 
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.checkBoxExportColor = new System.Windows.Forms.CheckBox();
            this.checkBoxMergeBlocks = new System.Windows.Forms.CheckBox();
            this.textBoxMaxVertsLimit = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxHideTriangles = new System.Windows.Forms.CheckBox();
            this.checkBoxHideDevice = new System.Windows.Forms.CheckBox();
            this.ConvertButton = new System.Windows.Forms.Button();
            this.toolTipCreate = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // checkBoxExportColor
            // 
            this.checkBoxExportColor.AutoSize = true;
            this.checkBoxExportColor.Location = new System.Drawing.Point(16, 28);
            this.checkBoxExportColor.Name = "checkBoxExportColor";
            this.checkBoxExportColor.Size = new System.Drawing.Size(116, 17);
            this.checkBoxExportColor.TabIndex = 0;
            this.checkBoxExportColor.Text = "Export blocks color";
            this.checkBoxExportColor.UseVisualStyleBackColor = true;
            // 
            // checkBoxMergeBlocks
            // 
            this.checkBoxMergeBlocks.AutoSize = true;
            this.checkBoxMergeBlocks.Location = new System.Drawing.Point(16, 51);
            this.checkBoxMergeBlocks.Name = "checkBoxMergeBlocks";
            this.checkBoxMergeBlocks.Size = new System.Drawing.Size(131, 17);
            this.checkBoxMergeBlocks.TabIndex = 1;
            this.checkBoxMergeBlocks.Text = "Merge block\'s meshes";
            this.checkBoxMergeBlocks.UseVisualStyleBackColor = true;
            // 
            // textBoxMaxVertsLimit
            // 
            this.textBoxMaxVertsLimit.Location = new System.Drawing.Point(16, 127);
            this.textBoxMaxVertsLimit.Name = "textBoxMaxVertsLimit";
            this.textBoxMaxVertsLimit.Size = new System.Drawing.Size(74, 20);
            this.textBoxMaxVertsLimit.TabIndex = 2;
            this.textBoxMaxVertsLimit.Text = "0";
            this.textBoxMaxVertsLimit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(96, 130);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Max vertices per block";
            // 
            // checkBoxHideTriangles
            // 
            this.checkBoxHideTriangles.AutoSize = true;
            this.checkBoxHideTriangles.Location = new System.Drawing.Point(16, 74);
            this.checkBoxHideTriangles.Name = "checkBoxHideTriangles";
            this.checkBoxHideTriangles.Size = new System.Drawing.Size(148, 17);
            this.checkBoxHideTriangles.TabIndex = 4;
            this.checkBoxHideTriangles.Text = "Remove invisible triangles";
            this.checkBoxHideTriangles.UseVisualStyleBackColor = true;
            // 
            // checkBoxHideDevice
            // 
            this.checkBoxHideDevice.AutoSize = true;
            this.checkBoxHideDevice.Location = new System.Drawing.Point(16, 97);
            this.checkBoxHideDevice.Name = "checkBoxHideDevice";
            this.checkBoxHideDevice.Size = new System.Drawing.Size(193, 17);
            this.checkBoxHideDevice.TabIndex = 5;
            this.checkBoxHideDevice.Text = "Remove surrounded device\'s block";
            this.checkBoxHideDevice.UseVisualStyleBackColor = true;
            // 
            // ConvertButton
            // 
            this.ConvertButton.Location = new System.Drawing.Point(16, 165);
            this.ConvertButton.Name = "ConvertButton";
            this.ConvertButton.Size = new System.Drawing.Size(148, 33);
            this.ConvertButton.TabIndex = 6;
            this.ConvertButton.Text = "Create 3D Model";
            this.ConvertButton.UseVisualStyleBackColor = true;
            this.ConvertButton.Click += new System.EventHandler(this.ConvertButton_Click);
            toolTipCreate.SetToolTip(ConvertButton,
                "Load the .epb blueprint file and build the 3d object.\n"+
                "To work the tool must have successfully loaded the:\n"+
                "\"BlockConfig.ecf\" and my \"Models\\modelStorage.data\"");
            // 
            // ConverterControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ConvertButton);
            this.Controls.Add(this.checkBoxHideDevice);
            this.Controls.Add(this.checkBoxHideTriangles);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxMaxVertsLimit);
            this.Controls.Add(this.checkBoxMergeBlocks);
            this.Controls.Add(this.checkBoxExportColor);
            this.Name = "ConverterControl";
            this.Size = new System.Drawing.Size(230, 224);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBoxExportColor;
        private System.Windows.Forms.CheckBox checkBoxMergeBlocks;
        private System.Windows.Forms.TextBox textBoxMaxVertsLimit;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxHideTriangles;
        private System.Windows.Forms.CheckBox checkBoxHideDevice;
        private System.Windows.Forms.Button ConvertButton;
        private System.Windows.Forms.ToolTip toolTipCreate;
    }
}
