using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using IJLib;

namespace NicoTools
{
    /// <summary>
    /// 検索チケットを操作するクラス。
    /// </summary>
    public class SearchingTicketManager
    {
        // 検索チケットのベースディレクトリ
        private static string base_directory_;

        // オプションファイルのファイル名
        private static string option_file_name_;

        /// <summary>
        /// ベースディレクトリを search、オプションファイル名を option.txt に初期化する。
        /// </summary>
        static SearchingTicketManager()
        {
            base_directory_ = "search";
            option_file_name_ = "option.txt";
        }

        /// <summary>
        /// 新しい検索チケットを作成する。
        /// </summary>
        /// <param name="ticket_id">作成する検索チケットのID。</param>
        /// <param name="option">検索条件。</param>
        /// <returns>検索チケットを作成した場合はtrue、指定されたチケットIDを持つ検索チケットが既にある場合はfalse。</returns>
        public static bool CreateNewTicket(string ticket_id, SearchingTagOption option)
        {
            string ticket_directory = GetTicketDirectory(ticket_id);
            if (Directory.Exists(ticket_directory))
            {
                return false;
            }

            string option_file = GetTicketOptionFile(ticket_id);

            Directory.CreateDirectory(ticket_directory);
            string option_text = SerializeSearchingOption(option);
            IJFile.WriteUTF8(option_file, option_text);

            return true;
        }

