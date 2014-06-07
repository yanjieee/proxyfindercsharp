using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProxyFinder.sites
{
    class Proxyworld_site : Site
    {
        private HttpTools _httpTools = new HttpTools();

        private string _host = "http://www.proxyworld.us/";
        private List<string> _urls = new List<string>();

        public Proxyworld_site()
            : base("Proxyworld")
        {
            //string url = "index.php?forums/anonymous-http.4/";
        }

        public override void run()
        {
            try
            {
                string html = _httpTools.GetPage(_host + "index.php?forums/anonymous-http.4/");
                string list_html = _httpTools.GetMid(html, "<ol class=\"discussionListItems\">", "</ol>");
                string pattern = @"<li[^>]*>[\s\S]*?</li>";
                MatchCollection mc = Regex.Matches(list_html, pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                if (mc.Count > 1)
                {
                    _urls.Add(getUrlFromLi(mc[0].Value));
                    _urls.Add(getUrlFromLi(mc[1].Value));
                }
                if (this.proxys == null)
                {
                    this.proxys = new List<Proxy>();
                }
                foreach (string url in _urls)
                {
                    html = _httpTools.GetPage(url);
                    pattern = @"\w{1,3}\.\w{1,3}\.\w{1,3}\.\w{1,3}:\w{1,5}";
                    mc = Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                    foreach (Match m in mc)
                    {
                        string[] ip_port = m.Value.Split(':');
                        this.proxys.Add(new Proxy(ip_port[0], int.Parse(ip_port[1])));
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

        private string getUrlFromLi(string src)
        {
            string pattern = "(<a href=\"index.php\\?threads)[^\\\"]*";
            Match m = Regex.Match(src, pattern, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            return _host + m.Value.Replace("<a href=\"", "");
        }

    }
}
