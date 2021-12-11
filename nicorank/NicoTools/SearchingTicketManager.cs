using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using IJLib;

namespace NicoTools
{
    /// <summary>
    /// �����`�P�b�g�𑀍삷��N���X�B
    /// </summary>
    public class SearchingTicketManager
    {
        // �����`�P�b�g�̃x�[�X�f�B���N�g��
        private static string base_directory_;

        // �I�v�V�����t�@�C���̃t�@�C����
        private static string option_file_name_;

        /// <summary>
        /// �x�[�X�f�B���N�g���� search�A�I�v�V�����t�@�C������ option.txt �ɏ���������B
        /// </summary>
        static SearchingTicketManager()
        {
            base_directory_ = "search";
            option_file_name_ = "option.txt";
        }

        /// <summary>
        /// �V���������`�P�b�g���쐬����B
        /// </summary>
        /// <param name="ticket_id">�쐬���錟���`�P�b�g��ID�B</param>
        /// <param name="option">���������B</param>
        /// <returns>�����`�P�b�g���쐬�����ꍇ��true�A�w�肳�ꂽ�`�P�b�gID���������`�P�b�g�����ɂ���ꍇ��false�B</returns>
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
        /// �����`�P�b�g���폜����B
        /// </summary>
        /// <param name="ticket_id">�폜���錟���`�P�b�g��ID�B</param>
        /// <returns>�����`�P�b�g���폜�����ꍇ��true�A�w�肳�ꂽ�`�P�b�gID���������`�P�b�g�������ꍇ��false�B</returns>
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
        /// �x�[�X�f�B���N�g�����̑S�Ă̌����`�P�b�g���폜����B
        /// </summary>
        public static void DeleteAllTickets()
        {
            foreach (string ticket_id in GetTicketIDs())
            {
                DeleteTicket(ticket_id);
            }
        }

        /// <summary>
        /// �����`�P�b�g�ɕۑ�����Ă���_�E�����[�h�ς݂̃f�[�^���폜����B
        /// </summary>
        /// <param name="ticket_id">�N���A���錟���`�P�b�g��ID�B</param>
        /// <returns>�����`�P�b�g���N���A�����ꍇ��true�A�w�肳�ꂽID���������`�P�b�g�������ꍇ��false�B</returns>
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
        /// �x�[�X�f�B���N�g�����̑S�Ă̌����`�P�b�g���N���A����B
        /// </summary>
        public static void ClearAllTickets()
        {
            foreach (string ticket_id in GetTicketIDs())
            {
                ClearTicket(ticket_id);
            }
        }

        /// <summary>
        /// �����`�P�b�gID��ύX����
        /// </summary>
        /// <param name="src_ticket_id">�ύX�������`�P�b�gID�B</param>
        /// <param name="dst_ticket_id">�ύX�挟���`�P�b�gID�B</param>
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
        /// �x�[�X�f�B���N�g�����ɑ��݂���S�Ă̌����`�P�b�g��ID��񋓂���B
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
        /// �x�[�X�f�B���N�g������1�ł������`�P�b�g�����邩�ǂ�����ԋp����B
        /// </summary>
        /// <returns>�x�[�X�f�B���N�g�����Ɍ����`�P�b�g��1�ł�����ꍇ��true�A�����`�P�b�g��1�������ꍇ��false�B</returns>
        public static bool TicketsExists()
        {
            foreach (string ticket_id in GetTicketIDs())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// �w�肳�ꂽ�����`�P�b�g�̌����������擾����B
        /// </summary>
        /// <param name="ticket_id">�����������擾���錟���`�P�b�g��ID�B</param>
        /// <returns>�w�肳�ꂽ�����`�P�b�g�̌��������B</returns>
        public static SearchingTagOption GetOption(string ticket_id)
        {
            string ticket_option_file = GetTicketOptionFile(ticket_id);
            string option_text = IJFile.ReadUTF8(ticket_option_file);
            return DeserializeSearchingOption(ticket_id, option_text);
        }

        /// <summary>
        /// ����������\��<see cref="SearchingTagOption"/>���A�t�@�C���ۑ��p�ɕ�����ɕϊ�����B
        /// </summary>
        /// <param name="option">�ϊ����錟�������B</param>
        /// <returns>�t�@�C���ۑ��p�ɕ�����ɕϊ����ꂽ���������B</returns>
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
        /// ����������\���������<see cref="SearchingTagOption"/>�ɕϊ�����B
        /// </summary>
        /// <param name="ticket_id">���������ɐݒ肷�錟���`�P�b�gID�B</param>
        /// <param name="option_text">����������\��������B</param>
        /// <returns>���������B</returns>
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
        /// �w�肳�ꂽ�`�P�b�gID�̌����`�P�b�g�̃f�B���N�g����ԋp����B
        /// </summary>
        /// <param name="ticket_id">�����`�P�b�gID�B</param>
        /// <returns>�����`�P�b�g�̃f�B���N�g���p�X�B</returns>
        public static string GetTicketDirectory(string ticket_id)
        {
            return Path.Combine(base_directory_, ticket_id);
        }

        /// <summary>
        /// �w�肳�ꂽ�`�P�b�gID�̌����`�P�b�g�̃I�v�V�����t�@�C����ԋp����B
        /// </summary>
        /// <param name="ticket_id">�����`�P�b�gID�B</param>
        /// <returns>�����`�P�b�g�̃I�v�V�����t�@�C���p�X�B</returns>
        public static string GetTicketOptionFile(string ticket_id)
        {
            string ticket_directory = GetTicketDirectory(ticket_id);
            return Path.Combine(ticket_directory, option_file_name_);
        }

        /// <summary>
        /// �_�E�����[�h�����t�@�C����ۑ�����t�@�C���p�X��ԋp����B
        /// </summary>
        /// <param name="ticket_id">�����`�P�b�gID�B</param>
        /// <param name="redundant_search_count">�璷�����̉񐔁B</param>
        /// <param name="page">�y�[�W���B</param>
        /// <returns>�_�E�����[�h�����t�@�C����ۑ�����t�@�C���p�X�B</returns>
        public static string GetPageDownloadPath(string ticket_id, int redundant_search_count, int page)
        {
            string ticket_directory = GetTicketDirectory(ticket_id);
            string file_name = string.Format("{0}_{1,3:d3}", redundant_search_count, page);
            return Path.Combine(ticket_directory, file_name);
        }

        /// <summary>
        /// �����`�P�b�g�̃x�[�X�f�B���N�g���B
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
        /// �I�v�V�����t�@�C���̃t�@�C�����B
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
