using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Controllers
{
    public class AskController : Ctrl_User
    {
        B_Ask askBll = new B_Ask();
        B_GuestAnswer ansBll = new B_GuestAnswer();
        B_AskCommon askComBll = new B_AskCommon();
        B_TempUser _tuBll = null;
        B_TempUser tuBll
        {
            get
            {
                if (_tuBll == null) { _tuBll = new B_TempUser(HttpContext); }
                return _tuBll;
            }
        }
        public string quetype { get { return Request.GetParam("quetype"); } }
        //------------------------------------------------
        private bool AskAuth()
        {
            M_UserInfo mu = buser.GetLogin(false);
            //用户组查看权限
            if (!string.IsNullOrEmpty(GuestConfig.GuestOption.WDOption.selGroup))
            {
                string groups = "," + GuestConfig.GuestOption.WDOption.selGroup + ",";
                if (!groups.Contains("," + mu.GroupID.ToString() + ","))
                {
                    return false;
                }
            }
            return  true;
        }
        private DataTable GetQueTypeDT()
        {
            return new B_GradeOption().GetGradeList(2, 0); 
        }

        #region 
        public IActionResult SearchList()
        {
            PageSetting setting = askBll.SelWaitQuest_SPage(CPage, 10, quetype, 2, HttpUtility.HtmlEncode(RequestEx["strWhere"]));
            if (Request.IsAjaxRequest()) { return PartialView("SearchList_List", setting); }
            ViewBag.islogin = buser.CheckLogin();
            return View(setting);
        }
        public IActionResult CommonView()
        {
            return View("comp/CommonView");
        }
        //问题库
        public IActionResult List()
        {
            AskAuth();
            int type = DataConverter.CLng(RequestEx["type"]);
            PageSetting setting = askBll.SelWaitQuest_SPage(CPage, 10, quetype, type);
            if (Request.IsAjaxRequest()) { return PartialView("List_List", setting); }
            return View(setting);
        }
        //分类大全
        public IActionResult Classification()
        {
            AskAuth();
            ViewBag.askDt = new B_GradeOption().GetGradeList(2, DataConverter.CLng(RequestEx["ParentID"]));
            return View(new PageSetting() { itemCount = 0 });
        }
        public PartialViewResult Classification_Data()
        {
            return PartialView("Classification_List", new PageSetting() { itemCount = 0 });
        }
        //问答专家
        public IActionResult Star()
        {
            AskAuth();
            DataTable dt = SqlHelper.ExecuteTable(CommandType.Text, "select top 10 a.* from (select a.UserID as cuid,count(a.UserID) as ccount from ZL_User a,ZL_GuestAnswer b where b.Status=1 and a.UserID=b.UserID group by(a.UserID)) c,ZL_User a where c.cuid=a.UserID order by c.ccount desc", null);//取被采纳问题数前十为知道之星
            return View(dt);
        }
        public IActionResult SearchDetails()
        {
            AskAuth();
            M_Ask askMod = askBll.SelReturnModel(Mid);
            if (askMod == null) { return WriteErr("问题不存在"); return Content(""); }
            ViewBag.dt1 = ansBll.Sel(" QueId=" + Mid + " And Status=1", " Status desc", null);
            ViewBag.dt2 = ansBll.Sel(" QueId=" + Mid + " And Status=0", " AddTime desc", null);
            ViewBag.dt3 = askBll.Sel("Status=1", "Addtime desc", null);
            return View(askMod);
        }
        //赞同
        public int Approval(string id)
        {
            M_UserInfo mu = buser.GetLogin();
            int Mid = DataConverter.CLng(id);
            DataTable dt = askComBll.U_SelByAnswer(Mid, mu.UserID, 0);
            if (dt.Rows.Count > 0) { return 1; }
            else
            {
                DataTable dt2 = ansBll.Sel("ID=" + id, "", null);
                M_AskCommon askcomMod = new M_AskCommon();
                askcomMod.AskID = DataConverter.CLng(dt2.Rows[0]["QueID"]);
                askcomMod.AswID = Mid;
                askcomMod.UserID = mu.UserID;
                askcomMod.Content = "赞同";
                askcomMod.AddTime = DateTime.Now;
                askcomMod.Type = 0;
                int flag = askComBll.insert(askcomMod);
                if (flag == 1) { return 2; }
                else { return 3; }
            }
        }
        //评论
        public IActionResult Comment()
        {
            M_UserInfo mu = buser.GetLogin();
            int Mid = DataConverter.CLng(Request.Form["mid"]);
            DataTable dt = ansBll.Sel("ID=" + Mid, "", null);
            M_AskCommon askcomMod = new M_AskCommon();
            askcomMod.AskID = DataConverter.CLng(dt.Rows[0]["QueID"]);
            askcomMod.AswID = Mid;
            askcomMod.UserID = mu.UserID;
            askcomMod.Content = Request.Form["txtSupplyment"];
            askcomMod.AddTime = DateTime.Now;
            askcomMod.Type = 1;
            int Tid = DataConvert.CLng(RequestEx["ID"]);
            if (askComBll.insert(askcomMod) == 1)
            {
                return WriteOK("评论成功", "SearchDetails?ID=" + Tid); 
            }
            else
            {
                return WriteErr("评论失败", "SearchDetails?ID=" + Tid); 
            }
        }
        #endregion
        public IActionResult Default()
        {
            AskAuth();
            ViewBag.solveDt = askBll.Sel("Status=2", "AddTime desc", null);
            ViewBag.hotDt = askBll.SelfieldOrd("QueType", 10);//根据被采纳问题数取知道之星
            return View();
        }
        public IActionResult MoreProblems()
        {
            AskAuth();
            if (!buser.CheckLogin()) { Response.Redirect("/User/Login?ReturnUrl=/Ask/Add"); return Content(""); }
            string type = RequestEx["type"] ?? "";
            PageSetting setting = askBll.SelPage(CPage, 10, new Com_Filter() { });
            if (Request.IsAjaxRequest()) { return PartialView("AskController", setting); }
            return View(setting);
        }
        //问题发起人操作
        public IActionResult Add()
        {
            ViewBag.title = HttpUtility.HtmlEncode(RequestEx["strWhere"]);
            ViewBag.typeDt = GetQueTypeDT();
            return View();
        }
        
        [HttpPost]
        //添加提问
        public IActionResult Add_Submit()
        {
            if (GuestConfig.GuestOption.WDOption.IsLogin && mu.IsNull) {return  RedirectToAction("Login", "", new { area = "User", ReturnUrl = "/Ask/Add" }); }
            int score = DataConverter.CLng(Request.Form["ddlScore"]);
            if (mu.UserID > 0 && mu.UserExp < score) { return WriteErr("积分不足");  }

            //---------------
            M_Ask askMod = new M_Ask();
            askMod.QTitle = RequestEx["QTitle"];
            askMod.Qcontent = RequestEx["QContent"].Trim();
            askMod.UserId = mu.UserID;
            askMod.UserName = mu.IsNull ? "游客" : mu.UserName;
            askMod.Score = score;
            askMod.IsNi = DataConvert.CLng(Request.Form["isAnony_chk"]);
            askMod.IsSole = DataConvert.CLng(Request.Form["isSole_chk"]);
            askMod.QueType = Request.Form["QueType"];
            //if (string.IsNullOrEmpty(askMod.QueType)) { askMod.QueType = Request.Form["ddlCate"]; }
            askMod.ID = askBll.insert(askMod);
            if (!mu.IsNull)
            {
                if (score > 0)
                {
                    //悬赏积分
                    buser.MinusVMoney(mu.UserID, score, M_UserExpHis.SType.Point, mu.UserName + "提交问题[" + askMod.Qcontent + "],扣除悬赏积分-" + score);
                }
                if (GuestConfig.GuestOption.WDOption.QuestPoint > 0)
                {
                    buser.AddMoney(mu.UserID, GuestConfig.GuestOption.WDOption.QuestPoint, ((M_UserExpHis.SType)(Enum.Parse(typeof(M_UserExpHis.SType), GuestConfig.GuestOption.WDOption.PointType))), mu.UserName + "提交问题[" + askMod.Qcontent + "],增加问答积分" + GuestConfig.GuestOption.WDOption.QuestPoint);
                }
            }
            return RedirectToAction("AddSuccess");
        }
        public IActionResult AddSuccess()
        {
            AskAuth();
            ViewBag.conflogin = GuestConfig.GuestOption.WDOption.IsLogin;
            return View("AddSuccess");
        }
        public IActionResult MyAskList()
        {
            AskAuth();
            PageSetting setting = askBll.SelMyAsk_SPage(CPage, 10, buser.GetLogin().UserID, quetype);
            if (Request.IsAjaxRequest()) { return PartialView("List_List", setting); }
            return View(setting);
        }
        public void Interactive()
        {
            Response.Redirect("AskDetail?ID=" + Mid);
        }
        //问题的发起人修改问题
        public IActionResult AskDetail()
        {
            //AskAuth();
            M_UserInfo mu = buser.GetLogin();
            M_Ask askMod = askBll.SelReturnModel(Mid);
            if (askMod == null) { return WriteErr("问题不存在"); return null; }
            if (askMod.UserId != mu.UserID)
            {
                Response.Redirect("/Ask/MyAnswer?ID=" + askMod.ID);
            }
            ViewBag.ansDt = ansBll.Sel(" QueId=" + Mid + " And supplymentid=0", " Status desc", null);
            return View(askMod);
        }
        //补充问题
        [HttpPost]
        
        public IActionResult Supple()
        {
            int Tid = DataConvert.CLng(RequestEx["ID"]);
            askBll.UpdateByField("Supplyment", Request.Form["Txtsupment"], Tid);
            return WriteOK("提交成功", "Interactive?ID=" + Tid);
        }
        //提交追问
        [HttpPost]
        
        public IActionResult SuppleAsk()
        {
            M_UserInfo mu = buser.GetLogin();
            M_GuestAnswer ansMod = new M_GuestAnswer();
            ansMod.QueId = DataConverter.CLng(RequestEx["ID"]);
            ansMod.Content = this.Request.Form["txtSupplyment"];
            ansMod.AddTime = DateTime.Now;
            ansMod.UserId = buser.GetLogin().UserID;
            ansMod.UserName = buser.GetLogin().UserName;
            ansMod.Status = 0;
            ansMod.audit = 0;
            ansMod.supplymentid = DataConverter.CLng(Request.Form["Rid"]);
            ansBll.insert(ansMod);
            return WriteOK("追问成功!", "Interactive?ID=" + ansMod.QueId); 
        }
        //推荐为满意答案
        public IActionResult Recomand()
        {
            M_GuestAnswer ansMod = ansBll.SelReturnModel(DataConverter.CLng(RequestEx["aid"]));
            M_UserInfo ansUser = buser.SelReturnModel(ansMod.UserId);//回答人用户模型
            if (ansMod == null) { return WriteErr("回答不存在");  }
            M_Ask askMod = askBll.SelReturnModel(Mid);
            if (askMod == null) { return WriteErr("问题不存在");  }
            if (ansMod.QueId != askMod.ID) { return WriteErr("问题与答案不匹配"); }
            //---------------------------------------
            ansMod.Status = 1;//满意答案
            DBCenter.UpdateSQL(ansMod.TbName, "[Status]=1", "ID=" + ansMod.ID);
            askMod.Status = 2;//问题状态设置为已解决
            DBCenter.UpdateSQL(askMod.TbName, "[Status]=2", "ID=" + askMod.ID);

            if (askMod.Score > 0)
            {
                buser.ChangeVirtualMoney(ansMod.UserId, new M_UserExpHis()//悬赏积分
                {
                    score = askMod.Score,
                    ScoreType = (int)M_UserExpHis.SType.Point,
                    detail = ansUser.UserName + "对问题[" + askMod.QTitle + "]的回答被推荐为满意答案,增加悬赏积分+" + askMod.Score
                });
            }
            buser.ChangeVirtualMoney(ansMod.UserId, new M_UserExpHis()//问答积分
            {
                score = GuestConfig.GuestOption.WDOption.WDRecomPoint,
                ScoreType = (int)((M_UserExpHis.SType)(Enum.Parse(typeof(M_UserExpHis.SType), GuestConfig.GuestOption.WDOption.PointType))),
                detail = ansUser.UserName + "对问题[" + askMod.QTitle + "]的回答被推荐为满意答案,增加问答积分+" + GuestConfig.GuestOption.WDOption.WDRecomPoint
            });
            return WriteOK("推荐成功！", "Interactive?ID=" + Mid);
        }
        //======================回答人操作
        //我的回答
        public IActionResult MyAnswerlist()
        {
            AskAuth();
            M_UserInfo mu = buser.GetLogin();
            string qids = ansBll.GetUserAnswerIDS(mu.UserID);
            if (string.IsNullOrEmpty(qids)) { return WriteErr("你没有对任何问题进行回答！请回答后在进行查看！", "/Ask/list"); return Content(""); }
            PageSetting setting = askBll.SelPage(CPage, 10, new Com_Filter() { ids = qids });
            if (Request.IsAjaxRequest()) { return PartialView("MyAnswerlist_List", setting); }
            ViewBag.islogin = buser.CheckLogin();
            return View(setting);
        }
        public IActionResult MyAnswer()
        {
            AskAuth();
            M_UserInfo mu = tuBll.GetLogin();
            //if (!string.IsNullOrEmpty(GuestConfig.GuestOption.WDOption.ReplyGroup))
            //{
            //    //用户组回复权限
            //    string groups = "," + GuestConfig.GuestOption.WDOption.ReplyGroup + ",";
            //    if (!groups.Contains("," + mu.GroupID.ToString() + ","))
            //    { return WriteErr("您没有回复问题的权限!");return Content(""); }
            //}
            M_Ask askMod = askBll.SelReturnModel(Mid);
            if (askMod == null) { return WriteErr("问题不存在!");return Content(""); }
            if (askMod.Status == 0) { return WriteErr("该问题未经过审核,无法对其答复!", "/Ask/List"); return Content(""); }
            if (askMod.UserId == mu.UserID) { Response.Redirect("Interactive?ID=" + Mid); return Content(""); }
            //----检测自己是否有回复过该问题
            M_GuestAnswer ansMod = ansBll.SelModelByAsk(askMod.ID, mu.UserID);
            if (ansMod == null) { ansMod = new M_GuestAnswer(); }
            ViewBag.ansMod = ansMod;
            return View(askMod);
        }
        //每个用户在一个问题下,只能有一个回答
        
        public IActionResult Answer_Submit()
        {
            M_UserInfo tmu = tuBll.GetLogin();
            if (GuestConfig.GuestOption.WDOption.IsReply && tmu.UserID <= 0)
            {
                Response.Redirect("/User/Login?ReturnUrl=/Ask/MyAnswer"); 
            }
            M_GuestAnswer ansMod = ansBll.SelModelByAsk(Mid, tmu.UserID);
            if (ansMod == null || tmu.UserID < 1) { ansMod = new M_GuestAnswer(); }
            //-----------------------
            ansMod.UserId = tmu.UserID;
            ansMod.Content = StringHelper.Strip_Script(Request.Form["ans_content"]);
            ansMod.QueId = Mid;
            ansMod.UserName = tmu.UserID > 0 ? tmu.UserName : tmu.UserName + "[" + tmu.WorkNum + "]";
            ansMod.IsNi = DataConvert.CLng(Request.Form["ans_IsNi"]);
            if (ansMod.ID < 1)
            {
                ansMod.ID = ansBll.insert(ansMod);
            }
            else
            {
                ansBll.UpdateByID(ansMod);
            }
            if (tmu.UserID > 0)
            {
                M_Ask askMod = askBll.SelReturnModel(ansMod.QueId);
                buser.ChangeVirtualMoney(tmu.UserID, new M_UserExpHis()
                {
                    score = GuestConfig.GuestOption.WDOption.WDPoint,
                    ScoreType = (int)((M_UserExpHis.SType)(Enum.Parse(typeof(M_UserExpHis.SType), GuestConfig.GuestOption.WDOption.PointType))),
                    detail = string.Format("{0} {1}在问答中心回答了{2}问题,赠送{3}分", DateTime.Now, tmu.UserName, "", GuestConfig.GuestOption.WDOption.WDPoint)
                });
            }
            return WriteOK("回答成功", "List");
        }
    }
}
