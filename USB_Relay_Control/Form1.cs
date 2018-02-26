using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace USB_Relay_Control
{

    public partial class Form1 : Form
    {
        int _deviceHandle = 0;
        List<Button> _buttonList;

        public Form1()
        {
            InitializeComponent();
            
            refreshList();

            _buttonList = new List<Button>{
                button1, button2, button3, button4,
                button5, button6, button7, button8
            };

            if (listBox1.Items.Count > 0)
                listBox1.SelectedIndex = 0;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_deviceHandle > 0)
            {
                RelayDeviceWrapper.usb_relay_device_close(_deviceHandle);
                _deviceHandle = 0;
            }

            for (int i = 0; i < _buttonList.Count; i++){
                _buttonList[i].BackColor = BackColor;
                _buttonList[i].Enabled = false;
            }

            var lb = (ListBox)sender;
            usb_relay_device_info device = (usb_relay_device_info)lb.SelectedItem;
            _deviceHandle = RelayDeviceWrapper.usb_relay_device_open(ref device);
            int numberOfRelays = (int)device.type;
            
            uint status = 0;
            RelayDeviceWrapper.usb_relay_device_get_status(_deviceHandle, ref status);

            for (int i = 0; i < numberOfRelays; i++)
            {
                _buttonList[i].Enabled = true;
                if (status > numberOfRelays)
                    _buttonList[i].BackColor = Color.Red;
                else if (i + 1 == status)
                    _buttonList[i].BackColor = Color.Red;
            }
        }

        private void refreshList()
        {
            listBox1.Items.Clear();
            if (RelayDeviceWrapper.usb_relay_init() != 0)
            {
                Console.WriteLine("Couldn't initialize!");
                MessageBox.Show("Couldn't initialize!");
                return;
            }
            else { Console.WriteLine("Initialized successfully!"); }

            List<usb_relay_device_info> devicesInfos = new List<usb_relay_device_info>();
            usb_relay_device_info deviceInfo = RelayDeviceWrapper.usb_relay_device_enumerate();
            devicesInfos.Add(deviceInfo);

            while (deviceInfo.next.ToInt32() > 0)
            {
                deviceInfo = (usb_relay_device_info)Marshal.PtrToStructure(deviceInfo.next, typeof(usb_relay_device_info));
                devicesInfos.Add(deviceInfo);
            }

            foreach (var device in devicesInfos)
            {
                listBox1.Items.Add(device);
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            if (_deviceHandle <= 0)
                return;

            Button btn = (Button)sender;
            string strNum = btn.Text.Substring(btn.Text.Length - 1, 1);
            int num = int.Parse(strNum);

            if (btn.BackColor == BackColor || btn.BackColor == Color.Transparent)
            {
                int openResult = RelayDeviceWrapper.usb_relay_device_open_one_relay_channel(_deviceHandle, num);
            }
            else
            {
                int closeResult = RelayDeviceWrapper.usb_relay_device_close_one_relay_channel(_deviceHandle, num);
            }
            btn.BackColor = (btn.BackColor == Color.Red) ? BackColor : Color.Red;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_deviceHandle > 0)
                RelayDeviceWrapper.usb_relay_device_close(_deviceHandle);
        }

    }
}
