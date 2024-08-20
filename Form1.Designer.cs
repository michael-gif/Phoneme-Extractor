namespace Phoneme_Extractor
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            menuStrip1 = new MenuStrip();
            openAudioFileToolStripMenuItem = new ToolStripMenuItem();
            closeAudioFileToolStripMenuItem = new ToolStripMenuItem();
            analyzeLoadedFileToolStripMenuItem = new ToolStripMenuItem();
            waveViewer1 = new NAudio.Gui.WaveViewer();
            hScrollBar1 = new HScrollBar();
            panel1 = new Panel();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { openAudioFileToolStripMenuItem, closeAudioFileToolStripMenuItem, analyzeLoadedFileToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(984, 24);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // openAudioFileToolStripMenuItem
            // 
            openAudioFileToolStripMenuItem.Name = "openAudioFileToolStripMenuItem";
            openAudioFileToolStripMenuItem.Size = new Size(100, 20);
            openAudioFileToolStripMenuItem.Text = "Open audio file";
            openAudioFileToolStripMenuItem.Click += openAudioFileToolStripMenuItem_Click;
            // 
            // closeAudioFileToolStripMenuItem
            // 
            closeAudioFileToolStripMenuItem.Enabled = false;
            closeAudioFileToolStripMenuItem.Name = "closeAudioFileToolStripMenuItem";
            closeAudioFileToolStripMenuItem.Size = new Size(100, 20);
            closeAudioFileToolStripMenuItem.Text = "Close audio file";
            closeAudioFileToolStripMenuItem.Click += closeAudioFileToolStripMenuItem_Click;
            // 
            // analyzeLoadedFileToolStripMenuItem
            // 
            analyzeLoadedFileToolStripMenuItem.Enabled = false;
            analyzeLoadedFileToolStripMenuItem.Name = "analyzeLoadedFileToolStripMenuItem";
            analyzeLoadedFileToolStripMenuItem.Size = new Size(118, 20);
            analyzeLoadedFileToolStripMenuItem.Text = "Analyze loaded file";
            analyzeLoadedFileToolStripMenuItem.Click += analyzeLoadedFileToolStripMenuItem_Click;
            // 
            // waveViewer1
            // 
            waveViewer1.AutoScroll = true;
            waveViewer1.BackColor = Color.Transparent;
            waveViewer1.BackgroundImageLayout = ImageLayout.Stretch;
            waveViewer1.Dock = DockStyle.Fill;
            waveViewer1.Location = new Point(0, 24);
            waveViewer1.Name = "waveViewer1";
            waveViewer1.SamplesPerPixel = 128;
            waveViewer1.Size = new Size(984, 437);
            waveViewer1.StartPosition = 0L;
            waveViewer1.TabIndex = 1;
            waveViewer1.WaveStream = null;
            // 
            // hScrollBar1
            // 
            hScrollBar1.Dock = DockStyle.Bottom;
            hScrollBar1.Location = new Point(0, 444);
            hScrollBar1.Name = "hScrollBar1";
            hScrollBar1.Size = new Size(984, 17);
            hScrollBar1.TabIndex = 2;
            hScrollBar1.Scroll += hScrollBar1_Scroll;
            // 
            // panel1
            // 
            panel1.Dock = DockStyle.Bottom;
            panel1.Location = new Point(0, 394);
            panel1.Name = "panel1";
            panel1.Size = new Size(984, 50);
            panel1.TabIndex = 3;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImage = (Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = ImageLayout.Stretch;
            ClientSize = new Size(984, 461);
            Controls.Add(panel1);
            Controls.Add(hScrollBar1);
            Controls.Add(waveViewer1);
            Controls.Add(menuStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Phoneme Extractor";
            Resize += Form1_Resize;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem openAudioFileToolStripMenuItem;
        private ToolStripMenuItem analyzeLoadedFileToolStripMenuItem;
        private NAudio.Gui.WaveViewer waveViewer1;
        private HScrollBar hScrollBar1;
        private ToolStripMenuItem closeAudioFileToolStripMenuItem;
        private Panel panel1;
    }
}