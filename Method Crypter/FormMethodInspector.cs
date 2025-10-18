using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Method_Crypter
{
    public partial class FormMethodInspector : Form
    {
        public FormMethodInspector()
        {
            InitializeComponent();
        }

        private void FormMethodInspector_Load(object sender, EventArgs e)
        {
            listViewMethodInspector.FullRowSelect = true;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            listViewMethodInspector.Items.Clear();
            this.Close();
        }

        private void buttonExportCSV_Click(object sender, EventArgs e)
        {
            ExportToCsv();
        }

        private void FormMethodInspector_Resize(object sender, EventArgs e)
        {
            buttonOK.Left = (ClientSize.Width - buttonOK.Width) / 2;
        }

        private void ExportToCsv()
        {
            if (listViewMethodInspector.Items.Count == 0)
            {
                MessageBox.Show("Nothing to Export!", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.Title = "Save Method List As";
                saveFileDialog.FileName = "MethodList.csv";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(saveFileDialog.FileName))
                        {
                            // Write headers
                            var headers = listViewMethodInspector.Columns
                                .Cast<ColumnHeader>()
                                .Select(col => col.Text);
                            sw.WriteLine(string.Join(",", headers));

                            // Write rows
                            foreach (ListViewItem item in listViewMethodInspector.Items)
                            {
                                var row = item.SubItems
                                    .Cast<ListViewItem.ListViewSubItem>()
                                    .Select(sub => EscapeCsv(sub.Text));
                                sw.WriteLine(string.Join(",", row));
                            }
                        }

                        MessageBox.Show("Export successful.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting file:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private string EscapeCsv(string value)
        {
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return $"\"{value.Replace("\"", "\"\"")}\"";
            return value;
        }
    }
}
