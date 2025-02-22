using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace nicorank
{
    public partial class MessageBoxWithTextBox : Form
    {
        public MessageBoxWithTextBox()
        {
            InitializeComponent();
        }

        public string InputText
        {
            get { return textBoxMain.Text; }
        }

        public void SetText(string text, string title, string textbox_text)
        {
            Text = title;
            labelMain.Text = text;
            textBoxMain.Text = textbox_text;
            Width = Math.Max(labelMain.Right + 20, 320);
            Height = 120;
        }
    }
}
