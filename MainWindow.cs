using Microsoft.Phone.Tools;
using Microsoft.SmartDevice.Connectivity.Interface;
using Microsoft.SmartDevice.MultiTargeting.Connectivity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WPScreenShoter.Controller;
using WPScreenShoter.Model;

namespace WPScreenShoter
{
    public partial class MainWindow : Form
    {

        private EmulatorController _controller;

        public MainWindow()
        {
            _controller = EmulatorController.GetEmulatorController();
            _controller.EmulatorConnected += _controller_EmulatorConnected;
            _controller.XapInstalled += _controller_XapInstalled;
            _controller.SnapshotTaken += _controller_SnapshotTaken;
            InitializeComponent();
            cbEmulators.DataSource = _controller.Emulators;
            lbXaps.DataSource = Directory.GetFiles(".", "*.xap");
        }

        void _controller_SnapshotTaken(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                pictureBox1.Image = (Bitmap)sender;
                if (lbXaps.SelectedIndex < lbXaps.Items.Count - 1) lbXaps.SelectedIndex += 1;
                else
                {
                    this.Activate();
                    MessageBox.Show(this, "All screenshots have been taken!", "Work done", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            });
        }

        void _controller_XapInstalled(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate() { lbStatus.Text = "Application installed successfully! :)"; });
        }

        void _controller_EmulatorConnected(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate() { lbStatus.Text = "Emulator started successfully! :)"; });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            lbStatus.Text = "Starting emulator...";
            _controller.StartEmulator(((Emulator)cbEmulators.SelectedItem).Device.Id);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            lbStatus.Text = "Installing XAP...";
            _controller.InstallXap(@"C:\Users\Pablo\Documents\visual studio 2013\Projects\PhoneApp2\PhoneApp2\Bin\Debug\PhoneApp2_Debug_AnyCPU.xap");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _controller.TakeSnapshot(@"C:\Users\Pablo\test.png");
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            new Thread(new ParameterizedThreadStart(ProcessXaps)).Start(lbXaps.Items);
        }

        private void ProcessXaps(object xaps)
        {
            var list = (ListBox.ObjectCollection)xaps;
            for (int i = 0; i < list.Count; i++)
                _controller.InstallAndSnap((string)list[i]);
        }

        private void cbEmulators_SelectedValueChanged(object sender, EventArgs e)
        {
            _controller.PreferredDevice = ((Emulator)cbEmulators.SelectedValue).Device.Id;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void aboutWindowsPhoneScreenshoterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().Show();
        }

    }
}
