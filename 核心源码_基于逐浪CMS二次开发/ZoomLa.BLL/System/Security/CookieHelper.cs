using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace ZoomLa.BLL.System.Security
{
    
    public class CookieHelper
    {
        public static void Set(HttpContext ctx,string name,string value)
        {
            ctx.Response.Cookies.Append(name,value);
        }
        public static string Get(HttpContext ctx,string name)
        {
            string value = "";
            ctx.Request.Cookies.TryGetValue(name,out value);
            return value;
        }
        public static void ClearAll(HttpContext ctx)
        {
            var keys = ctx.Request.Cookies.Keys;
            foreach (string key in keys)
            {
                ctx.Response.Cookies.Delete(key);
            }
        }
    }
}
