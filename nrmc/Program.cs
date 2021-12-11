// Copyright (c) 2008 - 2014 rankingloid
//
// under GNU General Public License Version 2.
//
using System;
using System.Collections.Generic;
using System.IO;
using IJLib;
using nicorank;
using NicoTools;

namespace nrmc
{
    class Program
    {
        private static Dictionary<string, string> option_ = new Dictionary<string, string>();
        private static string input_rank_file_ = null;
        private static string output_rank_file_ = null;
        private static string config_file_ = null;

        private class Receiver : MessageOut
        {
            public void Write(string text)
            {
                System.Console.Write(text);
            }

            public void WriteLine(string text)
            {
                System.Console.WriteLine(text);
            }
        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length <= 1)
                {
                    ShowUsage();
                    System.Environment.Exit(1);
                }

                Receiver receiver = new Receiver();
                CancelObject cancel_object = new CancelObject();
                NicoNetwork network = new NicoNetwork();
                NicoNetworkManager network_mgr = new NicoNetworkManager(network, receiver, cancel_object);
                NicoMylist nico_mylist = new NicoMylist(network, receiver, cancel_object);

                for (int i = 2; i < args.Length; ++i)
                {
                    switch (args[i])
                    {
                        case "-i":
                            if (i < args.Length - 1)
                            {
                                input_rank_file_ = Dequote(args[i + 1]);
                                if (!File.Exists(input_rank_file_))
                                {
                                    System.Console.WriteLine("入力ランクファイルが存在しません。");
                                    System.Environment.Exit(1);
                                }
                                ++i;
                            }
                            else
                            {
                                System.Console.WriteLine("入力ランクファイルを指定してください。");
                                System.Environment.Exit(1);
                            }
                            break;
                        case "-o":
                            if (i < args.Length - 1)
                            {
                                output_rank_file_ = Dequote(args[i + 1]);
                                ++i;
                            }
                            break;
                        case "-c":
                            if (i < args.Length - 1)
                            {
                                config_file_ = Dequote(args[i + 1]);
                                ++i;
                            }
                            break;
                    }
                }

                if (string.IsNullOrEmpty(config_file_)) // config ファイル指定なしの場合
                {
                    config_file_ = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "config.txt");
                    if (!File.Exists(config_file_))
                    {
                        System.Console.WriteLine("最初に nicorank.exe を起動してオプションを指定してください。");
                        System.Environment.Exit(1);
                    }
                }
                else
                {
                    if (!File.Exists(config_file_))
                    {
                        System.Console.WriteLine("config ファイルが見つかりません。");
                        System.Environment.Exit(1);
                    }
                }
                ParseConfig(IJFile.Read(config_file_));

                InputOutputOption iooption = new InputOutputOption(!string.IsNullOrEmpty(input_rank_file_), !string.IsNullOrEmpty(output_rank_file_));
                if (string.IsNullOrEmpty(input_rank_file_)) // 標準入力から
                {
                    iooption.SetInputFromStdin();
                }
                else
                {
                    iooption.SetInputPath(input_rank_file_);
                }
                if (string.IsNullOrEmpty(output_rank_file_)) // 標準出力へ
                {
                    iooption.SetOutputRankFileDelegate(delegate(string s)
                    {
                        System.Console.Write(s);
                    });
                }
                else
                {
                    iooption.SetOutputPath(output_rank_file_);
                }

