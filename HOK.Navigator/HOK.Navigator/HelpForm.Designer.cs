namespace HOK.Navigator
{
    partial class HelpForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HelpForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageSupport = new System.Windows.Forms.TabPage();
            this.buttonClose = new System.Windows.Forms.Button();
            this.groupBoxLinks = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.listViewWebPages = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageSupport.SuspendLayout();
            this.groupBoxLinks.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.buttonClose);
            this.splitContainer1.Size = new System.Drawing.Size(659, 481);
            this.splitContainer1.SplitterDistance = 426;
            this.splitContainer1.TabIndex = 0;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageSupport);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(659, 426);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPageSupport
            // 
            this.tabPageSupport.Controls.Add(this.groupBoxLinks);
            this.tabPageSupport.Location = new System.Drawing.Point(4, 22);
            this.tabPageSupport.Name = "tabPageSupport";
            this.tabPageSupport.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageSupport.Size = new System.Drawing.Size(651, 400);
            this.tabPageSupport.TabIndex = 0;
            this.tabPageSupport.Text = "Support";
            this.tabPageSupport.UseVisualStyleBackColor = true;
            // 
            // buttonClose
            // 
            this.buttonClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonClose.Location = new System.Drawing.Point(549, 9);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(99, 32);
            this.buttonClose.TabIndex = 18;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // groupBoxLinks
            // 
            this.groupBoxLinks.Controls.Add(this.label1);
            this.groupBoxLinks.Controls.Add(this.listViewWebPages);
            this.groupBoxLinks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBoxLinks.Location = new System.Drawing.Point(3, 3);
            this.groupBoxLinks.Name = "groupBoxLinks";
            this.groupBoxLinks.Padding = new System.Windows.Forms.Padding(5, 30, 5, 5);
            this.groupBoxLinks.Size = new System.Drawing.Size(645, 394);
            this.groupBoxLinks.TabIndex = 16;
            this.groupBoxLinks.TabStop = false;
            this.groupBoxLinks.Text = "Links";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Click on list item to open Web page.";
            // 
            // listViewWebPages
            // 
            this.listViewWebPages.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.listViewWebPages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewWebPages.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listViewWebPages.Location = new System.Drawing.Point(5, 43);
            this.listViewWebPages.MultiSelect = false;
            this.listViewWebPages.Name = "listViewWebPages";
            this.listViewWebPages.Size = new System.Drawing.Size(635, 346);
            this.listViewWebPages.TabIndex = 14;
            this.listViewWebPages.UseCompatibleStateImageBehavior = false;
            this.listViewWebPages.View = System.Windows.Forms.View.Details;
            this.listViewWebPages.SelectedIndexChanged += new System.EventHandler(this.listViewWebPages_SelectedIndexChanged_1);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Width = 340;
            // 
            // HelpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(659, 481);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(675, 520);
            this.Name = "HelpForm";
            this.Text = "HOK Navigator";
            this.Load += new System.EventHandler(this.HelpForm_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageSupport.ResumeLayout(false);
            this.groupBoxLinks.ResumeLayout(false);
            this.groupBoxLinks.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageSupport;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.GroupBox groupBoxLinks;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView listViewWebPages;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}