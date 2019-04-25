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
using System.Diagnostics;

namespace ScriptMerger
{
    public partial class Form1 : Form
    {

        //future settings
        private int BufferSize // 1024 * 500;
        {
            get
            {
                return Properties.Settings.Default.BufferSize;
            }
        }
        private string ScriptSeparator
        {
            get
            {
                return Properties.Settings.Default.ScriptSeparator;
            }
        }
        private string CommentStart
        {
            get
            {
                return Properties.Settings.Default.CommentStart;
            }
        }
        private string RootScriptFolder
        {
            get
            {
                return Properties.Settings.Default.RootScriptFolder;
            }
        }
        private string[] LastUsedPaths
        {
            get
            {
                return Properties.Settings.Default.LastUsedPaths.Cast<string>().ToArray();
            }
        }


        private bool Error;
        private StreamWriter OutputFile;
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
            CurrentVersionFolder = GetLastUsedPath();
        }

        private void Clean()
        {
            richTextBoxOutput.Clear();
            if(MessageBox.Show("¿Limpiar lista de directorios recientes también?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Properties.Settings.Default.LastUsedPaths.Clear();
                Properties.Settings.Default.Save();
                comboBox1.DataSource = null;
                comboBox1.Text = null;
            }
        }

        private List<string> GetProyFiles(string path)
        {
            var files = Directory.GetFiles(path, "*.txt").ToList();
            files.Sort();
            return files;
        }

        private void Merge()
        {
            CurrentVersionFolder = comboBox1.Text;

            if (string.IsNullOrWhiteSpace(CurrentVersionFolder))
            {
                MessageBox.Show("Selecciona un directorio.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Directory.Exists(CurrentVersionFolder))
            {
                MessageBox.Show($"El directorio {CurrentVersionFolder} no existe.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            richTextBoxOutput.Text = "";
            Error = false;
            var proyFiles = GetProyFiles(CurrentVersionFolder);
            string outputFileName = Path.Combine(CurrentVersionFolder, $"OUTPUT_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.sql");
            OutputFile = File.CreateText(outputFileName);// 
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
                Error = true;
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                OutputFile.Close();
            }

            if (Error)
            {
                File.Delete(outputFileName);
            }
            else
            {
                MessageBox.Show("Archivo procesado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Process.Start("explorer.exe", "/select," + outputFileName);
                AddLastUsedPath(CurrentVersionFolder);
            }
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
                    richTextBoxOutput.AppendText("\n      ***Error, no encontrado***\n            " + currentScript);
                    throw new Exception($"Error al procesar: {projectFile}\n\nNo se encontró:\n{currentScript}");
                }

                if (Error)
                    break;

                WriteToOutput(line);
            }
        }

        private void WriteToOutput(string script)
        {
            string newLine = "\r\n";
            StreamReader scriptStream = null;
            try
            {
                scriptStream = File.OpenText(Path.Combine(BasePath, script));
                
                string s = $"-->>>{script} {newLine}";
                char [] buffer = Encoding.UTF8.GetChars(Encoding.UTF8.GetBytes(s));
                OutputFile.Write(buffer, 0, buffer.Length);
                

                int bytesRead = 1;
                buffer = new char[BufferSize];

                while(bytesRead > 0)
                {
                    bytesRead = scriptStream.ReadBlock(buffer, 0, BufferSize);
                    OutputFile.Write(buffer, 0, bytesRead);
                }

                buffer = Encoding.UTF8.GetChars(Encoding.UTF8.GetBytes(newLine + ScriptSeparator + newLine + newLine + newLine));
                OutputFile.Write(buffer, 0, buffer.Length);

                richTextBoxOutput.AppendText("\n      " + script);
            }
            catch(Exception e)
            {
                Error = true;
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
                AddLastUsedPath(folderBrowserDialog1.SelectedPath);
            }
        }

        private string GetLastUsedPath()
        {
            comboBox1.DataSource = LastUsedPaths;
            return LastUsedPaths.FirstOrDefault();
        }

        private void AddLastUsedPath(string usedPath)
        {
            if (LastUsedPaths.Contains(usedPath))
            {
                Properties.Settings.Default.LastUsedPaths.Remove(usedPath);
            }

            Properties.Settings.Default.LastUsedPaths.Insert(0, usedPath);

            int maxEntries = 5;
            if(Properties.Settings.Default.LastUsedPaths.Count > maxEntries)
            {
                Properties.Settings.Default.LastUsedPaths.RemoveAt(maxEntries);
            }
                
            comboBox1.DataSource = LastUsedPaths;
            Properties.Settings.Default.Save();
        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                Merge();
        }
    }
}
