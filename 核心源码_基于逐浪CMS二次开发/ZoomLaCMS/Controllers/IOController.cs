using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.Plat;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Plat;
using ZoomLa.Safe;
using ZoomLaCMS.Ctrl;
using ZoomLaCMS.Ueditor;

namespace ZoomLaCMS.Controllers
{
    public class IOController : Ctrl_Admin
    {
        public IActionResult WebUP()
        {
            return View("/Views/Common/WebUP.cshtml");
        }
        //上传base64图片
        public IActionResult Base64()
        {
            string base64str = Request.Form["base64"];//上传的base64字符串
            string action = Request.Form["action"];
            string uploadPath = "";
            string saveName = function.GetRandomString(6) + ".jpg";
            string result = "";
            M_UserInfo mu = buser.GetLogin();
            try
            {
                if (!mu.IsNull)
                {
                    uploadPath = ZLHelper.GetUploadDir_User(mu, "dai");
                }
                else if (adminMod != null)
                {
                    uploadPath = ZLHelper.GetUploadDir_Admin(adminMod, "dai");
                }
                else //Not Login
                {
                    uploadPath = ZLHelper.GetUploadDir_Anony("dai");
                }
                ImgHelper imghelper = new ImgHelper();
                imghelper.Base64ToImg(uploadPath + saveName, base64str);
                result = uploadPath + saveName;
                return Content(result);
            }
            catch (Exception ex)
            {
                ZLLog.L(ZLEnum.Log.fileup, new M_Log()
                {
                    Source = Request.RawUrl(),
                    Message = "上传失败|文件名:" + uploadPath + saveName + "|" + "原因:" + ex.Message
                });
                return Content(Failed.ToString());
            }
        }
        public IActionResult UploadFileHandler()
        {
            //HttpRequest Request = context.Request;
            //context.Response.ContentType = "text/plain";
            //context.Request.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            //context.Response.ContentEncoding = System.Text.Encoding.GetEncoding("UTF-8");
            var file = Request.Form.Files["Filedata"];
            if (file == null)
            {
                file = Request.Form.Files["file"];//接受Uploadify或WebUploader传参,优先Uploadify 
            }
            if (file == null || file.Length < 1) { return Content(Failed.ToString()); }
            if (SafeSC.FileNameCheck(file.FileName))
            {
                throw new Exception("不允许上传该后缀名的文件");
            }
            M_UserInfo mu = buser.GetLogin();
            if (adminMod == null && mu.IsNull) { throw new Exception("未登录"); }
            /*-------------------------------------------------------------------------------------------*/
            M_User_Plat upMod = new B_User_Plat().SelReturnModel(mu.UserID);
            string uploadPath = SiteConfig.SiteOption.UploadDir.TrimEnd('/') + "/", filename = "", ppath = "", result = "0";//上传根目录,文件名,上物理路径,结果
            string action =GetParam("action"), value = GetParam("value");
            try
            {
                switch (action)
                {
                    #region OA与能力中心
                    case "OAattach"://OA--公文||事务--附件
                                    //uploadPath += "OA/" + mu.UserName + mu.UserID + "/" + DateTime.Now.ToString("yyyyMMdd") + "/";
                        uploadPath = ZLHelper.GetUploadDir_User(mu, "OA");
                        ppath = function.VToP(uploadPath);
                        //判断是否有同名文件的存在
                        break;
                    case "Blog"://能力中心--博客
                        uploadPath = B_Plat_Common.GetDirPath(upMod,B_Plat_Common.SaveType.Blog);
                        ppath = function.VToP(uploadPath);
                        break;
                    case "Plat_Doc"://能力中心--我的文档
                        uploadPath = B_Plat_Common.GetDirPath(upMod, B_Plat_Common.SaveType.Person) + SafeSC.PathDeal(GetParam("Dir"));
                        ppath = function.VToP(uploadPath);
                        break;
                    case "Plat_Doc_Common"://能力中心--公司文档
                        uploadPath = B_Plat_Common.GetDirPath(upMod, B_Plat_Common.SaveType.Company) + SafeSC.PathDeal(GetParam("Dir"));
                        ppath =function.VToP(uploadPath);
                        break;
                    case "Plat_Task"://能力中心--任务中心附件
                        int tid = Convert.ToInt32(value);
                        ZoomLa.Model.Plat.M_Plat_Task taskMod = new B_Plat_Task().SelReturnModel(tid);
                        uploadPath = B_Plat_Common.GetDirPath(upMod, B_Plat_Common.SaveType.Plat_Task) + taskMod.TaskName + "/";
                        break;
                    case "Plat_Project"://能力中心--项目
                        int pid = Convert.ToInt32(value);
                        ZoomLa.Model.Plat.M_Plat_Pro proMod = new B_Plat_Pro().SelReturnModel(pid);
                        uploadPath = B_Plat_Common.GetDirPath(upMod, B_Plat_Common.SaveType.Plat_Task) + proMod.Name + "/";
                        break;
                    #endregion
                    case "ModelFile"://组图,多图等
                        {
                            int nodeid = Convert.ToInt32(value);
                            //M_Node nodeMod = new B_Node().GetNodeXML(nodeid);
                            string exname = Path.GetExtension(file.FileName).Replace(".", "");
                            //string fpath = nodeMod.NodeDir + "/" + exname + "/" + DateTime.Now.ToString("yyyy/MM/");
                            uploadPath = ZLHelper.GetUploadDir_System("field", "images", "yyyyMMdd");
                            filename = DateTime.Now.ToString("HHmmss") + function.GetRandomString(6, 2) + "." + exname;
                        }
                        break;
                    case "admin_custom"://管理员上传,自定义路径
                        {
                            if (adminMod == null || adminMod.AdminId < 1) { throw new Exception("管理员未登录"); }
                            uploadPath =GetParam("save");//BannerAdd
                        }
                        break;
                    default://通常格式,不需做特殊处理的格式但必须登录
                        if (mu.UserID > 0)
                        {
                            //uploadPath = context.Server.UrlDecode(uploadPath + "User/" + mu.UserName + mu.UserID + "/");
                            uploadPath = ZLHelper.GetUploadDir_User(mu, "User", "", "");
                        }
                        else if (adminMod != null)
                        {
                            //uploadPath = context.Server.UrlDecode(uploadPath + "Admin/" + adminMod.AdminName + adminMod.AdminId + "/");
                            uploadPath = ZLHelper.GetUploadDir_Admin(adminMod, "", "", "yyyyMMdd");
                        }
                        else
                        {
                            //注册等页面用户未登录
                            uploadPath = ZLHelper.GetUploadDir_System("user", "register", DateTime.Now.ToString("yyyyMMdd"));
                        }
                        break;
                }
                string uploadDir = Path.GetDirectoryName(function.VToP(uploadPath));
                if (!Directory.Exists(uploadDir)) { SafeSC.CreateDir(function.PToV(uploadDir)); }
                if (action.Equals("Plat_Doc") || action.Equals("Plat_Doc_Common"))
                {
                    #region 能力中心文档
                    M_Plat_File fileMod = new M_Plat_File();
                    B_Plat_File fileBll = new B_Plat_File();
                    fileMod.FileName = file.FileName;
                    fileMod.SFileName = function.GetRandomString(12) + Path.GetExtension(file.FileName);
                    fileMod.VPath = uploadPath.Replace("//", "/");
                    fileMod.UserID = upMod.UserID.ToString();
                    fileMod.CompID = upMod.CompID;
                    //SafeSC.SaveFile(uploadPath, file, fileMod.SFileName);
                    fileMod.FileSize = new FileInfo(ppath + fileMod.SFileName).Length.ToString();
                    fileBll.Insert(fileMod);
                    #endregion
                }
                else if (action.Equals("Cloud_Doc"))
                {
                    #region 用户中心云盘
                    if (!buser.CheckLogin()) { throw new Exception("云盘,用户未登录"); }
                    M_User_Cloud cloudMod = new M_User_Cloud();
                    B_User_Cloud cloudBll = new B_User_Cloud();
                    uploadPath = HttpUtility.UrlDecode(cloudBll.H_GetFolderByFType(GetParam("type"), mu)) + GetParam("value");
                    cloudMod.FileName = file.FileName;
                    cloudMod.SFileName = function.GetRandomString(12) + Path.GetExtension(file.FileName);
                    cloudMod.VPath = (uploadPath + "/").Replace("//", "/");
                    cloudMod.UserID = mu.UserID;
                    cloudMod.FileType = 1;
                    //result = SafeSC.SaveFile(cloudMod.VPath, file, cloudMod.SFileName);
                    //if (SafeSC.IsImage(cloudMod.SFileName))
                    //{
                    //    string icourl = SiteConfig.SiteOption.UploadDir + "YunPan/" + mu.UserName + mu.UserID + "/ico" + value + "/";
                    //    if (!Directory.Exists(function.VToP(icourl))) { SafeSC.CreateDir(icourl); }
                    //    ImgHelper imghelp = new ImgHelper();
                    //    imghelp.CompressImg(file, 100, icourl + cloudMod.SFileName);
                    //}
                    cloudMod.FileSize = new FileInfo(function.VToP(cloudMod.VPath) + cloudMod.SFileName).Length.ToString();
                    cloudBll.Insert(cloudMod);
                    #endregion
                }
                else
                {
                    //string fname = CreateFName(file.FileName);
                    //if (SafeC.IsImageFile(file.FileName) && file.Length > (5 * 1024 * 1024))//图片超过5M则压缩
                    //{
                    //    result = uploadPath + function.GetRandomString(6) + fname;
                    //    new ImgHelper().CompressImg(file, 5 * 1024, result);
                    //}
                    //else
                    //{
                    result = SafeC.SaveFile(uploadPath, filename, file.OpenReadStream(), (int)file.Length);
                    //}
                    //添加水印
                    //if (WaterModuleConfig.WaterConfig.EnableUserWater)
                    //{
                    //    //未以管理员身份登录,并有会员身份登录记录
                    //    if (adminMod == null && !mu.IsNull)
                    //    {
                    //        Image img = WaterImages.DrawFont(ImgHelper.ReadImgToMS(result), mu.UserName + " " + DateTime.Now.ToString("yyyy/MM/dd"), 9);
                    //        ImgHelper.SaveImage(result, img);
                    //    }
                    //}
                    //else if (DataConverter.CStr(context.Request["IsWater"]).Equals("1"))
                    //{
                    //    //前台主动标识需要使用水印
                    //    result = ImgHelper.AddWater(result);
                    //}
                }
                ZLLog.L(ZLEnum.Log.fileup, new M_Log()
                {
                    UName = mu.UserName,
                    Source = Request.RawUrl(),
                    Message = "上传成功|文件名:" + file.FileName + "|" + "保存路径:" + uploadPath
                });
            }
            catch (Exception ex)
            {
                ZLLog.L(ZLEnum.Log.fileup, new M_Log()
                {
                    UName = mu.UserName,
                    Source = Request.RawUrl(),
                    Message = "上传失败|文件名:" + file.FileName + "|" + "原因:" + ex.Message
                });
            }
            return Content(result);
        }
        public void Ueditor()
        {
            var context = HttpContext;
            Handler action = null;
            string ttt = RequestEx["action"];
            switch (RequestEx["action"])
            {
                case "config":
                    action = new ConfigHandler(context);
                    break;
                case "uploadimage":
                    action = new UploadHandler(context, new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("imageAllowFiles"),
                        PathFormat = Config.GetString("imagePathFormat"),
                        SizeLimit = Config.GetInt("imageMaxSize"),
                        UploadFieldName = Config.GetString("imageFieldName")
                    });
                    break;
                case "uploadscrawl":
                    action = new UploadHandler(context, new UploadConfig()
                    {
                        AllowExtensions = new string[] { ".png" },
                        PathFormat = Config.GetString("scrawlPathFormat"),
                        SizeLimit = Config.GetInt("scrawlMaxSize"),
                        UploadFieldName = Config.GetString("scrawlFieldName"),
                        Base64 = true,
                        Base64Filename = "scrawl.png"
                    });
                    break;
                case "uploadvideo":
                    action = new UploadHandler(context, new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("videoAllowFiles"),
                        PathFormat = Config.GetString("videoPathFormat"),
                        SizeLimit = Config.GetInt("videoMaxSize"),
                        UploadFieldName = Config.GetString("videoFieldName")
                    });
                    break;
                case "uploadfile":
                    action = new UploadHandler(context, new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("fileAllowFiles"),
                        PathFormat = Config.GetString("filePathFormat"),
                        SizeLimit = Config.GetInt("fileMaxSize"),
                        UploadFieldName = Config.GetString("fileFieldName")
                    });
                    break;
                case "listimage":
                    action = new ListFileManager(context, Config.GetString("imageManagerListPath"), Config.GetStringList("imageManagerAllowFiles"));
                    break;
                case "listfile":
                    action = new ListFileManager(context, Config.GetString("fileManagerListPath"), Config.GetStringList("fileManagerAllowFiles"));
                    break;
                case "catchimage":
                    action = new CrawlerHandler(context);
                    break;
                default:
                    action = new NotSupportedHandler(context);
                    break;
            }
            action.Process();
        }
        private string CreateFName(string fname)
        {
            return DateTime.Now.ToString("yyyyMMdd") + function.GetRandomString(6) + Path.GetExtension(fname);
        }
    }
}