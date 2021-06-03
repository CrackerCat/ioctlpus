﻿namespace ioctlpus
{
    partial class AboutForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutForm));
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.olvComponents = new BrightIdeasSoftware.ObjectListView();
            this.colComponent = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.colAuthor = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.colLicence = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olvComponents)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(330, 86);
            this.label1.TabIndex = 0;
            this.label1.Text = "ioctlpus 0.2-alpha\r\n©2017 Jackson Thuraisamy (@Jackson_T)\r\nUpdated in 2021 by Pao" +
    "lo Stagno (@Void_Sec)\r\n\r\nDedicated to my partner and family for putting up with " +
    "me.";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.olvComponents);
            this.groupBox1.Location = new System.Drawing.Point(12, 92);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(330, 144);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Open Source Components";
            // 
            // olvComponents
            // 
            this.olvComponents.AllColumns.Add(this.colComponent);
            this.olvComponents.AllColumns.Add(this.colAuthor);
            this.olvComponents.AllColumns.Add(this.colLicence);
            this.olvComponents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colComponent,
            this.colAuthor,
            this.colLicence});
            this.olvComponents.Cursor = System.Windows.Forms.Cursors.Default;
            this.olvComponents.HasCollapsibleGroups = false;
            this.olvComponents.HideSelection = false;
            this.olvComponents.Location = new System.Drawing.Point(6, 19);
            this.olvComponents.Name = "olvComponents";
            this.olvComponents.ShowGroups = false;
            this.olvComponents.Size = new System.Drawing.Size(318, 114);
            this.olvComponents.SortGroupItemsByPrimaryColumn = false;
            this.olvComponents.TabIndex = 1;
            this.olvComponents.UseCompatibleStateImageBehavior = false;
            this.olvComponents.View = System.Windows.Forms.View.Details;
            // 
            // colComponent
            // 
            this.colComponent.AspectName = "Component";
            this.colComponent.CellPadding = null;
            this.colComponent.MinimumWidth = 120;
            this.colComponent.Text = "Component";
            this.colComponent.Width = 120;
            // 
            // colAuthor
            // 
            this.colAuthor.AspectName = "Author";
            this.colAuthor.CellPadding = null;
            this.colAuthor.MinimumWidth = 90;
            this.colAuthor.Text = "Author";
            this.colAuthor.Width = 90;
            // 
            // colLicence
            // 
            this.colLicence.AspectName = "Licence";
            this.colLicence.CellPadding = null;
            this.colLicence.FillsFreeSpace = true;
            this.colLicence.Text = "Licence";
            // 
            // AboutForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 241);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(370, 280);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(370, 280);
            this.Name = "AboutForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About IOCTLpus";
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olvComponents)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private BrightIdeasSoftware.ObjectListView olvComponents;
        private BrightIdeasSoftware.OLVColumn colComponent;
        private BrightIdeasSoftware.OLVColumn colAuthor;
        private BrightIdeasSoftware.OLVColumn colLicence;
    }
}