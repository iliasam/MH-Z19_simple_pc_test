using System;
using System.Windows.Forms;
using System.IO.Ports;


namespace graph1
{
	public partial class MainForm : Form
	{
		byte[] RxData = new byte[10];
		
		int MainTimeCnt = 0;
		int RxPacketCnt = 0;
		
		int CO2_Value = 0;
		string[] SerPorts;
		
		public MainForm()
		{
			InitializeComponent();
			chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
			chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
		}
		
		void btnOpenCloseClick(object sender, EventArgs e)
		{
			int index = comboBox1.SelectedIndex;
			if (serialPort1.IsOpen == false)
			{
				if (index > -1)
				{
					serialPort1.PortName = SerPorts[index];
					serialPort1.Open();
					btnOpenClose.Text = "CLOSE";
				}
			}
			else
			{
				serialPort1.Close();
				btnOpenClose.Text = "OPEN";
			}
		}
		
		
		void Timer1Tick(object sender, EventArgs e)
		{
			MainTimeCnt++;
			toolStripStatusLabel1.Text = "TIME: " + MainTimeCnt.ToString() + " sec";
			if (((MainTimeCnt%10) == 0) && (serialPort1.IsOpen == true))
			{
				//request data
				RequestState();
				System.Threading.Thread.Sleep(100);
				if (serialPort1.BytesToRead == 9)
				{
					serialPort1.Read(RxData,0,9);
					RxPacketCnt++;
					ProcessRxData(RxData);
					chart1.Series["Series1"].Points.Add(CO2_Value);
				}
				else
				{
					serialPort1.ReadExisting();
				}
			}
			toolStripStatusLabel2.Text = "PACKET: " + RxPacketCnt.ToString();
			label1.Text = "CO2: " + CO2_Value.ToString();
			
			if (serialPort1.IsOpen == true)
			{
				toolStripProgressBar1.Value = MainTimeCnt % 10;
			}
			else
			{
				toolStripProgressBar1.Value = 0;
			}
			
		}
		
		void ProcessRxData(byte[] data)
		{
			int result = -1;
			int i;
			
			string tmp_str = "";
			
			if  (data[1] == 0x86)
			{
				result = data[2]*256 + data[3];
				CO2_Value = result;
			}
			
			for (i = 2; i < 8; i++)
			{
				tmp_str+= "BYTE" + i.ToString() +": " +data[i].ToString() +"\r\n";
			}
			
			label2.Text = tmp_str;
		}
		
		void RequestState()
		{
			byte[] dataToSend = new byte[9];
			dataToSend[0] = 0xFF;
			dataToSend[1] = 0x01;
			dataToSend[2] = 0x86;
			dataToSend[8] = 0x79;
			serialPort1.Write(dataToSend,0,9);
		}
		
		void MainFormLoad(object sender, EventArgs e)
		{
			SerPorts = SerialPort.GetPortNames();
			foreach(string port in SerPorts)
            {
                comboBox1.Items.Add(port);
            }
		}		

	}
}
