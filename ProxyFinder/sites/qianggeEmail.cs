using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ProxyFinder.sites
{
    class qianggeEmail : Site
    {
        private POP3 _pop3 = new POP3("pop3.126.com", 110);

        public qianggeEmail()
            : base("qianggeEmail")
        {

        }

        private string _user = "EmailReceiver2014";
        private string _pwd = "er123456";

        private int _count = 5; //读5封邮件

        public override void run()
        {
            try
            {
                string html = "";
                int count = _pop3.GetMailCount(_user, _pwd);
                DataTable a = _pop3.GetMailTable(_user, _pwd);
                while(_count > 0)
                {
                    DataTable mail = _pop3.GetMail(_user, _pwd, count);
                    if (mail.Rows[0][1].ToString().Contains("proxy02@qq.com"))  //强哥的邮箱
                    {
                        html += mail.Rows[1][1].ToString();
                        html += "\r\n";
                    }
                    count--;
                    if (count <= 0)
                    {
                        break;
                    }
                    _count--;
                }
                if (html.Length > 0)
                {
                    html = html.Replace("\r", "");
                    string[] str_proxys = html.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                    this.proxys = new List<Proxy>();
                    foreach (string str_proxy in str_proxys)
                    {
                        string[] tmp = str_proxy.Split(':');
                        if (tmp.Length != 2)
                        {
                            continue;
                        }
                        string ip = tmp[0];
                        tmp = tmp[1].Split('@');
                        string port = tmp[0];
                        tmp = tmp[1].Split('#');
                        string type = tmp[0];
                        Proxy p = new Proxy(ip, int.Parse(port));
                        if (type.Contains("SOCKS4"))
                        {
                            p.type = eProxyType.SOCKS4;
                        } 
                        else if (type.Contains("SOCKS5"))
                        {
                            p.type = eProxyType.SOCKS5;
                        }
                        else
                        {
                            p.type = eProxyType.HTTP;
                        }
                        this.proxys.Add(p);
                    }
                }
                else
                {
                    this.onFindResult(false, this);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                this.onFindResult(false, this);
                return;
            }
            this.onFindResult(true, this);


        }


    }
}
