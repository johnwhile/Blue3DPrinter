using System.ComponentModel;
using System.Windows.Forms;

namespace Blue3DPrinter
{
    partial class Blue3DPrinterForm
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

        #region Codice generato da Progettazione Windows Form

        /// <summary>
        /// Metodo necessario per il supporto della finestra di progettazione. Non modificare
        /// il contenuto del metodo con l'editor di codice.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Blue3DPrinterForm));
            this.OpenEpbToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SaveEpbToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.OpenBlueprintMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debuggerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertToEbpuzToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.convertToTxtToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DebugShapeMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.unpackBundlesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.TabPageTool = new System.Windows.Forms.TabPage();
            this.converterControl = new ConverterControl();
            this.TabPageVoxelizer = new System.Windows.Forms.TabPage();
            this.voxelizerControl = new VoxelizerControl();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.menuStrip1.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.TabPageTool.SuspendLayout();
            this.TabPageVoxelizer.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.settingsToolStripMenuItem,
            this.debuggerToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(328, 24);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // FileMenu
            // 
            this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.OpenBlueprintMenu});
            this.FileMenu.Name = "FileMenu";
            this.FileMenu.Size = new System.Drawing.Size(37, 20);
            this.FileMenu.Text = "File";
            // 
            // OpenBlueprintMenu
            // 
            this.OpenBlueprintMenu.Name = "OpenBlueprintMenu";
            this.OpenBlueprintMenu.Size = new System.Drawing.Size(154, 22);
            this.OpenBlueprintMenu.Text = "Open Blueprint";
            this.OpenBlueprintMenu.ToolTipText = "Read and load internally the blueprint file (I don\'t remember the purpose of this" +
    " action :)";
            this.OpenBlueprintMenu.Click += new System.EventHandler(this.OpenBlueprintMenu_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            this.settingsToolStripMenuItem.ToolTipText = "Some prototype settings that I will use in the future";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsMenuItem_Click);
            // 
            // debuggerToolStripMenuItem
            // 
            this.debuggerToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.convertToEbpuzToolStripMenuItem,
            this.convertToTxtToolStripMenuItem,
            this.DebugShapeMenu,
            this.unpackBundlesToolStripMenuItem});
            this.debuggerToolStripMenuItem.Name = "debuggerToolStripMenuItem";
            this.debuggerToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
            this.debuggerToolStripMenuItem.Text = "Debugger";
            this.debuggerToolStripMenuItem.ToolTipText = "Collection of useful functions that i used to investigate .epb files";
            // 
            // convertToEbpuzToolStripMenuItem
            // 
            this.convertToEbpuzToolStripMenuItem.Name = "convertToEbpuzToolStripMenuItem";
            this.convertToEbpuzToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.convertToEbpuzToolStripMenuItem.Text = "Save {.epb} as {.epbx}";
            this.convertToEbpuzToolStripMenuItem.ToolTipText = "Convert the .epb file into an uncompressed binary file similar to .epb.\nIt\'s used" +
    " to investigate the compressed part that contain 3d block data";
            this.convertToEbpuzToolStripMenuItem.Click += new System.EventHandler(this.convertToEBPXmenuItem_Click);
            // 
            // convertToTxtToolStripMenuItem
            // 
            this.convertToTxtToolStripMenuItem.Name = "convertToTxtToolStripMenuItem";
            this.convertToTxtToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.convertToTxtToolStripMenuItem.Text = "Save {.epb} as {.txt}";
            this.convertToTxtToolStripMenuItem.Click += new System.EventHandler(this.convertToTXTmenuItem_Click);
            // 
            // DebugShapeMenu
            // 
            this.DebugShapeMenu.Name = "DebugShapeMenu";
            this.DebugShapeMenu.Size = new System.Drawing.Size(186, 22);
            this.DebugShapeMenu.Text = "Debug Single Shape";
            this.DebugShapeMenu.ToolTipText = "Estract single geometry, only for debug purpose";
            this.DebugShapeMenu.Click += new System.EventHandler(this.DebugShapeMenu_Click);
            // 
            // unpackBundlesToolStripMenuItem
            // 
            this.unpackBundlesToolStripMenuItem.Name = "unpackBundlesToolStripMenuItem";
            this.unpackBundlesToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
            this.unpackBundlesToolStripMenuItem.Text = "Unpack Bundles";
            this.unpackBundlesToolStripMenuItem.ToolTipText = "Estract all game assets, don\'t use it";
            // 
            // tabControl
            // 
            this.tabControl.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.tabControl.Controls.Add(this.TabPageTool);
            this.tabControl.Controls.Add(this.TabPageVoxelizer);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 24);
            this.tabControl.Multiline = true;
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(328, 255);
            this.tabControl.TabIndex = 8;
            // 
            // TabPageTool
            // 
            this.TabPageTool.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.TabPageTool.Controls.Add(this.converterControl);
            this.TabPageTool.Location = new System.Drawing.Point(4, 25);
            this.TabPageTool.Name = "TabPageTool";
            this.TabPageTool.Size = new System.Drawing.Size(320, 226);
            this.TabPageTool.TabIndex = 1;
            this.TabPageTool.Text = "Tool";
            // 
            // converterControl
            // 
            this.converterControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.converterControl.Location = new System.Drawing.Point(0, 0);
            this.converterControl.main = null;
            this.converterControl.Name = "converterControl";
            this.converterControl.Size = new System.Drawing.Size(320, 226);
            this.converterControl.TabIndex = 0;
            // 
            // TabPageVoxelizer
            // 
            this.TabPageVoxelizer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.TabPageVoxelizer.Controls.Add(this.voxelizerControl);
            this.TabPageVoxelizer.Location = new System.Drawing.Point(4, 25);
            this.TabPageVoxelizer.Name = "TabPageVoxelizer";
            this.TabPageVoxelizer.Size = new System.Drawing.Size(320, 226);
            this.TabPageVoxelizer.TabIndex = 0;
            this.TabPageVoxelizer.Text = "Voxelizer";
            // 
            // voxelizerControl
            // 
            this.voxelizerControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.voxelizerControl.Location = new System.Drawing.Point(0, 0);
            this.voxelizerControl.main = null;
            this.voxelizerControl.Name = "voxelizerControl";
            this.voxelizerControl.Size = new System.Drawing.Size(320, 226);
            this.voxelizerControl.TabIndex = 0;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 279);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(328, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // Blue3DPrinterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(330, 310);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.HelpButton = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Blue3DPrinterForm";
            this.Load += new System.EventHandler(this.Blue3DPrinterForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.TabPageTool.ResumeLayout(false);
            this.TabPageVoxelizer.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private ToolTip OpenEpbToolTip;
        private ToolTip SaveEpbToolTip;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem FileMenu;
        private ToolStripMenuItem OpenBlueprintMenu;
        private ToolStripMenuItem debuggerToolStripMenuItem;
        private ToolStripMenuItem convertToEbpuzToolStripMenuItem;
        private ToolStripMenuItem DebugShapeMenu;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private ToolStripMenuItem unpackBundlesToolStripMenuItem;
        private TabControl tabControl;
        private TabPage TabPageVoxelizer;
        private TabPage TabPageTool;

        private ToolStripStatusLabel toolStripStatusLabel1;
        private StatusStrip statusStrip1;

        private VoxelizerControl voxelizerControl;
        private ConverterControl converterControl;
        private HelpProvider helpProvider1;
        private ToolStripMenuItem convertToTxtToolStripMenuItem;
    }
}

