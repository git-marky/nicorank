// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using NicoTools;

namespace nicorank
{
    public partial class FormChoicePic : Form
    {
        List<int> picture_list_;
        private string trans_after_dir_;
        private static Random random_ = new Random();
        private InputOutputOption iooption_ = null;

        public FormChoicePic()
        {
            InitializeComponent();
        }

        public void SetPath(InputOutputOption iooption, string trans_after_dir)
        {
            trans_after_dir_ = trans_after_dir;
            iooption_ = iooption;
        }

        private void FormChoicePic_Load(object sender, EventArgs e)
        {
            try
            {
                RankFile rank_file = iooption_.GetRankFile();
                comboBoxChoiceVideo.Items.Add(" ");
                for (int i = 0; i < rank_file.Count; ++i)
                {
                    Video video = rank_file.GetVideo(i);
                    // 動画ID: タイトル
                    comboBoxChoiceVideo.Items.Add(video.video_id + ": " + video.title);
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("エラー：" + err.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void buttonShowPic_Click(object sender, EventArgs e)
        {
            try
            {
                string video_id = GetVideoId();
                if (video_id == "")
                {
                    MessageBox.Show("動画IDが指定されていません。");
                    return;
                }
                SetImage(pictureBoxCurrent, trans_after_dir_ + video_id + ".png");
                ChoosePic(video_id);
            }
            catch (Exception err)
            {
                MessageBox.Show("エラー：" + err.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void buttonChange_Click(object sender, EventArgs e)
        {
            try
            {
                RadioButton[] radio_button = { radioButtonChoice1, radioButtonChoice2, radioButtonChoice3,
                                           radioButtonChoice4, radioButtonChoice5, radioButtonChoice6,
                                           radioButtonChoice7, radioButtonChoice8, radioButtonChoice9};
                string video_id = GetVideoId();
                if (video_id == "")
                {
                    MessageBox.Show("動画IDが指定されていません。");
                    return;
                }
                int pic_num = -1;
                if (checkBoxChoiceHand.Checked)
                {
                    if (!int.TryParse(textBoxHandNo.Text, out pic_num))
                    {
                        MessageBox.Show("No.指定が不正です。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
                else
                {
                    for (int i = 0; i < radio_button.Length; ++i)
                    {
                        if (radio_button[i].Checked)
                        {
                            pic_num = picture_list_[i];
                            break;
                        }
                    }
                }
                string pic_filename = trans_after_dir_ + video_id + "\\" +
                    video_id + "_" + pic_num.ToString("0000") + ".png";
                File.Copy(pic_filename, trans_after_dir_ + video_id + ".png", true);
                SetImage(pictureBoxCurrent, pic_filename);
            }
            catch (Exception err)
            {
                MessageBox.Show("エラー：" + err.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void buttonHandShow_Click(object sender, EventArgs e)
        {
            string video_id = GetVideoId();
            if (video_id == "")
            {
                MessageBox.Show("動画IDが指定されていません。");
                return;
            }
            try
            {
                string pic_filename = trans_after_dir_ + video_id + "\\" +
                    video_id + "_" + int.Parse(textBoxHandNo.Text).ToString("0000") + ".png";
                System.IO.File.Copy(pic_filename, trans_after_dir_ + video_id + ".png", true);
                SetImage(pictureBoxChoiceHand, pic_filename);
            }
            catch (Exception err)
            {
                MessageBox.Show("エラー：" + err.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void SetChoicePic(string pic_filename, int num, bool is_enable)
        {
            PictureBox[] pic_choices = { pictureBoxChoice1, pictureBoxChoice2, pictureBoxChoice3,
                                         pictureBoxChoice4, pictureBoxChoice5, pictureBoxChoice6,
                                         pictureBoxChoice7, pictureBoxChoice8, pictureBoxChoice9};
            RadioButton[] radioButton = {radioButtonChoice1, radioButtonChoice2, radioButtonChoice3,
                                            radioButtonChoice4, radioButtonChoice5, radioButtonChoice6,
                                            radioButtonChoice7, radioButtonChoice8, radioButtonChoice9};

            pic_choices[num - 1].Enabled = is_enable;
            radioButton[num - 1].Enabled = is_enable;

            if (is_enable)
            {
                SetImage(pic_choices[num - 1], pic_filename);
            }
            else
            {
                if (pic_choices[num - 1].Image != null)
                {
                    pic_choices[num - 1].Image.Dispose();
                }
                pic_choices[num - 1].Image = null;
            }
        }

        private static void SetImage(PictureBox pictureBox, string filename)
        {
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            pictureBox.Image = System.Drawing.Image.FromStream(fs);
            fs.Close();
        }

        private string GetVideoId()
        {
            if (radioButton1.Checked)
            {
                string id_title = (string)comboBoxChoiceVideo.SelectedItem;
                if (id_title == null || id_title == "")
                {
                    return "";
                }
                int index = id_title.IndexOf(':');
                if (index < 0)
                {
                    return "";
                }
                return id_title.Substring(0, index);
            }
            else
            {
                return textBoxChoiceVideo.Text;
            }
        }

        private void ChoosePic(string video_id)
        {
            picture_list_ = new List<int>();

            if (!Directory.Exists(trans_after_dir_ + video_id))
            {
                MessageBox.Show(video_id + " の画像フォルダが存在しません。\r\n「FLV変換」で画像を先に作成してください。");
                return;
            }
            string[] files = Directory.GetFiles(trans_after_dir_ + video_id, "*.png");
            int num_of_pic = files.Length;

            if (num_of_pic > 9)
            {
                for (int i = 0; i < 9; ++i)
                {
                    int num = random_.Next(num_of_pic) + 1;
                    for (int j = 0; j < picture_list_.Count; ++j) // 重複チェック
                    {
                        if (picture_list_[j] == num) // やり直し
                        {
                            --i;
                            continue;
                        }
                    }
                    picture_list_.Add(num);
                    SetChoicePic(trans_after_dir_ +
                        video_id + "\\" + video_id + "_" + num.ToString("0000") + ".png", i + 1, true);
                }
            }
            else
            {
                for (int i = 1; i <= num_of_pic; ++i)
                {
                    picture_list_.Add(i);
                    SetChoicePic(trans_after_dir_ +
                        video_id + "\\" + video_id + "_" + i.ToString("0000") + ".png", i, true);
                }
                for (int i = num_of_pic + 1; i <= 9; ++i)
                {
                    SetChoicePic("", i, false);
                }
            }
            radioButtonChoice1.Checked = true;
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            comboBoxChoiceVideo.Enabled = radioButton1.Checked;
            textBoxChoiceVideo.Enabled = !radioButton1.Checked;
        }

        private void checkBoxChoiceHand_CheckedChanged(object sender, EventArgs e)
        {
            textBoxHandNo.Enabled = checkBoxChoiceHand.Checked;
            buttonHandShow.Enabled = checkBoxChoiceHand.Checked;
            groupBox1.Enabled = !checkBoxChoiceHand.Checked;
        }
    }
}
