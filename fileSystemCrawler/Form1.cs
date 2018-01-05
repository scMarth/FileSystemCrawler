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
        public Form1()
        {
            InitializeComponent();
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

            textBox1.Text += Environment.NewLine + "Files in " + path + Environment.NewLine + Environment.NewLine;
            // print filenames
            foreach (string file in files)
            {
                textBox1.Text += file + Environment.NewLine;
                textBox1.Text += "Date Created: " + File.GetCreationTime(file).ToString() + Environment.NewLine;
                textBox1.Text += "Date Created UTC: " + File.GetCreationTimeUtc(file).ToString() + Environment.NewLine;
                textBox1.Text += "Date Modified: " + File.GetLastWriteTime(file).ToString() + Environment.NewLine;
                textBox1.Text += "Date Modified UTC: " + File.GetLastWriteTimeUtc(file).ToString() + Environment.NewLine + Environment.NewLine; ;

            }
            // recurse
            foreach (string folder in folders)
            {
                visitDirectory(folder);
            }
        }
    }
}
