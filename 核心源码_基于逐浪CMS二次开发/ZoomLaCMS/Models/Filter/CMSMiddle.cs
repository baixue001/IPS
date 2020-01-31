using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ZoomLaCMS.Filter
{
    /*
     * 
     * app.UseMiddleware<CMSMiddle>();
     * -----controller
     * public IActionResult Index()
        {
            ViewBag.str= service.Getstring();
            ViewBag.MiddlewareID = HttpContext.Items["CMSMiddleID"];
            return View();
        }
     */
    public class CMSMiddle
    {
        private RequestDelegate nextMiddleware;
        public CMSMiddle(RequestDelegate next)
        {
            this.nextMiddleware = next;
        }
        public async Task Invoke(HttpContext context)
        {
            context.Items.Add("CMSMiddleID","Text here");
            await this.nextMiddleware.Invoke(context);
        }
    }
}
