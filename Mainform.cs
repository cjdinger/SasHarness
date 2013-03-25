using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SasHarness
{
    public partial class Mainform : Form
    {
        // name of the "saved settings" file that we'll use to remember
        // the server config used from session to session
        const string appSettings = "SasHarnessSettings.xml";

        public SasServer activeSession = null;
        public Mainform()
        {
            InitializeComponent();
            txtProgram.KeyDown += txtProgram_KeyDown;
        }


        private void ConnectToServer()
        {
            activeSession = new SasServer();

            string settingsFile = Path.Combine(Application.LocalUserAppDataPath, appSettings);
            if (File.Exists(settingsFile))
            {
                string xml = File.ReadAllText(settingsFile);
                activeSession = SasServer.FromXml(xml);
            }
            SasServerLoginDlg login = new SasServerLoginDlg(activeSession);

            if (login.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                activeSession = login.SasServer;

                try
                {
                    activeSession.Connect();
                    File.WriteAllText(settingsFile, activeSession.ToXml());
                    if (activeSession.UseLocal)
                        statusMsg.Text = string.Format("Connected to Local SAS session as {0}", 
                            System.Environment.UserName);
                    else
                        statusMsg.Text = string.Format("Connected to {0} ({1}) as {2}",
                            activeSession.Name,
                            activeSession.Host,
                            string.IsNullOrEmpty(activeSession.UserId) ? Environment.UserName : activeSession.UserId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Could not connect: {0}", ex.Message));
                    statusMsg.Text = "";
                }
            }
        }

        #region Run a SAS job on a background thread
        private void RunProgram()
        {
            if (activeSession != null && activeSession.Workspace != null)
            {
                // if we don't use a background thread when running this program
                // we'll BLOCK the UI of the app while a long-running
                // SAS job completes.
                // This allows us to keep the UI responsive.
                BackgroundWorker bg = new BackgroundWorker();
                bg.DoWork += bg_DoWork;
                bg.RunWorkerCompleted += bg_RunWorkerCompleted;

                statusMsg.Text = "Running SAS program...";
                bg.RunWorkerAsync();
            }
        }

        void bg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            FetchResults();
        }

        void bg_DoWork(object sender, DoWorkEventArgs e)
        {
            activeSession.Workspace.LanguageService.Submit(txtProgram.Text);
        }
        #endregion

        /// <summary>
        /// Collect the results of a job from the SAS server
        /// </summary>
        private void FetchResults()
        {
            bool hasErrors = false, hasWarnings = false ;

            // when code is complete, update the log viewer
            Array carriage, lineTypes, lines;
            do
            {
                activeSession.Workspace.LanguageService.FlushLogLines(1000, 
                    out carriage, 
                    out lineTypes, 
                    out lines);
                for (int i = 0; i < lines.GetLength(0); i++)
                {
                    SAS.LanguageServiceLineType pre = 
                        (SAS.LanguageServiceLineType)lineTypes.GetValue(i);
                    switch (pre)
                    {
                        case SAS.LanguageServiceLineType.LanguageServiceLineTypeError:
                            hasErrors = true;
                            txtLog.SelectionColor = Color.Red;
                            break;
                        case SAS.LanguageServiceLineType.LanguageServiceLineTypeNote:
                            txtLog.SelectionColor = Color.DarkGreen;
                            break;
                        case SAS.LanguageServiceLineType.LanguageServiceLineTypeWarning:
                            hasWarnings = true;
                            txtLog.SelectionColor = Color.DarkCyan;
                            break;
                        case SAS.LanguageServiceLineType.LanguageServiceLineTypeTitle:
                        case SAS.LanguageServiceLineType.LanguageServiceLineTypeFootnote:
                            txtLog.SelectionColor = Color.Blue;
                            break;
                        default:
                            txtLog.SelectionColor = txtLog.ForeColor;
                            break;
                    }

                    txtLog.AppendText(string.Format("{0}{1}", lines.GetValue(i) as string, Environment.NewLine));
                }

            }
            while (lines != null && lines.Length > 0);

            // and update the Listing viewer
            do
            {
                activeSession.Workspace.LanguageService.FlushListLines(1000, out carriage, out lineTypes, out lines);
                for (int i = 0; i < lines.GetLength(0); i++)
                {
                    SAS.LanguageServiceLineType pre = (SAS.LanguageServiceLineType)lineTypes.GetValue(i);
                    switch (pre)
                    {
                        case SAS.LanguageServiceLineType.LanguageServiceLineTypeTitle:
                        case SAS.LanguageServiceLineType.LanguageServiceLineTypeFootnote:
                        case SAS.LanguageServiceLineType.LanguageServiceLineTypeByline:
                            txtListing.SelectionColor = Color.Blue;
                            break;
                        default:
                            txtListing.SelectionColor = txtListing.ForeColor;
                            break;
                    }

                    txtListing.AppendText(string.Format("{0}{1}", lines.GetValue(i) as string, Environment.NewLine));
                }

            }
            while (lines != null && lines.Length > 0);

            if (hasWarnings && hasErrors) 
                statusMsg.Text = "Program complete - has ERRORS and WARNINGS";
            else if (hasErrors)
                statusMsg.Text = "Program complete - has ERRORS";
            else  if (hasWarnings)
                statusMsg.Text = "Program complete - has WARNINGS";
            else 
                statusMsg.Text = "Program complete - no warnings or errors!";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (activeSession!=null && activeSession.IsConnected)
                activeSession.Close();

            base.OnClosing(e);
        }

        #region Key press handlers (F3 for Run program)
        void txtProgram_KeyDown(object sender, KeyEventArgs e)
        {
            OnKeyDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                onRun_Click(null,null);
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
        #endregion

        #region Button event handlers
        private void onConnect_Click(object sender, EventArgs e)
        {
            ConnectToServer();
        }

        private void onRun_Click(object sender, EventArgs e)
        {
            RunProgram();
        }
        #endregion

        private void OnOpenData_Click(object sender, EventArgs e)
        {
            if (activeSession != null && activeSession.IsConnected)
            {
                DataViewerForm dlg = new DataViewerForm(activeSession);
                dlg.Show(this);
            }
            else
            {
                MessageBox.Show("You must connect to a SAS session before you can open data.", "Connect to SAS");
            }
        }

    }
}
