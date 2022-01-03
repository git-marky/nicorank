using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using IJLib;
using NicoTools;
using System.Runtime.Serialization;         // 2019/06/26 ADD marky
using System.Runtime.Serialization.Json;    // 2019/06/26 ADD marky

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

    // 2019-07-22 ADD marky
    public class GenreTagManager : CategoryManager
    {

        protected  Dictionary<string, GenreTagItem> genre_item_dic_ = new Dictionary<string, GenreTagItem>();
        protected  string genre_config_ = "全ジャンル";
        protected  DateTime getdate_ = DateTime.Now.Date; //当日の過去ログをデフォルトとする

        public string GetGenre()
        {
            return genre_config_;
        }

        public void SetGenre(string genre)
        {
            genre_config_ = genre;
        }

        public DateTime GetDate
        {
            get { return getdate_; }
            set { getdate_ = value; }
        }

        // ランキングジャンル、人気のタグ一覧を取得
        public virtual void ParseGenreTagFile(string json)
        {
            genre_item_dic_.Clear();

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GenreTagList[]));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                GenreTagList[] result = (GenreTagList[])serializer.ReadObject(ms);
                if (result != null)
                {
                    for (int j = 0; j < result.Length; ++j)
                    {
                        GenreTagItem tag = new GenreTagItem();
                        tag.genre = result[j].genre;
                        tag.file = result[j].file;
                        tag.id = tag.file.Replace(".json", "");
                        if (!string.IsNullOrEmpty(result[j].tag))
                        {
                            tag.tag = result[j].tag;
                            tag.name = tag.genre + "：" + result[j].tag;
                        }
                        else
                        {
                            tag.tag = "";
                            tag.name = tag.genre;
                        }
                        genre_item_dic_.Add(tag.name, tag);
                    }
                }
                SetTagList(genre_config_);
            }
        }

        public virtual void SetTagList(string genre)
        {
            category_item_dic_.Clear();

            foreach (GenreTagItem genretag in genre_item_dic_.Values)
            {
                if (genre.Equals("全ジャンル") || genretag.genre.Equals(genre))
                {
                    CategoryItem item = new CategoryItem();
                    item.id = genretag.id;
                    item.short_name = genretag.tag;
                    item.name = genretag.name;
                    int[] page = new int[5];
                    for (int j = 0; j < page.Length; ++j)
                    {
                        page[j] = 1;
                    }
                    item.page = page;
                    item.genre = genretag.genre;
                    category_item_dic_.Add(item.name, item);
                }
            }
        }

        public override List<CategoryItem> GetDownloadCategoryItemList()
        {
            List<CategoryItem> c_list = new List<CategoryItem>();

            foreach (CategoryItem item in category_item_dic_.Values)
            {
                if (Array.IndexOf(category_config_, item.name) >= 0)
                {
                    c_list.Add(item);
                }
            }

            return c_list;
        }

        // 2020/02/16 ADD marky ジャンル名からジャンル英字を返す
        public string GetGenreId(string name)
        {
            string id = "all";  //初期値

            foreach (GenreTagItem genre in genre_item_dic_.Values)
            {
                if (genre.genre.Equals(name))
                {
                    id = genre.id;
                    break;
                }
            }
            return id;
        }

    }

    // 2019/06/26 ADD marky
    public class GenreTagManagerWithCListBox : GenreTagManager
    {
        private CheckedListBox clistbox_;
        private ComboBox combobox_;
        private bool loadflag = true;

        public GenreTagManagerWithCListBox(CheckedListBox clistbox, ComboBox combobox)
        {
            clistbox_ = clistbox;
            combobox_ = combobox;
        }

        public string GetSaveString()
        {
            StringBuilder cate_buff = new StringBuilder();
            for (int i = 0; i < clistbox_.CheckedItems.Count; ++i)
            {
                cate_buff.Append(category_item_dic_[(string)clistbox_.CheckedItems[i]].name);   // idが表すタグは毎回変わるためタグ名で保存
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

        // ランキングジャンル、人気のタグ一覧を取得
        public override void ParseGenreTagFile(string json)
        {
            int index = -1;
            genre_item_dic_.Clear();
            combobox_.Items.Clear();

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(GenreTagList[]));
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                GenreTagList[] result = (GenreTagList[])serializer.ReadObject(ms);
                if (result != null)
                {
                    for (int j = 0; j < result.Length; ++j)
                    {
                        GenreTagItem tag = new GenreTagItem();
                        tag.genre = result[j].genre;
                        tag.file = result[j].file;
                        tag.id = tag.file.Replace(".json", "");
                        if (!string.IsNullOrEmpty(result[j].tag))
                        {
                            tag.tag = result[j].tag;
                            tag.name = tag.genre + "：" + result[j].tag;
                        }
                        else
                        {
                            tag.tag = "";
                            tag.name = tag.genre;
                            combobox_.Items.Add(tag.name);
                        }
                        genre_item_dic_.Add(tag.name, tag);
                        // 初期値取得
                        if (tag.name.Equals(genre_config_)){
                            index = combobox_.Items.Count - 1;
                        }
                    }
                }
            }
            combobox_.SelectedIndex = index; //ChangedイベントでSetTagListが呼び出される
            loadflag = false;
        }

        public override void SetTagList(string genre)
        {
            if (!loadflag)  //ロード中はコンフィグ値を上書きしない
            {
                // 現在のタグリストチェック状態を保存
                SetString(GetSaveString());
                genre_config_ = genre;
            }

            category_item_dic_.Clear();
            clistbox_.Items.Clear();

            foreach (GenreTagItem genretag in genre_item_dic_.Values)
            {
                if (genre.Equals("全ジャンル") || genretag.genre.Equals(genre))
                {
                    CategoryItem item = new CategoryItem();
                    item.id = genretag.id;
                    item.short_name = genretag.tag;
                    item.name = genretag.name;
                    int[] page = new int[5];
                    for (int j = 0; j < page.Length; ++j)
                    {
                        page[j] = 1;
                    }
                    item.page = page;
                    item.genre = genretag.genre;
                    category_item_dic_.Add(item.name, item);

                    clistbox_.Items.Add(item.name, Array.IndexOf(category_config_, item.name) >= 0);
                }
            }
        }
    }

    // 2019/06/26 ADD marky ジャンル＋人気のタグ ファイル
    [DataContract]
    class GenreTagList
    {
        [DataMember]
        public string genre = "";

        [DataMember]
        public string tag = "";

        [DataMember]
        public string file = "";
    }

}
