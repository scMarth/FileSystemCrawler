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
                lvi.SubItems.Add(File.GetCreationTime(file).ToString());
                lvi.SubItems.Add(File.GetCreationTimeUtc(file).ToString());
                lvi.SubItems.Add(File.GetLastWriteTime(file).ToString());
                lvi.SubItems.Add(File.GetLastWriteTimeUtc(file).ToString());

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
    }
}
