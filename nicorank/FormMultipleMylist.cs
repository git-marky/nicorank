using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace nicorank
{
    public partial class FormMultipleMylist : Form
    {
        FormMain form_main_;

        public FormMultipleMylist(FormMain form_main)
        {
            form_main_ = form_main;
            InitializeComponent();
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            List<string> mylist_id_list = new List<string>();
            List<int> mylist_count_list = new List<int>();

            for (int i = 0; i < dataGridViewMylist.Rows.Count; ++i)
            {
                if (!string.IsNullOrEmpty((string)dataGridViewMylist[0, i].Value))
                {
                    mylist_id_list.Add((string)dataGridViewMylist[0, i].Value);
                    mylist_count_list.Add(int.Parse(((string)dataGridViewMylist[1, i].Value)));
                }
            }
            form_main_.AddMultipleMylist(mylist_id_list, mylist_count_list);
        }
    }
}
