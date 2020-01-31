using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Design;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Design;
using ZoomLa.SQLDAL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Controllers
{
    public class M_SubOption
    {
        public int qid = 0;
        public string answer = "";
    }
    public class VoteController : Ctrl_User
    {
        B_Design_Ask askBll = new B_Design_Ask();
        B_Design_Question questBll = new B_Design_Question();
        B_Design_Answer ansBll = new B_Design_Answer();
        B_Design_AnsDetail ansdeBll = new B_Design_AnsDetail();
        public IActionResult Index()
        {
            M_Design_Ask askMod = askBll.SelReturnModel(Mid);
            if (!CheckAsk(askMod, ref err)) { return WriteErr(err); return null; }
            DataTable questDT = questBll.Sel(askMod.ID, "qlist");
            ViewBag.questDT = questDT;
            ViewBag.Mode = "user";
            return View(askMod);
        }
        public string Vote_Submit()
        {
            M_Design_Ask askMod = askBll.SelReturnModel(Mid);
            if (askMod == null) { return "问卷不存在"; }
            if (askMod.IsEnableVCode == 1)
            {
                if (!ZoomlaSecurityCenter.VCodeCheck(GetParam("vkey"), GetParam("vcode"))) { return "验证码不正确"; }
            }
            if (!CheckAsk(askMod, ref err)) { return err; }
            M_Design_Answer ansMod = new M_Design_Answer();
            ansMod.AskID = askMod.ID;
            //ansMod.Answer = Req("answer");//{qid:1,answer:'is answer'}
            ansMod.Answer = GetParam("answer");
            ansMod.UserID = mu.UserID;
            ansMod.IP =IPScaner.GetUserIP(HttpContext);
            //ansMod.Source = DeviceHelper.GetBrower().ToString();
            ansMod.ID = ansBll.Insert(ansMod);
            //-----------单独写入表中,便于后期分析(后期优化为批量插入)
            List<M_SubOption> ansList = JsonConvert.DeserializeObject<List<M_SubOption>>(GetParam("answer"));
            foreach (M_SubOption ans in ansList)
            {
                M_Design_AnsDetail ansdeMod = new M_Design_AnsDetail();
                ansdeMod.AskID = ansMod.AskID;
                ansdeMod.AnsID = ansMod.ID;
                ansdeMod.Qid = ans.qid;
                ansdeMod.Answer = ans.answer;
                ansdeMod.UserID = ansMod.UserID;
                ansdeBll.Insert(ansdeMod);
            }
            return Success.ToString();
        }
        public void VoteResult()
        {
            Response.Redirect("/Plugins/Vote/VoteResult?id=" + Mid, true);
        }
        public IActionResult VoteResultAdmin()
        {
            return View();
        }
        //-------------
        //检测问卷可否被提交
        private bool CheckAsk(M_Design_Ask askMod, ref string err)
        {
            string ip =IPScaner.GetUserIP(HttpContext);
            if (askMod == null) { err = "问卷不存在"; return false; }
            if (askMod.IsNeedLogin == 1 && mu.IsNull) { err = "[" + askMod.Title + "]必须登录后才可访问"; return false; }
            if (askMod.ZStatus != (int)ZLEnum.ConStatus.Audited) { err = "[" + askMod.Title + "]未开放"; return false; }
            if (askMod.StartDate >= DateTime.Now) { err = "[" + askMod.Title + "]尚未到开放时间"; return false; }
            if (askMod.EndDate <= DateTime.Now) { err = "[" + askMod.Title + "]已经结束"; return false; }
            if (askMod.IsIPLimit > 0)
            {
                //if (ip.StartsWith("192.168") || ip.Equals("::1")) {  }
                List<SqlParameter> sp = new List<SqlParameter>() {
                new SqlParameter("ip", ip),
                new SqlParameter("sdate",DateTime.Now.ToString("yyyy/MM/dd 00:00:00")),
                new SqlParameter("edate",DateTime.Now.ToString("yyyy/MM/dd 23:59:59"))};
                int count = DBCenter.Count(ansBll.TbName, "IP=@ip AND AskID=" + askMod.ID + " AND (CDate>@sdate AND CDate<@edate)", sp);
                if (count >= askMod.IsIPLimit) { err = "系统限定：一天只能提交" + askMod.IsIPLimit + "份,请明天再来"; return false; }
            }
            if (askMod.IPInterval > 0)
            {
                //取最近的一条记录
                M_Design_Answer ansMod = ansBll.SelModelByIP(ip);
                if (ansMod == null || (DateTime.Now - ansMod.CDate).TotalSeconds > askMod.IPInterval)
                {

                }
                else
                {
                    err = "系统限定:每隔" + DateHelper.ShowSeconds(askMod.IPInterval) + "才可再次提交";
                    return false;
                }
            }
            return true;
        }
        //------------------------------后台管理
        public IActionResult ResultAdd()
        {
            if (!AdminCheck()) { return null; }
            M_Design_Ask askMod = askBll.SelReturnModel(Mid);
            DataTable questDT = questBll.Sel(askMod.ID, "qlist");
            ViewBag.questDT = questDT;
            ViewBag.Mode = "admin";
            return View("Index", askMod);
        }
        public string ResultAdd_Submit()
        {
            if (!AdminCheck()) { return "无权访问"; }
            int ansnum = DataConvert.CLng(GetParam("ansnum"));
            if (ansnum < 1) { return "投票人数不正确"; }
            //[后期改为附加虚拟值的方式]
            M_Design_Ask askMod = askBll.SelReturnModel(Mid);
            if (askMod == null) { return "问卷不存在"; }
            //--------------------------------
            M_Design_Answer ansMod = new M_Design_Answer();
            ansMod.AskID = askMod.ID;
            //ansMod.Answer = Req("answer");//{qid:1,answer:'is answer'}
            ansMod.Answer = GetParam("answer");
            ansMod.UserID = -1;
            ansMod.IP =IPScaner.GetUserIP(HttpContext);
            ansMod.Source = "管理员添加";
            for (int i = 0; i < ansnum; i++)
            {
                ansMod.ID = 0;
                ansMod.ID = ansBll.Insert(ansMod);
                //-----------单独写入表中,便于后期分析(后期优化为批量插入)
                List<M_SubOption> ansList = JsonConvert.DeserializeObject<List<M_SubOption>>(GetParam("answer"));
                foreach (M_SubOption ans in ansList)
                {
                    M_Design_AnsDetail ansdeMod = new M_Design_AnsDetail();
                    ansdeMod.AskID = ansMod.AskID;
                    ansdeMod.AnsID = ansMod.ID;
                    ansdeMod.Qid = ans.qid;
                    ansdeMod.Answer = ans.answer;
                    ansdeMod.UserID = ansMod.UserID;
                    ansdeBll.Insert(ansdeMod);
                }
            }
            return Success.ToString();
        }
        //用户的回答
        public IActionResult ShowAnswer()
        {
            if (!AdminCheck()) { return null; }
            M_Design_Ask askMod = askBll.SelReturnModel(Mid);
            M_Design_Answer ansMod = ansBll.SelReturnModel(DataConvert.CLng(GetParam("ansid")));
            DataTable questDT = questBll.Sel(askMod.ID, "qlist");
            ViewBag.questDT = questDT;
            ViewBag.Mode = "answer";
            ViewBag.ansMod = ansMod;
            if (ansMod == null) { return WriteErr("传入的参数不正确"); }
            return View("Index", askMod);
        }
        private bool AdminCheck()
        {
            M_AdminInfo adminMod = B_Admin.GetLogin(HttpContext);
            if (adminMod == null) { throw new Exception("你无权访问该页面");  }
            return true;
        }
    }
}
