using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyFinder.sites
{
    class qqbb_site : Site
    {
        private HttpTools _httpTools = new HttpTools();

        public qqbb_site() : base("qqbb")
        {

        }

        public override void run()
        {
            try
            {
                string url = "http://23.19.43.201/qqbb.php";
                string html = _httpTools.GetPage(url);
                html = html.Replace("\r", "");
                string[] str_proxys = html.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                this.proxys = new List<Proxy>();
                foreach (string str_proxy in str_proxys)
                {
                    string[] tmp = str_proxy.Split('|');
                    if (tmp.Length < 3)
                    {
                        continue;
                    }
                    Proxy p = new Proxy(tmp[0], int.Parse(tmp[1]));
                    if (tmp[2] == "HTTP")
                    {
                        p.type = eProxyType.HTTP;
                    }
                    else if (tmp[2] == "SOCK4")
                    {
                        p.type = eProxyType.SOCKS4;
                    }
                    else if (tmp[2] == "SOCK5")
                    {
                        p.type = eProxyType.SOCKS5;
                    }
                    p.otherMeInfo = tmp[2] + "|" + tmp[3] + "|" + tmp[4] + "|" + tmp[5] + "|" + tmp[6] + "|" + tmp[7];
                    p.countryCode = tmp[4]; ;
                    this.proxys.Add(p);
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
