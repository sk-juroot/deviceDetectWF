using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace deviceDetectWF
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void About_Load(object sender, EventArgs e)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            labelVersion.Text = "version " + fvi.FileVersion;
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            string decodedString = Encoding.ASCII.GetString(Convert.FromBase64String("aHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1wQWduSkRKTjRWQQ=="));
            Process.Start(decodedString);
        }

        private void About_Leave(object sender, EventArgs e)
        {
            this.Close();
        }

        private void About_Deactivate(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
