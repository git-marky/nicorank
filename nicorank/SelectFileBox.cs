using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace nicorank
{
    public partial class SelectFileBox : UserControl
    {
        // 以下の2つは null ではない方を呼び出す
        private FileDialog file_dialog_ = null;
        private FolderBrowserDialog folder_dialog_ = null;

        public SelectFileBox()
        {
            InitializeComponent();
        }

        public FileDialog FileDialog
        {
            get { return file_dialog_; }
            set { file_dialog_ = value; }
        }

        public FolderBrowserDialog FolderBrowserDialog
        {
            get { return folder_dialog_; }
            set { folder_dialog_ = value; }
        }

        public string FileName
        {
            get { return textBoxFileName.Text; }
            set { textBoxFileName.Text = value; }
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            string old_path = Directory.GetCurrentDirectory();
            string initial_dir = textBoxFileName.Text;

            if (initial_dir != "")
            {
                if (!(initial_dir.Length >= 2 && initial_dir[1] == ':')) // 絶対パスでないなら
                {
                    initial_dir = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), initial_dir);
                }
                if (!Directory.Exists(initial_dir))
                {
                    try
                    {
                        initial_dir = Path.GetDirectoryName(initial_dir);
                    }
                    catch (ArgumentException) { }
                    catch (PathTooLongException) { }
                }
            }
            if (file_dialog_ != null)
            {
                file_dialog_.FileName = "";
                file_dialog_.InitialDirectory = initial_dir;
                if (file_dialog_.ShowDialog(this) == DialogResult.OK)
                {
                    textBoxFileName.Text = file_dialog_.FileName;
                }
            }
            else if (folder_dialog_ != null)
            {
                folder_dialog_.SelectedPath = initial_dir;
                if (folder_dialog_.ShowDialog(this) == DialogResult.OK)
                {
                    textBoxFileName.Text = folder_dialog_.SelectedPath;
                }
                Directory.SetCurrentDirectory(old_path);
            }
            Directory.SetCurrentDirectory(old_path);
        }

        private void textBoxFileName_DragDrop(object sender, DragEventArgs e)
        {
            string filename = GetDropFilename(e);
            if (filename != "")
            {
                ((TextBox)sender).Text = filename;
            }
        }

        private void textBoxFileName_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private string GetDropFilename(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    return files[0];
                }
                else
                {
                    return "";
                }
            }
            return "";
        }

        private void textBoxFileName_TextChanged(object sender, EventArgs e)
        {
            toolTip.SetToolTip(textBoxFileName, textBoxFileName.Text);
        }
    }
}
