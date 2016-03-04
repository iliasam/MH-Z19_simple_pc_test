using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;


namespace graph1
{
	public partial class MainForm : Form
	{
		byte[] rx_data = new byte[10];
		
		int main_time_cnt = 0;
		int packet_cnt = 0;
		
		int co2_value = 0;
		string[] ser_ports;
		
		public MainForm()
		{
			InitializeComponent();
			chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
			chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
		}
		
		void Button1Click(object sender, EventArgs e)
		{
			int index = comboBox1.SelectedIndex;
			if (serialPort1.IsOpen == false)
			{
				if (index > -1)
				{
					serialPort1.PortName = ser_ports[index];
					serialPort1.Open();
					button1.Text = "CLOSE";
				}
			}
			else
			{
				serialPort1.Close();
				button1.Text = "OPEN";
			}
		}
		
		
		void Timer1Tick(object sender, EventArgs e)
		{
			main_time_cnt++;
			toolStripStatusLabel1.Text = "TIME: " + main_time_cnt.ToString() + " sec";
			if (((main_time_cnt%10) == 0) && (serialPort1.IsOpen == true))
			{
				//request data
				request_value();
				System.Threading.Thread.Sleep(100);
				if (serialPort1.BytesToRead == 9)
				{
					serialPort1.Read(rx_data,0,9);
					packet_cnt++;
					process_data(rx_data);
					chart1.Series["Series1"].Points.Add(co2_value);
				}
				else
				{
					serialPort1.ReadExisting();
				}
			}
			toolStripStatusLabel2.Text = "PACKET: " + packet_cnt.ToString();
			label1.Text = "CO2: " + co2_value.ToString();
			
			if (serialPort1.IsOpen == true)
			{
				toolStripProgressBar1.Value = main_time_cnt%10;
			}
			else
			{
				toolStripProgressBar1.Value = 0;
			}
			
		}
		
		void process_data(byte[] data)
		{
			int result = -1;
			int i;
			
			string tmp_str = "";
			
			if  (data[1] == 0x86)
			{
				result = data[2]*256 + data[3];
				
				co2_value = result;
			}
			
			for (i=2;i<8;i++)
			{
				tmp_str+= "BYTE" + i.ToString() +": " +data[i].ToString() +"\r\n";
			}
			
			label2.Text = tmp_str;
		}
		
		void request_value()
		{
			byte[] data_to_send = new byte[9];
			data_to_send[0] = 0xFF;
			data_to_send[1] = 0x01;
			data_to_send[2] = 0x86;
			data_to_send[8] = 0x79;
			serialPort1.Write(data_to_send,0,9);
		}
		
		void MainFormLoad(object sender, EventArgs e)
		{
			ser_ports = SerialPort.GetPortNames();
			foreach(string port in ser_ports)
            {
                comboBox1.Items.Add(port);
            }
		}		

	}
}
