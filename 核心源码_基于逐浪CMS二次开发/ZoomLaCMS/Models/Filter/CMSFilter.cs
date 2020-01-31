using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ZoomLa.BLL.Helper;
using ZoomLa.Components;

namespace ZoomLaCMS.Filter
{
    //services.AddMvc(options =>
    //    {
    //        //options.Filters.Add<ZoomLaCMS.Filter.CMSFilter>(); //注册过滤器
    //    })
    public class CMSFilter:IActionFilter
    {
        //强制Https
        public static void ForceToHttps(HttpContext ctx)
        {
            if (!SiteConfig.SiteOption.SafeDomain.Equals("1") || ctx.Request.IsHttps) { return; }
            HttpResponse rep = ctx.Response;
            HttpRequest req = ctx.Request;
            string path = req.RawUrl().ToLower();
            if (path.Equals("/tools/default")) { }
            else
            {
                rep.Redirect(req.FullUrl().Replace("http://", "https://"), true);
            }
        }
        public CMSFilter()
        {

        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            HttpResponse rep = context.HttpContext.Response;
            HttpRequest req = context.HttpContext.Request;
            string path = req.RawUrl().ToLower();
            if (path.Equals("/tools/")) { return; }
            #region  域名归并,支持带端口跳转
            SiteConfig.SiteOption.DomainMerge = true;
            //对不同的设备,自动替换域名
            if (SiteConfig.SiteOption.DomainMerge)
            {
                string domain = "";
                string orgin = Regex.Split(req.FullUrl(), "://")[1].Split('/')[0];
                switch (DeviceHelper.GetAgent(req.UserAgent()))
                {
                    case DeviceHelper.Agent.PC:
                        {
                            domain = SiteConfig.SiteOption.Domain_PC;
                        }
                        break;
                    default:
                        if (DeviceHelper.GetBrower(req.UserAgent()) == DeviceHelper.Brower.Micro)
                        {
                            domain = SiteConfig.SiteOption.Domain_Wechat;
                        }
                        else
                        {
                            domain = SiteConfig.SiteOption.Domain_Mobile;
                        }
                        break;
                }
                if (domain.Contains("://")) { domain = Regex.Split(domain, "://")[1]; }
                //有设置域名,并且和当前访问的域名不匹配
                if (!string.IsNullOrEmpty(domain)&& !orgin.ToLower().Equals(domain.ToLower()))
                {
                    string url = req.FullUrl().Replace(orgin, domain).Trim('/');
                    rep.Redirect(url);
                }
            }
            #endregion
            ForceToHttps(context.HttpContext);
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {

        }
    }
}
