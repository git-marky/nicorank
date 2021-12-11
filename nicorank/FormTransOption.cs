// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Text;
using System.Windows.Forms;

namespace nicorank
{
    public partial class FormTransOption : Form
    {
        public FormTransOption()
        {
            InitializeComponent();
        }

        private FormMain form_main_ = null;

        public void SetAppOption(TransDetailOption option)
        {
            checkBoxIsChangeSize.Checked = option.is_change_size;
            textBoxTransChangeWidth.Text = (option.change_width > 0 ? option.change_width.ToString() : "");
            textBoxTransChangeHeight.Text = (option.change_height > 0 ? option.change_height.ToString() : "");
            checkBoxTransIsFixAspect.Checked = option.is_fix_aspect;
            switch (option.trans_avi_kind)
            {
                case TransDetailOption.TransAviKind.Normal:
                    radioButtonTransNormal.Checked = true;
                    break;
                case TransDetailOption.TransAviKind.Bgr24Flip:
                    radioButtonTransBgr24Flip.Checked = true;
                    break;
                case TransDetailOption.TransAviKind.Bgr24:
                    radioButtonTransBgr24.Checked = true;
                    break;
                case TransDetailOption.TransAviKind.Yuv420p:
                    radioButtonTransYuv420p.Checked = true;
                    break;
                case TransDetailOption.TransAviKind.Huffyuv:
                    radioButtonTransHuffyuv.Checked = true;
                    break;
            }
            checkBoxWindowShow.Checked = option.is_show_window;
            checkBoxIsOnlySm.Checked = option.is_only_sm;
            checkBoxIsOnlyNm.Checked = option.is_only_nm;
            if (option.is_framerate_change)
            {
                radioButtonFrameRateChange.Checked = true;
            }
            else
            {
                radioButtonFrameRateNoChange.Checked = true;
            }
            textBoxFrameRate.Text = option.frame_rate;
        }

        public TransDetailOption GetAppOption()
        {
            TransDetailOption option = new TransDetailOption();
            option.is_change_size = checkBoxIsChangeSize.Checked;
            if (!int.TryParse(textBoxTransChangeWidth.Text, out option.change_width))
            {
                option.change_width = 0;
            }
            if (!int.TryParse(textBoxTransChangeHeight.Text, out option.change_height))
            {
                option.change_height = 0;
            }
            option.is_fix_aspect = checkBoxTransIsFixAspect.Checked;
            if (radioButtonTransNormal.Checked)
            {
                option.trans_avi_kind = TransDetailOption.TransAviKind.Normal;
            }
            else if (radioButtonTransBgr24Flip.Checked)
            {
                option.trans_avi_kind = TransDetailOption.TransAviKind.Bgr24Flip;
            }
            else if (radioButtonTransBgr24.Checked)
            {
                option.trans_avi_kind = TransDetailOption.TransAviKind.Bgr24;
            }
            else if (radioButtonTransYuv420p.Checked)
            {
                option.trans_avi_kind = TransDetailOption.TransAviKind.Yuv420p;
            }
            else if (radioButtonTransHuffyuv.Checked)
            {
                option.trans_avi_kind = TransDetailOption.TransAviKind.Huffyuv;
            }
            option.is_show_window = checkBoxWindowShow.Checked;
            option.is_only_sm = checkBoxIsOnlySm.Checked;
            option.is_only_nm = checkBoxIsOnlyNm.Checked;
            option.is_framerate_change = radioButtonFrameRateChange.Checked;
            option.frame_rate = textBoxFrameRate.Text;
            return option;
        }

        private void FormTransOption_FormClosed(object sender, FormClosedEventArgs e)
        {
            form_main_.InformClosedFormTransOption(GetAppOption());
        }

        public void SetFormMain(FormMain form_main)
        {
            form_main_ = form_main;
        }

        private void checkBoxIsChangeSize_CheckedChanged(object sender, EventArgs e)
        {
            textBoxTransChangeWidth.Enabled = checkBoxIsChangeSize.Checked;
            textBoxTransChangeHeight.Enabled = checkBoxIsChangeSize.Checked;
            labelChangeSize.Enabled = checkBoxIsChangeSize.Checked;
            checkBoxTransIsFixAspect.Enabled = checkBoxIsChangeSize.Checked;
        }
    }

