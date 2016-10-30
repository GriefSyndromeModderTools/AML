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
            this.btnContinue = new System.Windows.Forms.Button();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnSetBreakPoint = new System.Windows.Forms.Button();
            this.btnStepInto = new System.Windows.Forms.Button();
            this.btnStepOver = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnExeToRet = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnContinue
            // 
            this.btnContinue.Enabled = false;
            this.btnContinue.Location = new System.Drawing.Point(106, 336);
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
            this.listBox1.Size = new System.Drawing.Size(616, 232);
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
            this.textBox1.Size = new System.Drawing.Size(615, 80);
            this.textBox1.TabIndex = 7;
            // 
            // btnSetBreakPoint
            // 
            this.btnSetBreakPoint.Location = new System.Drawing.Point(200, 336);
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
            this.btnStepInto.Location = new System.Drawing.Point(294, 336);
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
            this.btnStepOver.Location = new System.Drawing.Point(388, 336);
            this.btnStepOver.Name = "btnStepOver";
            this.btnStepOver.Size = new System.Drawing.Size(83, 41);
            this.btnStepOver.TabIndex = 4;
            this.btnStepOver.Text = "单步步过";
            this.btnStepOver.UseVisualStyleBackColor = true;
            this.btnStepOver.Click += new System.EventHandler(this.btnStepOver_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(12, 336);
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
            this.btnExeToRet.Location = new System.Drawing.Point(482, 336);
            this.btnExeToRet.Name = "btnExeToRet";
            this.btnExeToRet.Size = new System.Drawing.Size(83, 41);
            this.btnExeToRet.TabIndex = 5;
            this.btnExeToRet.Text = "执行到返回";
            this.btnExeToRet.UseVisualStyleBackColor = true;
            this.btnExeToRet.Click += new System.EventHandler(this.btnExeToRet_Click);
            // 
            // DebuggerWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 392);
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
    }
}