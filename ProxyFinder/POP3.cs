using System;
using System.Net.Sockets;
using System.Net;
using System.Security.Cryptography;
using System.IO;
using System.Data; 
using System.Text;

namespace ProxyFinder
{
    class POP3
    {
        private string m_Address = "127.0.0.1";

        private int m_Port = 110;

        /// <summary>
        /// 超时定时器
        /// </summary>
        private System.Timers.Timer m_TimeoutTimer = null;

        /// <summary>
        /// 超时定时器判断用的client
        /// </summary>
        private POP3Client m_Client = null;

        public POP3(string p_Address, int p_Port)
        {
            m_Address = p_Address;
            m_Port = p_Port;
        }

        /// <summary> 
        /// 获取Mail列表 
        /// </summary> 
        /// <param name="p_Name">用户名</param> 
        /// <param name="p_PassWord">密码</param> 
        /// <returns>Mail信息</returns> 
        public DataTable GetMailTable(string p_Name, string p_PassWord)
        {
            POP3Client _Client = new POP3Client();
            _Client.UserName = p_Name;
            _Client.PassWord = p_PassWord;
            _Client.Client = new TcpClient();
            initTimeoutTimer(_Client);
            _Client.Client.BeginConnect(m_Address, m_Port, new AsyncCallback(OnConnectRequest), _Client);
            while (!_Client.ReturnEnd)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            if (_Client.Error.Length != 0) throw new Exception("错误信息!" + _Client.Error);
            stopTimeoutTimer();
            return _Client.MailDataTable;
        }

        /// <summary>
        /// 获取Mail数量
        /// </summary>
        /// <param name="p_Name"></param>
        /// <param name="p_PassWord"></param>
        /// <returns></returns>
        public int GetMailCount(string p_Name, string p_PassWord)
        {
            POP3Client _Client = new POP3Client();
            _Client.UserName = p_Name;
            _Client.PassWord = p_PassWord;
            _Client.Client = new TcpClient();
            _Client.isReadCount = true;
            initTimeoutTimer(_Client);
            _Client.Client.BeginConnect(m_Address, m_Port, new AsyncCallback(OnConnectRequest), _Client);
            while (!_Client.ReturnEnd)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            if (_Client.Error.Length != 0) throw new Exception("错误信息!" + _Client.Error);
            stopTimeoutTimer();
            return _Client.MailDataTable.Rows.Count;
        }


        /// <summary> 
        /// 获取邮件内容 
        /// </summary> 
        /// <param name="p_Name">名称</param> 
        /// <param name="p_PassWord">密码</param> 
        /// <param name="p_MailIndex">邮件编号</param> 
        /// <returns>数据集</returns> 
        public DataTable GetMail(string p_Name, string p_PassWord, int p_MailIndex)
        {
            POP3Client _Client = new POP3Client();
            _Client.UserName = p_Name;
            _Client.PassWord = p_PassWord;
            _Client.Client = new TcpClient();
            _Client.ReadIndex = p_MailIndex;
            initTimeoutTimer(_Client);
            _Client.Client.BeginConnect(m_Address, m_Port, new AsyncCallback(OnConnectRequest), _Client);
            while (!_Client.ReturnEnd)
            {
                System.Windows.Forms.Application.DoEvents();
            }
            if (_Client.Error.Length != 0) throw new Exception("错误信息!" + _Client.Error);
            stopTimeoutTimer();
            return _Client.MailTable;
        }

        /// <summary>
        /// 初始化超时计时器，因为socket的异步处理无法设定SendTimeout、ReceiveTimeout来启用超时机制
        /// </summary>
        /// <param name="client"></param>
        private void initTimeoutTimer(POP3Client client)
        {
            m_Client = client;
            m_TimeoutTimer = new System.Timers.Timer();
            m_TimeoutTimer.Interval = 60000;    //超时一分钟
            m_TimeoutTimer.Elapsed += new System.Timers.ElapsedEventHandler(TimeoutTimer_Elapsed);
            m_TimeoutTimer.Enabled = true;
            m_TimeoutTimer.AutoReset = false;
        }

        private void stopTimeoutTimer()
        {
            m_Client = null;
            if (m_TimeoutTimer != null)
            {
                m_TimeoutTimer.Stop();
                m_TimeoutTimer.Close();
            }
        }

