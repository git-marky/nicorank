// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace nicorank
{
    public partial class FormShowingCand : Form
    {
        public FormShowingCand()
        {
            InitializeComponent();
        }

        private List<string> src_file_list_  = new List<string>();
        private List<string> dest_file_list_ = new List<string>();
        private List<string> trans_extension_list_ = new List<string>();
        private TranslatingOption trans_option_;

        public void SetTransOption(TranslatingOption trans_option)
        {
            trans_option_ = trans_option;
        }

        public void SetFileList(List<string> src_file_list, List<string> dest_file_list, List<string> trans_extension_list)
        {
            src_file_list_ = src_file_list;
            dest_file_list_ = dest_file_list;
            trans_extension_list_ = trans_extension_list;
            UpdateList();
        }

        public void UpdateList()
        {
            listViewCand.Items.Clear();
            for (int i = 0; i < src_file_list_.Count; ++i)
            {
                for (int j = 0; j < trans_extension_list_.Count; ++j)
                {
                    string src_filename = src_file_list_[i];
                    string dest_filename = Path.ChangeExtension(dest_file_list_[i], trans_extension_list_[j]);
                    if (checkBoxOnlyFileName.Checked)
                    {
                        src_filename = Path.GetFileName(src_filename);
                        dest_filename = Path.GetFileName(dest_filename);
                    }
                    string[] item = { src_filename, dest_filename };
                    listViewCand.Items.Add(new ListViewItem(item));
                }
            }
            labelMessage.Text = "";
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FormShowingCand_Load(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(MakingListThread));
        }

        private void MakingListThread(object obj)
        {
            VideoConverter.MakeTransList(trans_option_, out src_file_list_, out dest_file_list_);
            if (trans_option_.is_flv_to_avi)
            {
                trans_extension_list_.Add("avi");
            }
            if (trans_option_.is_flv_to_wav)
            {
                trans_extension_list_.Add("wav");
            }
            if (trans_option_.is_flv_to_mp3)
            {
                trans_extension_list_.Add("mp3");
            }
            if (trans_option_.is_flv_to_png)
            {
                trans_extension_list_.Add("png");
            }
            BeginInvoke(new MethodInvoker(UpdateList));
        }

        private void checkBoxOnlyFileName_CheckedChanged(object sender, EventArgs e)
        {
            UpdateList();
        }
    }
}
