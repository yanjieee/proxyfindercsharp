using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProxyFinder.sites
{
    class Cnproxy_site: Site
    {

        private HttpTools _httpTools = new HttpTools();
        private string _js;


        public Cnproxy_site()
            : base("Cnproxy")
        {
            this.urls = new string[] { 
                "http://www.cnproxy.com/proxy1.html",
                "http://www.cnproxy.com/proxy2.html",
                "http://www.cnproxy.com/proxy3.html",
                "http://www.cnproxy.com/proxy4.html",
                "http://www.cnproxy.com/proxy5.html",
                "http://www.cnproxy.com/proxy6.html",
                "http://www.cnproxy.com/proxy7.html",
                "http://www.cnproxy.com/proxy8.html",
                "http://www.cnproxy.com/proxy9.html",
                "http://www.cnproxy.com/proxy10.html"};
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
                        proxylisttb = proxylisttb.Remove(0, proxylisttb.IndexOf("</tr>") + "</tr>".Length);
                        readIPs(proxylisttb);
                    }
                }
            }
            catch (Exception e)
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
            while(tr_index != -1)
            {
                string one_proxy_info = _httpTools.GetMid(tmp, "<tr><td>", "</td></tr>");
                string ip = _httpTools.GetIpFromStr(one_proxy_info);
                string port_js = _httpTools.GetMid(one_proxy_info, "<SCRIPT type=text/javascript>", "</SCRIPT>");
                port_js = port_js.Replace("document.write", "");
                string port_str = _httpTools.RunJs(_js + "\n" + port_js);
                port_str = port_str.Replace(":", "");
                int port = int.Parse(port_str);
                Proxy p = new Proxy(ip, port);
                if (one_proxy_info.IndexOf("HTTP", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    p.type = eProxyType.HTTP;
                }
                else if (one_proxy_info.IndexOf("SOCKS4", StringComparison.CurrentCultureIgnoreCase) >= 0) 
                {
                    p.type = eProxyType.SOCKS4;
                }
                else if (one_proxy_info.IndexOf("SOCKS5", StringComparison.CurrentCultureIgnoreCase) >= 0)
                {
                    p.type = eProxyType.SOCKS5;
                }
                this.proxys.Add(p);
                tmp = tmp.Remove(0, tmp.IndexOf("</tr>") + "</tr>".Length);
                tr_index = tmp.IndexOf("</tr>");
            }
        }


        private string getJS(string src)
        {
           return _httpTools.GetMid(src, "<SCRIPT type=\"text/javascript\">", "</SCRIPT>");
        }

        private string get_proxylisttb(string src)
        {
            return _httpTools.GetMid(src, "<div id=\"proxylisttb\">", "</div>");
        }

    }
}
