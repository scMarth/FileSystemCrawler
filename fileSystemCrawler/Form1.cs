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
        }

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

        private void listView1_MouseUp(object sender, MouseEventArgs e)
        {
            var hit = listView1.HitTest(e.Location);
            if (hit.SubItem != null && hit.SubItem == hit.Item.SubItems[0])
            {
                var url = new Uri(hit.SubItem.Text);
                //OpenFolder(Convert.ToString(url)); // Doesn't work on paths that look like \\etc.
            }else if (hit.SubItem != null && hit.SubItem != hit.Item.SubItems[0])
            {
                return;
            }
        }


        private void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };
            }else
            {
                MessageBox.Show(string.Format("{0} Directory does not exist!", folderPath));
            }
        }

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
                MessageBox.Show("Error: Could not copy the file's path.");
            }
        }

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
    }
}
