namespace Method_Crypter
{
    partial class FormMethodInspector
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
            this.listViewMethodInspector = new System.Windows.Forms.ListView();
            this.columnHeaderType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMethod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderMDToken = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderRVA = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderEncodedSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonExportCSV = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listViewMethodInspector
            // 
            this.listViewMethodInspector.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewMethodInspector.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderType,
            this.columnHeaderMethod,
            this.columnHeaderMDToken,
            this.columnHeaderRVA,
            this.columnHeaderEncodedSize});
            this.listViewMethodInspector.HideSelection = false;
            this.listViewMethodInspector.Location = new System.Drawing.Point(2, 0);
            this.listViewMethodInspector.Name = "listViewMethodInspector";
            this.listViewMethodInspector.Size = new System.Drawing.Size(991, 456);
            this.listViewMethodInspector.TabIndex = 0;
            this.listViewMethodInspector.UseCompatibleStateImageBehavior = false;
            this.listViewMethodInspector.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderType
            // 
            this.columnHeaderType.Text = "Type";
            this.columnHeaderType.Width = 200;
            // 
            // columnHeaderMethod
            // 
            this.columnHeaderMethod.Text = "Method";
            this.columnHeaderMethod.Width = 120;
            // 
            // columnHeaderMDToken
            // 
            this.columnHeaderMDToken.Text = "MDToken";
            this.columnHeaderMDToken.Width = 120;
            // 
            // columnHeaderRVA
            // 
            this.columnHeaderRVA.Text = "RVA";
            this.columnHeaderRVA.Width = 120;
            // 
            // columnHeaderEncodedSize
            // 
            this.columnHeaderEncodedSize.Text = "Encoded Size (bytes)";
            this.columnHeaderEncodedSize.Width = 120;
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonOK.Location = new System.Drawing.Point(450, 462);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(95, 35);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonExportCSV
            // 
            this.buttonExportCSV.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonExportCSV.Location = new System.Drawing.Point(828, 469);
            this.buttonExportCSV.Name = "buttonExportCSV";
            this.buttonExportCSV.Size = new System.Drawing.Size(155, 23);
            this.buttonExportCSV.TabIndex = 2;
            this.buttonExportCSV.Text = "&Export to CSV";
            this.buttonExportCSV.UseVisualStyleBackColor = true;
            this.buttonExportCSV.Click += new System.EventHandler(this.buttonExportCSV_Click);
            // 
            // FormMethodInspector
            // 
            this.AcceptButton = this.buttonOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.buttonOK;
            this.ClientSize = new System.Drawing.Size(995, 504);
            this.Controls.Add(this.buttonExportCSV);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.listViewMethodInspector);
            this.MinimizeBox = false;
            this.Name = "FormMethodInspector";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Method Inspector";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FormMethodInspector_Load);
            this.Resize += new System.EventHandler(this.FormMethodInspector_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ListView listViewMethodInspector;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonExportCSV;
        private System.Windows.Forms.ColumnHeader columnHeaderType;
        private System.Windows.Forms.ColumnHeader columnHeaderMethod;
        private System.Windows.Forms.ColumnHeader columnHeaderMDToken;
        private System.Windows.Forms.ColumnHeader columnHeaderRVA;
        private System.Windows.Forms.ColumnHeader columnHeaderEncodedSize;
    }
}