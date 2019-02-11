using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Management;
using System.Threading;


namespace deviceDetectWF
{
    public partial class MainForm : Form
    {

        private bool running = false;
        public BindingList<DevicesList> Snapshots { get; set; } = new BindingList<DevicesList>();

        public MainForm()
        {
            InitializeComponent();
            toolStripStatusLabel1.Text = DateTime.Now.ToString() + " Ready";
            InitBinding();
        }

        private void InitBinding()
        {
            SnapshotsList.DataSource = Snapshots;
            SnapshotsList.DisplayMember = "Name";
            SnapshotsList.ValueMember = "Id";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (running && (m.WParam.ToInt32() == 0x7))
                GetDevicesList("Device change");
        }

        private void GetDevicesList(string eventType)
        {
            toolStripStatusLabel1.Text = DateTime.Now.ToString() + " " + eventType;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * from Win32_PnpEntity");
            Thread thread = new Thread(new ThreadStart(() =>
            {
                DevicesList currentList = new DevicesList(eventType);

                foreach (ManagementBaseObject devices in searcher.Get())
                {
                    string deviceName;
                    string deviceID;

                    try {
                        deviceName = devices.GetPropertyValue("Name").ToString();
                    } catch {
                        deviceName = "Unknown device";
                    }

                    try {
                        deviceID = devices.GetPropertyValue("DeviceID").ToString();
                    } catch {
                        deviceID = "Unknown ID";
                    }

                    currentList.Devices.Add(new DeviceEntry { DeviceId = deviceID, DeviceName = deviceName });
                }

                Snapshots.Add(currentList);
                Invoke(new ResetBindingCallback(ResetBinding));

            }));
            thread.Start();
        }

        private delegate void ResetBindingCallback();

        private void ResetBinding()
        {
            if (running)
                Snapshots.ResetBindings();
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.Enabled = true;
            GetDevicesList("Start logging");
            this.running = true;
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            running = false;
            toolStripStatusLabel1.Text = DateTime.Now.ToString() + " Stop logging";
            stopToolStripMenuItem.Enabled = false;
            startToolStripMenuItem.Enabled = true;
        }

        private void clearLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextBox.Clear();
            toolStripStatusLabel1.Text = DateTime.Now.ToString() + " Log cleared";
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About aboutForm = new About();
            aboutForm.ShowDialog();
        }

        private void SelectedSnapshotChanged(object sender, EventArgs e)
        {
            if (SnapshotsList.SelectedItem != null)
            {
                TextBox.Clear();
                TextBox.AppendText(Snapshots[SnapshotsList.SelectedIndex].Name + "\n");

                if (SnapshotsList.SelectedIndex > 0)
                {
                    List<DeviceEntry> removedDevices = Snapshots[SnapshotsList.SelectedIndex-1].Devices.Except(Snapshots[SnapshotsList.SelectedIndex].Devices).ToList();
                    List<DeviceEntry> addedDevices = Snapshots[SnapshotsList.SelectedIndex].Devices.Except(Snapshots[SnapshotsList.SelectedIndex-1].Devices).ToList();

                    if (removedDevices.Count > 0)
                        PopulateDevices(removedDevices, Color.Red, "--- REMOVED DEVICES ---");

                    if (addedDevices.Count > 0)
                        PopulateDevices(addedDevices, Color.Green, "--- ADDED DEVICES ---");
                }

                PopulateDevices(Snapshots[SnapshotsList.SelectedIndex].Devices, TextBox.ForeColor, "--- COMPLETE LIST OF DEVICES DETECTED ---");

                saveToFileToolStripMenuItem.Enabled = true;
                copyToClipboardToolStripMenuItem.Enabled = true;
            }
            else
            {
                saveToFileToolStripMenuItem.Enabled = false;
                copyToClipboardToolStripMenuItem.Enabled = false;
            }
        }

        private void PopulateDevices(List<DeviceEntry> list, Color color, string header)
        {
            TextBox.SelectionColor = color;
            TextBox.SelectionFont = new Font(TextBox.Font, FontStyle.Bold);
            TextBox.AppendText(header + "\n");
            TextBox.SelectionFont = new Font(TextBox.Font, FontStyle.Regular);
            foreach (DeviceEntry deviceLine in list)
            {
                TextBox.SelectionColor = color;
                TextBox.AppendText(deviceLine.ToString() + "\n");
            }
            TextBox.AppendText("\n");
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                TextBox.SaveFile(saveFileDialog1.OpenFile(), RichTextBoxStreamType.PlainText);
                toolStripStatusLabel1.Text = DateTime.Now.ToString() + " Log saved to file: " + saveFileDialog1.FileName;
            }
        }

        private void copyToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextBox.SelectAll();
            TextBox.Copy();
            TextBox.DeselectAll();
            toolStripStatusLabel1.Text = DateTime.Now.ToString() + " Log copied to clipboard";
        }

        private void ShowHowTo(object sender, EventArgs e)
        {
            TextBox.Clear();
            SnapshotsList.SelectedIndex = 0;
            saveToFileToolStripMenuItem.Enabled = false;
            copyToClipboardToolStripMenuItem.Enabled = false;

            TextBox.AppendText("\nHOWTO:\n1.Use Listener menu to start logging device changes\n2.When listener is enabled, initial scan of devices will occur\n");
            TextBox.AppendText("3.Any detected change will add new snapshot of device tree to side panel\n4.Select any other snapshot to see complete list of devices and compare with previous snap\n\nEnjoy!");
        }
    }
}
