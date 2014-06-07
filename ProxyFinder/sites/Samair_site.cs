using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyFinder.sites
{
    class Samair_site : Site
    {

        private HttpTools _httpTools = new HttpTools();
        private string _js;

        private string _host = "http://samair.ru";

        public Samair_site()
            : base("Samair")
        {
            string url = "http://samair.ru/proxy/proxy-{0}.htm";
            List<string> list_urls = new List<string>();
            for (int i = 1; i < 31; i++ )
            {
                string page_num = "";
                if (i<10)
                {
                    page_num = "0" + i.ToString();
                } 
                else
                {
                    page_num = i.ToString();
                }
                list_urls.Add(String.Format(url, page_num));
            }
            this.urls = list_urls.ToArray();
        }



        public override void run()
        {
            try
            {
                foreach (string url in this.urls)
                {
                    string html = _httpTools.GetPage(url);
                    if (html != "")
                    {
                        _js = getJS(html);
                        string proxylisttb = get_proxylisttb(html);
                        readIPs(proxylisttb);
                    }
                }
            }
            catch
            {
                this.onFindResult(false, this);
                return;
            }
            this.onFindResult(true, this);
            
        }

        private void readIPs(string proxylist)
        {
            string tmp = proxylist;
            if (this.proxys == null)
            {
                this.proxys = new List<Proxy>();
            }
            int tr_index = tmp.IndexOf("</tr>");
            while (tr_index != -1)
            {
                string one_proxy_info = _httpTools.GetMid(tmp, "<tr><td>", "</td></tr>");
                string ip = _httpTools.GetIpFromStr(one_proxy_info);
                string port_js = _httpTools.GetMid(one_proxy_info, "<script type=\"text/javascript\">", "</script>");
                port_js = port_js.Replace("document.write", "");
                string port_str = _httpTools.RunJs(_js + "\n" + port_js);
                port_str = port_str.Replace(":", "");
                int port = int.Parse(port_str);
                Proxy p = new Proxy(ip, port);
                this.proxys.Add(p);
                tmp = tmp.Remove(0, tmp.IndexOf("</tr>") + "</tr>".Length);
                tr_index = tmp.IndexOf("</tr>");
            }
        }

        private string getJS(string src)
        {
            string tmp = _httpTools.GetMid(src, "<script src=\"", "\"");
            tmp = _httpTools.GetPage(_host + tmp);
            tmp = _httpTools.RunJs(tmp.Replace("eval", ""));
            return tmp;
        }

        private string get_proxylisttb(string src)
        {
            string tmp = _httpTools.GetMid(src, "Country</a></th>", "<div id=\"pageinfo\">");
            tmp = _httpTools.GetMid(tmp, "</tr>", "</table>");
            return tmp;
        }
    }
}
