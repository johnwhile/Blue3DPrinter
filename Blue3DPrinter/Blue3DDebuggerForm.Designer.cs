
namespace Blue3DPrinter
{
    partial class Blue3DDebuggerForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.StorageExportButton = new System.Windows.Forms.Button();
            this.storageComboBox = new System.Windows.Forms.ComboBox();
            this.fbxfilenameLabel = new System.Windows.Forms.Label();
            this.BlockIdLabel = new System.Windows.Forms.Label();
            this.BlockIdSearch = new System.Windows.Forms.TextBox();
            this.childLabel = new System.Windows.Forms.Label();
            this.ComboBoxChildShape = new System.Windows.Forms.ComboBox();
            this.button2 = new System.Windows.Forms.Button();
            this.blockNameLabel = new System.Windows.Forms.Label();
            this.DescriptionNameSearch = new System.Windows.Forms.TextBox();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.BtnConvertTreeMesh = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(3, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(150, 40);
            this.button1.TabIndex = 0;
            this.button1.Text = "Reload BlocksConfig.ecf";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.fbxfilenameLabel);
            this.panel1.Controls.Add(this.BlockIdLabel);
            this.panel1.Controls.Add(this.BlockIdSearch);
            this.panel1.Controls.Add(this.childLabel);
            this.panel1.Controls.Add(this.ComboBoxChildShape);
            this.panel1.Controls.Add(this.button2);
            this.panel1.Controls.Add(this.blockNameLabel);
            this.panel1.Controls.Add(this.DescriptionNameSearch);
            this.panel1.Controls.Add(this.button1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(271, 601);
            this.panel1.TabIndex = 1;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.button3);
            this.groupBox2.Location = new System.Drawing.Point(15, 346);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(234, 60);
            this.groupBox2.TabIndex = 10;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Fbx Wrapper";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(8, 28);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(221, 23);
            this.button3.TabIndex = 1;
            this.button3.Text = "Convert Fbx To Obj";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.ConvertFbxToObj_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.StorageExportButton);
            this.groupBox1.Controls.Add(this.storageComboBox);
            this.groupBox1.Location = new System.Drawing.Point(16, 179);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(234, 82);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "storage debug";
            // 
            // StorageExportButton
            // 
            this.StorageExportButton.Location = new System.Drawing.Point(7, 48);
            this.StorageExportButton.Name = "StorageExportButton";
            this.StorageExportButton.Size = new System.Drawing.Size(221, 23);
            this.StorageExportButton.TabIndex = 1;
            this.StorageExportButton.Text = "extract file";
            this.StorageExportButton.UseVisualStyleBackColor = true;
            this.StorageExportButton.Click += new System.EventHandler(this.StorageExportButton_Click);
            // 
            // storageComboBox
            // 
            this.storageComboBox.FormattingEnabled = true;
            this.storageComboBox.Location = new System.Drawing.Point(7, 20);
            this.storageComboBox.Name = "storageComboBox";
            this.storageComboBox.Size = new System.Drawing.Size(221, 21);
            this.storageComboBox.TabIndex = 0;
            // 
            // fbxfilenameLabel
            // 
            this.fbxfilenameLabel.AutoSize = true;
            this.fbxfilenameLabel.Location = new System.Drawing.Point(12, 133);
            this.fbxfilenameLabel.Name = "fbxfilenameLabel";
            this.fbxfilenameLabel.Size = new System.Drawing.Size(109, 13);
            this.fbxfilenameLabel.TabIndex = 8;
            this.fbxfilenameLabel.Text = "filename of model Fbx";
            // 
            // BlockIdLabel
            // 
            this.BlockIdLabel.AutoSize = true;
            this.BlockIdLabel.Location = new System.Drawing.Point(193, 50);
            this.BlockIdLabel.Name = "BlockIdLabel";
            this.BlockIdLabel.Size = new System.Drawing.Size(46, 13);
            this.BlockIdLabel.TabIndex = 7;
            this.BlockIdLabel.Text = "Block Id";
            // 
            // BlockIdSearch
            // 
            this.BlockIdSearch.Location = new System.Drawing.Point(196, 66);
            this.BlockIdSearch.Name = "BlockIdSearch";
            this.BlockIdSearch.Size = new System.Drawing.Size(69, 20);
            this.BlockIdSearch.TabIndex = 6;
            this.BlockIdSearch.Text = "403";
            this.BlockIdSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BlockIdSearch_KeyPress);
            // 
            // childLabel
            // 
            this.childLabel.AutoSize = true;
            this.childLabel.Location = new System.Drawing.Point(13, 89);
            this.childLabel.Name = "childLabel";
            this.childLabel.Size = new System.Drawing.Size(61, 13);
            this.childLabel.TabIndex = 5;
            this.childLabel.Text = "ChildShape";
            // 
            // ComboBoxChildShape
            // 
            this.ComboBoxChildShape.FormattingEnabled = true;
            this.ComboBoxChildShape.Location = new System.Drawing.Point(12, 105);
            this.ComboBoxChildShape.Name = "ComboBoxChildShape";
            this.ComboBoxChildShape.Size = new System.Drawing.Size(253, 21);
            this.ComboBoxChildShape.TabIndex = 4;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(156, 3);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(109, 40);
            this.button2.TabIndex = 3;
            this.button2.Text = "Get Data";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.CreateShapeObj_Click);
            // 
            // blockNameLabel
            // 
            this.blockNameLabel.AutoSize = true;
            this.blockNameLabel.Location = new System.Drawing.Point(13, 50);
            this.blockNameLabel.Name = "blockNameLabel";
            this.blockNameLabel.Size = new System.Drawing.Size(65, 13);
            this.blockNameLabel.TabIndex = 2;
            this.blockNameLabel.Text = "Block Name";
            // 
            // DescriptionNameSearch
            // 
            this.DescriptionNameSearch.Location = new System.Drawing.Point(12, 66);
            this.DescriptionNameSearch.Name = "DescriptionNameSearch";
            this.DescriptionNameSearch.Size = new System.Drawing.Size(177, 20);
            this.DescriptionNameSearch.TabIndex = 1;
            this.DescriptionNameSearch.Text = "HullFullLarge";
            this.DescriptionNameSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DescriptionNameSearch_KeyPress);
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(271, 0);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(646, 601);
            this.treeView1.TabIndex = 2;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.BtnConvertTreeMesh);
            this.groupBox3.Location = new System.Drawing.Point(16, 280);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(234, 60);
            this.groupBox3.TabIndex = 11;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "My TreeMesh data storage";
            // 
            // button4
            // 
            this.BtnConvertTreeMesh.Location = new System.Drawing.Point(8, 28);
            this.BtnConvertTreeMesh.Name = "BtnConvertTreeMesh";
            this.BtnConvertTreeMesh.Size = new System.Drawing.Size(221, 23);
            this.BtnConvertTreeMesh.TabIndex = 1;
            this.BtnConvertTreeMesh.Text = "Convert Treemesh To Obj";
            this.BtnConvertTreeMesh.UseVisualStyleBackColor = true;
            this.BtnConvertTreeMesh.Click += new System.EventHandler(this.BtnConvertTreeMesh_Click);
            // 
            // Blue3DDebuggerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(917, 601);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.panel1);
            this.Name = "Blue3DDebuggerForm";
            this.Text = "ShapeDebugger";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Label blockNameLabel;
        private System.Windows.Forms.TextBox DescriptionNameSearch;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label childLabel;
        private System.Windows.Forms.ComboBox ComboBoxChildShape;
        private System.Windows.Forms.Label BlockIdLabel;
        private System.Windows.Forms.TextBox BlockIdSearch;
        private System.Windows.Forms.Label fbxfilenameLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button StorageExportButton;
        private System.Windows.Forms.ComboBox storageComboBox;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button BtnConvertTreeMesh;
    }
}