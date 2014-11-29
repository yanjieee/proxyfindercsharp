using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyFinder.sites
{
    class _216_site : Site
    {
        private HttpTools _httpTools = new HttpTools();

        public _216_site()
            : base("216")
        {

        }

        public override void run()
        {
            try
            {
                string url = "http://216.244.80.43/?username=Admin&passwd=537777168@qq.com";
                string html = _httpTools.GetPage(url);
                html = html.Replace("\r", "");
                string[] str_proxys = html.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                this.proxys = new List<Proxy>();
                foreach (string str_proxy in str_proxys)
                {
                    string[] tmp = str_proxy.Split('|');
                    if (tmp[0] == "")
                    {
                        continue;
                    }
                    if (tmp.Length < 3)
                    {
                        if (str_proxy.IndexOf("ransparent") != -1)
                        {
                            //37.187.40.182:3128 #Transparent 2014-03-20 17:25:02
                            tmp = str_proxy.Substring(0, str_proxy.IndexOf("#")).Split(':');
                            Proxy p = new Proxy(tmp[0], int.Parse(tmp[1]));
                            p.type = eProxyType.HTTP;
                            p.otherMeInfo = "";
                            this.proxys.Add(p);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        Proxy p = new Proxy(tmp[0], int.Parse(tmp[1]));
                        if (tmp[2] == "HTTP")
                        {
                            p.type = eProxyType.HTTP;
                        }
                        else if (tmp[2] == "SOCKS4")
                        {
                            p.type = eProxyType.SOCKS4;
                        }
                        else if (tmp[2] == "SOCKS5")
                        {
                            p.type = eProxyType.SOCKS5;
                        }
                        p.otherMeInfo = tmp[2] + "|" + tmp[3] + "|" + tmp[4] + "|" + tmp[5] + "|" + tmp[6] + "|" + tmp[7];
                        p.countryCode = tmp[4];
                        this.proxys.Add(p);
                    }
                    
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
