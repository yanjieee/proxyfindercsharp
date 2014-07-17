using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyFinder.sites
{
    class eemmuu_site : Site
    {
        private HttpTools _httpTools = new HttpTools();

        public eemmuu_site() : base("eemmuu")
        {
        }


        public override void run()
        {
            try
            {
                string url = "http://eemmuu.com/proxy.asp?u=82_cc5fb3a05819b6b2&p=6e1a74f83ca76e17&c=c";
                string html = _httpTools.GetPage(url);
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
