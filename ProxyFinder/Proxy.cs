using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxyFinder
{
    public enum eProxyLevel
    {
        BAD = 0,    //不能用的
        TRANSPARENT = 1,
        ANONYMOUS = 2,
        HIGH_ANONYMITY = 4,
    };

    public enum eProxyType
    {
        HTTP = 0,
        SOCKS4 = 1,
        SOCKS5 = 2,
    };

    class Proxy
    {
        public String ip;
        public int port;

        public static String[] PROXY_TYPE = new String[] { "HTTP", "SOCKS4", "SOCKS5" };

        public String otherMeInfo = "";
        public String countryCode = "TW";

        public eProxyLevel level = eProxyLevel.BAD;
        public eProxyType type = eProxyType.HTTP;

        public Proxy(String ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }

        public String toString()
        {
            return ip + ":" + port.ToString();
        }

        /// <summary>
        /// 返回不带端口的ip字符串
        /// </summary>
        /// <returns></returns>
        public String toIpString()
        {
            return ip;
        }

        public String toPersonString()
        {
            return ip + "|" + port.ToString() + "|" + PROXY_TYPE[(int)type] + "|" + countryCode;
        }

        public String toMeString()
        {
            if (otherMeInfo != "")
            {
                return ip + "|" + port.ToString() + "|" + otherMeInfo;
            }
            else
            {
                return "";
            }
            
        }

    }
}
