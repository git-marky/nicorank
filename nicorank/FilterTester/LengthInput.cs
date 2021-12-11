using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace nicorank.FilterTester
{
    public partial class LengthInput : UserControl
    {
        public LengthInput()
        {
            InitializeComponent();
        }

        private void minutesTextBox_Validating(object sender, CancelEventArgs e)
        {
            string error_message;
            if (!IsValidMinutes(textBoxMinutes.Text, out error_message))
            {
                e.Cancel = true;
                textBoxMinutes.SelectAll();

                textErrorProvider.SetError(textBoxMinutes, error_message);
            }
        }

        private bool IsValidMinutes(string minutes_str, out string error_message)
        {
            int minutes;
            if (int.TryParse(minutes_str, out minutes))
            {
                if (0 <= minutes)
                {
                    error_message = string.Empty;
                    return true;
                }
                else
                {
                    error_message = "分が不の値です。";
                    return false;
                }
            }
            else
            {
                error_message = "分を整数に変換できません。";
                return false;
            }
        }

        private void secondsTextBox_Validating(object sender, CancelEventArgs e)
        {
            string error_message;
            if (!IsValidSeconds(textBoxSeconds.Text, out error_message))
            {
                e.Cancel = true;
                textBoxSeconds.SelectAll();

                textErrorProvider.SetError(textBoxSeconds, error_message);
            }
        }

        private bool IsValidSeconds(string seconds_str, out string error_message)
        {
            int seconds;
            if (int.TryParse(seconds_str, out seconds))
            {
                if (0 <= seconds && seconds <= 59)
                {
                    error_message = string.Empty;
                    return true;
                }
                else
                {
                    error_message = "秒が0以上59以下の範囲ではありません。";
                    return false;
                }
            }
            else
            {
                error_message = "秒を整数に変換できません。";
                return false;
            }
        }

        private void textBox_Validated(object sender, EventArgs e)
        {
            Control control = sender as Control;
            if (control != null)
            {
                textErrorProvider.SetError(control, string.Empty);
            }
        }

        public TimeSpan LengthValue
        {
            get
            {
                int hours = 0;
                int minutes = int.Parse(textBoxMinutes.Text);
                int seconds = int.Parse(textBoxSeconds.Text);

                return new TimeSpan(hours, minutes, seconds);
            }
        }
    }
}
