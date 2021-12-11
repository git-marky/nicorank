using System;
using System.Collections.Generic;
using System.Text;
using IJLib;

namespace nicoranktest
{
    public class TestMessageOut : MessageOut
    {
        private Action<string> on_write_;
        private Action<string> on_write_line_;

        public Action<string> OnWrite
        {
            get { return on_write_; }
            set { on_write_ = value; }
        }

        public Action<string> OnWriteLine
        {
            get { return on_write_line_; }
            set { on_write_line_ = value; }
        }

        #region MessageOut Members

        public void Write(string text)
        {
            if (on_write_ != null)
            {
                on_write_(text);
            }
            Console.Write(text);
        }

        public void WriteLine(string text)
        {
            if (on_write_line_ != null)
            {
                on_write_line_(text);
            }
            Console.WriteLine(text);
        }

        #endregion
    }
}