    public class TransDetailOption
    {
        public enum TransAviKind { Normal, Bgr24Flip, Bgr24, Yuv420p, Huffyuv };

        public bool is_change_size;
        public int change_width;
        public int change_height;
        public bool is_fix_aspect;
        public TransAviKind trans_avi_kind = TransAviKind.Bgr24Flip;
        public bool is_show_window;
        public bool is_only_sm;
        public bool is_only_nm;
        public bool is_framerate_change;
        public string frame_rate;

        public string SaveData()
        {
            StringBuilder buff = new StringBuilder();
            buff.Append("TransDetailOption.IsChangeSize\t" + is_change_size);
            buff.Append("\r\n");
            buff.Append("TransDetailOption.ChangeWidth\t" + change_width);
            buff.Append("\r\n");
            buff.Append("TransDetailOption.ChangeHeight\t" + change_height);
            buff.Append("\r\n");
            buff.Append("TransDetailOption.IsFixAspect\t" + is_fix_aspect);
            buff.Append("\r\n");
            buff.Append("TransDetailOption.TransAviKind\t" + trans_avi_kind.ToString());
            buff.Append("\r\n");
            buff.Append("TransDetailOption.IsShowWindow\t" + is_show_window);
            buff.Append("\r\n");
            buff.Append("TransDetailOption.IsOnlySm\t" + is_only_sm);
            buff.Append("\r\n");
            buff.Append("TransDetailOption.IsOnlyNm\t" + is_only_nm);
            buff.Append("\r\n");
            buff.Append("TransDetailOption.IsFramerateChange\t" + is_framerate_change);
            buff.Append("\r\n");
            buff.Append("TransDetailOption.FrameRate\t" + frame_rate);
            buff.Append("\r\n");
            return buff.ToString();
        }

        public void SetDataFromLine(string data)
        {
            string[] ar = data.Split('\t');
            switch (ar[0])
            {
                case "TransDetailOption.IsChangeSize":
                    is_change_size = bool.Parse(ar[1]);
                    break;
                case "TransDetailOption.ChangeWidth":
                    change_width = int.Parse(ar[1]);
                    break;
                case "TransDetailOption.ChangeHeight":
                    change_height = int.Parse(ar[1]);
                    break;
                case "TransDetailOption.IsFixAspect":
                    is_fix_aspect = bool.Parse(ar[1]);
                    break;
                case "TransDetailOption.IsPixFmtBgr24": // 過去との互換性のため
                    trans_avi_kind = (bool.Parse(ar[1]) ? TransAviKind.Bgr24 : TransAviKind.Yuv420p);
                    break;
                case "TransDetailOption.TransAviKind":
                    switch (ar[1])
                    {
                        case "Normal":
                            trans_avi_kind = TransAviKind.Normal;
                            break;
                        case "Bgr24Flip":
                            trans_avi_kind = TransAviKind.Bgr24Flip;
                            break;
                        case "Bgr24":
                            trans_avi_kind = TransAviKind.Bgr24;
                            break;
                        case "Yuv420p":
                            trans_avi_kind = TransAviKind.Yuv420p;
                            break;
                        case "Huffyuv":
                            trans_avi_kind = TransAviKind.Huffyuv;
                            break;
                    }
                    break;
                case "TransDetailOption.IsShowWindow":
                    is_show_window = bool.Parse(ar[1]);
                    break;
                case "TransDetailOption.IsOnlySm":
                    is_only_sm = bool.Parse(ar[1]);
                    break;
                case "TransDetailOption.IsOnlyNm":
                    is_only_nm = bool.Parse(ar[1]);
                    break;
                case "TransDetailOption.IsFramerateChange":
                    is_framerate_change = bool.Parse(ar[1]);
                    break;
                case "TransDetailOption.IsHighFramerateChange": // 過去との互換性のため
                    if (bool.Parse(ar[1]))
                    {
                        is_framerate_change = false;
                    }
                    break;
                case "TransDetailOption.FrameRate":
                    frame_rate = ar[1];
                    break;
            }
        }
    }
}
