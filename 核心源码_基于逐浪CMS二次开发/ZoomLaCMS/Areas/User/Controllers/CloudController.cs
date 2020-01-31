using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class CloudController : Ctrl_User
    {
        B_User_Cloud cloudBll = new B_User_Cloud();
        public IActionResult Index()
        {
            //var model = new VM_Cloud(CPage, PSize, mu, Request);
            //if (Request.IsAjax())
            //{
            //    return PartialView("Cloud_List", model);
            //}
            //return View(model);
            return View();
        }
        public IActionResult Cloud_Open()
        {
            string baseDir = ZLHelper.GetUploadDir_User(mu, "Cloud", "", "");
            string pathfile = baseDir + "我的文档/";
            string pathphoto = baseDir + "我的相册/";
            string pathmusic = baseDir + "我的音乐/";
            string pathvideo = baseDir + "我的视频/";
            SafeSC.CreateDir(pathfile);
            SafeSC.CreateDir(pathphoto);
            SafeSC.CreateDir(pathmusic);
            SafeSC.CreateDir(pathvideo);

            buser.UpdateIsCloud(mu.UserID, 1);
            return WriteOK("云盘开通成功", "Index");
        }
        public int Cloud_NewDir()
        {
            //var model = new VM_Cloud(mu, Request);
            //M_User_Cloud cloudMod = new M_User_Cloud();
            //cloudMod.FileName = RequestEx["DirName_T"];
            //cloudMod.VPath = model.CurrentDir;
            //cloudMod.UserID = mu.UserID;
            //cloudMod.FileType = 2;
            //SafeSC.CreateDir(function.VToP(cloudMod.VPath), cloudMod.FileName);
            //cloudBll.Insert(cloudMod);
            return Success;
        }
        public int Cloud_Del(string id)
        {
            M_User_Cloud cloudMod = cloudBll.SelReturnModel(id);
            if (cloudMod == null) { return -1; }
            if (cloudMod.FileType == 1)
            {
                SafeSC.DelFile(cloudMod.VPath + cloudMod.SFileName);
            }
            else
            {
                SafeSC.DelFile(cloudMod.VPath + cloudMod.FileName);
            }
            cloudBll.DelByFile(cloudMod.Guid);
            return 1;
        }
    }
}
