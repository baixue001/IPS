using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Components;

namespace ZoomLaCMS.Ctrl
{
    public class Ctrl_Front:Ctrl_User
    {
        private B_CreateHtml _bll = null;
        public B_CreateHtml bll
        {
            get
            {
                if (_bll == null) { _bll = new B_CreateHtml(HttpContext); }
                return _bll;
            }
        }
        public int ItemID { get { return DataConverter.CLng(GetParam("ID")); } }
        public int NodeID { get { return DataConverter.CLng(GetParam("NodeID")); } }
        public int Cpage { get { return DataConverter.CLng(GetParam("Cpage"), 0); } }
        public IActionResult HtmlToClient(string html)
        {
            ViewBag.html = html;
            return View("/Views/Front/Default.cshtml");
        }
        /// <summary>
        /// 每个页面可自实现,用于将模板URL转换为html
        /// </summary>
        public virtual string TemplateToHtml(string templateUrl)
        {
            string html = "";
            string vpath = (SiteConfig.SiteOption.TemplateDir.TrimEnd('/') + "/" + templateUrl);
            string TemplatePath = function.VToP(vpath);
            if (!System.IO.File.Exists(TemplatePath)) { return "[产生错误的可能原因：(" + vpath + ")文件不存在]"; }
            html = FileSystemObject.ReadFile(TemplatePath);
            html = bll.CreateHtml(html, Cpage, ItemID, "1");
            //if (SiteConfig.SiteOption.IsSensitivity == 1) { html = B_Sensitivity.Process(html); }
            return html;
        }
    }
}
