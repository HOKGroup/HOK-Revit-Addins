namespace HOK.LPDCalculator
{
    partial class CommandForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommandForm));
            this.bttnBuilding = new System.Windows.Forms.Button();
            this.bttnSpace = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButtonBoth = new System.Windows.Forms.RadioButton();
            this.radioButtonLink = new System.Windows.Forms.RadioButton();
            this.radioButtonHost = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.bttnCancel = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.progressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.linkLabelHelp = new System.Windows.Forms.LinkLabel();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // bttnBuilding
            // 
            this.bttnBuilding.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.bttnBuilding.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.bttnBuilding.Image = ((System.Drawing.Image)(resources.GetObject("bttnBuilding.Image")));
            this.bttnBuilding.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.bttnBuilding.Location = new System.Drawing.Point(35, 56);
            this.bttnBuilding.Name = "bttnBuilding";
            this.bttnBuilding.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.bttnBuilding.Size = new System.Drawing.Size(95, 95);
            this.bttnBuilding.TabIndex = 1;
            this.bttnBuilding.Text = "Building Area ";
            this.bttnBuilding.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.bttnBuilding.UseVisualStyleBackColor = false;
            this.bttnBuilding.Click += new System.EventHandler(this.bttnBuilding_Click);
            // 
            // bttnSpace
            // 
            this.bttnSpace.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.bttnSpace.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.bttnSpace.Image = ((System.Drawing.Image)(resources.GetObject("bttnSpace.Image")));
            this.bttnSpace.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.bttnSpace.Location = new System.Drawing.Point(164, 56);
            this.bttnSpace.Name = "bttnSpace";
            this.bttnSpace.Padding = new System.Windows.Forms.Padding(0, 0, 0, 5);
            this.bttnSpace.Size = new System.Drawing.Size(95, 95);
            this.bttnSpace.TabIndex = 2;
            this.bttnSpace.Text = "Space by Space";
            this.bttnSpace.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.bttnSpace.UseVisualStyleBackColor = false;
            this.bttnSpace.Click += new System.EventHandler(this.bttnSpace_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButtonBoth);
            this.groupBox1.Controls.Add(this.radioButtonLink);
            this.groupBox1.Controls.Add(this.radioButtonHost);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(295, 62);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Lighting Fixtures Source";
            // 
            // radioButtonBoth
            // 
            this.radioButtonBoth.AutoSize = true;
            this.radioButtonBoth.Checked = true;
            this.radioButtonBoth.Location = new System.Drawing.Point(228, 28);
            this.radioButtonBoth.Name = "radioButtonBoth";
            this.radioButtonBoth.Size = new System.Drawing.Size(50, 17);
            this.radioButtonBoth.TabIndex = 2;
            this.radioButtonBoth.TabStop = true;
            this.radioButtonBoth.Text = "Both ";
            this.radioButtonBoth.UseVisualStyleBackColor = true;
            // 
            // radioButtonLink
            // 
            this.radioButtonLink.AutoSize = true;
            this.radioButtonLink.Location = new System.Drawing.Point(125, 28);
            this.radioButtonLink.Name = "radioButtonLink";
            this.radioButtonLink.Size = new System.Drawing.Size(97, 17);
            this.radioButtonLink.TabIndex = 1;
            this.radioButtonLink.Text = "Revit Link Only";
            this.radioButtonLink.UseVisualStyleBackColor = true;
            // 
            // radioButtonHost
            // 
            this.radioButtonHost.AutoSize = true;
            this.radioButtonHost.Location = new System.Drawing.Point(16, 28);
            this.radioButtonHost.Name = "radioButtonHost";
            this.radioButtonHost.Size = new System.Drawing.Size(103, 17);
            this.radioButtonHost.TabIndex = 0;
            this.radioButtonHost.Text = "Host Model Only";
            this.radioButtonHost.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.bttnBuilding);
            this.groupBox2.Controls.Add(this.bttnSpace);
            this.groupBox2.Location = new System.Drawing.Point(13, 81);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(294, 173);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Analysis Type";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(194, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Select one of LPD calculation methods.";
            // 
            // bttnCancel
            // 
            this.bttnCancel.Location = new System.Drawing.Point(232, 260);
            this.bttnCancel.Name = "bttnCancel";
            this.bttnCancel.Size = new System.Drawing.Size(75, 23);
            this.bttnCancel.TabIndex = 5;
            this.bttnCancel.Text = "Cancel";
            this.bttnCancel.UseVisualStyleBackColor = true;
            this.bttnCancel.Click += new System.EventHandler(this.bttnCancel_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.progressBar,
            this.statusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 290);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(324, 22);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 16);
            this.progressBar.Step = 1;
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(39, 17);
            this.statusLabel.Text = "Ready";
            // 
            // linkLabelHelp
            // 
            this.linkLabelHelp.AutoSize = true;
            this.linkLabelHelp.Location = new System.Drawing.Point(25, 265);
            this.linkLabelHelp.Name = "linkLabelHelp";
            this.linkLabelHelp.Size = new System.Drawing.Size(29, 13);
            this.linkLabelHelp.TabIndex = 7;
            this.linkLabelHelp.TabStop = true;
            this.linkLabelHelp.Text = "Help";
            this.linkLabelHelp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelHelp_LinkClicked);
            // 
            // CommandForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 312);
            this.Controls.Add(this.linkLabelHelp);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.bttnCancel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CommandForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "LPD Analysis";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bttnBuilding;
        private System.Windows.Forms.Button bttnSpace;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButtonHost;
        private System.Windows.Forms.RadioButton radioButtonBoth;
        private System.Windows.Forms.RadioButton radioButtonLink;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button bttnCancel;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar progressBar;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.LinkLabel linkLabelHelp;
    }
}