        /// <summary>
        /// 超时后执行的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimeoutTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (m_Client.Client != null)
            {
                m_Client.ReturnEnd = true;
                m_Client.ReadEnd = true;
                m_Client.Error = "timeout";
                m_Client.Client.Close();
                m_Client = null;
            }
        }

        private class POP3Client
        {
            public TcpClient Client;

            public string UserName = "";

            public string PassWord = "";

            public bool ReturnEnd = false;

            public DataTable MailDataTable = new DataTable();

            public DataTable MailTable = new DataTable();

            public string Error = "";

            public bool ReadEnd = false;

            public int ReadIndex = -1;

            public bool isReadCount = false;


            public POP3Client()
            {
                MailDataTable.Columns.Add("NUM");
                MailDataTable.Columns.Add("Size");
                MailDataTable.Columns.Add("Form");
                MailDataTable.Columns.Add("To");
                MailDataTable.Columns.Add("Subject");
                MailDataTable.Columns.Add("Date");

                MailTable.Columns.Add("Type", typeof(string));
                MailTable.Columns.Add("Text", typeof(object));
                MailTable.Columns.Add("Name", typeof(string));
            }

            private int m_SendMessage = 0;
            private int m_TOPIndex = 1;

            /// <summary> 
            /// 获取下一个登陆到获取列表需要的命令 
            /// </summary> 
            /// <param name="p_Value"></param> 
            /// <returns></returns> 
            public byte[] GetSendBytes(byte[] p_Value)
            {
                ReadEnd = false;
                string _Value = System.Text.Encoding.Default.GetString(p_Value).Replace("\0", "");
                if (_Value.IndexOf("+OK") == 0)
                {
                    m_SendMessage++;
                    switch (m_SendMessage)
                    {
                        case 1:
                            return System.Text.Encoding.ASCII.GetBytes("USER " + UserName + "\r\n");
                        case 2:
                            return System.Text.Encoding.ASCII.GetBytes("PASS " + PassWord + "\r\n");
                        case 3:
                            ReadEnd = true;
                            if (ReadIndex != -1)
                            {
                                m_SendMessage = 5;
                                return System.Text.Encoding.ASCII.GetBytes("RETR " + ReadIndex.ToString() + "\r\n");
                            }
                            else
                            {
                                return System.Text.Encoding.ASCII.GetBytes("LIST\r\n");
                            }
                        case 4:
                            string[] _List = _Value.Split(new char[] { '\r', '\n', '.' }, StringSplitOptions.RemoveEmptyEntries);
                            for (int i = 1; i != _List.Length; i++)
                            {
                                string[] _MaliSize = _List[i].Split(' ');
                                MailDataTable.Rows.Add(new object[] { _MaliSize[0], _MaliSize[1] });
                            }
                            if (isReadCount)
                            {
                                //如果只是读取数量，到这里就结束了
                                ReturnEnd = true;
                                return System.Text.Encoding.ASCII.GetBytes("QUIT");
                            }
                            if (MailDataTable.Rows.Count == 0)
                            {
                                ReturnEnd = true;
                                return new byte[0];
                            }
                            else
                            {
                                ReadEnd = true;
                                m_TOPIndex = 1;
                                return System.Text.Encoding.ASCII.GetBytes("TOP 1 1\r\n");
                            }
                        case 5:
                            System.Text.RegularExpressions.Regex _Regex = new System.Text.RegularExpressions.Regex(@"(?<=Date: ).*?(\r\n)+");
                            System.Text.RegularExpressions.MatchCollection _Collection = _Regex.Matches(_Value);
                            if (_Collection.Count != 0) MailDataTable.Rows[m_TOPIndex - 1]["Date"] = GetReadText(_Collection[0].Value);

                            System.Text.RegularExpressions.Regex _RegexFrom = new System.Text.RegularExpressions.Regex(@"(?<=From: ).*?(\r\n)+");
                            System.Text.RegularExpressions.MatchCollection _CollectionForm = _RegexFrom.Matches(_Value);
                            if (_CollectionForm.Count != 0) MailDataTable.Rows[m_TOPIndex - 1]["Form"] = GetReadText(_CollectionForm[0].Value);


                            System.Text.RegularExpressions.Regex _RegexTo = new System.Text.RegularExpressions.Regex(@"(?<=To: ).*?(\r\n)+");
                            System.Text.RegularExpressions.MatchCollection _CollectionTo = _RegexTo.Matches(_Value);
                            if (_CollectionTo.Count != 0) MailDataTable.Rows[m_TOPIndex - 1]["To"] = GetReadText(_CollectionTo[0].Value);

                            System.Text.RegularExpressions.Regex _RegexSubject = new System.Text.RegularExpressions.Regex(@"(?<=Subject: ).*?(\r\n)+");
                            System.Text.RegularExpressions.MatchCollection _CollectionSubject = _RegexSubject.Matches(_Value);
                            if (_CollectionSubject.Count != 0) MailDataTable.Rows[m_TOPIndex - 1]["Subject"] = GetReadText(_CollectionSubject[0].Value);

                            m_TOPIndex++;
                            m_SendMessage--;
                            ReadEnd = true;
                            if (m_TOPIndex > MailDataTable.Rows.Count)
                            {
                                ReturnEnd = true;
                                return System.Text.Encoding.ASCII.GetBytes("QUIT");
                            }
                            else
                            {
                                return System.Text.Encoding.ASCII.GetBytes("TOP " + m_TOPIndex.ToString() + " 0\r\n");
                            }
                        case 6:
                            string from = GetTextType(_Value, "From:", ">");
                            MailTable.Rows.Add(new object[] { "From", from });
                            GetMailText(_Value);
                            ReturnEnd = true;
                            return System.Text.Encoding.ASCII.GetBytes("QUIT");

                    }
                }
                Error = _Value;
                ReturnEnd = true;
                return new byte[0];
            }

