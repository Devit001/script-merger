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

        //future settings
        private const string CommentStart = "//";
        private const string RootScriptFolder = "Scripts";
        private const string LastUsedPath = "LastUsedPath.txt";
        //todo: find a more subtle way to save the settings  rather than using a file

        private FileStream OutputFile;
        private string CurrentVersionFolder;
        private string BasePath;
        
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
            textBoxPath.Text = CurrentVersionFolder = GetLastUsedPath();
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
            OutputFile = File.Create($"{CurrentVersionFolder}OUTPUT_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.sql");// 
            var indexOfRootScriptFolder = CurrentVersionFolder.LastIndexOf(RootScriptFolder);
            try
            {
                if (indexOfRootScriptFolder < 0)
                    throw new Exception($"El directorio raíz de Scripts \"{RootScriptFolder}\" no se encontró en la ruta de la carpeta seleccionada.");

                BasePath = CurrentVersionFolder.Substring(0, indexOfRootScriptFolder);

                foreach (var f in proyFiles)
                {
                    richTextBoxOutput.AppendText($"<<<{Path.GetFileName(f)}>>>");

                    ProcessFile(f);

                    richTextBoxOutput.AppendText("\n\n\n");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                OutputFile.Close();
            }
            //open file if success
        }

        private void ProcessFile(string projectFile)
        {
            var lines = File.ReadAllLines(projectFile);
            foreach(var line in lines)
            {
                if (line.StartsWith(CommentStart) || string.IsNullOrWhiteSpace(line))
                    continue;
                var currentScript = Path.Combine(BasePath, line);

                if (!File.Exists(currentScript))
                {
                    richTextBoxOutput.AppendText("      ***Error, no encontrado***  " + currentScript);
                    throw new Exception($"Error al procesar {projectFile}\n\nNo se encontró:{currentScript}");
                }

                WriteToOutput(currentScript);
            }
        }

        private void WriteToOutput(string script)
        {
            var scriptStream = File.OpenRead(script);
            //read from script and write to output
            try
            {
                //OutputFile.
                //scriptStream.si;

                richTextBoxOutput.AppendText("      " + script);
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                scriptStream.Close();
            }

        }
        private void ShowFolderBrowserDialog()
        {
            folderBrowserDialog1.SelectedPath = GetLastUsedPath();
            var dialogResult = folderBrowserDialog1.ShowDialog();
            if (dialogResult == DialogResult.OK)
            {
                CurrentVersionFolder = folderBrowserDialog1.SelectedPath;
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
