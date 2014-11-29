using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyFinder.sites
{
    class emutool_site : Site
    {
        private HttpTools _httpTools = new HttpTools();

        public emutool_site() : base("emutool")
        {
        }


        public override void run()
        {
            try
            {
                string url = "http://www.emutool.com/admin_binguan/me.asp";
                string html = _httpTools.GetPage(url);
                html = html.Replace("\r", "");
                string[] str_proxys = html.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                this.proxys = new List<Proxy>();
                foreach (string str_proxy in str_proxys)
                {
                    string[] tmp = str_proxy.Split(':');
                    if (tmp.Length < 2)
                    {
                        continue;
                    }
                    Proxy p = new Proxy(tmp[0], int.Parse(tmp[1]));
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