            /// <summary> 
            /// 转换文字里的字符集 
            /// </summary> 
            /// <param name="p_Text"></param> 
            /// <returns></returns> 
            public string GetReadText(string p_Text)
            {
                System.Text.RegularExpressions.Regex _Regex = new System.Text.RegularExpressions.Regex(@"(?<=\=\?).*?(\?\=)+");
                System.Text.RegularExpressions.MatchCollection _Collection = _Regex.Matches(p_Text);
                string _Text = p_Text;
                foreach (System.Text.RegularExpressions.Match _Match in _Collection)
                {
                    string _Value = "=?" + _Match.Value;
                    if (_Value[0] == '=')
                    {
                        string[] _BaseData = _Value.Split('?');
                        if (_BaseData.Length == 5)
                        {
                            System.Text.Encoding _Coding = System.Text.Encoding.GetEncoding(_BaseData[1]);
                            if (_BaseData[2] == "B")
                            {
                                _Text = _Text.Replace(_Value, DecodeBase64(_BaseData[3], _Coding));
                            }
                            else
                            {
                                _Text = _Text.Replace(_Value, DecodeQuotedPrintable(_BaseData[3], _Coding));
                            }
                        }
                    }
                    else
                    {
                    }
                }
                return _Text;
            }


            #region 获取邮件正文 和 附件
            /// <summary> 
            /// 获取文字主体 
            /// </summary> 
            /// <param name="p_Mail"></param> 
            /// <returns></returns> 
            public void GetMailText(string p_Mail)
            {
                string _ConvertType = GetTextType(p_Mail, "\r\nContent-Type: ", ";");
                if (_ConvertType.Length == 0)
                {
                    _ConvertType = GetTextType(p_Mail, "\r\nContent-Type: ", "\r");
                }
                string from = GetTextType(p_Mail, "From:", ">");
                if (from.Length != 0)
                {
                     
                }
                int _StarIndex = -1;
                int _EndIndex = -1;
                string _ReturnText = "";
                string _Transfer = "";
                string _Boundary = "";
                string _EncodingName = GetTextType(p_Mail, "charset=\"", "\"").Replace("\"", "");
                System.Text.Encoding _Encoding = System.Text.Encoding.Default;
                if (_EncodingName != "") _Encoding = System.Text.Encoding.GetEncoding(_EncodingName);
                switch (_ConvertType)
                {
                    case "text/html;":
                        _Transfer = GetTextType(p_Mail, "\r\nContent-Transfer-Encoding: ", "\r\n").Trim();
                        _StarIndex = p_Mail.IndexOf("\r\n\r\n");
                        if (_StarIndex != -1) _ReturnText = p_Mail.Substring(_StarIndex, p_Mail.Length - _StarIndex);
                        switch (_Transfer)
                        {
                            case "8bit":

                                break;
                            case "quoted-printable":
                                _ReturnText = DecodeQuotedPrintable(_ReturnText, _Encoding);
                                break;
                            case "base64":
                                _ReturnText = DecodeBase64(_ReturnText, _Encoding);
                                break;
                        }
                        MailTable.Rows.Add(new object[] { "text/html", _ReturnText });
                        break;
                    case "text/plain;":
                        _Transfer = GetTextType(p_Mail, "\r\nContent-Transfer-Encoding: ", "\r\n").Trim();
                        _StarIndex = p_Mail.IndexOf("\r\n\r\n");
                        if (_StarIndex != -1) _ReturnText = p_Mail.Substring(_StarIndex, p_Mail.Length - _StarIndex);
                        switch (_Transfer)
                        {
                            case "8bit":

                                break;
                            case "quoted-printable":
                                _ReturnText = DecodeQuotedPrintable(_ReturnText, _Encoding);
                                break;
                            case "base64":
                                _ReturnText = DecodeBase64(_ReturnText, _Encoding);
                                break;
                        }
                        MailTable.Rows.Add(new object[] { "text/plain", _ReturnText });
                        break;
                    case "multipart/alternative;":
                        _Boundary = GetTextType(p_Mail, "boundary=\"", "\"").Replace("\"", "");
                        _StarIndex = p_Mail.IndexOf("--" + _Boundary + "\r\n");
                        if (_StarIndex == -1) return;
                        while (true)
                        {
                            _EndIndex = p_Mail.IndexOf("--" + _Boundary, _StarIndex + _Boundary.Length);
                            if (_EndIndex == -1) break;
                            GetMailText(p_Mail.Substring(_StarIndex, _EndIndex - _StarIndex));
                            _StarIndex = _EndIndex;
                        }
                        break;
                    case "multipart/mixed;":
                        _Boundary = GetTextType(p_Mail, "boundary=\"", "\"").Replace("\"", "");
                        _StarIndex = p_Mail.IndexOf("--" + _Boundary + "\r\n");
                        if (_StarIndex == -1) return;
                        while (true)
                        {
                            _EndIndex = p_Mail.IndexOf("--" + _Boundary, _StarIndex + _Boundary.Length);
                            if (_EndIndex == -1) break;
                            GetMailText(p_Mail.Substring(_StarIndex, _EndIndex - _StarIndex));
                            _StarIndex = _EndIndex;
                        }
                        break;
                    default:
                        if (_ConvertType.IndexOf("application/") == 0)
                        {
                            _StarIndex = p_Mail.IndexOf("\r\n\r\n");
                            if (_StarIndex != -1) _ReturnText = p_Mail.Substring(_StarIndex, p_Mail.Length - _StarIndex);
                            _Transfer = GetTextType(p_Mail, "\r\nContent-Transfer-Encoding: ", "\r\n").Trim();
                            string _Name = GetTextType(p_Mail, "filename=\"", "\"").Replace("\"", "");
                            _Name = GetReadText(_Name);
                            byte[] _FileBytes = new byte[0];
                            switch (_Transfer)
                            {
                                case "base64":
                                    _FileBytes = Convert.FromBase64String(_ReturnText);
                                    break;
                            }
                            MailTable.Rows.Add(new object[] { "application/octet-stream", _FileBytes, _Name });

                        }
                        break;
                }
            }

