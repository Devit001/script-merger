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
using System.Configuration;

namespace ScriptMgerger
{
    public partial class Form1 : Form
    {
        private const string LastUsedPath = "LastUsedPath.txt";
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonSet_Click(object sender, EventArgs e)
        {
            ShowFolderBrowserDialog();
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {

            Clean();
        }

        private void buttonMerge_Click(object sender, EventArgs e)
        {
            Merge();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBoxPath.Text = GetLastUsedPath();
        }

        private void Clean()
        {
            textBoxPath.Clear();
            richTextBoxOutput.Clear();
        }

        private List<string> GetProyFiles(string path)
        {
            var files = Directory.GetFiles(path, "*.txt").ToList();
            files.Sort();
            return files;
        }

        private void Merge()
        {
            var proyFiles = GetProyFiles(textBoxPath.Text);

            foreach (var f in proyFiles)
                richTextBoxOutput.AppendText(f + "\n");
        }

        private void ShowFolderBrowserDialog()
        {
            folderBrowserDialog1.SelectedPath = GetLastUsedPath();
            var dialogResult = folderBrowserDialog1.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                textBoxPath.Text = folderBrowserDialog1.SelectedPath;
                SetLastUsedPath( folderBrowserDialog1.SelectedPath);
            }
        }

        private string GetLastUsedPath()
        {
            return File.Exists(LastUsedPath)
                        ? File.ReadAllText(LastUsedPath)
                        : "";
        }

        private void SetLastUsedPath(string usedPath)
        {
            File.WriteAllText(LastUsedPath, usedPath);
        }

    }
}
