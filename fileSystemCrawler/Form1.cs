using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace fileSystemCrawler
{
    public partial class Form1 : Form
    {

        private ListViewColumnSorter lvwColumnSorter;


        public Form1()
        {
            InitializeComponent();
            // Create an instance of a ListView column sorter and assign it 
            // to the ListView control.
            lvwColumnSorter = new ListViewColumnSorter();
            this.listView1.ListViewItemSorter = lvwColumnSorter;

            // add new handler for left / right clicks
            this.listView1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.OnListViewMouseUp);

            // Select the item and subitems when selection is made.
            listView1.FullRowSelect = true;
        }

        // open the path if the "Select Folder" button is clicked
        private void button1_Click(object sender, EventArgs e)
        {
            // open the path that the user selected in the browser dialog
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    // clear the listView
                    listView1.Items.Clear();
                    
                    // visit the path that the user selected
                    visitDirectory(fbd.SelectedPath);
                }
            }
        }

        public void visitDirectory(string path)
        {
            string[] files = Directory.GetFiles(path); // files
            string[] folders = Directory.GetDirectories(path); // folders

            // print filenames
            foreach (string file in files)
            {
                ListViewItem lvi = new ListViewItem(file);
                lvi.SubItems.Add(File.GetCreationTime(file).ToString()); // date created
                lvi.SubItems.Add(File.GetCreationTimeUtc(file).ToString()); // date created UTC
                lvi.SubItems.Add(File.GetLastWriteTime(file).ToString()); // date modified
                lvi.SubItems.Add(File.GetLastWriteTimeUtc(file).ToString()); // date modified UTC

                FileInfo fi = new FileInfo(file);
                lvi.SubItems.Add(fi.Length.ToString()); //file size

                lvi.SubItems.Add(Path.GetExtension(file)); // file extension

                listView1.Items.Add(lvi);
            }
            // recurse
            foreach (string folder in folders)
            {
                visitDirectory(folder);
            }
        }

        // logic for sorting columns if the column header is clicked
        private void colClick(object sender, ColumnClickEventArgs e)
        {
            // Determine if clicked column is already the column that is being sorted.
            if (e.Column == lvwColumnSorter.SortColumn)
            {
                // Reverse the current sort direction for this column.
                if (lvwColumnSorter.Order == SortOrder.Ascending)
                {
                    lvwColumnSorter.Order = SortOrder.Descending;
                }
                else
                {
                    lvwColumnSorter.Order = SortOrder.Ascending;
                }
            }
            else
            {
                // Set the column number that is to be sorted; default to ascending.
                lvwColumnSorter.SortColumn = e.Column;
                lvwColumnSorter.Order = SortOrder.Ascending;
                if (lvwColumnSorter.SortColumn == 5) lvwColumnSorter.intSort = 1; // sort based on size of int
                else lvwColumnSorter.intSort = 0; // sort strings in lexicographic order
            }

            // Perform the sort with these new sort options.
            this.listView1.Sort();
        }

        // change mouse cursor if user hovers over a path
        private void listView1_MouseMove(object sender, MouseEventArgs e)
        {
            var hit = listView1.HitTest(e.Location);
            if (hit.SubItem != null && hit.SubItem == hit.Item.SubItems[0]) listView1.Cursor = Cursors.Hand;
            else listView1.Cursor = Cursors.Default;
        }

        // if the user right clicks, select that item
        private void OnListViewMouseUp(object sender, MouseEventArgs e)
        {
            ListView listView = sender as ListView;
            if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Left)
            {
                ListViewItem item = listView.GetItemAt(e.X, e.Y);
                if (item != null) item.Selected = true;
            }
        }

        // handler for "Copy Parent Folder Path" option in the context menu strip
        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = listView1.SelectedItems[0].SubItems[0].Text; // get the full file path
                path = Directory.GetParent(path).FullName; // find the path of the file's parent directory
                Clipboard.SetText(path); // copy the parent directory path to the clipboard
            }
            catch (Exception)
            {
                MessageBox.Show("Error: Could not copy the parent folder's path.");
            }
        }

        // handler for "Open File Location" option in the context menu strip
        private void openFileLocationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = listView1.SelectedItems[0].SubItems[0].Text; // get the full file path
                path = Directory.GetParent(path).FullName; // find the path of the file's parent directory
                Process.Start("explorer.exe", path); // open the parent directory path in the file explorer
            }
            catch (Exception)
            {
                MessageBox.Show("Error: Could not open the file's location.");
            }
        }

        // the "Export to CSV" button is pressed
        private void exportToCSV(object sender, EventArgs e)
        {
            // only execute if there are entries to export
            if (listView1.Items.Count > 0)
            {
                // Displays SaveFileDialog so the user can save the CSV
                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "CSV|*.csv";
                sfd.Title = "Export CSV";
                sfd.ShowDialog();

                // If the file name is not an empty string, open it for saving
                if (sfd.FileName != "")
                {
                    // reset and rescale the progress bar
                    rescaleResetProgressBar();

                    // show the progress bar
                    progressBar1.Visible = true;

                    // Write the header
                    string outfile = sfd.FileName;
                    string csvHeader = "PATH,Date Created,Date Created UTC,Date Modified,Date Modified UTC,File Size(Bytes),File Extension";
                    csvHeader += Environment.NewLine;
                    System.IO.File.WriteAllText(outfile, csvHeader);

                    // set progress bar maximum
                    progressBar1.Maximum = listView1.Items.Count;

                    // write the file data
                    for (int i = 0; i < listView1.Items.Count; i++) // for each entry
                    {
                        ListViewItem item = listView1.Items[i];

                        appendToFile(outfile, item.SubItems[0].Text + ","); // PATH
                        appendToFile(outfile, item.SubItems[1].Text + ","); // Date Created
                        appendToFile(outfile, item.SubItems[2].Text + ","); // Date Created UTC
                        appendToFile(outfile, item.SubItems[3].Text + ","); // Date Modified
                        appendToFile(outfile, item.SubItems[4].Text + ","); // Date Modified UTC
                        appendToFile(outfile, item.SubItems[5].Text + ","); // File Size(Bytes)
                        appendToFile(outfile, item.SubItems[6].Text); // File Extension

                        if (i != (listView1.Items.Count - 1)) appendToFile(outfile, Environment.NewLine);

                        progressBar1.Increment(1);
                    }

                    // hide the progress bar
                    progressBar1.Visible = false;
                    MessageBox.Show(string.Format("Exported {0} entries.", listView1.Items.Count.ToString()));
                }
            }else
            {
                MessageBox.Show("Error: No entries to be written, aborting.");
            }
        }

        private void rescaleResetProgressBar()
        {
            // reset the progress bar value to 0
            progressBar1.Value = 0;

            // rescale the progress bar
            int thirds = (int)(this.Width / 3);
            int marginTop = (int)(0.49 * this.Height);
            progressBar1.Top = marginTop;
            progressBar1.Left = thirds;
            progressBar1.Width = thirds;
        }

        // append the string 'str' to file 'filename'
        private void appendToFile(string filename, string str)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(filename, true))
            {
                file.Write(str);
            }
        }

        // handler for "Copy File Path" option in the context menu strip
        private void copyFilePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string path = listView1.SelectedItems[0].SubItems[0].Text; // get the full file path
                Clipboard.SetText(path); // copy the parent directory path to the clipboard
            }
            catch (Exception)
            {
                MessageBox.Show("Error: Could not copy the file's path.");
            }
        }
    }
}
