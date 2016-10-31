namespace Debugger
{
    partial class DebuggerWindow
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
            this.btnContinue = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnSetBreakPoint = new System.Windows.Forms.Button();
            this.btnStepInto = new System.Windows.Forms.Button();
            this.btnStepOver = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnExeToRet = new System.Windows.Forms.Button();
            this.btnInteractive = new System.Windows.Forms.Button();
            this.lstLocalVar = new System.Windows.Forms.ListView();
            this.colVarName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colVarValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colVarType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lstCallStack = new System.Windows.Forms.ListBox();
            this.mnuLocalVar = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuCopyVar = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCallStack = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuCopyCallStack = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLocalVar.SuspendLayout();
            this.mnuCallStack.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnContinue
            // 
            this.btnContinue.Enabled = false;
            this.btnContinue.Location = new System.Drawing.Point(105, 438);
            this.btnContinue.Name = "btnContinue";
            this.btnContinue.Size = new System.Drawing.Size(83, 41);
            this.btnContinue.TabIndex = 1;
            this.btnContinue.Text = "继续";
            this.btnContinue.UseVisualStyleBackColor = true;
            this.btnContinue.Click += new System.EventHandler(this.btnContinue_Click);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.ItemHeight = 12;
            this.listBox1.Location = new System.Drawing.Point(12, 12);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(642, 232);
            this.listBox1.TabIndex = 6;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(13, 250);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(641, 80);
            this.textBox1.TabIndex = 7;
            // 
            // btnSetBreakPoint
            // 
            this.btnSetBreakPoint.Location = new System.Drawing.Point(198, 438);
            this.btnSetBreakPoint.Name = "btnSetBreakPoint";
            this.btnSetBreakPoint.Size = new System.Drawing.Size(83, 41);
            this.btnSetBreakPoint.TabIndex = 2;
            this.btnSetBreakPoint.Text = "设置断点";
            this.btnSetBreakPoint.UseVisualStyleBackColor = true;
            this.btnSetBreakPoint.Click += new System.EventHandler(this.btnSetBreakPoint_Click);
            // 
            // btnStepInto
            // 
            this.btnStepInto.Enabled = false;
            this.btnStepInto.Location = new System.Drawing.Point(291, 438);
            this.btnStepInto.Name = "btnStepInto";
            this.btnStepInto.Size = new System.Drawing.Size(83, 41);
            this.btnStepInto.TabIndex = 3;
            this.btnStepInto.Text = "单步步入";
            this.btnStepInto.UseVisualStyleBackColor = true;
            this.btnStepInto.Click += new System.EventHandler(this.btnStepInto_Click);
            // 
            // btnStepOver
            // 
            this.btnStepOver.Enabled = false;
            this.btnStepOver.Location = new System.Drawing.Point(384, 438);
            this.btnStepOver.Name = "btnStepOver";
            this.btnStepOver.Size = new System.Drawing.Size(83, 41);
            this.btnStepOver.TabIndex = 4;
            this.btnStepOver.Text = "单步步过";
            this.btnStepOver.UseVisualStyleBackColor = true;
            this.btnStepOver.Click += new System.EventHandler(this.btnStepOver_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(12, 438);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(83, 41);
            this.btnPause.TabIndex = 0;
            this.btnPause.Text = "暂停";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnExeToRet
            // 
            this.btnExeToRet.Enabled = false;
            this.btnExeToRet.Location = new System.Drawing.Point(477, 438);
            this.btnExeToRet.Name = "btnExeToRet";
            this.btnExeToRet.Size = new System.Drawing.Size(83, 41);
            this.btnExeToRet.TabIndex = 5;
            this.btnExeToRet.Text = "执行到返回";
            this.btnExeToRet.UseVisualStyleBackColor = true;
            this.btnExeToRet.Click += new System.EventHandler(this.btnExeToRet_Click);
            // 
            // btnInteractive
            // 
            this.btnInteractive.Location = new System.Drawing.Point(571, 438);
            this.btnInteractive.Name = "btnInteractive";
            this.btnInteractive.Size = new System.Drawing.Size(83, 41);
            this.btnInteractive.TabIndex = 8;
            this.btnInteractive.Text = "交互";
            this.btnInteractive.UseVisualStyleBackColor = true;
            this.btnInteractive.Click += new System.EventHandler(this.btnInteractive_Click);
            // 
            // lstLocalVar
            // 
            this.lstLocalVar.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colVarName,
            this.colVarValue,
            this.colVarType});
            this.lstLocalVar.ContextMenuStrip = this.mnuLocalVar;
            this.lstLocalVar.Enabled = false;
            this.lstLocalVar.Location = new System.Drawing.Point(12, 337);
            this.lstLocalVar.Name = "lstLocalVar";
            this.lstLocalVar.Size = new System.Drawing.Size(232, 88);
            this.lstLocalVar.TabIndex = 9;
            this.lstLocalVar.UseCompatibleStateImageBehavior = false;
            this.lstLocalVar.View = System.Windows.Forms.View.Details;
            // 
            // colVarName
            // 
            this.colVarName.Text = "本地变量名";
            this.colVarName.Width = 90;
            // 
            // colVarValue
            // 
            this.colVarValue.Text = "值";
            // 
            // colVarType
            // 
            this.colVarType.Text = "类型";
            // 
            // lstCallStack
            // 
            this.lstCallStack.ContextMenuStrip = this.mnuCallStack;
            this.lstCallStack.DisplayMember = "View";
            this.lstCallStack.Enabled = false;
            this.lstCallStack.FormattingEnabled = true;
            this.lstCallStack.ItemHeight = 12;
            this.lstCallStack.Location = new System.Drawing.Point(250, 337);
            this.lstCallStack.Name = "lstCallStack";
            this.lstCallStack.Size = new System.Drawing.Size(404, 88);
            this.lstCallStack.TabIndex = 10;
            this.lstCallStack.ValueMember = "View";
            this.lstCallStack.SelectedIndexChanged += new System.EventHandler(this.lstCallStack_SelectedIndexChanged);
            // 
            // mnuLocalVar
            // 
            this.mnuLocalVar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCopyVar});
            this.mnuLocalVar.Name = "mnuLocalVar";
            this.mnuLocalVar.Size = new System.Drawing.Size(101, 26);
            // 
            // mnuCopyVar
            // 
            this.mnuCopyVar.Name = "mnuCopyVar";
            this.mnuCopyVar.Size = new System.Drawing.Size(100, 22);
            this.mnuCopyVar.Text = "复制";
            this.mnuCopyVar.Click += new System.EventHandler(this.mnuCopyVar_Click);
            // 
            // mnuCallStack
            // 
            this.mnuCallStack.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCopyCallStack});
            this.mnuCallStack.Name = "mnuCallStack";
            this.mnuCallStack.Size = new System.Drawing.Size(101, 26);
            // 
            // mnuCopyCallStack
            // 
            this.mnuCopyCallStack.Name = "mnuCopyCallStack";
            this.mnuCopyCallStack.Size = new System.Drawing.Size(152, 22);
            this.mnuCopyCallStack.Text = "复制";
            this.mnuCopyCallStack.Click += new System.EventHandler(this.mnuCopyCallStack_Click);
            // 
            // DebuggerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(666, 491);
            this.Controls.Add(this.lstCallStack);
            this.Controls.Add(this.lstLocalVar);
            this.Controls.Add(this.btnInteractive);
            this.Controls.Add(this.btnExeToRet);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnStepOver);
            this.Controls.Add(this.btnStepInto);
            this.Controls.Add(this.btnSetBreakPoint);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.btnContinue);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "DebuggerWindow";
            this.Text = "DebuggerWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DebuggerWindow_FormClosing);
            this.Load += new System.EventHandler(this.DebuggerWindow_Load);
            this.mnuLocalVar.ResumeLayout(false);
            this.mnuCallStack.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnContinue;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnSetBreakPoint;
        private System.Windows.Forms.Button btnStepInto;
        private System.Windows.Forms.Button btnStepOver;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnExeToRet;
        private System.Windows.Forms.Button btnInteractive;
        private System.Windows.Forms.ListView lstLocalVar;
        private System.Windows.Forms.ColumnHeader colVarName;
        private System.Windows.Forms.ColumnHeader colVarValue;
        private System.Windows.Forms.ListBox lstCallStack;
        private System.Windows.Forms.ColumnHeader colVarType;
        private System.Windows.Forms.ContextMenuStrip mnuLocalVar;
        private System.Windows.Forms.ToolStripMenuItem mnuCopyVar;
        private System.Windows.Forms.ContextMenuStrip mnuCallStack;
        private System.Windows.Forms.ToolStripMenuItem mnuCopyCallStack;
    }
}