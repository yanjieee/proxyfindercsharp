using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProxyFinder
{
    class ProxyJudger
    {
        public ProxyJudger(Proxy p)
        {
            _httpTools = new HttpTools();
            _proxy = p;
            _httpTools.setProxy(p);
        }

        private HttpTools _httpTools;
        private Proxy _proxy;

        public delegate void onJudgeDoneDelegate();

        public event onJudgeDoneDelegate onJudgeDone;

        private string getProxyJuageResultFormHTML(string htmlcontent)
        {
            return _httpTools.GetMid(htmlcontent, "CS_ProxyJudge Result=", "\n");
        }

        public void doProxyJudge(object time_out)
        {
            int timeout = (int)time_out;
            string proxyJudgeUrl = getProxyJudgeUrl();
            string content = _httpTools.GetPage(proxyJudgeUrl, System.Text.Encoding.UTF8, timeout);
            content = getProxyJuageResultFormHTML(content);
            if (content.IndexOf("ANONYMOUS") != -1)
                _proxy.level = eProxyLevel.ANONYMOUS;
            else if (content.IndexOf("HIGH") != -1)
                _proxy.level = eProxyLevel.HIGH_ANONYMITY;
            else if (content.IndexOf("TRANS") != -1)
                _proxy.level = eProxyLevel.TRANSPARENT;
            else
                _proxy.level = eProxyLevel.BAD;
            //Console.WriteLine(_proxy.ip + ":" + _proxy.port.ToString() + "level:" + _proxy.level.ToString());
            if (_proxy.level != eProxyLevel.BAD)
            {
                //判断国家
                content = getCountryCodeByGeo(_proxy.ip);
                if (content != "--")
                {
                    _proxy.countryCode = content;
                }
                /*
                content = _httpTools.GetPage("http://myplantblog.com/geoip2.php", System.Text.Encoding.UTF8, timeout);
                if (content != "" && content.IndexOf("RUN GEOIP SUCCESS") != -1)
                {
                    if (content.Split('|')[0] != "")
                    {
                        _proxy.countryCode = content.Split('|')[0];
                    }
                }
                */
            }


            if (onJudgeDone != null)
            {
                onJudgeDone();
            }
        }

        /// <summary>
        /// 获取国家代码，如果失败，返回"--"
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public String getCountryCodeByGeo(String ip)
        {
            //string GeoipDbPath = "/usr/local/share/GeoIP/";
            string GeoipDb = "GeoIP.dat";
            //open the database
            LookupService ls = new LookupService(GeoipDb, LookupService.GEOIP_MEMORY_CACHE);
            //get country of the ip address
            Country c = ls.getCountry(ip);
            return c.getCode();
        }

        private string getProxyJudgeUrl() 
        {
            StreamReader sr = new StreamReader("proxyjudger.txt");
            String url = sr.ReadLine();
            sr.Close();
            return url;
        }
    }
}
