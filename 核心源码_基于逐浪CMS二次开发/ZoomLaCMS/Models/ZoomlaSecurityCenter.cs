using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZoomLa.AppCode.Verify;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Safe;

namespace ZoomLaCMS
{
    public class MvcHtmlString
    {
        public static IHtmlContent Create(string html)
        {
           
            return new HtmlContentBuilder().AppendHtml(html);
        }
    }
    public class CustomerPageAction
    {
        public const string customPath2 = "/Admin/";
    }
    public class ZoomlaSecurityCenter
    {
        private static string[] ExName = new string[] { "cer", "exe", "vbs", "bat", "com", "asp", "aspx", "cshtml", "cs", "php", "jsp", "py", "asa", "asax", "ascx", "ashx", "asmx", "axd", "java", "jsl", "js", "vb", "resx", "html", "htm", "shtml", "shtm" };
        //地址栏参,Get方式提交数据
        //public static bool GetData()
        //{
        //    bool result = false;
        //    for (int i = 0; i < HttpContext.Current.Request.QueryString.Count; i++)
        //    {
        //        result = SafeSC.CheckData(HttpContext.Current.RequestEx[i]);
        //        if (result)
        //        {
        //            break;
        //        }
        //    }
        //    return result;
        //}
        public static void CheckVersion()
        {

        }
        //检测版本号,如果不正确则修正
        public static void CheckVersion(Configuration config)
        {
            try
            {
                //没有Version则新建一个
                if (config.AppSettings.Settings["Version"] == null)
                {
                    config.AppSettings.Settings.Add("Version", BLLCommon.Version);
                }
                else
                {
                    //判断配置文件中版本号与BLL中是否一致
                    string ver = config.AppSettings.Settings["Version"].Value;
                    if (!BLLCommon.Version.Equals(ver))
                    {
                        config.AppSettings.Settings["Version"].Value = BLLCommon.Version;
                    }
                }
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("AppSettings");
            }
            catch (Exception ex) { ZLLog.L("CheckVersion" + ex.Message); }
        }
        /// <summary>
        /// 后缀名检测,如aspx等返回True,不带,用于全局上传检测
        /// </summary>
        public static bool ExNameCheck(string ext)
        {
            ext = ext.ToLower().Replace(";", "");//避免低IIS版本,通过;阻断方式绕过验证
            return ExName.Contains(ext);
        }
        //----------------------------------------
        //----上传文件检测,首先多重判断，避免对普通页面的影响,然后再对安全进行检测,如带多个后缀名,则每个都检测
        //public static void CheckUpladFiles()
        //{
        //    HttpRequest Request = HttpContext.Current.Request;
        //    HttpResponse Response = HttpContext.Current.Response;
        //    if (Request.ContentType.ToLower().IndexOf("multipart/form-data") == -1) {  }
        //    if (Request.Files.Count < 1) {  }
        //    string UploadFileExts = SiteConfig.SiteOption.UploadFileExts.ToLower();
        //    for (int i = 0; i < Request.Files.Count; i++)
        //    {
        //        HttpPostedFile file = Request.Files[i];
        //        string fname = Path.GetFileName(file.FileName).ToLower();
        //        if (file.ContentLength < 1 || string.IsNullOrEmpty(fname)) { continue; }
        //        if (fname.IndexOf(".") < 0) { Response.Write("取消上传,原因:文件必须有后缀名,请勿上传可疑文件"); Response.End(); }
        //        string[] filearr = fname.Split('.');//多重后缀名检测,用于处理低版本IIS安全问题(IIS7以上可不需)
        //        for (int o = 1; o < filearr.Length; o++)
        //        {
        //            ZLLog.L(ZLEnum.Log.fileup, "By:Global_CheckUpladFiles,文件名：" + file.FileName);
        //            string fileext = filearr[o].ToString().ToLower();
        //            if (!ExNameCheck(fileext))//;号检测
        //            {
        //                string exname = Path.GetExtension(fname).ToLower().Replace(".", "");
        //                if (!StringHelper.FoundCharInArr(UploadFileExts, exname, "|"))
        //                {
        //                    Response.Write("取消上传,原因:上传的文件不是符合扩展名" + UploadFileExts + "的文件");
        //                    Response.End();
        //                }
        //            }
        //            else
        //            {
        //                Response.Write("取消上传,原因:请勿上传可疑文件!");
        //                Response.End();
        //            }
        //        }
        //    }
        //}
        //----------------------------------------
        /// <summary>
        /// 验证码校验,无论对错，必须清除，避免安全漏洞
        /// </summary>
        /// <returns>通过返回True,未通过False</returns>
        public static bool VCodeCheck(string key, string s)
        {
            return VerifyHelper.Check(new M_Comp_Verify() { vtype = 0, sid = key, token = s });
        }
        /// <summary>
        /// 解密网站前后端传办理的字符串,主用于密码传输
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string SiteDecrypt(string text)
        {
            string key = ZoomLa.BLL.SafeSC.ReadFileStr("/config/safe/PrivateKey.config");
            if (string.IsNullOrEmpty(key)) { ZLLog.L(ZoomLa.Model.ZLEnum.Log.safe, "RSA Key不存在,无法解密"); return text; }
            //return ZoomLa.BLL.Helper.Encry.RSAHelper.RsaDecrypt(text, key);
            return text;
        }
    }
}
