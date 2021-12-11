// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Drawing;
using System.Windows.Forms;
using IJLib;
using System.IO;
using System.Text;

namespace nicorank
{
    public partial class FormLayout : Form
    {
        public bool is_changed = false;

        public FormLayout()
        {
            InitializeComponent();
        }

        private void FormLayout_Load(object sender, EventArgs e)
        {
            textBoxRankData.Text = "sm1234567	1	9,312	657	415	0.93	4.45	14,077" +
                "	タイトル見本タイトル見本タイトル見本タイトル見本	" +
                "2008年04月01日 01：01：00	sm1234567.png";
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            try
            {
                Layout layout = new Layout(textBoxLayout.Text, Path.GetDirectoryName(selectFileBoxLayout.FileName));
                pictureBox1.Width = layout.Width;
                pictureBox1.Height = layout.Height;
                Image old_image = pictureBox1.Image;
                pictureBox1.Image = new Bitmap(pictureBox1.Width, pictureBox1.Height);

                if (old_image != null)
                {
                    old_image.Dispose();
                }

                string rank_data;
                int line = (int)numericUpDown1.Value;

                if (radioButtonFromText.Checked)
                {
                    rank_data = textBoxRankData.Text;
                }
                else
                {
                    rank_data = File.ReadAllText(selectFileBoxRankFile.FileName, Encoding.GetEncoding(932));
                }
                string[] lines = rank_data.Split(new string[] { "\r\n" }, StringSplitOptions.None);
                if (lines.Length < line)
                {
                    MessageBox.Show(this, "行数をオーバーしています", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                using (Graphics graphics = Graphics.FromImage(pictureBox1.Image))
                {
                    int line_number = layout.LineNumber;
                    StringBuilder buff = new StringBuilder();
                    for (int i = line - 1; i < line - 1 + line_number && i < lines.Length; ++i)
                    {
                        buff.Append(lines[i]);
                        buff.Append("\r\n");
                    }
                    layout.DrawPicture(graphics, new LayoutData(buff.ToString()));
                }
            }
            catch (System.Exception err)
            {
                MessageBox.Show("エラー:\n" + err.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void buttonLayoutSave_Click(object sender, EventArgs e)
        {
            DialogResult result = DialogResult.Yes;
            if (System.IO.File.Exists(selectFileBoxLayout.FileName))
            {
                result = MessageBox.Show(this, "レイアウトファイルを上書きしますか？", "確認", MessageBoxButtons.YesNo);
            }
            if (result == DialogResult.Yes)
            {
                IJFile.Write(selectFileBoxLayout.FileName, textBoxLayout.Text);
                is_changed = false;
            }
        }

        private void textBoxLayout_TextChanged(object sender, EventArgs e)
        {
            is_changed = true;
        }

        private void FormLayout_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (is_changed)
            {
                DialogResult result;
                if (System.IO.File.Exists(selectFileBoxLayout.FileName))
                {
                    result = MessageBox.Show(this, "レイアウトファイルを上書き保存しますか？", "確認", MessageBoxButtons.YesNoCancel);
                }
                else
                {
                    result = MessageBox.Show(this, "レイアウトファイルを保存しますか？", "確認", MessageBoxButtons.YesNoCancel);
                }
                if (result == DialogResult.Yes)
                {
                    IJFile.Write(selectFileBoxLayout.FileName, textBoxLayout.Text);
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        private void buttonPicSave_Click(object sender, EventArgs e)
        {
            string old_path = System.IO.Directory.GetCurrentDirectory();
            DialogResult result = saveFileDialogPic.ShowDialog();
            System.IO.Directory.SetCurrentDirectory(old_path);
            if (result == DialogResult.OK)
            {
                try
                {
                    pictureBox1.Image.Save(saveFileDialogPic.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    MessageBox.Show(this, "保存しました。", "完了");
                }
                catch (System.Exception err)
                {
                    MessageBox.Show(this, "エラー:\n" + err.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        public void SetLayoutFile(string filename)
        {
            selectFileBoxLayout.FileName = filename;
        }

        public void SetRankFile(string filename)
        {
            selectFileBoxRankFile.FileName = filename;
        }

        private void buttonLayoutLoad_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists(selectFileBoxLayout.FileName))
            {
                DialogResult result = DialogResult.Yes;
                if (textBoxLayout.Text != "")
                {
                    result = MessageBox.Show(this, "レイアウトファイルを読み込みますか？", "確認", MessageBoxButtons.YesNo);
                }
                if (result == DialogResult.Yes)
                {
                    textBoxLayout.Text = IJFile.Read(selectFileBoxLayout.FileName);
                    is_changed = false;
                }
            }
            else
            {
                MessageBox.Show(this, "レイアウトファイルが存在しません。", "確認");
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            labelCoordinate.Text = "(" + e.X.ToString() + ", " + e.Y.ToString() + ")";
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            labelCoordinate.Text = "";
        }

        private void radioButtonFromText_CheckedChanged(object sender, EventArgs e)
        {
            selectFileBoxRankFile.Enabled = !radioButtonFromText.Checked;
            textBoxRankData.Enabled = radioButtonFromText.Checked;
        }
    }
}
