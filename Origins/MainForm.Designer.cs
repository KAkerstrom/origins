namespace Origins
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.statusPb = new System.Windows.Forms.PictureBox();
            this.titlePnl = new System.Windows.Forms.Panel();
            this.originsLbl = new System.Windows.Forms.Label();
            this.exitBtn = new System.Windows.Forms.Button();
            this.tutorialBtn = new System.Windows.Forms.Button();
            this.startBtn = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.statusPb)).BeginInit();
            this.titlePnl.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusPb
            // 
            this.statusPb.BackColor = System.Drawing.Color.Transparent;
            this.statusPb.Dock = System.Windows.Forms.DockStyle.Fill;
            this.statusPb.Location = new System.Drawing.Point(0, 0);
            this.statusPb.Name = "statusPb";
            this.statusPb.Size = new System.Drawing.Size(463, 326);
            this.statusPb.TabIndex = 0;
            this.statusPb.TabStop = false;
            this.statusPb.Paint += new System.Windows.Forms.PaintEventHandler(this.statusPb_Paint);
            // 
            // titlePnl
            // 
            this.titlePnl.BackgroundImage = global::Origins.Properties.Resources.water;
            this.titlePnl.Controls.Add(this.originsLbl);
            this.titlePnl.Controls.Add(this.exitBtn);
            this.titlePnl.Controls.Add(this.tutorialBtn);
            this.titlePnl.Controls.Add(this.startBtn);
            this.titlePnl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.titlePnl.Location = new System.Drawing.Point(0, 0);
            this.titlePnl.Name = "titlePnl";
            this.titlePnl.Size = new System.Drawing.Size(463, 326);
            this.titlePnl.TabIndex = 1;
            // 
            // originsLbl
            // 
            this.originsLbl.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.originsLbl.BackColor = System.Drawing.Color.Transparent;
            this.originsLbl.Font = new System.Drawing.Font("Arial", 40F);
            this.originsLbl.Location = new System.Drawing.Point(12, 42);
            this.originsLbl.Name = "originsLbl";
            this.originsLbl.Size = new System.Drawing.Size(439, 72);
            this.originsLbl.TabIndex = 3;
            this.originsLbl.Text = "Origins";
            this.originsLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // exitBtn
            // 
            this.exitBtn.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.exitBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.exitBtn.Font = new System.Drawing.Font("Arial", 20F);
            this.exitBtn.Location = new System.Drawing.Point(140, 245);
            this.exitBtn.Name = "exitBtn";
            this.exitBtn.Size = new System.Drawing.Size(182, 46);
            this.exitBtn.TabIndex = 2;
            this.exitBtn.Text = "Exit";
            this.exitBtn.UseVisualStyleBackColor = false;
            this.exitBtn.Click += new System.EventHandler(this.exitBtn_Click);
            // 
            // tutorialBtn
            // 
            this.tutorialBtn.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tutorialBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.tutorialBtn.Font = new System.Drawing.Font("Arial", 20F);
            this.tutorialBtn.Location = new System.Drawing.Point(140, 193);
            this.tutorialBtn.Name = "tutorialBtn";
            this.tutorialBtn.Size = new System.Drawing.Size(182, 46);
            this.tutorialBtn.TabIndex = 1;
            this.tutorialBtn.Text = "Tutorial";
            this.tutorialBtn.UseVisualStyleBackColor = false;
            this.tutorialBtn.Click += new System.EventHandler(this.tutorialBtn_Click);
            // 
            // startBtn
            // 
            this.startBtn.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.startBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.startBtn.Font = new System.Drawing.Font("Arial", 20F);
            this.startBtn.Location = new System.Drawing.Point(140, 141);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(182, 46);
            this.startBtn.TabIndex = 0;
            this.startBtn.Text = "Start Game";
            this.startBtn.UseVisualStyleBackColor = false;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Origins.Properties.Resources.water;
            this.ClientSize = new System.Drawing.Size(463, 326);
            this.Controls.Add(this.titlePnl);
            this.Controls.Add(this.statusPb);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(185, 185);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Origins";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.statusPb)).EndInit();
            this.titlePnl.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox statusPb;
        private System.Windows.Forms.Panel titlePnl;
        private System.Windows.Forms.Button exitBtn;
        private System.Windows.Forms.Button tutorialBtn;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.Label originsLbl;
    }
}