            /// <summary> 
            /// 获取类型（正则） 
            /// </summary> 
            /// <param name="p_Mail">原始文字</param> 
            /// <param name="p_TypeText">前文字</param> 
            /// <param name="p_End">结束文字</param> 
            /// <returns>符合的记录</returns> 
            public string GetTextType(string p_Mail, string p_TypeText, string p_End)
            {
                System.Text.RegularExpressions.Regex _Regex = new System.Text.RegularExpressions.Regex(@"(?<=" + p_TypeText + ").*?(" + p_End + ")+");
                System.Text.RegularExpressions.MatchCollection _Collection = _Regex.Matches(p_Mail);
                if (_Collection.Count == 0) return "";
                return _Collection[0].Value;
            }

            /// <summary> 
            /// QuotedPrintable编码接码 
            /// </summary> 
            /// <param name="p_Text">原始文字</param> 
            /// <param name="p_Encoding">编码方式</param> 
            /// <returns>接码后信息</returns> 
            public string DecodeQuotedPrintable(string p_Text, System.Text.Encoding p_Encoding)
            {
                System.IO.MemoryStream _Stream = new System.IO.MemoryStream();
                char[] _CharValue = p_Text.ToCharArray();
                for (int i = 0; i != _CharValue.Length; i++)
                {
                    switch (_CharValue[i])
                    {
                        case '=':
                            if (_CharValue[i + 1] == '\r' || _CharValue[i + 1] == '\n')
                            {
                                i += 2;
                            }
                            else
                            {
                                try
                                {
                                    _Stream.WriteByte(Convert.ToByte(_CharValue[i + 1].ToString() + _CharValue[i + 2].ToString(), 16));
                                    i += 2;
                                }
                                catch
                                {
                                    _Stream.WriteByte(Convert.ToByte(_CharValue[i]));
                                }
                            }
                            break;
                        default:
                            _Stream.WriteByte(Convert.ToByte(_CharValue[i]));
                            break;
                    }
                }
                return p_Encoding.GetString(_Stream.ToArray());
            }

