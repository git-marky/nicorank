using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NicoTools;

namespace nicorank.FilterTester
{
    public partial class FormFilterTester : Form
    {
        ICollection<Control> filter_source_controls_text_;
        ICollection<Control> filter_source_controls_file_;
        Encoding filter_file_encoding_;
        FilterParser parser_;

        private delegate TreeNode FilterNodeCreator(IVideoFilter filter);
        IDictionary<Type, FilterNodeCreator> node_creator_map_;

        private class TagLockPair
        {
            private TextBox tag_text_box_;
            private CheckBox lock_check_box_;

            public TagLockPair(TextBox tag_text_box, CheckBox lock_check_box)
            {
                tag_text_box_ = tag_text_box;
                lock_check_box_ = lock_check_box;
            }

            public TextBox TagTextBox
            {
                get { return tag_text_box_; }
            }

            public CheckBox LockCheckBox
            {
                get { return lock_check_box_; }
            }
        }

        private ICollection<TagLockPair> tag_controls_;

        public FormFilterTester()
        {
            InitializeComponent();

            filter_source_controls_text_ = new List<Control>();
            filter_source_controls_text_.Add(linkLabelCopyToClipboard);
            filter_source_controls_text_.Add(linkLabelSaveAs);
            filter_source_controls_text_.Add(filterTextBox);

            filter_source_controls_file_ = new List<Control>();
            filter_source_controls_file_.Add(textBoxFilterFile);
            filter_source_controls_file_.Add(buttonBrowseFilterFile);

            filter_file_encoding_ = new UTF8Encoding(true);

            parser_ = new FilterParser();

            node_creator_map_ = new Dictionary<Type, FilterNodeCreator>();
            node_creator_map_.Add(typeof(AndFilter), CreateAndFilterNode);
            node_creator_map_.Add(typeof(OrFilter), CreateOrFilterNode);
            node_creator_map_.Add(typeof(NotFilter), CreateNotFilterNode);
            node_creator_map_.Add(typeof(VideoIdFilter), CreateIdFilterNode);
            node_creator_map_.Add(typeof(VideoTitleFilter), CreateTitleFilterNode);
            node_creator_map_.Add(typeof(VideoDescriptionFilter), CreateDescriptionFilterNode);
            node_creator_map_.Add(typeof(VideoSubmitDateFilter), CreateSubmitDateFilterNode);
            node_creator_map_.Add(typeof(VideoViewFilter), CreateViewFilterNode);
            node_creator_map_.Add(typeof(VideoCommentFilter), CreateCommentFilterNode);
            node_creator_map_.Add(typeof(VideoMylistFilter), CreateMylistFilterNode);
            node_creator_map_.Add(typeof(VideoPNameFilter), CreatePNameFilterNode);
            node_creator_map_.Add(typeof(VideoLengthFilter), CreateLengthFilterNode);
            node_creator_map_.Add(typeof(VideoTagFilter), CreateTagFilterNode);
            node_creator_map_.Add(typeof(LockedVideoTagFilter), CreateLockedTagFilterNode);
            node_creator_map_.Add(typeof(UnlockedVideoTagFilter), CreateUnlockedTagFilterNode);

            tag_controls_ = new List<TagLockPair>();
            tag_controls_.Add(new TagLockPair(textBoxTag01, checkBoxTagLock01));
            tag_controls_.Add(new TagLockPair(textBoxTag02, checkBoxTagLock02));
            tag_controls_.Add(new TagLockPair(textBoxTag03, checkBoxTagLock03));
            tag_controls_.Add(new TagLockPair(textBoxTag04, checkBoxTagLock04));
            tag_controls_.Add(new TagLockPair(textBoxTag05, checkBoxTagLock05));
            tag_controls_.Add(new TagLockPair(textBoxTag06, checkBoxTagLock06));
            tag_controls_.Add(new TagLockPair(textBoxTag07, checkBoxTagLock07));
            tag_controls_.Add(new TagLockPair(textBoxTag08, checkBoxTagLock08));
            tag_controls_.Add(new TagLockPair(textBoxTag09, checkBoxTagLock09));
            tag_controls_.Add(new TagLockPair(textBoxTag10, checkBoxTagLock10));
        }

