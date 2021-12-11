using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace nicorank
{
    public partial class MessageBoxWithCheckBox : Form
    {
        public MessageBoxWithCheckBox()
        {
            InitializeComponent();
        }

        public bool CheckBoxState
        {
            get { return checkBoxMain.Checked; }
        }

        public void SetText(string text, string title, string checkbox_text)
        {
            Text = title;
            labelMain.Text = text;
            checkBoxMain.Text = checkbox_text;
            Width = Math.Max(labelMain.Right + 20, 320);
            Height = 120;
        }
    }
}
