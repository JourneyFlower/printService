using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace printServer.utils
{
    class FunUtils
    {
        public static String getFun(String[] contextFun, String key, String nullValue = "")
        {
            String ret = null;
            if (contextFun != null && contextFun.Length > 0)
            {
                foreach (String value in contextFun)
                {
                    if (value.Contains(key))
                    {
                        ret = value.Replace(key, "");
                    }
                }
            }
            return String.IsNullOrEmpty(ret) ? nullValue : ret;
        }
        public static List<String> getFuns(String[] contextFun, String key)
        {
            List<String> ret = new List<string>();
            if (contextFun != null && contextFun.Length > 0)
            {
                foreach (String value in contextFun)
                {
                    if (value.Contains(key))
                    {
                        ret.Add(value.Replace(key, ""));
                    }
                }
            }
            return ret;
        }
    }
}
