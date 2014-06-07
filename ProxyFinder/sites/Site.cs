using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyFinder.sites
{
    /// <summary>
    /// 所有的站点类都要继承该类，并给proxys赋值，在抓取成功/失败后调用onFindResult事件
    /// </summary>
    abstract class Site
    {
        protected string[] urls;
        protected string SiteName;
        protected List<Proxy> proxys;
        public int retryCount = 0;

        abstract public void run();


        public delegate void findResult(Boolean isSuccess, Site site);

        public event findResult onFindResultEvent;

        protected virtual void onFindResult(Boolean isSuccess, Site site)
        {
            if (onFindResultEvent != null)
            {
                onFindResultEvent(isSuccess, site);
            }
        }

        public virtual List<Proxy> getProxys()
        {
            return proxys;
        }

        public virtual String getSiteName()
        {
            return this.SiteName;
        }

        public Site(string siteName)
        {
            this.SiteName = siteName;
        }
    }
}
