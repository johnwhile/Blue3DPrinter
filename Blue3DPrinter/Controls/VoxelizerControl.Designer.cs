
using System.Windows.Forms;

namespace Blue3DPrinter
{
    partial class VoxelizerControl
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
            this.HeightTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.LengthTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.WidthTextBox = new System.Windows.Forms.TextBox();
            this.OpenBtn = new System.Windows.Forms.Button();
            this.ObjAlignmentComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxForceMirroY = new System.Windows.Forms.CheckBox();
            this.checkBoxForceMirroZ = new System.Windows.Forms.CheckBox();
            this.checkBoxForceMirroX = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.PrefabTypeComboBox = new System.Windows.Forms.ComboBox();
            this.BlockIDTextBox = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.debugfbx2obj = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // HeightTextBox
            // 
            this.HeightTextBox.Location = new System.Drawing.Point(16, 38);
            this.HeightTextBox.Name = "HeightTextBox";
            this.HeightTextBox.Size = new System.Drawing.Size(41, 20);
            this.HeightTextBox.TabIndex = 0;
            this.HeightTextBox.Text = "50";
            this.HeightTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.HeightTextBox.Leave += new System.EventHandler(this.SizeTextBox_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Blueprint Size";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(63, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(106, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Height ( blocks of Y )";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(63, 65);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(108, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Length ( blocks of Z )";
            // 
            // LengthTextBox
            // 
            this.LengthTextBox.Location = new System.Drawing.Point(16, 62);
            this.LengthTextBox.Name = "LengthTextBox";
            this.LengthTextBox.Size = new System.Drawing.Size(41, 20);
            this.LengthTextBox.TabIndex = 3;
            this.LengthTextBox.Text = "50";
            this.LengthTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.LengthTextBox.Leave += new System.EventHandler(this.SizeTextBox_Leave);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(63, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(103, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Width ( blocks of X )";
            // 
            // WidthTextBox
            // 
            this.WidthTextBox.Location = new System.Drawing.Point(16, 86);
            this.WidthTextBox.Name = "WidthTextBox";
            this.WidthTextBox.Size = new System.Drawing.Size(41, 20);
            this.WidthTextBox.TabIndex = 5;
            this.WidthTextBox.Text = "50";
            this.WidthTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.WidthTextBox.Leave += new System.EventHandler(this.SizeTextBox_Leave);
            // 
            // OpenBtn
            // 
            this.OpenBtn.Location = new System.Drawing.Point(196, 166);
            this.OpenBtn.Name = "OpenBtn";
            this.OpenBtn.Size = new System.Drawing.Size(112, 51);
            this.OpenBtn.TabIndex = 7;
            this.OpenBtn.Text = "Open 3D file";
            this.OpenBtn.UseVisualStyleBackColor = true;
            this.OpenBtn.Click += new System.EventHandler(this.OpenAndRun_Click);
            // 
            // ObjAlignmentComboBox
            // 
            this.ObjAlignmentComboBox.FormattingEnabled = true;
            this.ObjAlignmentComboBox.Items.AddRange(new object[] {
            "Maintain proportions",
            "Fill to blueprint\'s bounds"});
            this.ObjAlignmentComboBox.Location = new System.Drawing.Point(103, 133);
            this.ObjAlignmentComboBox.Name = "ObjAlignmentComboBox";
            this.ObjAlignmentComboBox.Size = new System.Drawing.Size(205, 21);
            this.ObjAlignmentComboBox.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 136);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "3D Mesh Filling";
            // 
            // checkBoxForceMirroY
            // 
            this.checkBoxForceMirroY.AutoSize = true;
            this.checkBoxForceMirroY.Location = new System.Drawing.Point(177, 41);
            this.checkBoxForceMirroY.Name = "checkBoxForceMirroY";
            this.checkBoxForceMirroY.Size = new System.Drawing.Size(131, 17);
            this.checkBoxForceMirroY.TabIndex = 10;
            this.checkBoxForceMirroY.Text = "force central symmetry";
            this.checkBoxForceMirroY.UseVisualStyleBackColor = true;
            this.checkBoxForceMirroY.CheckedChanged += new System.EventHandler(this.checkBoxForceMirroY_CheckedChanged);
            // 
            // checkBoxForceMirroZ
            // 
            this.checkBoxForceMirroZ.AutoSize = true;
            this.checkBoxForceMirroZ.Location = new System.Drawing.Point(177, 66);
            this.checkBoxForceMirroZ.Name = "checkBoxForceMirroZ";
            this.checkBoxForceMirroZ.Size = new System.Drawing.Size(131, 17);
            this.checkBoxForceMirroZ.TabIndex = 11;
            this.checkBoxForceMirroZ.Text = "force central symmetry";
            this.checkBoxForceMirroZ.UseVisualStyleBackColor = true;
            this.checkBoxForceMirroZ.CheckedChanged += new System.EventHandler(this.checkBoxForceMirroZ_CheckedChanged);
            // 
            // checkBoxForceMirroX
            // 
            this.checkBoxForceMirroX.AutoSize = true;
            this.checkBoxForceMirroX.Location = new System.Drawing.Point(177, 89);
            this.checkBoxForceMirroX.Name = "checkBoxForceMirroX";
            this.checkBoxForceMirroX.Size = new System.Drawing.Size(131, 17);
            this.checkBoxForceMirroX.TabIndex = 12;
            this.checkBoxForceMirroX.Text = "force central symmetry";
            this.checkBoxForceMirroX.UseVisualStyleBackColor = true;
            this.checkBoxForceMirroX.CheckedChanged += new System.EventHandler(this.checkBoxForceMirroX_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 169);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Vessel Type";
            // 
            // PrefabTypeComboBox
            // 
            this.PrefabTypeComboBox.FormattingEnabled = true;
            this.PrefabTypeComboBox.Location = new System.Drawing.Point(103, 166);
            this.PrefabTypeComboBox.Name = "PrefabTypeComboBox";
            this.PrefabTypeComboBox.Size = new System.Drawing.Size(82, 21);
            this.PrefabTypeComboBox.TabIndex = 14;
            // 
            // BlockIDTextBox
            // 
            this.BlockIDTextBox.Location = new System.Drawing.Point(103, 197);
            this.BlockIDTextBox.Name = "BlockIDTextBox";
            this.BlockIDTextBox.Size = new System.Drawing.Size(82, 20);
            this.BlockIDTextBox.TabIndex = 15;
            this.BlockIDTextBox.Text = "403";
            this.BlockIDTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(19, 200);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(48, 13);
            this.label7.TabIndex = 16;
            this.label7.Text = "Block ID";
            // 
            // debugfbx2obj
            // 
            this.debugfbx2obj.ForeColor = System.Drawing.Color.SteelBlue;
            this.debugfbx2obj.Location = new System.Drawing.Point(196, 223);
            this.debugfbx2obj.Name = "debugfbx2obj";
            this.debugfbx2obj.Size = new System.Drawing.Size(112, 26);
            this.debugfbx2obj.TabIndex = 17;
            this.debugfbx2obj.Text = "<debug: fbx to obj>";
            this.debugfbx2obj.UseVisualStyleBackColor = true;
            this.debugfbx2obj.Click += new System.EventHandler(this.DebugFbxToObj);
            // 
            // VoxelizerControl
            // 
            this.Controls.Add(this.debugfbx2obj);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.BlockIDTextBox);
            this.Controls.Add(this.PrefabTypeComboBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.checkBoxForceMirroX);
            this.Controls.Add(this.checkBoxForceMirroZ);
            this.Controls.Add(this.checkBoxForceMirroY);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ObjAlignmentComboBox);
            this.Controls.Add(this.OpenBtn);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.WidthTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.LengthTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.HeightTextBox);
            this.Name = "VoxelizerControl";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox HeightTextBox;
        private Label label1;
        private Label label2;
        private Label label3;
        private TextBox LengthTextBox;
        private Label label4;
        private TextBox WidthTextBox;
        private Button OpenBtn;
        private ComboBox ObjAlignmentComboBox;
        private Label label5;
        private CheckBox checkBoxForceMirroY;
        private CheckBox checkBoxForceMirroZ;
        private CheckBox checkBoxForceMirroX;
        private Label label6;
        private ComboBox PrefabTypeComboBox;
        private TextBox BlockIDTextBox;
        private Label label7;
        private Button debugfbx2obj;
    }
}
