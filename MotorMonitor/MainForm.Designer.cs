using System.Drawing;
using System.Windows.Forms;

namespace MotorMonitor
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // --- UI 패널 ---
        private System.Windows.Forms.Panel pnlConnection;
        private System.Windows.Forms.Panel pnlControl;
        private System.Windows.Forms.Panel pnlMonitor;

        // --- 타이틀 ---
        private System.Windows.Forms.Label lblTitleConnection;
        private System.Windows.Forms.Label lblTitleControl;
        private System.Windows.Forms.Label lblTitleMonitor;

        // --- Connection ---
        private System.Windows.Forms.ComboBox cboPort;
        private System.Windows.Forms.ComboBox cboBaud;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.Label lblBaud;

        // --- Motor Control (Common) ---
        private System.Windows.Forms.Label lblId;
        private System.Windows.Forms.NumericUpDown numId;
        private System.Windows.Forms.Button btnTorqueOn;
        private System.Windows.Forms.Button btnTorqueOff;

        // --- Tab Control (Modes) ---
        private System.Windows.Forms.TabControl tabModes;
        private System.Windows.Forms.TabPage tpPosition;
        private System.Windows.Forms.TabPage tpVelocity;
        private System.Windows.Forms.TabPage tpPWM;

        // --- Tab 1: Position ---
        private System.Windows.Forms.Label lblGoalPos;
        private System.Windows.Forms.NumericUpDown numGoalPos;
        private System.Windows.Forms.Label lblProfileVel;
        private System.Windows.Forms.NumericUpDown numProfileVel; // 속도 조절 흉내
        private System.Windows.Forms.Button btnMove;

        // --- Tab 2: Velocity (Fake) ---
        private System.Windows.Forms.Label lblGoalVel;
        private System.Windows.Forms.NumericUpDown numGoalVel;
        private System.Windows.Forms.Button btnRunVel;

        // --- Tab 3: PWM (Fake) ---
        private System.Windows.Forms.Label lblGoalPWM;
        private System.Windows.Forms.NumericUpDown numGoalPWM;
        private System.Windows.Forms.Button btnApplyPWM;

        // --- Monitor ---
        private System.Windows.Forms.DataVisualization.Charting.Chart chartPos;
        private System.Windows.Forms.Label lblCurrentPos;
        private System.Windows.Forms.CheckBox chkLog;
        private System.Windows.Forms.Button btnViewData;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            // --- 스타일 정의 ---
            Font titleFont = new Font("맑은 고딕", 11F, FontStyle.Bold);
            Font contentFont = new Font("맑은 고딕", 9.5F);
            Font buttonFont = new Font("맑은 고딕", 10F, FontStyle.Bold);

            Color backGroundColor = Color.FromArgb(240, 242, 245);
            Color cardColor = Color.White;
            Color primaryColor = Color.FromArgb(48, 63, 159); // Deep Indigo
            Color secondaryColor = Color.FromArgb(197, 202, 233);
            Color textColor = Color.FromArgb(60, 60, 60);
            Color disableColor = Color.FromArgb(224, 224, 224);
            Color accentColor = Color.FromArgb(255, 64, 129); // 핑크빛 포인트 (Torque OFF 등)

            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();

            // 객체 생성
            pnlConnection = new Panel();
            lblTitleConnection = new Label();
            lblPort = new Label();
            cboPort = new ComboBox();
            lblBaud = new Label();
            cboBaud = new ComboBox();
            btnConnect = new Button();
            btnDisconnect = new Button();

            pnlControl = new Panel();
            lblTitleControl = new Label();
            lblId = new Label();
            numId = new NumericUpDown();
            btnTorqueOn = new Button();
            btnTorqueOff = new Button();

            // 탭 관련 객체 생성
            tabModes = new TabControl();
            tpPosition = new TabPage();
            tpVelocity = new TabPage();
            tpPWM = new TabPage();

            // Position Tab Controls
            lblGoalPos = new Label();
            numGoalPos = new NumericUpDown();
            lblProfileVel = new Label();
            numProfileVel = new NumericUpDown();
            btnMove = new Button();

            // Velocity Tab Controls
            lblGoalVel = new Label();
            numGoalVel = new NumericUpDown();
            btnRunVel = new Button();

            // PWM Tab Controls
            lblGoalPWM = new Label();
            numGoalPWM = new NumericUpDown();
            btnApplyPWM = new Button();

            pnlMonitor = new Panel();
            lblTitleMonitor = new Label();
            chartPos = new System.Windows.Forms.DataVisualization.Charting.Chart();
            lblCurrentPos = new Label();
            chkLog = new CheckBox();
            btnViewData = new Button();

            // Suspend Layout
            pnlConnection.SuspendLayout();
            pnlControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numId).BeginInit();
            tabModes.SuspendLayout();
            tpPosition.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numGoalPos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numProfileVel).BeginInit();
            tpVelocity.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numGoalVel).BeginInit();
            tpPWM.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numGoalPWM).BeginInit();
            pnlMonitor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)chartPos).BeginInit();
            SuspendLayout();

            // 
            // MainForm Main Setting
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = backGroundColor;
            ClientSize = new Size(880, 720); // 높이 조금 늘림
            Controls.Add(pnlConnection);
            Controls.Add(pnlControl);
            Controls.Add(pnlMonitor);
            Name = "MainForm";
            Text = "Dynamixel Pro Controller";

            // ==========================================================
            // [Card 1] Connection Info
            // ==========================================================
            pnlConnection.BackColor = cardColor;
            pnlConnection.Controls.Add(lblTitleConnection);
            pnlConnection.Controls.Add(lblPort);
            pnlConnection.Controls.Add(cboPort);
            pnlConnection.Controls.Add(lblBaud);
            pnlConnection.Controls.Add(cboBaud);
            pnlConnection.Controls.Add(btnConnect);
            pnlConnection.Controls.Add(btnDisconnect);
            pnlConnection.Location = new Point(20, 20);
            pnlConnection.Name = "pnlConnection";
            pnlConnection.Size = new Size(280, 200);
            pnlConnection.TabIndex = 0;

            lblTitleConnection.AutoSize = true;
            lblTitleConnection.Font = titleFont;
            lblTitleConnection.ForeColor = primaryColor;
            lblTitleConnection.Location = new Point(15, 15);
            lblTitleConnection.Text = "Connection";

            lblPort.AutoSize = true;
            lblPort.Font = contentFont;
            lblPort.ForeColor = textColor;
            lblPort.Location = new Point(15, 50);
            lblPort.Text = "Port";
            cboPort.Font = contentFont;
            cboPort.BackColor = backGroundColor;
            cboPort.FlatStyle = FlatStyle.Flat;
            cboPort.Location = new Point(15, 72);
            cboPort.Size = new Size(110, 25);
            cboPort.TabIndex = 0;

            lblBaud.AutoSize = true;
            lblBaud.Font = contentFont;
            lblBaud.ForeColor = textColor;
            lblBaud.Location = new Point(140, 50);
            lblBaud.Text = "Baud Rate";
            cboBaud.Font = contentFont;
            cboBaud.BackColor = backGroundColor;
            cboBaud.FlatStyle = FlatStyle.Flat;
            cboBaud.Items.AddRange(new object[] { "57600", "115200", "1000000", "2000000", "3000000", "4000000" });
            cboBaud.Location = new Point(140, 72);
            cboBaud.Size = new Size(120, 25);
            cboBaud.TabIndex = 1;

            btnConnect.BackColor = primaryColor;
            btnConnect.FlatStyle = FlatStyle.Flat;
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.Font = buttonFont;
            btnConnect.ForeColor = Color.White;
            btnConnect.Location = new Point(15, 120);
            btnConnect.Size = new Size(245, 35);
            btnConnect.Text = "Connect";
            btnConnect.Cursor = Cursors.Hand;
            btnConnect.Click += btnConnect_Click;

            btnDisconnect.BackColor = disableColor;
            btnDisconnect.FlatStyle = FlatStyle.Flat;
            btnDisconnect.FlatAppearance.BorderSize = 0;
            btnDisconnect.Font = buttonFont;
            btnDisconnect.ForeColor = Color.Gray;
            btnDisconnect.Enabled = false;
            btnDisconnect.Location = new Point(15, 160);
            btnDisconnect.Size = new Size(245, 35);
            btnDisconnect.Text = "Disconnect";
            btnDisconnect.Cursor = Cursors.Hand;
            btnDisconnect.Click += btnDisconnect_Click;

            // ==========================================================
            // [Card 2] Motor Control (With Tabs)
            // ==========================================================
            pnlControl.BackColor = cardColor;
            pnlControl.Controls.Add(lblTitleControl);
            pnlControl.Controls.Add(lblId);
            pnlControl.Controls.Add(numId);
            pnlControl.Controls.Add(btnTorqueOn);
            pnlControl.Controls.Add(btnTorqueOff);
            pnlControl.Controls.Add(tabModes); // 탭 추가
            pnlControl.Location = new Point(20, 240);
            pnlControl.Name = "pnlControl";
            pnlControl.Size = new Size(280, 460); // 높이 증가
            pnlControl.TabIndex = 1;

            lblTitleControl.AutoSize = true;
            lblTitleControl.Font = titleFont;
            lblTitleControl.ForeColor = primaryColor;
            lblTitleControl.Location = new Point(15, 15);
            lblTitleControl.Text = "Motor Control";

            // ID
            lblId.AutoSize = true;
            lblId.Font = contentFont;
            lblId.ForeColor = textColor;
            lblId.Location = new Point(15, 55);
            lblId.Text = "Target ID";
            numId.Font = contentFont;
            numId.BackColor = backGroundColor;
            numId.BorderStyle = BorderStyle.None;
            numId.Location = new Point(100, 57);
            numId.Size = new Size(160, 25);
            numId.Value = new decimal(new int[] { 1, 0, 0, 0 });

            // Torque
            btnTorqueOn.BackColor = secondaryColor;
            btnTorqueOn.FlatStyle = FlatStyle.Flat;
            btnTorqueOn.FlatAppearance.BorderSize = 0;
            btnTorqueOn.Font = buttonFont;
            btnTorqueOn.ForeColor = primaryColor;
            btnTorqueOn.Location = new Point(15, 95);
            btnTorqueOn.Size = new Size(120, 35);
            btnTorqueOn.Text = "Torque ON";
            btnTorqueOn.Cursor = Cursors.Hand;
            btnTorqueOn.Click += btnTorqueOn_Click;

            btnTorqueOff.BackColor = Color.LightGray;
            btnTorqueOff.FlatStyle = FlatStyle.Flat;
            btnTorqueOff.FlatAppearance.BorderSize = 0;
            btnTorqueOff.Font = buttonFont;
            btnTorqueOff.ForeColor = Color.DimGray;
            btnTorqueOff.Location = new Point(140, 95);
            btnTorqueOff.Size = new Size(120, 35);
            btnTorqueOff.Text = "Torque OFF";
            btnTorqueOff.Cursor = Cursors.Hand;
            btnTorqueOff.Click += btnTorqueOff_Click;

            // --- Tab Control Styling ---
            tabModes.Location = new Point(10, 150);
            tabModes.Name = "tabModes";
            tabModes.SelectedIndex = 0;
            tabModes.Size = new Size(260, 300);
            tabModes.TabIndex = 2;
            tabModes.Controls.Add(tpPosition);
            tabModes.Controls.Add(tpVelocity);
            tabModes.Controls.Add(tpPWM);

            // 1. Position Tab (Working)
            tpPosition.Text = "Position";
            tpPosition.BackColor = Color.White; // 탭 배경 흰색
            tpPosition.Controls.Add(lblGoalPos);
            tpPosition.Controls.Add(numGoalPos);
            tpPosition.Controls.Add(lblProfileVel);
            tpPosition.Controls.Add(numProfileVel);
            tpPosition.Controls.Add(btnMove);

            lblGoalPos.AutoSize = true;
            lblGoalPos.Font = contentFont;
            lblGoalPos.ForeColor = textColor;
            lblGoalPos.Location = new Point(10, 20);
            lblGoalPos.Text = "Goal Position";

            numGoalPos.Font = new Font("맑은 고딕", 12F);
            numGoalPos.BackColor = backGroundColor;
            numGoalPos.BorderStyle = BorderStyle.None;
            numGoalPos.Location = new Point(10, 45);
            numGoalPos.Size = new Size(230, 30);
            numGoalPos.Maximum = new decimal(new int[] { 4095, 0, 0, 0 });
            numGoalPos.TextAlign = HorizontalAlignment.Center;

            lblProfileVel.AutoSize = true;
            lblProfileVel.Font = contentFont;
            lblProfileVel.ForeColor = textColor;
            lblProfileVel.Location = new Point(10, 90);
            lblProfileVel.Text = "Profile Velocity (Speed)";

            numProfileVel.Font = new Font("맑은 고딕", 12F);
            numProfileVel.BackColor = backGroundColor;
            numProfileVel.BorderStyle = BorderStyle.None;
            numProfileVel.Location = new Point(10, 115);
            numProfileVel.Size = new Size(230, 30);
            numProfileVel.Maximum = new decimal(new int[] { 1023, 0, 0, 0 });
            numProfileVel.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numProfileVel.TextAlign = HorizontalAlignment.Center;

            btnMove.BackColor = primaryColor;
            btnMove.FlatStyle = FlatStyle.Flat;
            btnMove.FlatAppearance.BorderSize = 0;
            btnMove.Font = new Font("맑은 고딕", 11F, FontStyle.Bold);
            btnMove.ForeColor = Color.White;
            btnMove.Location = new Point(10, 180);
            btnMove.Size = new Size(230, 50);
            btnMove.Text = "MOVE TO GOAL";
            btnMove.Cursor = Cursors.Hand;
            btnMove.Click += btnMove_Click;

            // 2. Velocity Tab (Fake)
            tpVelocity.Text = "Velocity";
            tpVelocity.BackColor = Color.White;
            tpVelocity.Controls.Add(lblGoalVel);
            tpVelocity.Controls.Add(numGoalVel);
            tpVelocity.Controls.Add(btnRunVel);

            lblGoalVel.AutoSize = true;
            lblGoalVel.Font = contentFont;
            lblGoalVel.Location = new Point(10, 20);
            lblGoalVel.Text = "Goal Velocity";

            numGoalVel.Font = new Font("맑은 고딕", 12F);
            numGoalVel.BackColor = backGroundColor;
            numGoalVel.BorderStyle = BorderStyle.None;
            numGoalVel.Location = new Point(10, 45);
            numGoalVel.Size = new Size(230, 30);
            numGoalVel.Maximum = new decimal(new int[] { 2047, 0, 0, 0 });
            numGoalVel.TextAlign = HorizontalAlignment.Center;

            btnRunVel.BackColor = Color.MediumSeaGreen; // 초록색
            btnRunVel.FlatStyle = FlatStyle.Flat;
            btnRunVel.FlatAppearance.BorderSize = 0;
            btnRunVel.Font = buttonFont;
            btnRunVel.ForeColor = Color.White;
            btnRunVel.Location = new Point(10, 180);
            btnRunVel.Size = new Size(230, 50);
            btnRunVel.Text = "RUN VELOCITY";

            // 3. PWM Tab (Fake)
            tpPWM.Text = "PWM";
            tpPWM.BackColor = Color.White;
            tpPWM.Controls.Add(lblGoalPWM);
            tpPWM.Controls.Add(numGoalPWM);
            tpPWM.Controls.Add(btnApplyPWM);

            lblGoalPWM.AutoSize = true;
            lblGoalPWM.Font = contentFont;
            lblGoalPWM.Location = new Point(10, 20);
            lblGoalPWM.Text = "Goal PWM (Torque)";

            numGoalPWM.Font = new Font("맑은 고딕", 12F);
            numGoalPWM.BackColor = backGroundColor;
            numGoalPWM.BorderStyle = BorderStyle.None;
            numGoalPWM.Location = new Point(10, 45);
            numGoalPWM.Size = new Size(230, 30);
            numGoalPWM.Maximum = new decimal(new int[] { 885, 0, 0, 0 });
            numGoalPWM.TextAlign = HorizontalAlignment.Center;

            btnApplyPWM.BackColor = accentColor; // 핑크색
            btnApplyPWM.FlatStyle = FlatStyle.Flat;
            btnApplyPWM.FlatAppearance.BorderSize = 0;
            btnApplyPWM.Font = buttonFont;
            btnApplyPWM.ForeColor = Color.White;
            btnApplyPWM.Location = new Point(10, 180);
            btnApplyPWM.Size = new Size(230, 50);
            btnApplyPWM.Text = "APPLY PWM";


            // ==========================================================
            // [Card 3] Real-time Monitor
            // ==========================================================
            pnlMonitor.BackColor = cardColor;
            pnlMonitor.Controls.Add(lblTitleMonitor);
            pnlMonitor.Controls.Add(chartPos);
            pnlMonitor.Controls.Add(lblCurrentPos);
            pnlMonitor.Controls.Add(chkLog);
            pnlMonitor.Controls.Add(btnViewData);
            pnlMonitor.Location = new Point(320, 20);
            pnlMonitor.Name = "pnlMonitor";
            pnlMonitor.Size = new Size(540, 680); // 높이 증가
            pnlMonitor.TabIndex = 2;

            lblTitleMonitor.AutoSize = true;
            lblTitleMonitor.Font = titleFont;
            lblTitleMonitor.ForeColor = primaryColor;
            lblTitleMonitor.Location = new Point(20, 15);
            lblTitleMonitor.Text = "Real-time Monitor";

            chartArea1.Name = "ChartArea1";
            chartArea1.BackColor = Color.White;
            chartArea1.AxisX.MajorGrid.LineColor = Color.FromArgb(240, 240, 240);
            chartArea1.AxisY.MajorGrid.LineColor = Color.FromArgb(240, 240, 240);
            chartArea1.BorderColor = Color.Transparent;
            chartPos.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            legend1.Enabled = false;
            chartPos.Legends.Add(legend1);
            chartPos.Location = new Point(20, 50);
            chartPos.Name = "chartPos";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Color = primaryColor;
            series1.BorderWidth = 2;
            series1.Name = "Position";
            chartPos.Series.Add(series1);
            chartPos.Size = new Size(500, 520);
            chartPos.TabIndex = 0;
            chartPos.BackColor = Color.White;
            chartPos.BorderlineColor = Color.Transparent;

            lblCurrentPos.AutoSize = true;
            lblCurrentPos.Font = new Font("맑은 고딕", 16F, FontStyle.Bold);
            lblCurrentPos.ForeColor = primaryColor;
            lblCurrentPos.Location = new Point(20, 590);
            lblCurrentPos.Text = "Current Pos: 0";

            chkLog.Font = contentFont;
            chkLog.ForeColor = textColor;
            chkLog.Location = new Point(25, 635);
            chkLog.Text = "Save to DB";
            chkLog.Size = new Size(120, 25);
            chkLog.Cursor = Cursors.Hand;

            btnViewData.BackColor = secondaryColor;
            btnViewData.FlatStyle = FlatStyle.Flat;
            btnViewData.FlatAppearance.BorderSize = 0;
            btnViewData.Font = buttonFont;
            btnViewData.ForeColor = primaryColor;
            btnViewData.Location = new Point(400, 630);
            btnViewData.Size = new Size(120, 35);
            btnViewData.Text = "View Log";
            btnViewData.Cursor = Cursors.Hand;
            btnViewData.Click += btnViewData_Click;


            // Layout Resume
            pnlConnection.ResumeLayout(false);
            pnlConnection.PerformLayout();
            pnlControl.ResumeLayout(false);
            pnlControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numId).EndInit();

            tabModes.ResumeLayout(false);

            tpPosition.ResumeLayout(false);
            tpPosition.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numGoalPos).EndInit();
            ((System.ComponentModel.ISupportInitialize)numProfileVel).EndInit();

            tpVelocity.ResumeLayout(false);
            tpVelocity.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numGoalVel).EndInit();

            tpPWM.ResumeLayout(false);
            tpPWM.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numGoalPWM).EndInit();

            pnlMonitor.ResumeLayout(false);
            pnlMonitor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)chartPos).EndInit();
            ResumeLayout(false);
        }
    }
}