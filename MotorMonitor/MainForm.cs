using System;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MotorMonitor
{
    public partial class MainForm : Form
    {
        private SerialPort? serialPort; 
        private System.Windows.Forms.Timer monitorTimer;
        private DateTime lastSaveTime = DateTime.MinValue;

        // --- 다이나믹셀 프로토콜 2.0 주소값 ---
        private const ushort ADDR_TORQUE_ENABLE = 64;
        private const ushort ADDR_PROFILE_VELOCITY = 112;
        private const ushort ADDR_GOAL_POSITION = 116;
        private const ushort ADDR_PRESENT_POSITION = 132;

        // --- SQLite 설정 ---
        private string dbPath = "MotorLog.sqlite";
        private SQLiteConnection? dbConn;

        public MainForm()
        {
            InitializeComponent();
            InitSerial();
            InitDatabase();

            monitorTimer = new System.Windows.Forms.Timer();
            monitorTimer.Interval = 50;
            monitorTimer.Tick += MonitorTimer_Tick;
        }

        private void InitSerial()
        {
            string[] ports = SerialPort.GetPortNames();
            cboPort.Items.AddRange(ports);
            if (ports.Length > 0) cboPort.SelectedIndex = 0;
        }

        private void InitDatabase()
        {
            try
            {
                string dbFile = Path.Combine(Application.StartupPath, dbPath);

                if (!File.Exists(dbFile))
                {
                    SQLiteConnection.CreateFile(dbFile);
                }

                dbConn = new SQLiteConnection($"Data Source={dbFile};Version=3;");
                dbConn.Open();

                string sql = "CREATE TABLE IF NOT EXISTS MotorData (Timestamp TEXT, Position INTEGER)";
                using SQLiteCommand command = new SQLiteCommand(sql, dbConn);
                command.ExecuteNonQuery();
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"DB 연결 실패!\n내용: {ex.Message}\n\n일단 DB 없이 실행합니다.");

                if (chkLog != null) chkLog.Enabled = false;
                dbConn = null;
            }
        }


        // --- UI Events ---

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboPort.SelectedItem == null || cboBaud.SelectedItem == null) return;

                string portName = cboPort.SelectedItem.ToString()!;
                int baudRate = int.Parse(cboBaud.SelectedItem.ToString()!);

                if (serialPort != null && serialPort.IsOpen) serialPort.Close();

                serialPort = new SerialPort(portName, baudRate);
                serialPort.Open();
                serialPort.RtsEnable = true;
                serialPort.DtrEnable = true;

                byte targetId = (byte)numId.Value;

                if (DoPing(targetId))
                {
                    btnConnect.Enabled = false;
                    btnDisconnect.Enabled = true;
                    monitorTimer.Start();
                    MessageBox.Show($"성공! ID {targetId} 연결됨!");
                }
                else
                {
                    serialPort.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("에러 발생: " + ex.Message);
                if (serialPort != null && serialPort.IsOpen) serialPort.Close();
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            monitorTimer.Stop();
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }
            btnConnect.Enabled = true;
            btnDisconnect.Enabled = false;
        }

        private void btnTorqueOn_Click(object sender, EventArgs e)
        {
            if (serialPort == null || !serialPort.IsOpen) return;

            // 토크 ON (주소 64, 1바이트, 데이터 1)
            SendWritePacket((byte)numId.Value, ADDR_TORQUE_ENABLE, 1, 1);
        }

        private void btnTorqueOff_Click(object sender, EventArgs e)
        {
            SendWritePacket((byte)numId.Value, ADDR_TORQUE_ENABLE, 0, 1);
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            if (serialPort == null || !serialPort.IsOpen) return;

            byte id = (byte)numId.Value;

            // 속도 프로파일 설정 후 목표 위치 전송 (각 4바이트)
            SendWritePacket(id, ADDR_PROFILE_VELOCITY, (int)numProfileVel.Value, 4);
            System.Threading.Thread.Sleep(10); // 첫 번째 Status Packet 수신 대기
            SendWritePacket(id, ADDR_GOAL_POSITION, (int)numGoalPos.Value, 4);
        }

        private void MonitorTimer_Tick(object? sender, EventArgs e)
        {
            if (serialPort == null || !serialPort.IsOpen)
            {
                monitorTimer.Stop(); 
                return;
            }
            try
            {
                serialPort.DiscardInBuffer();
                SendReadPacket((byte)numId.Value, ADDR_PRESENT_POSITION, 4);

                System.Threading.Thread.Sleep(20); 

                if (serialPort.BytesToRead >= 15)
                {
                    byte[] buffer = new byte[serialPort.BytesToRead];
                    serialPort.Read(buffer, 0, buffer.Length);
                    int presentPos = ParsePosition(buffer);
                    if (presentPos != -1) UpdateUI(presentPos);
                }
            }
            catch
            {
            }
        }

        private void UpdateUI(int pos)
        {
            lblCurrentPos.Text = $"Current Pos: {pos}";

            chartPos.Series["Position"].Points.AddY(pos);
            if (chartPos.Series["Position"].Points.Count > 100)
            {
                chartPos.Series["Position"].Points.RemoveAt(0);
            }
            chartPos.ChartAreas[0].RecalculateAxesScale();

            if (chkLog.Checked && dbConn != null)
            {
                if ((DateTime.Now - lastSaveTime).TotalSeconds >= 1.0)
                {
                    try
                    {
                        string sql = "INSERT INTO MotorData (Timestamp, Position) VALUES (@time, @pos)";
                        using SQLiteCommand cmd = new SQLiteCommand(sql, dbConn);
                        cmd.Parameters.AddWithValue("@time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                        cmd.Parameters.AddWithValue("@pos", pos);
                        cmd.ExecuteNonQuery();

                        lastSaveTime = DateTime.Now;
                    }
                    catch
                    {
                    }
                }
            }
        }

        // Dynamixel Protocol 2.0 
        private bool DoPing(byte id)
        {
            if (serialPort == null || !serialPort.IsOpen) return false;

            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();


            // Header, ID, Length(3), Instruction(PING) + CRC
            List<byte> packet = new List<byte> { 0xFF, 0xFF, 0xFD, 0x00, id, 0x03, 0x00, 0x01 };

            ushort crc = UpdateCRC(0, packet.ToArray(), packet.Count);
            packet.Add((byte)(crc & 0xFF));
            packet.Add((byte)((crc >> 8) & 0xFF));

            byte[] pingPacket = packet.ToArray();
            serialPort.Write(pingPacket, 0, pingPacket.Length);

            long timeoutTick = DateTime.Now.Ticks + (10000 * 300);

            while (DateTime.Now.Ticks < timeoutTick)
            {
                if (serialPort.BytesToRead > 0)
                {
                    System.Threading.Thread.Sleep(20);

                    int count = serialPort.BytesToRead;
                    if (count >= 10) 
                    {
                        return true;
                    }
                }
                Application.DoEvents(); 
            }

            return false;
        }
        private void SendWritePacket(byte id, ushort address, int value, int dataLen)
        {
            if (serialPort == null || !serialPort.IsOpen) return;

            List<byte> packet = new List<byte>();

            packet.AddRange(new byte[] { 0xFF, 0xFF, 0xFD, 0x00 });
            packet.Add(id);

            ushort length = (ushort)(1 + 2 + dataLen + 2);
            packet.Add((byte)(length & 0xFF));
            packet.Add((byte)((length >> 8) & 0xFF));

            packet.Add(0x03); // WRITE

            packet.Add((byte)(address & 0xFF));
            packet.Add((byte)((address >> 8) & 0xFF));

            for (int i = 0; i < dataLen; i++)
            {
                packet.Add((byte)((value >> (8 * i)) & 0xFF));
            }

            ushort crc = UpdateCRC(0, packet.ToArray(), packet.Count);
            packet.Add((byte)(crc & 0xFF));
            packet.Add((byte)((crc >> 8) & 0xFF));

            byte[] finalPacket = packet.ToArray();
            serialPort.Write(finalPacket, 0, finalPacket.Length);
        }

        private void SendReadPacket(byte id, ushort address, ushort dataLen)
        {
            if (serialPort == null || !serialPort.IsOpen) return;

            List<byte> packet = new List<byte>();

            packet.AddRange(new byte[] { 0xFF, 0xFF, 0xFD, 0x00 });
            packet.Add(id);

            ushort length = 7;
            packet.Add((byte)(length & 0xFF));
            packet.Add((byte)((length >> 8) & 0xFF));

            packet.Add(0x02); // READ

            packet.Add((byte)(address & 0xFF));
            packet.Add((byte)((address >> 8) & 0xFF));

            packet.Add((byte)(dataLen & 0xFF));
            packet.Add((byte)((dataLen >> 8) & 0xFF));

            ushort crc = UpdateCRC(0, packet.ToArray(), packet.Count);
            packet.Add((byte)(crc & 0xFF));
            packet.Add((byte)((crc >> 8) & 0xFF));

            byte[] finalPacket = packet.ToArray();
            serialPort.Write(finalPacket, 0, finalPacket.Length);
        }

        private int ParsePosition(byte[] buffer)
        {
            for (int i = 0; i < buffer.Length - 10; i++)
            {
                if (buffer[i] == 0xFF && buffer[i + 1] == 0xFF && buffer[i + 2] == 0xFD && buffer[i + 3] == 0x00)
                {
                    if (buffer[i + 7] == 0x55)
                    {
                        if (buffer[i + 8] != 0) return -1;
                        int pos = BitConverter.ToInt32(buffer, i + 9);
                        return pos;
                    }
                }
            }
            return -1;
        }

        private void btnViewData_Click(object sender, EventArgs e)
        {
            if (dbConn == null || dbConn.State != System.Data.ConnectionState.Open)
            {
                MessageBox.Show("DB가 연결되어 있지 않습니다.");
                return;
            }

            try
            {
                string sql = "SELECT Timestamp, Position FROM MotorData ORDER BY Timestamp DESC LIMIT 10";
                using SQLiteCommand cmd = new SQLiteCommand(sql, dbConn);
                using SQLiteDataReader rdr = cmd.ExecuteReader();

                string message = "--- 최신 데이터 10개 ---\n\n";

                while (rdr.Read())
                {
                    string? time = rdr["Timestamp"].ToString();
                    string? pos = rdr["Position"].ToString();
                    message += $"[{time}]  위치: {pos}\n";
                }
                rdr.Close();

                // 메시지 창 띄우기
                MessageBox.Show(message, "DB 데이터 확인");
            }
            catch (Exception ex)
            {
                MessageBox.Show("데이터 읽기 실패: " + ex.Message);
            }
        }

        private static readonly ushort[] crc_table = {
            0x0000, 0x8005, 0x800F, 0x000A, 0x801B, 0x001E, 0x0014, 0x8011,
            0x8033, 0x0036, 0x003C, 0x8039, 0x0028, 0x802D, 0x8027, 0x0022,
            0x8063, 0x0066, 0x006C, 0x8069, 0x0078, 0x807D, 0x8077, 0x0072,
            0x0050, 0x8055, 0x805F, 0x005A, 0x804B, 0x004E, 0x0044, 0x8041,
            0x80C3, 0x00C6, 0x00CC, 0x80C9, 0x00D8, 0x80DD, 0x80D7, 0x00D2,
            0x00F0, 0x80F5, 0x80FF, 0x00FA, 0x80EB, 0x00EE, 0x00E4, 0x80E1,
            0x00A0, 0x80A5, 0x80AF, 0x00AA, 0x80BB, 0x00BE, 0x00B4, 0x80B1,
            0x8093, 0x0096, 0x009C, 0x8099, 0x0088, 0x808D, 0x8087, 0x0082,
            0x8183, 0x0186, 0x018C, 0x8189, 0x0198, 0x819D, 0x8197, 0x0192,
            0x01B0, 0x81B5, 0x81BF, 0x01BA, 0x81AB, 0x01AE, 0x01A4, 0x81A1,
            0x01E0, 0x81E5, 0x81EF, 0x01EA, 0x81FB, 0x01FE, 0x01F4, 0x81F1,
            0x81D3, 0x01D6, 0x01DC, 0x81D9, 0x01C8, 0x81CD, 0x81C7, 0x01C2,
            0x0140, 0x8145, 0x814F, 0x014A, 0x815B, 0x015E, 0x0154, 0x8151,
            0x8173, 0x0176, 0x017C, 0x8179, 0x0168, 0x816D, 0x8167, 0x0162,
            0x8123, 0x0126, 0x012C, 0x8129, 0x0138, 0x813D, 0x8137, 0x0132,
            0x0110, 0x8115, 0x811F, 0x011A, 0x810B, 0x010E, 0x0104, 0x8101,
            0x8303, 0x0306, 0x030C, 0x8309, 0x0318, 0x831D, 0x8317, 0x0312,
            0x0330, 0x8335, 0x833F, 0x033A, 0x832B, 0x032E, 0x0324, 0x8321,
            0x0360, 0x8365, 0x836F, 0x036A, 0x837B, 0x037E, 0x0374, 0x8371,
            0x8353, 0x0356, 0x035C, 0x8359, 0x0348, 0x834D, 0x8347, 0x0342,
            0x03C0, 0x83C5, 0x83CF, 0x03CA, 0x83DB, 0x03DE, 0x03D4, 0x83D1,
            0x83F3, 0x03F6, 0x03FC, 0x83F9, 0x03E8, 0x83ED, 0x83E7, 0x03E2,
            0x83A3, 0x03A6, 0x03AC, 0x83A9, 0x03B8, 0x83BD, 0x83B7, 0x03B2,
            0x0390, 0x8395, 0x839F, 0x039A, 0x838B, 0x038E, 0x0384, 0x8381,
            0x0280, 0x8285, 0x828F, 0x028A, 0x829B, 0x029E, 0x0294, 0x8291,
            0x82B3, 0x02B6, 0x02BC, 0x82B9, 0x02A8, 0x82AD, 0x82A7, 0x02A2,
            0x82E3, 0x02E6, 0x02EC, 0x82E9, 0x02F8, 0x82FD, 0x82F7, 0x02F2,
            0x02D0, 0x82D5, 0x82DF, 0x02DA, 0x82CB, 0x02CE, 0x02C4, 0x82C1,
            0x8243, 0x0246, 0x024C, 0x8249, 0x0258, 0x825D, 0x8257, 0x0252,
            0x0270, 0x8275, 0x827F, 0x027A, 0x826B, 0x026E, 0x0264, 0x8261,
            0x0220, 0x8225, 0x822F, 0x022A, 0x823B, 0x023E, 0x0234, 0x8231,
            0x8213, 0x0216, 0x021C, 0x8219, 0x0208, 0x820D, 0x8207, 0x0202
        };

        private ushort UpdateCRC(ushort crc_accum, byte[] data_blk_ptr, int data_blk_size)
        {
            for (int j = 0; j < data_blk_size; j++)
            {
                int i = ((int)(crc_accum >> 8) ^ data_blk_ptr[j]) & 0xFF;
                crc_accum = (ushort)((crc_accum << 8) ^ crc_table[i]);
            }
            return crc_accum;
        }
    }
}