            /// <summary> 
            /// 解码BASE64 
            /// </summary> 
            /// <param name="p_Text"></param> 
            /// <param name="p_Encoding"></param> 
            /// <returns></returns> 
            public string DecodeBase64(string p_Text, System.Text.Encoding p_Encoding)
            {
                if (p_Text.Trim().Length == 0) return "";
                p_Text = p_Text.Replace("\r\n", "");
                p_Text = p_Text.Replace(".", "");
                byte[] _ValueBytes = Convert.FromBase64String(p_Text);
                return p_Encoding.GetString(_ValueBytes);
            }
            #endregion


        }

        /// <summary> 
        /// 连接事件 
        /// </summary> 
        /// <param name="ar"></param> 
        private void OnConnectRequest(IAsyncResult ar)
        {
            POP3Client _Client = (POP3Client)ar.AsyncState;
            byte[] _ReadBytes = new byte[0];
            _Client.Client.Client.BeginReceive(_ReadBytes, 0, 0, SocketFlags.None, new AsyncCallback(OnWrite), _Client);
        }

        /// <summary> 
        /// 连接事件 
        /// </summary> 
        /// <param name="ar"></param> 
        private void OnSend(IAsyncResult ar)
        {
            POP3Client _Client = (POP3Client)ar.AsyncState;
            byte[] _ReadBytes = new byte[0];
            _Client.Client.Client.BeginReceive(_ReadBytes, 0, 0, SocketFlags.None, new AsyncCallback(OnWrite), _Client);
        }

        /// <summary> 
        /// 连接事件 
        /// </summary> 
        /// <param name="ar"></param> 
        private void OnWrite(IAsyncResult ar)
        {
            try
            {
                POP3Client _Client = (POP3Client)ar.AsyncState;
                byte[] _WriteBytes = new byte[_Client.Client.Client.ReceiveBufferSize];
                _Client.Client.Client.Receive(_WriteBytes);
                if (_Client.ReadEnd) _WriteBytes = ReadEnd(_WriteBytes, _Client);
                byte[] _SendBytes = _Client.GetSendBytes(_WriteBytes);
                if (_SendBytes.Length == 0) return;
                _Client.Client.Client.BeginSend(_SendBytes, 0, _SendBytes.Length, SocketFlags.None, new AsyncCallback(OnSend), _Client);
            }
            catch (System.Exception ex)
            {
                return;
            }
        }

        /// <summary> 
        /// 获取知道获取到. 否则一直获取数据 
        /// </summary> 
        /// <param name="p_Value"></param> 
        /// <returns></returns> 
        private byte[] ReadEnd(byte[] p_Value, POP3Client p_Client)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(System.Text.Encoding.ASCII.GetString(p_Value).TrimEnd('\0'));
            //String tmp = System.Text.Encoding.ASCII.GetString(p_Value);
            if (sb.ToString().IndexOf("\r\n.\r\n") != -1) return p_Value;
            //MemoryStream _Stream = new MemoryStream();
            //_Stream.Write(p_Value, 0, p_Value.Length);
            while (true)
            {
                try
                {
                    byte[] _WriteBytes = new byte[p_Client.Client.ReceiveBufferSize];
                    p_Client.Client.Client.Receive(_WriteBytes);
                    //_Stream.Write(_WriteBytes, 0, _WriteBytes.Length);
                    System.Threading.Thread.Sleep(100);
                    sb.Append(System.Text.Encoding.ASCII.GetString(_WriteBytes).TrimEnd('\0'));
                    if (sb.ToString().IndexOf("\r\n.\r\n") != -1) return System.Text.Encoding.ASCII.GetBytes(sb.ToString());
                }
                catch (System.Exception ex)
                {
                    return new byte[0];
                }
                
            }
        }
    }
}