        private void filterFromTextBoxRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonFilterFromTextBox.Checked == true)
            {
                this.SuspendLayout();

                foreach (Control control in filter_source_controls_text_)
                {
                    control.Enabled = true;
                }

                radioButtonFilterFromFile.Checked = false;
                foreach (Control control in filter_source_controls_file_)
                {
                    control.Enabled = false;
                }

                this.ResumeLayout();
            }
        }

        private void filterFromFileRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonFilterFromFile.Checked == true)
            {
                this.SuspendLayout();

                foreach (Control control in filter_source_controls_file_)
                {
                    control.Enabled = true;
                }

                radioButtonFilterFromTextBox.Checked = false;
                foreach (Control control in filter_source_controls_text_)
                {
                    control.Enabled = false;
                }

                this.ResumeLayout();
            }
        }

        private void copyToClipboardLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!string.IsNullOrEmpty(filterTextBox.Text))
            {
                Clipboard.SetText(filterTextBox.Text);
            }
        }

        private void saveAsLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (filterSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = filterSaveFileDialog.FileName;
                using (Stream output = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                using (TextWriter writer = new StreamWriter(output, filter_file_encoding_))
                {
                    writer.WriteLine("version=2");
                    writer.Write(filterTextBox.Text);
                }
            }
        }

        private void browseFilterFileButton_Click(object sender, EventArgs e)
        {
            if (filterOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBoxFilterFile.Text = filterOpenFileDialog.FileName;
            }
        }

        private void parseButton_Click(object sender, EventArgs e)
        {
            try
            {
                string filter_expression = GetFilterExpression();
                IVideoFilter filter = parser_.Parse(filter_expression);
                ConstructFilterTree(filter);
            }
            catch (IOException ex)
            {
                string message = string.Format("フィルターファイルの読み込みに失敗しました。\n{0}", ex.Message);
                if (ex.InnerException != null)
                {
                    message = string.Format("{0}\n{1}", message, ex.InnerException.Message);
                }
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (FilterParseException ex)
            {
                string message = string.Format("フィルターのパースに失敗しました。\n{0}", ex.Message);
                if (ex.InnerException != null)
                {
                    message = string.Format("{0}\n{1}", message, ex.InnerException.Message);
                }
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConstructFilterTree(IVideoFilter filter)
        {
            treeViewFilter.Nodes.Clear();

            FilterNodeCreator node_creator;
            if (!node_creator_map_.TryGetValue(filter.GetType(), out node_creator))
            {
                node_creator = CreateUnknownFilterNode;
            }

            treeViewFilter.Nodes.Add(node_creator(filter));

            treeViewFilter.ExpandAll();
        }

        private void testVideoButton_Click(object sender, EventArgs e)
        {
            IVideoFilter filter;
            try
            {
                string filter_expression = GetFilterExpression();
                filter = parser_.Parse(filter_expression);
                ConstructFilterTree(filter);
            }
            catch (IOException ex)
            {
                string message = string.Format("フィルターファイルの読み込みに失敗しました。\n{0}", ex.Message);
                if (ex.InnerException != null)
                {
                    message = string.Format("{0}\n{1}", message, ex.InnerException.Message);
                }
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            catch (FilterParseException ex)
            {
                string message = string.Format("フィルターのパースに失敗しました。\n{0}", ex.Message);
                if (ex.InnerException != null)
                {
                    message = string.Format("{0}\n{1}", message, ex.InnerException.Message);
                }
                MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Video video = CreateVideo();

            string result;
            MessageBoxIcon icon;
            if (filter.IsThrough(video))
            {
                result = "フィルター通過";
                icon = MessageBoxIcon.Information;
            }
            else
            {
                result = "フィルター除外";
                icon = MessageBoxIcon.Exclamation;
            }

            MessageBox.Show(result, "Filter Result", MessageBoxButtons.OK, icon);
        }

        private Video CreateVideo()
        {
            Video video = new Video();
            video.video_id = textBoxId.Text;
            video.title = textBoxTitle.Text;
            video.description = textBoxDescription.Text;
            video.submit_date = dateTimePickerSubmit.Value;
            RankPoint point = new RankPoint();
            checked
            {
                point.view = (int)numericUpDownViewCount.Value;
                point.res = (int)numericUpDownCommentCount.Value;
                point.mylist = (int)numericUpDownMylistCount.Value;
            }
            video.point = point;
            video.length = GetLengthString(lengthInput.LengthValue);

            foreach (TagLockPair pair in tag_controls_)
            {
                TextBox text_box = pair.TagTextBox;
                CheckBox check_box = pair.LockCheckBox;

                if (!string.IsNullOrEmpty(text_box.Text))
                {
                    video.tag_set.Add(text_box.Text, check_box.Checked);
                }
            }

            return video;
        }

        private string GetFilterExpression()
        {
            if (radioButtonFilterFromTextBox.Checked == true)
            {
                return filterTextBox.Text;
            }
            else if (radioButtonFilterFromFile.Checked == true)
            {
                string filename = textBoxFilterFile.Text;
                if (string.IsNullOrEmpty(filename))
                {
                    throw new IOException();
                }
                using (Stream input = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (TextReader reader = new StreamReader(input, filter_file_encoding_))
                {
                    if (reader.ReadLine() != "version=2")
                    {
                        throw new IOException("ファイルの先頭に「version=2」がありません。");
                    }

                    return reader.ReadToEnd();
                }
            }
            else
            {
                throw new InvalidOperationException("コントロール状態矛盾");
            }
        }

        #region filter tree node creators

        private TreeNode CreateAndFilterNode(IVideoFilter ifilter)
        {
            AndFilter filter = ifilter as AndFilter;

            TreeNode node = new TreeNode("AND");

            foreach (IVideoFilter sub_filter in filter.SubFilters)
            {
                FilterNodeCreator node_creator;
                if (!node_creator_map_.TryGetValue(sub_filter.GetType(), out node_creator))
                {
                    node_creator = CreateUnknownFilterNode;
                }

                node.Nodes.Add(node_creator(sub_filter));
            }

            return node;
        }

        private TreeNode CreateOrFilterNode(IVideoFilter ifilter)
        {
            OrFilter filter = ifilter as OrFilter;

            TreeNode node = new TreeNode("OR");

            foreach (IVideoFilter sub_filter in filter.SubFilters)
            {
                FilterNodeCreator node_creator;
                if (!node_creator_map_.TryGetValue(sub_filter.GetType(), out node_creator))
                {
                    node_creator = CreateUnknownFilterNode;
                }

                node.Nodes.Add(node_creator(sub_filter));
            }

            return node;
        }

        private TreeNode CreateNotFilterNode(IVideoFilter ifilter)
        {
            NotFilter filter = ifilter as NotFilter;

            TreeNode node = new TreeNode("NOT");

            IVideoFilter inner_filter = filter.InnerFilter;

            FilterNodeCreator node_creator;
            if (!node_creator_map_.TryGetValue(inner_filter.GetType(), out node_creator))
            {
                node_creator = CreateUnknownFilterNode;
            }

            node.Nodes.Add(node_creator(inner_filter));


            return node;
        }

        private TreeNode CreateIdFilterNode(IVideoFilter ifilter)
        {
            VideoIdFilter filter = ifilter as VideoIdFilter;

            StringBuilder text = new StringBuilder("id");
            AppendTextFilterPattern(filter, text, false, false, true);

            TreeNode node = new TreeNode(text.ToString());

            return node;
        }

        private TreeNode CreateTitleFilterNode(IVideoFilter ifilter)
        {
            VideoTitleFilter filter = ifilter as VideoTitleFilter;

            StringBuilder text = new StringBuilder("title");
            AppendTextFilterPattern(filter, text, false, true, false);

            TreeNode node = new TreeNode(text.ToString());

            return node;
        }

        private TreeNode CreateDescriptionFilterNode(IVideoFilter ifilter)
        {
            VideoDescriptionFilter filter = ifilter as VideoDescriptionFilter;

            StringBuilder text = new StringBuilder("description");
            AppendTextFilterPattern(filter, text, false, true, false);

            TreeNode node = new TreeNode(text.ToString());

            return node;
        }

        private TreeNode CreateSubmitDateFilterNode(IVideoFilter ifilter)
        {
            VideoSubmitDateFilter filter = ifilter as VideoSubmitDateFilter;

            string datetime_format = "yyyy-MM-ddTHH:mm:ss";

            string text;
            if (filter.MinDate == DateTime.MinValue)
            {
                text = string.Format("submit:,{0}", filter.MaxDate.ToString(datetime_format));
            }
            else if (filter.MaxDate == DateTime.MaxValue)
            {
                text = string.Format("submit:{0},", filter.MinDate.ToString(datetime_format));
            }
            else
            {
                text = string.Format("submit:{0},{1}", filter.MinDate.ToString(datetime_format), filter.MaxDate.ToString(datetime_format));
            }

            TreeNode node = new TreeNode(text);

            return node;
        }

        private TreeNode CreateViewFilterNode(IVideoFilter ifilter)
        {
            VideoViewFilter filter = ifilter as VideoViewFilter;

            StringBuilder text = new StringBuilder("view:");
            AppendNumericFilterPattern(filter, text);

            TreeNode node = new TreeNode(text.ToString());

            return node;
        }

        private TreeNode CreateCommentFilterNode(IVideoFilter ifilter)
        {
            VideoCommentFilter filter = ifilter as VideoCommentFilter;

            StringBuilder text = new StringBuilder("comment:");
            AppendNumericFilterPattern(filter, text);

            TreeNode node = new TreeNode(text.ToString());

            return node;
        }

        private TreeNode CreateMylistFilterNode(IVideoFilter ifilter)
        {
            VideoMylistFilter filter = ifilter as VideoMylistFilter;

            StringBuilder text = new StringBuilder("mylist:");
            AppendNumericFilterPattern(filter, text);

            TreeNode node = new TreeNode(text.ToString());

            return node;
        }

        private TreeNode CreatePNameFilterNode(IVideoFilter ifilter)
        {
            VideoPNameFilter filter = ifilter as VideoPNameFilter;

            StringBuilder text = new StringBuilder("pname");
            AppendTextFilterPattern(filter, text, false, true, true);

            TreeNode node = new TreeNode(text.ToString());

            return node;
        }

        private TreeNode CreateLengthFilterNode(IVideoFilter ifilter)
        {
            VideoLengthFilter filter = ifilter as VideoLengthFilter;

            string text;
            if (filter.MinLength == TimeSpan.MinValue)
            {
                text = string.Format("length:,{0}", GetLengthString(filter.MaxLength));
            }
            else if (filter.MaxLength == TimeSpan.MaxValue)
            {
                text = string.Format("length:{0},", GetLengthString(filter.MinLength));
            }
            else
            {
                text = string.Format("length:{0},{1}", GetLengthString(filter.MinLength), GetLengthString(filter.MaxLength));
            }

            TreeNode node = new TreeNode(text);

            return node;
        }

        private TreeNode CreateTagFilterNode(IVideoFilter ifilter)
        {
            VideoTagFilter filter = ifilter as VideoTagFilter;

            StringBuilder text = new StringBuilder("tag");
            AppendTextFilterPattern(filter, text, false, true, true);

            TreeNode node = new TreeNode(text.ToString());

            return node;
        }

        private TreeNode CreateLockedTagFilterNode(IVideoFilter ifilter)
        {
            LockedVideoTagFilter filter = ifilter as LockedVideoTagFilter;

            StringBuilder text = new StringBuilder("ltag");
            AppendTextFilterPattern(filter, text, false, true, true);

            TreeNode node = new TreeNode(text.ToString());

            return node;
        }

        private TreeNode CreateUnlockedTagFilterNode(IVideoFilter ifilter)
        {
            UnlockedVideoTagFilter filter = ifilter as UnlockedVideoTagFilter;

            StringBuilder text = new StringBuilder("utag");
            AppendTextFilterPattern(filter, text, false, true, true);

            TreeNode node = new TreeNode(text.ToString());

            return node;
        }

        private TreeNode CreateUnknownFilterNode(IVideoFilter ifilter)
        {
            return new TreeNode("!Unknown Filter");
        }

        private static void AppendTextFilterPattern(TextFilterBase filter, StringBuilder text, bool isregex_default, bool ignorecase_default, bool matchall_default)
        {
            if ((filter.IsRegex != isregex_default) ||
                (filter.IgnoreCase != ignorecase_default) ||
                (filter.MatchAll != matchall_default))
            {
                text.Append("@");

                if (filter.IsRegex)
                {
                    text.Append("r");
                }
                if (filter.IgnoreCase)
                {
                    text.Append("i");
                }
                if (filter.MatchAll)
                {
                    text.Append("a");
                }

            }
            text.Append(":");
            text.Append(filter.Pattern);
        }

        private static void AppendTextFilterPattern(VideoTagFilter filter, StringBuilder text, bool isregex_default, bool ignorecase_default, bool matchall_default)
        {
            if ((filter.IsRegex != isregex_default) ||
                (filter.IgnoreCase != ignorecase_default) ||
                (filter.MatchAll != matchall_default))
            {
                text.Append("@");
                if (filter.IsRegex)
                {
                    text.Append("r");
                }
                if (filter.IgnoreCase)
                {
                    text.Append("i");
                }
                if (filter.MatchAll)
                {
                    text.Append("a");
                }
            }

            text.Append(":");
            text.Append(filter.Pattern);
        }

        private static void AppendNumericFilterPattern(NumericalRangeFilterBase filter, StringBuilder text)
        {
            if (filter.LowerBound == int.MinValue)
            {
                text.Append(string.Format(",{0}", filter.UpperBound));
            }
            else if (filter.UpperBound == int.MaxValue)
            {
                text.Append(string.Format("{0},", filter.LowerBound));
            }
            else
            {
                text.Append(string.Format("{0},{1}", filter.LowerBound, filter.UpperBound));
            }
        }

        private string GetLengthString(TimeSpan length)
        {
            checked
            {
                int minutes = (int)length.TotalMinutes;
                int seconds = length.Seconds;

                return string.Format("{0,2:d2}:{1,2:d2}", minutes, seconds);
            }
        }

        #endregion

        private void filterTextLegendLabel_Click(object sender, EventArgs e)
        {
            radioButtonFilterFromTextBox.Checked = true;

        }

        private void filterFileLegendLabel_Click(object sender, EventArgs e)
        {
            radioButtonFilterFromFile.Checked = true;
        }
    }
}
