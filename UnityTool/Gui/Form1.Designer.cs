namespace Gui
{
    partial class Form1
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
            this.buttonBundle = new System.Windows.Forms.Button();
            this.buttonAsset = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonBundle
            // 
            this.buttonBundle.Location = new System.Drawing.Point(12, 12);
            this.buttonBundle.Name = "buttonBundle";
            this.buttonBundle.Size = new System.Drawing.Size(166, 40);
            this.buttonBundle.TabIndex = 0;
            this.buttonBundle.Text = "Extract Bundle";
            this.buttonBundle.UseVisualStyleBackColor = true;
            this.buttonBundle.Click += new System.EventHandler(this.buttonBundle_Click);
            // 
            // buttonAsset
            // 
            this.buttonAsset.Location = new System.Drawing.Point(12, 58);
            this.buttonAsset.Name = "buttonAsset";
            this.buttonAsset.Size = new System.Drawing.Size(166, 40);
            this.buttonAsset.TabIndex = 1;
            this.buttonAsset.Text = "Extract Asset";
            this.buttonAsset.UseVisualStyleBackColor = true;
            this.buttonAsset.Click += new System.EventHandler(this.buttonAsset_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 104);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(166, 40);
            this.button1.TabIndex = 2;
            this.button1.Text = "Treemesh to obj";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonExportScene_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(190, 155);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonAsset);
            this.Controls.Add(this.buttonBundle);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonBundle;
        private System.Windows.Forms.Button buttonAsset;
        private System.Windows.Forms.Button button1;
    }
}

