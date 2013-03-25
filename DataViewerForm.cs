using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SasHarness
{
    public partial class DataViewerForm : Form
    {
        System.Data.OleDb.OleDbConnection connection = null;

        public DataViewerForm(SasServer session)
        {
            InitializeComponent();

            // init the OleDbConnection based on the Workspace ID
            // Note that this requires that the Workspace handle has
            // been added to the ObjectKeeper in SASObjectManager
            string wsId = session.Workspace.UniqueIdentifier;
            connection = new System.Data.OleDb.OleDbConnection("Provider=sas.iomprovider.1; SAS Workspace ID=" + wsId);
        }

        private void LoadBtn_Click(object sender, EventArgs e)
        {
            Cursor old = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                DataSet ds = new DataSet();
                string where = "";
                // Apply a filter to the result set if specified
                if (!string.IsNullOrEmpty(txtFilter.Text))
                {
                    where = String.Format("WHERE {0}", txtFilter.Text);
                }
                // Using SAS OLE DB provider to access the data
                OleDbCommand command = new OleDbCommand(String.Format("select * from {0} {1}", txtData.Text, where), connection);
                OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                adapter.Fill(ds);
                if (ds.Tables.Count > 0)
                    dgView.DataSource = ds.Tables[0];
                else MessageBox.Show("No table found!");

                this.Text = string.Format("{0} ({1})", txtData.Text, txtFilter.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error opening table");
            }
            finally
            {
                Cursor.Current = old;
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// This adds a ROW number to the grid, so you can see how many records
        /// are shown in the data set.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, grid.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }


    }
}
