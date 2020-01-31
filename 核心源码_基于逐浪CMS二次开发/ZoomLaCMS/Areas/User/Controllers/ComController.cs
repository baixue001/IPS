using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Safe;
using ZoomLaCMS.Control;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User
{
    [Area("User")]
    public class ComController : Ctrl_User
    {
        B_Admin badmin = new B_Admin();
        public string SaveFile()
        {
            C_SFileUP model = JsonConvert.DeserializeObject<C_SFileUP>(RequestEx["model"]);
            var file = Request.Form.Files["file"];
            Stream stream = file.OpenReadStream();
            int conLength = (int)Request.ContentLength;
            //=================================
            string result = "";
            if (Request.ContentLength < 100 || string.IsNullOrEmpty(file.FileName)) { return ""; }
            string fname = DateTime.Now.ToString("yyyyMMddHHmm") + function.GetRandomString(4) + Path.GetExtension(file.FileName);
            switch (model.FileType)
            {
                case "img":
                    {
                        
                        if (!SafeSC.IsImage(file.FileName)) { throw new Exception(Path.GetExtension(file.FileName) + "不是有效的图片格式!"); }
                        //ImgHelper imghelp = new ImgHelper();
                        //if (IsCompress)//压缩与最大比只能有一个生效
                        //{
                        //    imghelp.CompressImg(FileUp_File.PostedFile, 1000, vpath);
                        //}
                        bool hasSave = false;
                        //if (model.MaxWidth > 0 || model.MaxHeight > 0)
                        //{
                        //    System.Drawing.Image img = System.Drawing.Image.FromStream(file.InputStream);
                        //    img = imghelp.ZoomImg(img, model.MaxHeight, model.MaxWidth);
                        //    result = ImgHelper.SaveImage(GetSaveDir(model.SaveType) + fname, img);
                        //    hasSave = true;
                        //}
                        if (!hasSave) { result = SafeC.SaveFile(GetSaveDir(model.SaveType), fname, stream, conLength); }
                    }
                    break;
                //case "office":
                //    {
                //        string[] exname = "doc|docx|xls|xlsx".Split('|');
                //        if (!exname.Contains(Path.GetExtension(file.FileName))) { throw new Exception("必须上传doc|docx|xls|xlsx格式的文件!"); return ""; }
                //        result = ZoomLa.BLL.SafeSC.SaveFile(GetSaveDir(model.SaveType), fname, stream, conLength);
                //    }
                //    break;
                //case "all":
                //default:
                //    {
                //        result = ZoomLa.BLL.SafeSC.SaveFile(GetSaveDir(model.SaveType), fname, file.OpenReadStream(), conLength);
                //    }
                //    break;
            }
            return result;
        }
        //获取可保存到的目录,不含文件名
        private string GetSaveDir(string type)
        {
            string vpath = "";
            switch (type)
            {
                case "admin":
                    {
                        M_AdminInfo adminMod = B_Admin.GetLogin(HttpContext);
                        if (adminMod.IsNull) { throw new Exception("管理员未登录,无法上传文件"); }
                        vpath = ZLHelper.GetUploadDir_Admin(adminMod, "", "", "");
                    }
                    break;
                case "visitor":
                    {
                        vpath = ZLHelper.GetUploadDir_User(null, "", "", "");
                    }
                    break;
                case "user":
                default:
                    {
                        if (mu.IsNull) { throw new Exception("用户未登录,无法上传文件"); }
                        vpath = ZLHelper.GetUploadDir_User(mu, "", "", "");
                    }
                    break;
            }
            return vpath;
        }
    }
}