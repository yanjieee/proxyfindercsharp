using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Net.Sockets;

namespace ProxyFinder
{
    class HttpTools
    {
        private static int DEFAULT_TIMEOUT = 40;

        public String RunJs(String code)
        {
            MSScriptControl.ScriptControlClass sc = new MSScriptControl.ScriptControlClass();
            sc.Language = "javascript";
            object obj = sc.Eval(code);
            return obj.ToString();
        }

        public String GetIpFromStr(String src)
        {
            string pattern = @"\w{1,3}\.\w{1,3}\.\w{1,3}\.\w{1,3}";
            Match match = Regex.Match(src, pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            return match.Value;
        }

        public String GetMid(String input, String s, String e)
        {
            int pos = input.IndexOf(s);
            if (pos == -1)
            {
                return "";
            }

            pos += s.Length;

            int pos_end = 0;
            if (e == "")
            {
                pos_end = input.Length;
            }
            else
            {
                pos_end = input.IndexOf(e, pos);
            }

            if (pos_end == -1)
            {
                return "";
            }

            return input.Substring(pos, pos_end - pos);
        }

        public void setProxy(Proxy proxy)
        {
            _proxy = proxy;       
        }

        private Proxy _proxy = null;

        public string GetPage(string url)
        {
            return GetPage(url, Encoding.Default);
        }

        //timeout默认30秒
        public string GetPage(string url, Encoding encoding)
        {
            return GetPage(url, encoding, DEFAULT_TIMEOUT*1000);
        }

        //指定timeout和proxy
        public string GetPage(string url, Encoding encoding, int time_out)
        {
            if (_proxy == null || _proxy.type == eProxyType.HTTP)
            {
                return GetHTTPPage(url, encoding, time_out);
            } 
            else
            {
                return GetSocksPage(url, encoding, time_out);
            }
        }

   
        public string GetHTTPPage(string url, Encoding encoding, int time_out)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            StreamReader reader = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = "Mozilla/5.0 (Windows NT 5.1) AppleWebKit/537.1 (KHTML, like Gecko) Chrome/21.0.1180.89 Safari/537.1";
                request.Timeout = time_out;
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                if (_proxy != null)
                {
                    try
                    {
                        request.Proxy = new WebProxy(_proxy.ip, _proxy.port);
                    }
                    catch (System.Exception ex)
                    {
                        request.Proxy = null;
                    }
                }
                request.AllowAutoRedirect = false;
                response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK && response.ContentLength < 1024 * 1024)
                {
                    Stream stream = response.GetResponseStream();
                    stream.ReadTimeout = time_out;
                    if (response.ContentEncoding == "gzip")
                    {
                        reader = new StreamReader(new GZipStream(stream, CompressionMode.Decompress), encoding);
                    } 
                    else
                    {
                        reader = new StreamReader(stream, encoding);
                    }
                    string html = reader.ReadToEnd();
                    return html;
                }
            }
            catch (Exception ex)
            {
                //System.Console.WriteLine(ex.Message);
                return "";
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                    response = null;
                }
                if (reader != null)
                    reader.Close();
                if (request != null)
                    request = null;
            }
            return string.Empty;
        }


        /* ==========================================
         * SOCKS  
         * ==========================================
         */

        public string GetSocksPage(string url, Encoding encoding, int time_out)
        {

            string result = "";
            if (_proxy != null)
            {
                Socket socket = GetSocket(_proxy.ip, _proxy.port);
                if (socket != null)
                {
                    try
                    {
                        string host = GetMid(url, "http://", "/");
                        string path = url.Substring(url.IndexOf(host) + host.Length);
                        socket.SendTimeout = time_out;
                        socket.ReceiveTimeout = time_out;

                        if (_proxy.type == eProxyType.SOCKS4)
                        {
                            if (ConnectSocks4ProxyServer(host, 80, socket))
                            {
                                result = Socket_get(socket, host, path, "", "", encoding);
                            }
                        }
                        else
                        {
                            if (ConnectSocks5ProxyServer(host, 80, socket))
                            {
                                result = Socket_get(socket, host, path, "", "", encoding);
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        return "";
                    }
                    finally
                    {
                        socket.Close();
                    }
                }
            }
            return result;
        }


        private bool ConnectSocks4ProxyServer(string strRemoteHost, int iRemotePort, Socket sProxyServer)
        {
            byte[] bySock4Send = new Byte[10];
            byte[] bySock4Receive = new byte[10];

            bySock4Send[0] = 4;
            bySock4Send[1] = 1;

            bySock4Send[2] = (byte)(iRemotePort / 256);
            bySock4Send[3] = (byte)(iRemotePort % 256);

            IPAddress ipAdd = Dns.GetHostEntry(strRemoteHost).AddressList[0];
            string strIp = ipAdd.ToString();
            string[] strAryTemp = strIp.Split(new char[] { '.' });
            bySock4Send[4] = Convert.ToByte(strAryTemp[0]);
            bySock4Send[5] = Convert.ToByte(strAryTemp[1]);
            bySock4Send[6] = Convert.ToByte(strAryTemp[2]);
            bySock4Send[7] = Convert.ToByte(strAryTemp[3]);

            bySock4Send[8] = 0;

            sProxyServer.Send(bySock4Send, bySock4Send.Length, SocketFlags.None);
            int iRecCount = sProxyServer.Receive(bySock4Receive, bySock4Receive.Length, SocketFlags.None);
            if (bySock4Receive[1] == 90)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private bool ConnectSocks5ProxyServer(string strRemoteHost, int iRemotePort, Socket sProxyServer)
        {
            //构造Socks5代理服务器第一连接头(无用户名密码) 
            byte[] bySock5Send = new Byte[10];
            bySock5Send[0] = 5;
            bySock5Send[1] = 1;
            bySock5Send[2] = 0;

            //发送Socks5代理第一次连接信息 
            sProxyServer.Send(bySock5Send, 3, SocketFlags.None);

            byte[] bySock5Receive = new byte[10];
            int iRecCount = sProxyServer.Receive(bySock5Receive, bySock5Receive.Length, SocketFlags.None);

            if (iRecCount < 2)
            {
                sProxyServer.Close();
                throw new Exception("不能获得代理服务器正确响应。");
            }

            if (bySock5Receive[0] != 5 || (bySock5Receive[1] != 0 && bySock5Receive[1] != 2))
            {
                sProxyServer.Close();
                throw new Exception("代理服务其返回的响应错误。");
            }

            if (bySock5Receive[1] == 0)
            {
                bySock5Send[0] = 5;
                bySock5Send[1] = 1;
                bySock5Send[2] = 0;
                bySock5Send[3] = 1;

                IPAddress ipAdd = Dns.GetHostEntry(strRemoteHost).AddressList[0];
                string strIp = ipAdd.ToString();
                string[] strAryTemp = strIp.Split(new char[] { '.' });
                bySock5Send[4] = Convert.ToByte(strAryTemp[0]);
                bySock5Send[5] = Convert.ToByte(strAryTemp[1]);
                bySock5Send[6] = Convert.ToByte(strAryTemp[2]);
                bySock5Send[7] = Convert.ToByte(strAryTemp[3]);

                bySock5Send[8] = (byte)(iRemotePort / 256);
                bySock5Send[9] = (byte)(iRemotePort % 256);

                sProxyServer.Send(bySock5Send, bySock5Send.Length, SocketFlags.None);
                iRecCount = sProxyServer.Receive(bySock5Receive, bySock5Receive.Length, SocketFlags.None);

                if (bySock5Receive[0] != 5 || bySock5Receive[1] != 0)
                {
                    sProxyServer.Close();
                    throw new Exception("第二次连接Socks5代理返回数据出错。");
                }
                return true;
            }
            else
            {
                if (bySock5Receive[1] == 2)
                    throw new Exception("代理服务器需要进行身份确认。");
                else return false;
            }
        }

        private Socket GetSocket(string strIpAdd, int iPort)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Connect(strIpAdd, iPort);
            }
            catch (System.Exception ex)
            {
                s.Close();
                s = null;
            }
            return s;
        }

        private byte[] _rec = new byte[256];

        private String Socket_get(Socket socket, String host, String url, String refer, String cookie, Encoding encoding)
        {
            String get = "GET " + url + " HTTP/1.0\r\n";
            get += "Host: " + host + "\r\n";
            get += "User-agent:Mozilla/4.0\r\n";
            get += "Accept: */*\r\n";
            get += "Accept-Language: zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3\r\n";
            get += "Accept-Encoding: deflate\r\n";
            //get += "Connection: keep-alive\r\n";
            if (cookie != "")
            {
                get += cookie + "\r\n";
            }
            if (refer != "")
            {
                get += "Referer:" + refer + "\r\n";
            }
            get += "\r\n";

            byte[] buf = System.Text.Encoding.ASCII.GetBytes(get);
            String data = "";
            int recsize = 256;

            try
            {
                socket.Send(buf, 0);
            }
            catch
            {
                return data;
            }

            // ==256专供SOCKS代理
            while ((recsize >0) && (data.Length < 2500))
            {
                try
                {
                    recsize = socket.Receive(_rec, 256, 0);
                    //sw.Write(_rec, 0, (int)recsize);
                }
                catch
                {
                    return data;
                }

                data += encoding.GetString(_rec);
                System.Array.Clear(_rec, 0, 256);
            }

            //sw.Close();
            if (data != "" && data.IndexOf("\r\n\r\n") != -1)
            {
                data = data.Substring(data.IndexOf("\r\n\r\n") + "\r\n\r\n".Length);
            }

            return data;
        }

    }
}
