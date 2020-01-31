using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Controllers
{
    //提供视频,PDF,Word,Office在线预览
    public class PreViewController : Ctrl_User
    {
        //------------------------
        //虚拟路径,仅允许/UploadFiles/下
        public string VPath { get { return GetParam("vpath"); } }
        //文件的Guid(Plat|Cloud)
        public string FileId { get { return GetParam("FileId"); } }
        public IActionResult Index()
        {
            string fileVPath = "";
            if (!string.IsNullOrEmpty(VPath))
            {
                fileVPath = SafeSC.PathDeal(VPath);
                if (!fileVPath.ToLower().StartsWith("/uploadfiles/")) { return WriteErr("该目录下的文件不支持预览"); }
            }
            else if (!string.IsNullOrEmpty(FileId))
            {
                B_User_Cloud cloudBll = new B_User_Cloud();
                M_User_Cloud cloudMod = cloudBll.SelReturnModel(FileId);
                if (cloudMod == null) {return WriteErr("文件不存在!"); }
                fileVPath = cloudMod.VPath + cloudMod.SFileName;
            }
            else
            {
                return WriteErr("未指定文件");
            }
            string filePath = function.VToP(fileVPath);
            string exName = System.IO.Path.GetExtension(fileVPath).ToLower().Replace(".", "");//doc
            if (string.IsNullOrEmpty(exName)) { return WriteErr("文件无后缀名"); }
            if (!System.IO.File.Exists(filePath)) { return WriteErr("文件[" + fileVPath + "]不存在"); }
            if (SafeSC.FileNameCheck(fileVPath))
            {
                return WriteErr("文件名异常,请去除特殊符号，或更换后缀名");
            }
            else if (exName.Equals("config"))
            {
                return WriteErr("该类型文件不提供预览服务!!");
            }
            /*---------------------------------------------------------------------------------------------------*/
            ViewBag.filePath = filePath;
            ViewBag.fileVPath = fileVPath;
            ViewBag.exName = exName;
            return View();
        }
    }
}