                switch (args[0])
                {
                    case "download":
                        switch (args[1])
                        {
                            case "ranking":
                                CategoryManager category_manager = new CategoryManager();
                                category_manager.SetString(option_["dlrank_category"]);
                                category_manager.ParseCategoryFile();
                                network_mgr.DownloadRanking(GetDownloadKind(category_manager), option_["textBoxRankDlDir"]);
                                break;
                            case "video":
                                LoadCookie(network);
                                network_mgr.DownloadFlv(iooption, option_["textBoxDlInterval"], MakeDirectoryPath(option_["textBoxFlvDlDir"]),
                                    bool.Parse(option_["checkBoxIsFixFlvDlExtension"]));
                                break;
                            default:
                                ShowInvalidAndUsage(args[1]);
                                System.Environment.Exit(1);
                                break;
                        }
                        break;
                    case "list":
                        switch (args[1])
                        {
                            case "searchtag":
                                network_mgr.MakeListAndWriteBySearchTag(iooption, MakeSearchingTagOption(), MakeRankingMethod());
                                break;
                            default:
                                ShowInvalidAndUsage(args[1]);
                                System.Environment.Exit(1);
                                break;
                        }
                        break;
                    case "mylist":
                        switch (args[1])
                        {
                            case "add":
                                LoadCookie(network);
                                nico_mylist.AddMylist(iooption, option_["textBoxMylistId"]);
                                break;
                            default:
                                ShowInvalidAndUsage(args[1]);
                                System.Environment.Exit(1);
                                break;
                        }
                        break;
                    default:
                        ShowInvalidAndUsage(args[0]);
                        System.Environment.Exit(1);
                        break;
                }
            }
            catch (KeyNotFoundException e)
            {
                System.Console.WriteLine("エラーが発生しました。");
                System.Console.WriteLine("キーが存在しません: " + e.Data.ToString());
            }
            catch (Exception e)
            {
                System.Console.WriteLine("エラーが発生しました。");
                System.Console.WriteLine("エラー\r\n---メッセージ\r\n" +
                e.Message + "\r\n---ソース\r\n" + e.Source + "\r\n---スタックトレース\r\n" +
                e.StackTrace + "\r\n---ターゲット\r\n" + e.TargetSite + "\r\n---文字列\r\n" +
                e.ToString());
            }
        }

        static void ShowInvalidAndUsage(string option)
        {
            System.Console.WriteLine("コマンド \"" + option + "\" はありません。");
            ShowUsage();
        }

        static void ShowUsage()
        {
            string text = "使用方法:" + System.Environment.NewLine +
                "nrmc.exe 第1引数 第2引数 [-i input_rank_file] [-o output_rank_file] [-c config_file]" + System.Environment.NewLine +
                "  download ranking  ランキングをダウンロード" + System.Environment.NewLine +
                "  download video  動画をダウンロード" + System.Environment.NewLine +
                "  list searchtag  タグ検索" + System.Environment.NewLine +
                "  mylist add  マイリスト追加" + System.Environment.NewLine +
                "オプション:" + System.Environment.NewLine +
                "  -i input_rank_file  入力ランクファイルを指定。指定しない場合は標準入力から読み込み" + System.Environment.NewLine +
                "  -o output_rank_file  出力ランクファイルを指定。指定しない場合は標準出力に書き出し" + System.Environment.NewLine +
                "  -c config_file  設定ファイルを指定。デフォルトは nrmc.exe と同じ場所にある config.txt" + System.Environment.NewLine +
                "  （空白を含むファイルパスは引用符 \" \" で囲ってください。）";
            System.Console.WriteLine(text);
        }

        static void ParseConfig(string text)
        {
            string[] lines = IJStringUtil.SplitWithCRLF(text);

            for (int i = 0; i < lines.Length; ++i)
            {
                string[] ar = lines[i].Split('\t');
                if (ar[0] != "version")
                {
                    if (ar.Length >= 3)
                    {
                        option_[ar[1]] = ar[2];
                    }
                    else
                    {
                        option_[ar[0]] = ar[1];
                    }
                }
            }
        }

        static RankingMethod MakeRankingMethod()
        {
            HoseiKind kind;
            bool b = bool.Parse(option_["radioButtonHoseiVocaran"]);
            if (b)
            {
                kind = HoseiKind.Vocaran;
            }
            else
            {
                b = bool.Parse(option_["radioButtonHoseiNicoran"]);
                if (b)
                {
                    kind = HoseiKind.Nicoran;
                }
                else
                {
                    kind = HoseiKind.Nothing;
                }
            }

            SortKind skind;
            b = bool.Parse(option_["radioButtonSortPoint"]);
            if (b)
            {
                skind = SortKind.Point;
            }
            else
            {
                b = bool.Parse(option_["radioButtonSortMylist"]);
                if (b)
                {
                    skind = SortKind.Mylist;
                }
                else
                {
                    skind = SortKind.Nothing;
                }
            }

            bool is_using_filter = bool.Parse(option_["checkBoxFilter"]);

            if (is_using_filter && !string.IsNullOrEmpty(option_["textBoxFilterPath"])) // フィルターを使う場合
            {
                return new RankingMethod(kind, skind, int.Parse(option_["numericUpDownMylistRate"]),
                    true, option_["textBoxFilterPath"], bool.Parse(option_["checkBoxIsOutputFilteredVideo"]));
            }
            else
            {
                return new RankingMethod(kind, skind, int.Parse(option_["numericUpDownMylistRate"]));
            }
        }

        static SearchingTagOption MakeSearchingTagOption()
        {
            SearchingTagOption searching_tag_option = new SearchingTagOption();
            searching_tag_option.SetTagList(option_["textBoxTagNew"]);
            searching_tag_option.is_searching_kind_tag = bool.Parse(option_["radioButtonSearchKindTag"]);
            searching_tag_option.is_detail_getting = bool.Parse(option_["checkBoxIsGettingDetailNew"]);
            searching_tag_option.detail_info_lower = int.Parse(option_["numericUpDownConditionMylistNew"]);
            searching_tag_option.sort_kind_num = int.Parse(option_["listBoxSortNew"]);
            searching_tag_option.is_page_all = bool.Parse(option_["radioButtonTagSearchPageAll"]);
            searching_tag_option.page_start = IJStringUtil.ToNumberWithDef(option_["textBoxTagSearchPageStart"], 1);
            searching_tag_option.page_end = IJStringUtil.ToNumberWithDef(option_["textBoxTagSearchPageEnd"], int.MaxValue);
            searching_tag_option.is_using_condition = bool.Parse(option_["checkBoxTagSearchIsUsingCondition"]);
            searching_tag_option.condition_lower = IJStringUtil.ToNumberWithDef(option_["textBoxTagSearchLower"], 0);
            searching_tag_option.condition_upper = IJStringUtil.ToNumberWithDef(option_["textBoxTagSearchUpper"], int.MaxValue);
            searching_tag_option.date_from = NicoUtil.StringToDate(option_["dateTimePickerTagSearchFrom"]);
            searching_tag_option.date_to = NicoUtil.StringToDate(option_["dateTimePickerTagSearchTo"]);
            searching_tag_option.searching_interval = option_["textBoxTagSearchInterval"];
            searching_tag_option.getting_detail_interval = option_["textBoxGettingDetailInterval"];
            searching_tag_option.is_create_ticket = bool.Parse(option_["checkBoxSaveSearch"]);
            searching_tag_option.SetRedundantSearchMethod(int.Parse(option_["comboBoxRedundantSearchMethod"]));
            searching_tag_option.is_sending_user_session = bool.Parse(option_["checkBoxIsSendingUserSession"]);

            return searching_tag_option;
        }

        static void LoadCookie(NicoNetwork network)
        {
            if (bool.Parse(option_["radioButtonBrowserIE"]))
            {
                network.SetCookieKind(NicoNetwork.CookieKind.IE);
            }
            else if (bool.Parse(option_["radioButtonBrowserFirefox3"]))
            {
                network.SetCookieKind(NicoNetwork.CookieKind.Firefox3);
            }
            else if (bool.Parse(option_["radioButtonBrowserOpera"]))
            {
                network.SetCookieKind(NicoNetwork.CookieKind.Opera);
            }
            else if (bool.Parse(option_["radioButtonBrowserChrome"]))
            {
                network.SetCookieKind(NicoNetwork.CookieKind.Chrome);
            }
        }

        private static DownloadKind GetDownloadKind(CategoryManager category_manager)
        {
            DownloadKind download_kind = new DownloadKind();
            download_kind.SetDuration(bool.Parse(option_["checkBoxDlRankDurationTotal"]),
                bool.Parse(option_["checkBoxDlRankDurationMonthly"]),
                bool.Parse(option_["checkBoxDlRankDurationWeekly"]),
                bool.Parse(option_["checkBoxDlRankDurationDaily"]),
                bool.Parse(option_["checkBoxDlRankDurationHourly"]));
            download_kind.SetTarget(false, bool.Parse(option_["checkBoxDlRankView"]), // 「総合」には未対応
                bool.Parse(option_["checkBoxDlRankRes"]),
                bool.Parse(option_["checkBoxDlRankMylist"]));

            download_kind.CategoryList = category_manager.GetDownloadCategoryItemList();

            if (bool.Parse(option_["radioButtonDlRankRss"]))
            {
                download_kind.IsRss = true;
            }
            return download_kind;
        }

        private static string MakeDirectoryPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            if (!path.EndsWith("\\"))
            {
                return path + "\\";
            }
            else
            {
                return path;
            }
        }

        private static string Dequote(string text)
        {
            if (text.StartsWith("\"") && text.EndsWith("\""))
            {
                return text.Substring(1, text.Length - 2);
            }
            else
            {
                return text;
            }
        }
    }
}