        /// <summary>
        /// 検索チケットを削除する。
        /// </summary>
        /// <param name="ticket_id">削除する検索チケットのID。</param>
        /// <returns>検索チケットを削除した場合はtrue、指定されたチケットIDを持つ検索チケットが無い場合はfalse。</returns>
        public static bool DeleteTicket(string ticket_id)
        {
            string ticket_directory = GetTicketDirectory(ticket_id);

            if (Directory.Exists(ticket_directory))
            {
                Directory.Delete(ticket_directory, true);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ベースディレクトリ内の全ての検索チケットを削除する。
        /// </summary>
        public static void DeleteAllTickets()
        {
            foreach (string ticket_id in GetTicketIDs())
            {
                DeleteTicket(ticket_id);
            }
        }

        /// <summary>
        /// 検索チケットに保存されているダウンロード済みのデータを削除する。
        /// </summary>
        /// <param name="ticket_id">クリアする検索チケットのID。</param>
        /// <returns>検索チケットをクリアした場合はtrue、指定されたIDを持つ検索チケットが無い場合はfalse。</returns>
        public static bool ClearTicket(string ticket_id)
        {
            string ticket_directory = GetTicketDirectory(ticket_id);

            if (Directory.Exists(ticket_directory))
            {
                foreach (string file in Directory.GetFiles(ticket_directory))
                {
                    if (Path.GetFileName(file) != option_file_name_)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (IOException)
                        {
                        }
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// ベースディレクトリ内の全ての検索チケットをクリアする。
        /// </summary>
        public static void ClearAllTickets()
        {
            foreach (string ticket_id in GetTicketIDs())
            {
                ClearTicket(ticket_id);
            }
        }

        /// <summary>
        /// 検索チケットIDを変更する
        /// </summary>
        /// <param name="src_ticket_id">変更元検索チケットID。</param>
        /// <param name="dst_ticket_id">変更先検索チケットID。</param>
        public static void RenameTicket(string src_ticket_id, string dst_ticket_id)
        {
            if (string.Equals(src_ticket_id, dst_ticket_id, StringComparison.Ordinal))
            {
                return;
            }
            string src_ticket_directory = GetTicketDirectory(src_ticket_id);
            string dst_ticket_directory = GetTicketDirectory(dst_ticket_id);
            Directory.Move(src_ticket_directory, dst_ticket_directory);
        }

        /// <summary>
        /// ベースディレクトリ内に存在する全ての検索チケットのIDを列挙する。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetTicketIDs()
        {
            if (!Directory.Exists(base_directory_))
            {
                yield break;
            }
            string[] ticket_directories = Directory.GetDirectories(base_directory_);
            foreach (string ticket_directory in ticket_directories)
            {
                string ticket_id = Path.GetFileName(ticket_directory);
                string option_file = GetTicketOptionFile(ticket_id);

                if (!File.Exists(option_file))
                {
                    continue;
                }

                yield return ticket_id;
            }
        }

        /// <summary>
        /// ベースディレクトリ内に1つでも検索チケットがあるかどうかを返却する。
        /// </summary>
        /// <returns>ベースディレクトリ内に検索チケットが1つでもある場合はtrue、検索チケットが1つも無い場合はfalse。</returns>
        public static bool TicketsExists()
        {
            foreach (string ticket_id in GetTicketIDs())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 指定された検索チケットの検索条件を取得する。
        /// </summary>
        /// <param name="ticket_id">検索条件を取得する検索チケットのID。</param>
        /// <returns>指定された検索チケットの検索条件。</returns>
        public static SearchingTagOption GetOption(string ticket_id)
        {
            string ticket_option_file = GetTicketOptionFile(ticket_id);
            string option_text = IJFile.ReadUTF8(ticket_option_file);
            return DeserializeSearchingOption(ticket_id, option_text);
        }

        /// <summary>
        /// 検索条件を表す<see cref="SearchingTagOption"/>を、ファイル保存用に文字列に変換する。
        /// </summary>
        /// <param name="option">変換する検索条件。</param>
        /// <returns>ファイル保存用に文字列に変換された検索条件。</returns>
        public static string SerializeSearchingOption(SearchingTagOption option)
        {
            StringBuilder option_text = new StringBuilder();

            using (TextWriter writer = new StringWriter(option_text))
            {
                for (int i = 0; i < option.searching_tag_list.Count; i++)
                {
                    string searching_tag = option.searching_tag_list[i];
                    writer.WriteLine("searching_tag={0}", searching_tag);
                }

                writer.WriteLine("is_searching_kind_tag={0}", option.is_searching_kind_tag.ToString() ?? string.Empty);

                writer.WriteLine("is_detail_getting={0}", option.is_detail_getting.ToString());
                writer.WriteLine("detail_info_lower={0}", option.detail_info_lower.ToString());

                writer.WriteLine("sort_kind_num={0}", option.sort_kind_num.ToString());
                writer.WriteLine("is_page_all={0}", option.is_page_all.ToString());

                writer.WriteLine("page_start={0}", option.page_start.ToString());
                writer.WriteLine("page_end={0}", option.page_end.ToString());

                writer.WriteLine("is_using_condition={0}", option.is_using_condition.ToString());

                writer.WriteLine("date_from={0},{1}", option.date_from.Ticks.ToString(), option.date_from.Kind.ToString());
                writer.WriteLine("date_to={0},{1}", option.date_to.Ticks.ToString(), option.date_to.Kind.ToString());

                writer.WriteLine("condition_lower={0}", option.condition_lower.ToString());
                writer.WriteLine("condition_upper={0}", option.condition_upper.ToString());

                writer.WriteLine("searching_interval={0}", option.searching_interval ?? string.Empty);
                writer.WriteLine("getting_detail_interval={0}", option.getting_detail_interval ?? string.Empty);

                writer.WriteLine("redundant_seatching_method={0}", option.redundant_seatching_method.ToString());

                writer.WriteLine("save_html_dir={0}", option.save_html_dir);
                writer.WriteLine("is_sending_user_session={0}", option.is_sending_user_session);
            }

            return option_text.ToString();
        }

        /// <summary>
        /// 検索条件を表す文字列を<see cref="SearchingTagOption"/>に変換する。
        /// </summary>
        /// <param name="ticket_id">検索条件に設定する検索チケットID。</param>
        /// <param name="option_text">検索条件を表す文字列。</param>
        /// <returns>検索条件。</returns>
        public static SearchingTagOption DeserializeSearchingOption(string ticket_id, string option_text)
        {
            SearchingTagOption option = new SearchingTagOption();
            option.searching_tag_list = new List<string>();

            option.ticket_id = ticket_id;
            option.is_create_ticket = false;

            using (TextReader reader = new StringReader(option_text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Match match = Regex.Match(line, "^(?<name>[^=]+)=(?<value>.+)$");
                    if (!match.Success)
                    {
                        continue;
                    }

                    string name = match.Groups["name"].Value;
                    string value = match.Groups["value"].Value;

                    try
                    {
                        switch (name)
                        {
                            case "searching_tag":
                                option.searching_tag_list.Add(value);
                                break;
                            case "is_searching_kind_tag":
                                option.is_searching_kind_tag = bool.Parse(value);
                                break;
                            case "is_detail_getting":
                                option.is_detail_getting = bool.Parse(value);
                                break;
                            case "detail_info_lower":
                                option.detail_info_lower = int.Parse(value);
                                break;
                            case "sort_kind_num":
                                option.sort_kind_num = int.Parse(value);
                                break;
                            case "is_page_all":
                                option.is_page_all = bool.Parse(value);
                                break;
                            case "page_start":
                                option.page_start = int.Parse(value);
                                break;
                            case "page_end":
                                option.page_end = int.Parse(value);
                                break;
                            case "is_using_condition":
                                option.is_using_condition = bool.Parse(value);
                                break;
                            case "date_from":
                                string[] ticks_kind_from = value.Split(',');
                                if (ticks_kind_from.Length < 2)
                                {
                                    throw new FormatException();
                                }
                                long ticks_from = long.Parse(ticks_kind_from[0]);
                                DateTimeKind kind_from = (DateTimeKind)Enum.Parse(typeof(DateTimeKind), ticks_kind_from[1]);
                                option.date_from = new DateTime(ticks_from, kind_from);
                                break;
                            case "date_to":
                                string[] ticks_kind_to = value.Split(',');
                                if (ticks_kind_to.Length < 2)
                                {
                                    throw new FormatException();
                                }
                                long ticks_to = long.Parse(ticks_kind_to[0]);
                                DateTimeKind kind_to = (DateTimeKind)Enum.Parse(typeof(DateTimeKind), ticks_kind_to[1]);
                                option.date_to = new DateTime(ticks_to, kind_to);
                                break;
                            case "condition_lower":
                                option.condition_lower = int.Parse(value);
                                break;
                            case "condition_upper":
                                option.condition_upper = int.Parse(value);
                                break;
                            case "searching_interval":
                                option.searching_interval = value;
                                break;
                            case "getting_detail_interval":
                                option.getting_detail_interval = value;
                                break;
                            case "redundant_seatching_method":
                                option.redundant_seatching_method = (RedundantSearchingMethod)Enum.Parse(typeof(RedundantSearchingMethod), value);
                                break;
                            case "save_html_dir":
                                option.save_html_dir = value;
                                break;
                            case "is_sending_user_session":
                                option.is_sending_user_session = bool.Parse(value);
                                break;
                            default:
                                break;
                        }
                    }
                    catch (OverflowException e)
                    {
                        throw new FormatException(null, e);
                    }
                    catch (ArgumentException e)
                    {
                        throw new FormatException(null, e);
                    }
                }
            }

            return option;
        }

        /// <summary>
        /// 指定されたチケットIDの検索チケットのディレクトリを返却する。
        /// </summary>
        /// <param name="ticket_id">検索チケットID。</param>
        /// <returns>検索チケットのディレクトリパス。</returns>
        public static string GetTicketDirectory(string ticket_id)
        {
            return Path.Combine(base_directory_, ticket_id);
        }

        /// <summary>
        /// 指定されたチケットIDの検索チケットのオプションファイルを返却する。
        /// </summary>
        /// <param name="ticket_id">検索チケットID。</param>
        /// <returns>検索チケットのオプションファイルパス。</returns>
        public static string GetTicketOptionFile(string ticket_id)
        {
            string ticket_directory = GetTicketDirectory(ticket_id);
            return Path.Combine(ticket_directory, option_file_name_);
        }

        /// <summary>
        /// ダウンロードしたファイルを保存するファイルパスを返却する。
        /// </summary>
        /// <param name="ticket_id">検索チケットID。</param>
        /// <param name="redundant_search_count">冗長検索の回数。</param>
        /// <param name="page">ページ数。</param>
        /// <returns>ダウンロードしたファイルを保存するファイルパス。</returns>
        public static string GetPageDownloadPath(string ticket_id, int redundant_search_count, int page)
        {
            string ticket_directory = GetTicketDirectory(ticket_id);
            string file_name = string.Format("{0}_{1,3:d3}", redundant_search_count, page);
            return Path.Combine(ticket_directory, file_name);
        }

        /// <summary>
        /// 検索チケットのベースディレクトリ。
        /// </summary>
        public static string BaseDirectory
        {
            get
            {
                return base_directory_;
            }

            set
            {
                base_directory_ = value;
            }
        }

        /// <summary>
        /// オプションファイルのファイル名。
        /// </summary>
        public static string OptionFileName
        {
            get
            {
                return option_file_name_;
            }

            set
            {
                option_file_name_ = value;
            }
        }
    }
}
