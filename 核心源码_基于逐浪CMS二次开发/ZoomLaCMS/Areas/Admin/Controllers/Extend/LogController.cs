using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;
using Hangfire;
using ZoomLa.Model.Content;
using ZoomLa.BLL.Content;

namespace ZoomLaCMS.Areas.Admin.Controllers.Extend
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    [Route("[area]/Extend/[controller]/[action]")]
    public class LogController : Ctrl_Admin
    {
        public string LogType { get { return RequestEx["LogType"]; } }
        //public IActionResult LogManage()
        //{
        //    return View();
        //}
        public IActionResult TxtLog()
        {
            DataTable dt = FileSystemObject.GetDirectoryInfos(function.VToP(GetLogPath()), FsoMethod.File);
            dt.DefaultView.Sort = "CreateTime DESC";
            return View(dt);
        }
        public IActionResult TxtLogContent()
        {
            string fname = RequestEx["fname"];
            string logpath = GetLogPath() + fname;
            string text = SafeSC.ReadFileStr(logpath);
            if (text.Trim().Length < 30) { text = "该日志无记录"; }
            else { text = text.Replace("\r\n", "<br />"); }
            ViewBag.text = text;
            return View();
        }
        public IActionResult TxtLog_Down()
        {
            string fname = RequestEx["fname"];
            string logpath = GetLogPath() + fname;
            //SafeSC.DownFile(logpath);
            var fs = new FileStream(function.VToP(logpath), FileMode.Open);
            return File(fs, "text/plain", fname);
        }
        private string GetLogPath()
        {
            return "/Log/" + LogType + "/";
        }
        //-----------------------
        B_Content_ScheTask scheBll = new B_Content_ScheTask();
        M_Content_ScheTask scheMod = new M_Content_ScheTask();
        public IActionResult TaskList()
        {
            PageSetting setting = scheBll.SelPage(CPage, PSize, new Com_Filter() { });
            if (Request.IsAjax())
            {
                return PartialView("Task_List", setting);
            }
            //dt.Columns.Add(new DataColumn("IsRun", typeof(string)));
            //foreach (var model in TaskCenter.TaskList)
            //{
            //    DataRow dr = dt.NewRow();
            //    dr["ID"] = model.scheMod.ID;
            //    dr["TaskName"] = model.scheMod.TaskName;
            //    dr["LastTime"] = model.scheMod.LastTime;
            //    dr["ExecuteType"] = model.scheMod.ExecuteType;
            //    dr["ExecuteTime"] = model.scheMod.ExecuteTime;
            //    dr["CDate"] = model.scheMod.CDate;
            //    dr["Interval"] = model.scheMod.Interval;
            //    dr["Remind"] = model.scheMod.Remind;
            //    dr["IsRun"] = model.flag ? "运行中" : "已停止";
            //    dt.Rows.Add(dr);
            //}
            return View(setting);
        }
        public IActionResult TaskAdd()
        {
            if (Mid > 0)
            {
                scheMod = scheBll.GetModel(Mid);
            }
            return View(scheMod);
        }
        public IActionResult TaskAdd_Submit()
        {
            if (Mid > 0) { scheMod = scheBll.GetModel(Mid); }
            scheMod.TaskName = RequestEx["TaskName_T"];
            //scheMod.ExecuteTime = ExecuteTime_T1.Text;
            scheMod.Interval = DataConvert.CLng(RequestEx["Interval_T"]);
            string taskContent = RequestEx["TaskContent_T"].Trim();
            if (taskContent.StartsWith("/"))//任务内容为脚本路径时
            {
                if (!System.IO.File.Exists(function.VToP(taskContent))) { return WriteErr("脚本不存在"); }
                else { scheMod.TaskContent = taskContent; }
            }
            else
            {
                scheMod.TaskContent = taskContent;
            }
            scheMod.Remind = RequestEx["Remind_T"];
            //任务类型不允许修改
            if (Mid <= 0) { scheMod.TaskType = DataConvert.CLng(Request.Form["taskType_rad"]); }
            scheMod.ExecuteType = DataConvert.CLng(Request.Form["executeType_rad"]);
            scheMod.Status = DataConvert.CLng(Request.Form["status_rad"]);
            if (scheMod.ExecuteType == (int)M_Content_ScheTask.ExecuteTypeEnum.Interval)
            {
                if (scheMod.Interval <= 0) { return WriteErr("未指定正确的间隔时间"); }
            }
            else if (scheMod.ExecuteType == (int)M_Content_ScheTask.ExecuteTypeEnum.JustOnce)
            {
                scheMod.ExecuteTime = RequestEx["ExecuteTime_T1"];
                if (DataConvert.CDate(scheMod.ExecuteTime) < DateTime.Now) { return WriteErr("执行时间无效"); }
            }
            else if (scheMod.ExecuteType == (int)M_Content_ScheTask.ExecuteTypeEnum.EveryDay)
            {
                scheMod.ExecuteTime = RequestEx["ExecuteTime_T2"];
            }
            else if (scheMod.ExecuteType == (int)M_Content_ScheTask.ExecuteTypeEnum.EveryMonth)
            {
                scheMod.ExecuteTime = RequestEx["ExecuteTime_T1"];
            }
            if (Mid > 0)
            {
                scheBll.Update(scheMod);
            }
            else
            {
                scheMod.CDate = DateTime.Now;
                scheMod.AdminID = adminMod.AdminId;
                scheMod.ID = scheBll.Add(scheMod);
            }
            HFHelper.AddTask(scheMod);
            return WriteOK("操作成功", "TaskList");
        }
        public ContentResult Task_API()
        {
            scheMod = scheBll.SelReturnModel(DataConvert.CLng(ids));
            if (scheMod == null) { return Content("任务不存在"); }
            switch (action)
            {
                case "del":
                    {
                        scheBll.Delete(scheMod.ID);
                        RecurringJob.RemoveIfExists(scheMod.TaskFlag);
                    }
                    break;
                case "stop":
                    {
                        if (scheMod.Status != -1)
                        {
                            scheMod.Status = -1;
                            scheBll.Update(scheMod);
                        }
                        RecurringJob.RemoveIfExists(scheMod.TaskFlag);
                    }
                    break;
                case "start":
                    {
                        if (scheMod.Status == -1)
                        {
                            scheMod.Status = 0;
                            scheBll.Update(scheMod);
                            HFHelper.AddTask(scheMod);
                        }
                    }
                    break;
                case "execute":
                    {
                        BackgroundJob.Enqueue<HF_Task_ExecuteSql>(x => x.Execute(scheMod));
                    }
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult TaskCenter() { return Redirect("/Admin/Hangfire/"); }
    }
    public class HFHelper
    {
        public static void AddTask(M_Content_ScheTask scheMod)
        {
            if (scheMod.ID < 1) { throw new Exception("任务未正确存储"); }
            switch (scheMod.ExecuteType)
            {
                case (int)M_Content_ScheTask.ExecuteTypeEnum.JustOnce:
                    BackgroundJob.Enqueue<HF_Task_ExecuteSql>(x => x.Execute(scheMod));
                    break;
                case (int)M_Content_ScheTask.ExecuteTypeEnum.EveryDay:
                    RecurringJob.AddOrUpdate<HF_Task_ExecuteSql>(scheMod.TaskFlag, x => x.Execute(scheMod), Cron.DayInterval(1));
                    break;
                case (int)M_Content_ScheTask.ExecuteTypeEnum.EveryMonth:
                    RecurringJob.AddOrUpdate<HF_Task_ExecuteSql>(scheMod.TaskFlag, x => x.Execute(scheMod), Cron.Monthly);
                    break;
                case (int)M_Content_ScheTask.ExecuteTypeEnum.Interval:
                    RecurringJob.AddOrUpdate<HF_Task_ExecuteSql>(scheMod.TaskFlag, x => x.Execute(scheMod), Cron.MinuteInterval(Convert.ToInt32(scheMod.ExecuteTime)));
                    break;
                case (int)M_Content_ScheTask.ExecuteTypeEnum.Passive:
                    break;
            }
        }
    }
    public class HF_Task_Base
    {
        public void AddToLog(M_Content_ScheTask scheMod)
        {
            scheMod.LastTime = DateTime.Now.ToString();
            DBCenter.UpdateSQL(scheMod.TbName, "LastTime='" + DateTime.Now + "'", "ID=" + scheMod.ID);
            //M_Content_ScheLog logMod = new M_Content_ScheLog();
            //logMod.TaskID = scheMod.ID;
            //logMod.TaskName = scheMod.TaskName;
            //logMod.Result = 1;
            //new B_Content_ScheLog().Insert(logMod);
        }
        public virtual Task Execute(M_Content_ScheTask scheMod){ return Task.CompletedTask; }
    }
    public class HF_Task_ExecuteSql:HF_Task_Base
    {
        public override Task Execute(M_Content_ScheTask scheMod)
        {
            if (scheMod.TaskContent.StartsWith("/")) //若以'/'或'\'开头则为脚本
            {
                DBHelper.ExecuteSqlScript(DBCenter.DB.ConnectionString, function.VToP(scheMod.TaskContent));
            }
            else
            {
                SqlHelper.ExecuteSql(scheMod.TaskContent);
            }
            AddToLog(scheMod);
            return Task.CompletedTask;
        }
    }
}