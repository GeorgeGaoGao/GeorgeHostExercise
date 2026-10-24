namespace AutoFocusDemo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.mainGroupBox = new System.Windows.Forms.GroupBox();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.cbDeviceList = new System.Windows.Forms.ComboBox();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.bnRefreshFocalPower = new System.Windows.Forms.Button();
            this.tbTriggerDelayTime = new System.Windows.Forms.TextBox();
            this.tbCurrentFocalPower = new System.Windows.Forms.TextBox();
            this.tbMinFocalPower = new System.Windows.Forms.TextBox();
            this.tbMaxFocalPower = new System.Windows.Forms.TextBox();
            this.tbStableTime = new System.Windows.Forms.TextBox();
            this.tbLensFirmwareVersion = new System.Windows.Forms.TextBox();
            this.tbLensModelName = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.bnSetFocalPower = new System.Windows.Forms.Button();
            this.tbInputFocalPower = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.bnExecFocus = new System.Windows.Forms.Button();
            this.tbResultFocalPower = new System.Windows.Forms.TextBox();
            this.tbCoarseSteps = new System.Windows.Forms.TextBox();
            this.tbEndFocalPower = new System.Windows.Forms.TextBox();
            this.tbStartFocalPower = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.bnCloseLens = new System.Windows.Forms.Button();
            this.bnClose = new System.Windows.Forms.Button();
            this.bnOpenLens = new System.Windows.Forms.Button();
            this.bnOpen = new System.Windows.Forms.Button();
            this.bnEnum = new System.Windows.Forms.Button();
            this.mainGroupBox.SuspendLayout();
            this.leftPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.rightPanel.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainGroupBox
            // 
            this.mainGroupBox.Controls.Add(this.leftPanel);
            this.mainGroupBox.Controls.Add(this.rightPanel);
            resources.ApplyResources(this.mainGroupBox, "mainGroupBox");
            this.mainGroupBox.Name = "mainGroupBox";
            this.mainGroupBox.TabStop = false;
            // 
            // leftPanel
            // 
            this.leftPanel.Controls.Add(this.pictureBox1);
            this.leftPanel.Controls.Add(this.cbDeviceList);
            resources.ApplyResources(this.leftPanel, "leftPanel");
            this.leftPanel.Name = "leftPanel";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            resources.ApplyResources(this.pictureBox1, "pictureBox1");
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.TabStop = false;
            // 
            // cbDeviceList
            // 
            this.cbDeviceList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDeviceList.FormattingEnabled = true;
            resources.ApplyResources(this.cbDeviceList, "cbDeviceList");
            this.cbDeviceList.Name = "cbDeviceList";
            // 
            // rightPanel
            // 
            this.rightPanel.Controls.Add(this.groupBox8);
            this.rightPanel.Controls.Add(this.groupBox7);
            this.rightPanel.Controls.Add(this.groupBox6);
            this.rightPanel.Controls.Add(this.groupBox1);
            resources.ApplyResources(this.rightPanel, "rightPanel");
            this.rightPanel.Name = "rightPanel";
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.bnRefreshFocalPower);
            this.groupBox8.Controls.Add(this.tbTriggerDelayTime);
            this.groupBox8.Controls.Add(this.tbCurrentFocalPower);
            this.groupBox8.Controls.Add(this.tbMinFocalPower);
            this.groupBox8.Controls.Add(this.tbMaxFocalPower);
            this.groupBox8.Controls.Add(this.tbStableTime);
            this.groupBox8.Controls.Add(this.tbLensFirmwareVersion);
            this.groupBox8.Controls.Add(this.tbLensModelName);
            this.groupBox8.Controls.Add(this.label18);
            this.groupBox8.Controls.Add(this.label17);
            this.groupBox8.Controls.Add(this.label16);
            this.groupBox8.Controls.Add(this.label15);
            this.groupBox8.Controls.Add(this.label14);
            this.groupBox8.Controls.Add(this.label7);
            this.groupBox8.Controls.Add(this.label6);
            resources.ApplyResources(this.groupBox8, "groupBox8");
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.TabStop = false;
            // 
            // bnRefreshFocalPower
            // 
            resources.ApplyResources(this.bnRefreshFocalPower, "bnRefreshFocalPower");
            this.bnRefreshFocalPower.Name = "bnRefreshFocalPower";
            this.bnRefreshFocalPower.UseVisualStyleBackColor = true;
            this.bnRefreshFocalPower.Click += new System.EventHandler(this.bnRefreshFocalPower_Click);
            // 
            // tbTriggerDelayTime
            // 
            resources.ApplyResources(this.tbTriggerDelayTime, "tbTriggerDelayTime");
            this.tbTriggerDelayTime.Name = "tbTriggerDelayTime";
            this.tbTriggerDelayTime.ReadOnly = true;
            // 
            // tbCurrentFocalPower
            // 
            resources.ApplyResources(this.tbCurrentFocalPower, "tbCurrentFocalPower");
            this.tbCurrentFocalPower.Name = "tbCurrentFocalPower";
            this.tbCurrentFocalPower.ReadOnly = true;
            // 
            // tbMinFocalPower
            // 
            resources.ApplyResources(this.tbMinFocalPower, "tbMinFocalPower");
            this.tbMinFocalPower.Name = "tbMinFocalPower";
            this.tbMinFocalPower.ReadOnly = true;
            // 
            // tbMaxFocalPower
            // 
            resources.ApplyResources(this.tbMaxFocalPower, "tbMaxFocalPower");
            this.tbMaxFocalPower.Name = "tbMaxFocalPower";
            this.tbMaxFocalPower.ReadOnly = true;
            // 
            // tbStableTime
            // 
            resources.ApplyResources(this.tbStableTime, "tbStableTime");
            this.tbStableTime.Name = "tbStableTime";
            this.tbStableTime.ReadOnly = true;
            // 
            // tbLensFirmwareVersion
            // 
            resources.ApplyResources(this.tbLensFirmwareVersion, "tbLensFirmwareVersion");
            this.tbLensFirmwareVersion.Name = "tbLensFirmwareVersion";
            this.tbLensFirmwareVersion.ReadOnly = true;
            // 
            // tbLensModelName
            // 
            resources.ApplyResources(this.tbLensModelName, "tbLensModelName");
            this.tbLensModelName.Name = "tbLensModelName";
            this.tbLensModelName.ReadOnly = true;
            // 
            // label18
            // 
            resources.ApplyResources(this.label18, "label18");
            this.label18.Name = "label18";
            // 
            // label17
            // 
            resources.ApplyResources(this.label17, "label17");
            this.label17.Name = "label17";
            // 
            // label16
            // 
            resources.ApplyResources(this.label16, "label16");
            this.label16.Name = "label16";
            // 
            // label15
            // 
            resources.ApplyResources(this.label15, "label15");
            this.label15.Name = "label15";
            // 
            // label14
            // 
            resources.ApplyResources(this.label14, "label14");
            this.label14.Name = "label14";
            // 
            // label7
            // 
            resources.ApplyResources(this.label7, "label7");
            this.label7.Name = "label7";
            // 
            // label6
            // 
            resources.ApplyResources(this.label6, "label6");
            this.label6.Name = "label6";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.bnSetFocalPower);
            this.groupBox7.Controls.Add(this.tbInputFocalPower);
            this.groupBox7.Controls.Add(this.label13);
            resources.ApplyResources(this.groupBox7, "groupBox7");
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.TabStop = false;
            // 
            // bnSetFocalPower
            // 
            resources.ApplyResources(this.bnSetFocalPower, "bnSetFocalPower");
            this.bnSetFocalPower.Name = "bnSetFocalPower";
            this.bnSetFocalPower.UseVisualStyleBackColor = true;
            this.bnSetFocalPower.Click += new System.EventHandler(this.bnSetFocalPower_Click);
            // 
            // tbInputFocalPower
            // 
            resources.ApplyResources(this.tbInputFocalPower, "tbInputFocalPower");
            this.tbInputFocalPower.Name = "tbInputFocalPower";
            // 
            // label13
            // 
            resources.ApplyResources(this.label13, "label13");
            this.label13.Name = "label13";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.bnExecFocus);
            this.groupBox6.Controls.Add(this.tbResultFocalPower);
            this.groupBox6.Controls.Add(this.tbCoarseSteps);
            this.groupBox6.Controls.Add(this.tbEndFocalPower);
            this.groupBox6.Controls.Add(this.tbStartFocalPower);
            this.groupBox6.Controls.Add(this.label10);
            this.groupBox6.Controls.Add(this.label19);
            this.groupBox6.Controls.Add(this.label9);
            this.groupBox6.Controls.Add(this.label8);
            resources.ApplyResources(this.groupBox6, "groupBox6");
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.TabStop = false;
            // 
            // bnExecFocus
            // 
            resources.ApplyResources(this.bnExecFocus, "bnExecFocus");
            this.bnExecFocus.Name = "bnExecFocus";
            this.bnExecFocus.UseVisualStyleBackColor = true;
            this.bnExecFocus.Click += new System.EventHandler(this.bnExecFocus_Click);
            // 
            // tbResultFocalPower
            // 
            resources.ApplyResources(this.tbResultFocalPower, "tbResultFocalPower");
            this.tbResultFocalPower.Name = "tbResultFocalPower";
            this.tbResultFocalPower.ReadOnly = true;
            // 
            // tbCoarseSteps
            // 
            resources.ApplyResources(this.tbCoarseSteps, "tbCoarseSteps");
            this.tbCoarseSteps.Name = "tbCoarseSteps";
            // 
            // tbEndFocalPower
            // 
            resources.ApplyResources(this.tbEndFocalPower, "tbEndFocalPower");
            this.tbEndFocalPower.Name = "tbEndFocalPower";
            // 
            // tbStartFocalPower
            // 
            resources.ApplyResources(this.tbStartFocalPower, "tbStartFocalPower");
            this.tbStartFocalPower.Name = "tbStartFocalPower";
            // 
            // label10
            // 
            resources.ApplyResources(this.label10, "label10");
            this.label10.Name = "label10";
            // 
            // label19
            // 
            resources.ApplyResources(this.label19, "label19");
            this.label19.Name = "label19";
            // 
            // label9
            // 
            resources.ApplyResources(this.label9, "label9");
            this.label9.Name = "label9";
            // 
            // label8
            // 
            resources.ApplyResources(this.label8, "label8");
            this.label8.Name = "label8";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.bnCloseLens);
            this.groupBox1.Controls.Add(this.bnClose);
            this.groupBox1.Controls.Add(this.bnOpenLens);
            this.groupBox1.Controls.Add(this.bnOpen);
            this.groupBox1.Controls.Add(this.bnEnum);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // bnCloseLens
            // 
            resources.ApplyResources(this.bnCloseLens, "bnCloseLens");
            this.bnCloseLens.Name = "bnCloseLens";
            this.bnCloseLens.UseVisualStyleBackColor = true;
            this.bnCloseLens.Click += new System.EventHandler(this.bnCloseLens_Click);
            // 
            // bnClose
            // 
            resources.ApplyResources(this.bnClose, "bnClose");
            this.bnClose.Name = "bnClose";
            this.bnClose.UseVisualStyleBackColor = true;
            this.bnClose.Click += new System.EventHandler(this.bnClose_Click);
            // 
            // bnOpenLens
            // 
            resources.ApplyResources(this.bnOpenLens, "bnOpenLens");
            this.bnOpenLens.Name = "bnOpenLens";
            this.bnOpenLens.UseVisualStyleBackColor = true;
            this.bnOpenLens.Click += new System.EventHandler(this.bnOpenLens_Click);
            // 
            // bnOpen
            // 
            resources.ApplyResources(this.bnOpen, "bnOpen");
            this.bnOpen.Name = "bnOpen";
            this.bnOpen.UseVisualStyleBackColor = true;
            this.bnOpen.Click += new System.EventHandler(this.bnOpen_Click);
            // 
            // bnEnum
            // 
            resources.ApplyResources(this.bnEnum, "bnEnum");
            this.bnEnum.Name = "bnEnum";
            this.bnEnum.UseVisualStyleBackColor = true;
            this.bnEnum.Click += new System.EventHandler(this.bnEnum_Click);
            // 
            // Form1
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.mainGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.mainGroupBox.ResumeLayout(false);
            this.leftPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.rightPanel.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox mainGroupBox;
        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.ComboBox cbDeviceList;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button bnClose;
        private System.Windows.Forms.Button bnOpen;
        private System.Windows.Forms.Button bnEnum;
        private System.Windows.Forms.Button bnCloseLens;
        private System.Windows.Forms.Button bnOpenLens;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Button bnExecFocus;
        private System.Windows.Forms.TextBox tbCoarseSteps;
        private System.Windows.Forms.TextBox tbEndFocalPower;
        private System.Windows.Forms.TextBox tbStartFocalPower;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button bnRefreshFocalPower;
        private System.Windows.Forms.Button bnSetFocalPower;
        private System.Windows.Forms.TextBox tbInputFocalPower;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.TextBox tbResultFocalPower;
        private System.Windows.Forms.TextBox tbTriggerDelayTime;
        private System.Windows.Forms.TextBox tbCurrentFocalPower;
        private System.Windows.Forms.TextBox tbMinFocalPower;
        private System.Windows.Forms.TextBox tbMaxFocalPower;
        private System.Windows.Forms.TextBox tbStableTime;
        private System.Windows.Forms.TextBox tbLensFirmwareVersion;
        private System.Windows.Forms.TextBox tbLensModelName;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
    }
}