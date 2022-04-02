
namespace ExportData
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btn_machineA = new System.Windows.Forms.Button();
            this.btn_errorA = new System.Windows.Forms.Button();
            this.btn_dataA = new System.Windows.Forms.Button();
            this.dateTimePicker_endA = new System.Windows.Forms.DateTimePicker();
            this.dateTimePicker_startA = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btn_errorB = new System.Windows.Forms.Button();
            this.btn_machineB = new System.Windows.Forms.Button();
            this.btn_dataB = new System.Windows.Forms.Button();
            this.dateTimePicker_endB = new System.Windows.Forms.DateTimePicker();
            this.label3 = new System.Windows.Forms.Label();
            this.dateTimePicker_startB = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.progressBarB = new System.Windows.Forms.ProgressBar();
            this.progressBarA = new System.Windows.Forms.ProgressBar();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btn_save = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(743, 412);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.progressBarA);
            this.tabPage1.Controls.Add(this.btn_machineA);
            this.tabPage1.Controls.Add(this.btn_errorA);
            this.tabPage1.Controls.Add(this.btn_dataA);
            this.tabPage1.Controls.Add(this.dateTimePicker_endA);
            this.tabPage1.Controls.Add(this.dateTimePicker_startA);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 34);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(735, 374);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "A轴数据";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btn_machineA
            // 
            this.btn_machineA.Location = new System.Drawing.Point(302, 226);
            this.btn_machineA.Name = "btn_machineA";
            this.btn_machineA.Size = new System.Drawing.Size(167, 130);
            this.btn_machineA.TabIndex = 12;
            this.btn_machineA.Text = "导出MachineData源数据";
            this.btn_machineA.UseVisualStyleBackColor = true;
            this.btn_machineA.Click += new System.EventHandler(this.btn_machineA_Click);
            // 
            // btn_errorA
            // 
            this.btn_errorA.Location = new System.Drawing.Point(520, 226);
            this.btn_errorA.Name = "btn_errorA";
            this.btn_errorA.Size = new System.Drawing.Size(167, 130);
            this.btn_errorA.TabIndex = 11;
            this.btn_errorA.Text = "导出ErrorData源数据";
            this.btn_errorA.UseVisualStyleBackColor = true;
            this.btn_errorA.Click += new System.EventHandler(this.btn_errorA_Click);
            // 
            // btn_dataA
            // 
            this.btn_dataA.Location = new System.Drawing.Point(88, 226);
            this.btn_dataA.Name = "btn_dataA";
            this.btn_dataA.Size = new System.Drawing.Size(167, 130);
            this.btn_dataA.TabIndex = 10;
            this.btn_dataA.Text = "生成报表";
            this.btn_dataA.UseVisualStyleBackColor = true;
            this.btn_dataA.Click += new System.EventHandler(this.btn_dataA_Click);
            // 
            // dateTimePicker_endA
            // 
            this.dateTimePicker_endA.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker_endA.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_endA.Location = new System.Drawing.Point(213, 91);
            this.dateTimePicker_endA.Name = "dateTimePicker_endA";
            this.dateTimePicker_endA.Size = new System.Drawing.Size(349, 31);
            this.dateTimePicker_endA.TabIndex = 9;
            // 
            // dateTimePicker_startA
            // 
            this.dateTimePicker_startA.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker_startA.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_startA.Location = new System.Drawing.Point(213, 40);
            this.dateTimePicker_startA.Name = "dateTimePicker_startA";
            this.dateTimePicker_startA.Size = new System.Drawing.Size(349, 31);
            this.dateTimePicker_startA.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(83, 91);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 25);
            this.label2.TabIndex = 2;
            this.label2.Text = "结束时间";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(83, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "开始时间";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.progressBarB);
            this.tabPage2.Controls.Add(this.btn_errorB);
            this.tabPage2.Controls.Add(this.btn_machineB);
            this.tabPage2.Controls.Add(this.btn_dataB);
            this.tabPage2.Controls.Add(this.dateTimePicker_endB);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.dateTimePicker_startB);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Location = new System.Drawing.Point(4, 34);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(735, 374);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "B轴数据";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btn_errorB
            // 
            this.btn_errorB.Location = new System.Drawing.Point(498, 229);
            this.btn_errorB.Name = "btn_errorB";
            this.btn_errorB.Size = new System.Drawing.Size(167, 130);
            this.btn_errorB.TabIndex = 15;
            this.btn_errorB.Text = "导出ErrorData源数据";
            this.btn_errorB.UseVisualStyleBackColor = true;
            this.btn_errorB.Click += new System.EventHandler(this.btn_errorB_Click);
            // 
            // btn_machineB
            // 
            this.btn_machineB.Location = new System.Drawing.Point(293, 229);
            this.btn_machineB.Name = "btn_machineB";
            this.btn_machineB.Size = new System.Drawing.Size(167, 130);
            this.btn_machineB.TabIndex = 14;
            this.btn_machineB.Text = "导出MachineData源数据";
            this.btn_machineB.UseVisualStyleBackColor = true;
            this.btn_machineB.Click += new System.EventHandler(this.btn_machineB_Click);
            // 
            // btn_dataB
            // 
            this.btn_dataB.Location = new System.Drawing.Point(83, 229);
            this.btn_dataB.Name = "btn_dataB";
            this.btn_dataB.Size = new System.Drawing.Size(167, 130);
            this.btn_dataB.TabIndex = 13;
            this.btn_dataB.Text = "生成报表";
            this.btn_dataB.UseVisualStyleBackColor = true;
            this.btn_dataB.Click += new System.EventHandler(this.btn_dataB_Click);
            // 
            // dateTimePicker_endB
            // 
            this.dateTimePicker_endB.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker_endB.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_endB.Location = new System.Drawing.Point(221, 114);
            this.dateTimePicker_endB.Name = "dateTimePicker_endB";
            this.dateTimePicker_endB.Size = new System.Drawing.Size(349, 31);
            this.dateTimePicker_endB.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(106, 114);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 25);
            this.label3.TabIndex = 6;
            this.label3.Text = "结束时间";
            // 
            // dateTimePicker_startB
            // 
            this.dateTimePicker_startB.CustomFormat = "yyyy-MM-dd HH:mm:ss";
            this.dateTimePicker_startB.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePicker_startB.Location = new System.Drawing.Point(221, 46);
            this.dateTimePicker_startB.Name = "dateTimePicker_startB";
            this.dateTimePicker_startB.Size = new System.Drawing.Size(349, 31);
            this.dateTimePicker_startB.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(106, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 25);
            this.label4.TabIndex = 4;
            this.label4.Text = "开始时间";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // progressBarB
            // 
            this.progressBarB.Location = new System.Drawing.Point(111, 158);
            this.progressBarB.Name = "progressBarB";
            this.progressBarB.Size = new System.Drawing.Size(553, 53);
            this.progressBarB.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBarB.TabIndex = 16;
            this.progressBarB.Visible = false;
            // 
            // progressBarA
            // 
            this.progressBarA.Location = new System.Drawing.Point(113, 151);
            this.progressBarA.Name = "progressBarA";
            this.progressBarA.Size = new System.Drawing.Size(553, 53);
            this.progressBarA.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBarA.TabIndex = 17;
            this.progressBarA.Visible = false;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btn_save);
            this.tabPage3.Controls.Add(this.textBox1);
            this.tabPage3.Controls.Add(this.label5);
            this.tabPage3.Location = new System.Drawing.Point(4, 34);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(735, 374);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "设置";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(84, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(119, 25);
            this.label5.TabIndex = 1;
            this.label5.Text = "设计的UPH";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(228, 69);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(218, 31);
            this.textBox1.TabIndex = 2;
            this.textBox1.Text = "120";
            // 
            // btn_save
            // 
            this.btn_save.Location = new System.Drawing.Point(465, 62);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(144, 45);
            this.btn_save.TabIndex = 3;
            this.btn_save.Text = "保存";
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DateTimePicker dateTimePicker_endB;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DateTimePicker dateTimePicker_startB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DateTimePicker dateTimePicker_endA;
        private System.Windows.Forms.DateTimePicker dateTimePicker_startA;
        private System.Windows.Forms.Button btn_machineA;
        private System.Windows.Forms.Button btn_errorA;
        private System.Windows.Forms.Button btn_dataA;
        private System.Windows.Forms.Button btn_errorB;
        private System.Windows.Forms.Button btn_machineB;
        private System.Windows.Forms.Button btn_dataB;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ProgressBar progressBarB;
        private System.Windows.Forms.ProgressBar progressBarA;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label5;
    }
}

