using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace SasHarness
{
    public partial class SasServerLoginDlg : Form
    {
        public SasServer SasServer = null;

        public SasServerLoginDlg()
        {
            InitializeComponent();
            UpdateFields();
        }

        private void chkLocal_CheckedChanged(object sender, EventArgs e)
        {
            UpdateFields();
        }

        private void UpdateFields()
        {
            txtHost.Enabled = !chkLocal.Checked;
            txtName.Enabled = !chkLocal.Checked;
            txtUser.Enabled = !chkLocal.Checked;
            txtPort.Enabled = !chkLocal.Checked;
            txtPassword.Enabled = !chkLocal.Checked;
        }

        public SasServerLoginDlg(SasServer s) : this()
        {
            SasServer = s;
            txtHost.Text = s.Host;
            txtName.Text = s.Name;
            txtUser.Text = s.UserId;
            txtPort.Text = s.Port;
            chkLocal.Checked = SasServer.UseLocal;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                SasServer = new SasServer();
                SasServer.UseLocal = chkLocal.Checked;
                SasServer.Host = txtHost.Text;
                SasServer.Port = txtPort.Text;
                SasServer.UserId = txtUser.Text;
                SasServer.Password = txtPassword.Text;
                SasServer.Name = txtName.Text;
            }
            base.OnClosing(e);
        }

    }
}
