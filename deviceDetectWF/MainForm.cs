using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Management;
using System.Threading;
using System.Diagnostics;

namespace deviceDetectWF
{
    public partial class MainForm : Form
    {

        private bool running = false;
        public BindingList<DevicesList> Snapshots { get; set; } = new BindingList<DevicesList>();

        public MainForm()
        {
            InitializeComponent();
            SnapshotsList.DataSource = Snapshots;
            SnapshotsList.DisplayMember = "Name";
            SnapshotsList.ValueMember = "Id";
            SetStatus("Ready");
            ShowHowTo(this, null);
        }

        private void SetStatus(string status)
        {
            StatusLabel.Text = DateTime.Now.ToString() + " " + status;
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
             if (running && (m.WParam.ToInt32() == 0x7))     // WParam 0x7 = some hw has changed message
                GetDevicesList("Device change");
        }

        private void GetDevicesList(string eventType)
        {
            SetStatus(eventType + " [running]");
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
            SetStatus(eventType);
        }

        private delegate void ResetBindingCallback();

        private void ResetBinding()
        {
            if (running)
                Snapshots.ResetBindings();
            SetStatus("Snapshot captured");
        }

        private void StartListener(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = false;
            startToolStripMenuItem.ShowShortcutKeys = false;
            stopToolStripMenuItem.Enabled = true;
            stopToolStripMenuItem.ShowShortcutKeys = true;
            GetDevicesList("Start logging");
            this.running = true;
        }

        private void StopListener(object sender, EventArgs e)
        {
            running = false;
            SetStatus("Logging stopped");
            stopToolStripMenuItem.Enabled = false;
            stopToolStripMenuItem.ShowShortcutKeys = false;
            startToolStripMenuItem.Enabled = true;
            startToolStripMenuItem.ShowShortcutKeys = true;
        }

        private void ShowAboutDialog(object sender, EventArgs e)
        {
            About aboutForm = new About();
            aboutForm.ShowDialog();
        }

        private void SelectedSnapshotChanged(object sender, EventArgs e)
        {
            if (SnapshotsList.SelectedItem != null)
            {
                TextBox.Clear();
                TextBox.AppendText(Snapshots[SnapshotsList.SelectedIndex].Name + "\n\n");

                if (SnapshotsList.SelectedIndex > 0)
                {
                    List<DeviceEntry> removedDevices = Snapshots[SnapshotsList.SelectedIndex-1].Devices.Except(Snapshots[SnapshotsList.SelectedIndex].Devices).ToList();
                    List<DeviceEntry> addedDevices = Snapshots[SnapshotsList.SelectedIndex].Devices.Except(Snapshots[SnapshotsList.SelectedIndex-1].Devices).ToList();

                    if (removedDevices.Count > 0)
                        PopulateDevices(removedDevices, Color.Red, "REMOVED DEVICES");

                    if (addedDevices.Count > 0)
                        PopulateDevices(addedDevices, Color.Green, "ADDED DEVICES");
                }

                PopulateDevices(Snapshots[SnapshotsList.SelectedIndex].Devices, TextBox.ForeColor, "COMPLETE LIST OF DEVICES DETECTED");

                saveToFileToolStripMenuItem.Enabled = true;
                copyToClipboardToolStripMenuItem.Enabled = true;
            }
            else
            {
                saveToFileToolStripMenuItem.Enabled = false;
                copyToClipboardToolStripMenuItem.Enabled = false;
            }
        }

        private void PopulateDevices(List<DeviceEntry> List, Color Color, string Header)
        {
            TextBox.SelectionColor = Color;
            TextBox.SelectionFont = new Font(TextBox.Font, FontStyle.Bold);
            TextBox.AppendText("--- " + Header + " ---\n");
            TextBox.SelectionFont = new Font(TextBox.Font, FontStyle.Regular);
            foreach (DeviceEntry Device in List)
            {
                TextBox.SelectionColor = Color;
                TextBox.AppendText(Device.ToString() + "\n");
            }
            TextBox.AppendText("\n");
        }

        private void SaveToFile(object sender, EventArgs e)
        {
            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                TextBox.SaveFile(SaveDialog.OpenFile(), RichTextBoxStreamType.PlainText);
                SetStatus("Log saved to file: " + SaveDialog.FileName);
            }
        }

        private void CopyToClipboard(object sender, EventArgs e)
        {
            TextBox.SelectAll();
            TextBox.Copy();
            TextBox.DeselectAll();
            SetStatus("Log copied to clipboard");
        }

        private void ShowHowTo(object sender, EventArgs e)
        {
            TextBox.Clear();
            saveToFileToolStripMenuItem.Enabled = false;
            copyToClipboardToolStripMenuItem.Enabled = false;
            TextBox.AppendText("\nHOWTO:\n 1. Use Listener menu to start logging device changes\n 2. When listener is enabled, initial scan of devices will occur\n");
            TextBox.AppendText(" 3. Any detected change will add new snapshot of device tree to side panel\n 4. Select any other snapshot to see complete list of devices and compare with previous snap\n\nEnjoy!");
        }

        private void OpenSourceSite(object sender, EventArgs e)
        {
            Process.Start("https://github.com/sk-juroot/deviceDetectWF");
        }
    }
}
