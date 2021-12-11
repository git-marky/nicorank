using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using IJLib;
using NicoTools;

namespace nicorank
{
    public class CategoryManager
    {
        protected Dictionary<string, CategoryItem> category_item_dic_ = new Dictionary<string, CategoryItem>();
        protected string[] category_config_ = new string[0];

        public void ParseCategoryFile()
        {
            string filename = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "category.txt");
            string str;

            if (File.Exists(filename))
            {
                str = IJFile.ReadUTF8(filename);
            }
            else
            {
                throw new FileNotFoundException("category.txt が存在しません。nicorank.exe を起動して category.txt を作成してください。");
            }

            string[] lines = IJStringUtil.SplitWithCRLF(str);

            for (int i = 1; i < lines.Length; ++i)
            {
                string[] ar = lines[i].Split('\t');
                CategoryItem item = new CategoryItem();
                item.id = ar[0];
                item.short_name = ar[1];
                item.name = ar[2];
                int[] page = new int[5];
                for (int j = 0; j < page.Length; ++j)
                {
                    page[j] = int.Parse(ar[3 + j]);
                }
                item.page = page;
                category_item_dic_.Add(item.name, item);
            }
        }

        public virtual List<CategoryItem> GetDownloadCategoryItemList()
        {
            List<CategoryItem> c_list = new List<CategoryItem>();

            foreach (CategoryItem item in category_item_dic_.Values)
            {
                if (Array.IndexOf(category_config_, item.id) >= 0)
                {
                    c_list.Add(item);
                }
            }

            return c_list;
        }

        public void SetString(string text)
        {
            category_config_ = text.Split(',');
        }
    }

    public class CategoryManagerWithCListBox : CategoryManager
    {
        private CheckedListBox clistbox_;

        public CategoryManagerWithCListBox(CheckedListBox clistbox)
        {
            clistbox_ = clistbox;
        }

        public string GetSaveString()
        {
            StringBuilder cate_buff = new StringBuilder();
            for (int i = 0; i < clistbox_.CheckedItems.Count; ++i)
            {
                cate_buff.Append(category_item_dic_[(string)clistbox_.CheckedItems[i]].id);
                cate_buff.Append(",");
            }
            if (cate_buff.Length >= 1) // Remove last comma
            {
                cate_buff.Remove(cate_buff.Length - 1, 1);
            }
            return cate_buff.ToString();
        }

        public override List<CategoryItem> GetDownloadCategoryItemList()
        {
            List<CategoryItem> c_list = new List<CategoryItem>();

            for (int i = 0; i < clistbox_.CheckedItems.Count; ++i)
            {
                string name = (string)clistbox_.CheckedItems[i];
                c_list.Add(category_item_dic_[name]);
            }

            return c_list;
        }

        public void ParseCategoryFile2(Form form)
        {
            string filename = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "category.txt");
            string str;

            if (File.Exists(filename))
            {
                str = IJFile.ReadUTF8(filename);

                if (!str.StartsWith("version")) // 昔のバージョンの category.txt なら
                {
                    if (str != Properties.Resources.category204) // ユーザによって改変されているなら
                    {
                        File.Move(filename,
                            Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),
                                "category_old1.txt")); // 念のためバックアップを取る
                        if (form != null)
                        {
                            MessageBox.Show(form, "category.txt を category_old1.txt にリネームしました。",
                                "ニコニコランキングメーカー", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else // for console
                        {
                            System.Console.WriteLine("category.txt を category_old1.txt にリネームしました。");
                        }
                    }
                    else
                    {
                        File.Delete(filename);
                    }
                    str = Properties.Resources.category;
                    IJFile.WriteUTF8(filename, str);
                }
            }
            else
            {
                str = Properties.Resources.category;
                IJFile.WriteUTF8(filename, str);
            }

            string[] lines = IJStringUtil.SplitWithCRLF(str);

            for (int i = 1; i < lines.Length; ++i)
            {
                string[] ar = lines[i].Split('\t');
                CategoryItem item = new CategoryItem();
                item.id = ar[0];
                item.short_name = ar[1];
                item.name = ar[2];
                int[] page = new int[5];
                for (int j = 0; j < page.Length; ++j)
                {
                    page[j] = int.Parse(ar[3 + j]);
                }
                item.page = page;
                category_item_dic_.Add(item.name, item);

                clistbox_.Items.Add(item.name, Array.IndexOf(category_config_, item.id) >= 0);
            }
        }
    }
}
