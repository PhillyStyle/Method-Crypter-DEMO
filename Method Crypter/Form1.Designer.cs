namespace Method_Crypter
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.listViewMethods = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxPathToCryptedDemoEXE = new System.Windows.Forms.TextBox();
            this.buttonPathToCryptedDemoEXE = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxMethodType = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxMethodName = new System.Windows.Forms.TextBox();
            this.buttonAddMethod = new System.Windows.Forms.Button();
            this.buttonEncrypt = new System.Windows.Forms.Button();
            this.buttonClearMethods = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxAESKey = new System.Windows.Forms.TextBox();
            this.textBoxAESIV = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxPathToServer = new System.Windows.Forms.TextBox();
            this.buttonPathToServerEXE = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.listViewStringArrays = new System.Windows.Forms.ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.buttonClearStringArray = new System.Windows.Forms.Button();
            this.buttonAddStringArray = new System.Windows.Forms.Button();
            this.textBoxStringMethodName = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxStringMethodType = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.checkBoxRandomizeMethodNames = new System.Windows.Forms.CheckBox();
            this.checkBoxInsertJunkCode = new System.Windows.Forms.CheckBox();
            this.checkBoxRandomizeGUID = new System.Windows.Forms.CheckBox();
            this.checkBoxShowMethodInspector = new System.Windows.Forms.CheckBox();
            this.buttonPathToOutputEXE = new System.Windows.Forms.Button();
            this.textBoxPathToOutputEXE = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.contextMenuStrip1.SuspendLayout();
            this.contextMenuStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // listViewMethods
            // 
            this.listViewMethods.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listViewMethods.ContextMenuStrip = this.contextMenuStrip1;
            this.listViewMethods.HideSelection = false;
            this.listViewMethods.Location = new System.Drawing.Point(6, 33);
            this.listViewMethods.Name = "listViewMethods";
            this.listViewMethods.Size = new System.Drawing.Size(421, 171);
            this.listViewMethods.TabIndex = 0;
            this.listViewMethods.UseCompatibleStateImageBehavior = false;
            this.listViewMethods.View = System.Windows.Forms.View.Details;
            this.listViewMethods.SelectedIndexChanged += new System.EventHandler(this.listViewMethods_SelectedIndexChanged);
            this.listViewMethods.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewMethods_KeyDown);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Type";
            this.columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Method Name";
            this.columnHeader2.Width = 200;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStripDelete";
            this.contextMenuStrip1.Size = new System.Drawing.Size(108, 26);
            this.contextMenuStrip1.Text = "Delete";
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 350);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Path To Crypted Demo EXE:";
            // 
            // textBoxPathToCryptedDemoEXE
            // 
            this.textBoxPathToCryptedDemoEXE.Location = new System.Drawing.Point(144, 346);
            this.textBoxPathToCryptedDemoEXE.Name = "textBoxPathToCryptedDemoEXE";
            this.textBoxPathToCryptedDemoEXE.Size = new System.Drawing.Size(255, 20);
            this.textBoxPathToCryptedDemoEXE.TabIndex = 2;
            this.textBoxPathToCryptedDemoEXE.Text = ".\\..\\..\\..\\Crypted Demo\\bin\\Release\\Crypted Demo.exe";
            // 
            // buttonPathToCryptedDemoEXE
            // 
            this.buttonPathToCryptedDemoEXE.Location = new System.Drawing.Point(403, 345);
            this.buttonPathToCryptedDemoEXE.Name = "buttonPathToCryptedDemoEXE";
            this.buttonPathToCryptedDemoEXE.Size = new System.Drawing.Size(24, 23);
            this.buttonPathToCryptedDemoEXE.TabIndex = 3;
            this.buttonPathToCryptedDemoEXE.Text = "...";
            this.buttonPathToCryptedDemoEXE.UseVisualStyleBackColor = true;
            this.buttonPathToCryptedDemoEXE.Click += new System.EventHandler(this.buttonPathToCryptedDemoEXE_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(46, 214);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Type:";
            // 
            // textBoxMethodType
            // 
            this.textBoxMethodType.Location = new System.Drawing.Point(79, 210);
            this.textBoxMethodType.Name = "textBoxMethodType";
            this.textBoxMethodType.Size = new System.Drawing.Size(348, 20);
            this.textBoxMethodType.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 240);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Method Name:";
            // 
            // textBoxMethodName
            // 
            this.textBoxMethodName.Location = new System.Drawing.Point(79, 237);
            this.textBoxMethodName.Name = "textBoxMethodName";
            this.textBoxMethodName.Size = new System.Drawing.Size(348, 20);
            this.textBoxMethodName.TabIndex = 7;
            // 
            // buttonAddMethod
            // 
            this.buttonAddMethod.Location = new System.Drawing.Point(352, 263);
            this.buttonAddMethod.Name = "buttonAddMethod";
            this.buttonAddMethod.Size = new System.Drawing.Size(75, 23);
            this.buttonAddMethod.TabIndex = 8;
            this.buttonAddMethod.Text = "Add";
            this.buttonAddMethod.UseVisualStyleBackColor = true;
            this.buttonAddMethod.Click += new System.EventHandler(this.buttonAddMethod_Click);
            // 
            // buttonEncrypt
            // 
            this.buttonEncrypt.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonEncrypt.Location = new System.Drawing.Point(309, 423);
            this.buttonEncrypt.Name = "buttonEncrypt";
            this.buttonEncrypt.Size = new System.Drawing.Size(252, 40);
            this.buttonEncrypt.TabIndex = 9;
            this.buttonEncrypt.Text = "Encrypt Methods And Strings";
            this.buttonEncrypt.UseVisualStyleBackColor = true;
            this.buttonEncrypt.Click += new System.EventHandler(this.buttonEncrypt_Click);
            // 
            // buttonClearMethods
            // 
            this.buttonClearMethods.Location = new System.Drawing.Point(6, 263);
            this.buttonClearMethods.Name = "buttonClearMethods";
            this.buttonClearMethods.Size = new System.Drawing.Size(75, 23);
            this.buttonClearMethods.TabIndex = 10;
            this.buttonClearMethods.Text = "Clear All";
            this.buttonClearMethods.UseVisualStyleBackColor = true;
            this.buttonClearMethods.Click += new System.EventHandler(this.buttonClearMethods_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 299);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "AES Key:";
            // 
            // textBoxAESKey
            // 
            this.textBoxAESKey.Location = new System.Drawing.Point(66, 294);
            this.textBoxAESKey.Name = "textBoxAESKey";
            this.textBoxAESKey.Size = new System.Drawing.Size(249, 20);
            this.textBoxAESKey.TabIndex = 12;
            // 
            // textBoxAESIV
            // 
            this.textBoxAESIV.Location = new System.Drawing.Point(66, 320);
            this.textBoxAESIV.Name = "textBoxAESIV";
            this.textBoxAESIV.Size = new System.Drawing.Size(249, 20);
            this.textBoxAESIV.TabIndex = 14;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 324);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(44, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "AES IV:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(3, 376);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(102, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Path to Server EXE:";
            // 
            // textBoxPathToServer
            // 
            this.textBoxPathToServer.Location = new System.Drawing.Point(106, 372);
            this.textBoxPathToServer.Name = "textBoxPathToServer";
            this.textBoxPathToServer.Size = new System.Drawing.Size(293, 20);
            this.textBoxPathToServer.TabIndex = 16;
            this.textBoxPathToServer.Text = ".\\..\\..\\..\\Server\\bin\\Release\\Server.exe";
            // 
            // buttonPathToServerEXE
            // 
            this.buttonPathToServerEXE.Location = new System.Drawing.Point(403, 371);
            this.buttonPathToServerEXE.Name = "buttonPathToServerEXE";
            this.buttonPathToServerEXE.Size = new System.Drawing.Size(24, 23);
            this.buttonPathToServerEXE.TabIndex = 17;
            this.buttonPathToServerEXE.Text = "...";
            this.buttonPathToServerEXE.UseVisualStyleBackColor = true;
            this.buttonPathToServerEXE.Click += new System.EventHandler(this.buttonPathToServerEXE_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(321, 294);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 46);
            this.button1.TabIndex = 18;
            this.button1.Text = "Random AES Key && IV";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.buttonRndAESKeyAndIV_Click);
            // 
            // listViewStringArrays
            // 
            this.listViewStringArrays.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
            this.listViewStringArrays.ContextMenuStrip = this.contextMenuStrip2;
            this.listViewStringArrays.HideSelection = false;
            this.listViewStringArrays.Location = new System.Drawing.Point(442, 33);
            this.listViewStringArrays.Name = "listViewStringArrays";
            this.listViewStringArrays.Size = new System.Drawing.Size(421, 171);
            this.listViewStringArrays.TabIndex = 19;
            this.listViewStringArrays.UseCompatibleStateImageBehavior = false;
            this.listViewStringArrays.View = System.Windows.Forms.View.Details;
            this.listViewStringArrays.SelectedIndexChanged += new System.EventHandler(this.listViewStringMethods_SelectedIndexChanged);
            this.listViewStringArrays.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listViewStringMethods_KeyDown);
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Type";
            this.columnHeader3.Width = 200;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Array Name";
            this.columnHeader4.Width = 200;
            // 
            // contextMenuStrip2
            // 
            this.contextMenuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem1});
            this.contextMenuStrip2.Name = "contextMenuStrip2";
            this.contextMenuStrip2.Size = new System.Drawing.Size(108, 26);
            // 
            // deleteToolStripMenuItem1
            // 
            this.deleteToolStripMenuItem1.Name = "deleteToolStripMenuItem1";
            this.deleteToolStripMenuItem1.Size = new System.Drawing.Size(107, 22);
            this.deleteToolStripMenuItem1.Text = "Delete";
            this.deleteToolStripMenuItem1.Click += new System.EventHandler(this.deleteToolStripMenuItem1_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(9, 9);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(165, 18);
            this.label7.TabIndex = 20;
            this.label7.Text = "Methods To Encrypt:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(445, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(197, 18);
            this.label8.TabIndex = 21;
            this.label8.Text = "String Arrays To Encrypt:";
            // 
            // buttonClearStringArray
            // 
            this.buttonClearStringArray.Location = new System.Drawing.Point(442, 263);
            this.buttonClearStringArray.Name = "buttonClearStringArray";
            this.buttonClearStringArray.Size = new System.Drawing.Size(75, 23);
            this.buttonClearStringArray.TabIndex = 27;
            this.buttonClearStringArray.Text = "Clear All";
            this.buttonClearStringArray.UseVisualStyleBackColor = true;
            this.buttonClearStringArray.Click += new System.EventHandler(this.buttonClearStringArrays_Click);
            // 
            // buttonAddStringArray
            // 
            this.buttonAddStringArray.Location = new System.Drawing.Point(788, 263);
            this.buttonAddStringArray.Name = "buttonAddStringArray";
            this.buttonAddStringArray.Size = new System.Drawing.Size(75, 23);
            this.buttonAddStringArray.TabIndex = 26;
            this.buttonAddStringArray.Text = "Add";
            this.buttonAddStringArray.UseVisualStyleBackColor = true;
            this.buttonAddStringArray.Click += new System.EventHandler(this.buttonAddStringArray_Click);
            // 
            // textBoxStringMethodName
            // 
            this.textBoxStringMethodName.Location = new System.Drawing.Point(515, 237);
            this.textBoxStringMethodName.Name = "textBoxStringMethodName";
            this.textBoxStringMethodName.Size = new System.Drawing.Size(348, 20);
            this.textBoxStringMethodName.TabIndex = 25;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(450, 240);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(65, 13);
            this.label9.TabIndex = 24;
            this.label9.Text = "Array Name:";
            // 
            // textBoxStringMethodType
            // 
            this.textBoxStringMethodType.Location = new System.Drawing.Point(515, 210);
            this.textBoxStringMethodType.Name = "textBoxStringMethodType";
            this.textBoxStringMethodType.Size = new System.Drawing.Size(348, 20);
            this.textBoxStringMethodType.TabIndex = 23;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(482, 214);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(34, 13);
            this.label10.TabIndex = 22;
            this.label10.Text = "Type:";
            // 
            // checkBoxRandomizeMethodNames
            // 
            this.checkBoxRandomizeMethodNames.AutoSize = true;
            this.checkBoxRandomizeMethodNames.Location = new System.Drawing.Point(558, 314);
            this.checkBoxRandomizeMethodNames.Name = "checkBoxRandomizeMethodNames";
            this.checkBoxRandomizeMethodNames.Size = new System.Drawing.Size(154, 17);
            this.checkBoxRandomizeMethodNames.TabIndex = 28;
            this.checkBoxRandomizeMethodNames.Text = "Randomize Method Names";
            this.checkBoxRandomizeMethodNames.UseVisualStyleBackColor = true;
            // 
            // checkBoxInsertJunkCode
            // 
            this.checkBoxInsertJunkCode.AutoSize = true;
            this.checkBoxInsertJunkCode.Location = new System.Drawing.Point(558, 338);
            this.checkBoxInsertJunkCode.Name = "checkBoxInsertJunkCode";
            this.checkBoxInsertJunkCode.Size = new System.Drawing.Size(192, 17);
            this.checkBoxInsertJunkCode.TabIndex = 29;
            this.checkBoxInsertJunkCode.Text = "Inject junk code at start of methods";
            this.checkBoxInsertJunkCode.UseVisualStyleBackColor = true;
            // 
            // checkBoxRandomizeGUID
            // 
            this.checkBoxRandomizeGUID.AutoSize = true;
            this.checkBoxRandomizeGUID.Location = new System.Drawing.Point(558, 359);
            this.checkBoxRandomizeGUID.Name = "checkBoxRandomizeGUID";
            this.checkBoxRandomizeGUID.Size = new System.Drawing.Size(156, 17);
            this.checkBoxRandomizeGUID.TabIndex = 30;
            this.checkBoxRandomizeGUID.Text = "Randomize Assembly GUID";
            this.checkBoxRandomizeGUID.UseVisualStyleBackColor = true;
            // 
            // checkBoxShowMethodInspector
            // 
            this.checkBoxShowMethodInspector.AutoSize = true;
            this.checkBoxShowMethodInspector.Location = new System.Drawing.Point(558, 382);
            this.checkBoxShowMethodInspector.Name = "checkBoxShowMethodInspector";
            this.checkBoxShowMethodInspector.Size = new System.Drawing.Size(139, 17);
            this.checkBoxShowMethodInspector.TabIndex = 31;
            this.checkBoxShowMethodInspector.Text = "Show Method Inspector";
            this.checkBoxShowMethodInspector.UseVisualStyleBackColor = true;
            // 
            // buttonPathToOutputEXE
            // 
            this.buttonPathToOutputEXE.Location = new System.Drawing.Point(403, 397);
            this.buttonPathToOutputEXE.Name = "buttonPathToOutputEXE";
            this.buttonPathToOutputEXE.Size = new System.Drawing.Size(24, 23);
            this.buttonPathToOutputEXE.TabIndex = 34;
            this.buttonPathToOutputEXE.Text = "...";
            this.buttonPathToOutputEXE.UseVisualStyleBackColor = true;
            this.buttonPathToOutputEXE.Click += new System.EventHandler(this.buttonPathToOutputEXE_Click);
            // 
            // textBoxPathToOutputEXE
            // 
            this.textBoxPathToOutputEXE.Location = new System.Drawing.Point(106, 398);
            this.textBoxPathToOutputEXE.Name = "textBoxPathToOutputEXE";
            this.textBoxPathToOutputEXE.Size = new System.Drawing.Size(293, 20);
            this.textBoxPathToOutputEXE.TabIndex = 33;
            this.textBoxPathToOutputEXE.Text = ".\\Crypted.exe";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 402);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(103, 13);
            this.label11.TabIndex = 32;
            this.label11.Text = "Path to Output EXE:";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(871, 467);
            this.Controls.Add(this.buttonPathToOutputEXE);
            this.Controls.Add(this.textBoxPathToOutputEXE);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.checkBoxShowMethodInspector);
            this.Controls.Add(this.checkBoxRandomizeGUID);
            this.Controls.Add(this.checkBoxInsertJunkCode);
            this.Controls.Add(this.checkBoxRandomizeMethodNames);
            this.Controls.Add(this.buttonClearStringArray);
            this.Controls.Add(this.buttonAddStringArray);
            this.Controls.Add(this.textBoxStringMethodName);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textBoxStringMethodType);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.listViewStringArrays);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonPathToServerEXE);
            this.Controls.Add(this.textBoxPathToServer);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBoxAESIV);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBoxAESKey);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.buttonClearMethods);
            this.Controls.Add(this.buttonEncrypt);
            this.Controls.Add(this.buttonAddMethod);
            this.Controls.Add(this.textBoxMethodName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxMethodType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonPathToCryptedDemoEXE);
            this.Controls.Add(this.textBoxPathToCryptedDemoEXE);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listViewMethods);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Method Crypter DEMO";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.contextMenuStrip2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listViewMethods;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxPathToCryptedDemoEXE;
        private System.Windows.Forms.Button buttonPathToCryptedDemoEXE;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxMethodType;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxMethodName;
        private System.Windows.Forms.Button buttonAddMethod;
        private System.Windows.Forms.Button buttonEncrypt;
        private System.Windows.Forms.Button buttonClearMethods;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxAESKey;
        private System.Windows.Forms.TextBox textBoxAESIV;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxPathToServer;
        private System.Windows.Forms.Button buttonPathToServerEXE;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListView listViewStringArrays;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button buttonClearStringArray;
        private System.Windows.Forms.Button buttonAddStringArray;
        private System.Windows.Forms.TextBox textBoxStringMethodName;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxStringMethodType;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip2;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem1;
        private System.Windows.Forms.CheckBox checkBoxRandomizeMethodNames;
        private System.Windows.Forms.CheckBox checkBoxInsertJunkCode;
        private System.Windows.Forms.CheckBox checkBoxRandomizeGUID;
        private System.Windows.Forms.CheckBox checkBoxShowMethodInspector;
        private System.Windows.Forms.Button buttonPathToOutputEXE;
        private System.Windows.Forms.TextBox textBoxPathToOutputEXE;
        private System.Windows.Forms.Label label11;
    }
}

