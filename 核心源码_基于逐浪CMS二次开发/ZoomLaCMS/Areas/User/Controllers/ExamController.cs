using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Exam;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLaCMS.Control;
using ZoomLa.Model;
using ZoomLa.Model.Exam;
using ZoomLa.Safe;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Models.Exam;
using BH = ZoomLa.BLL.Helper;
using Microsoft.AspNetCore.Authorization;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class ExamController : Ctrl_User
    {
        B_GradeOption gradeBll = new B_GradeOption();
        B_Exam_Sys_Questions questBll = new B_Exam_Sys_Questions();
        B_Exam_Sys_Papers paperBll = new B_Exam_Sys_Papers();
        B_Exam_Class clsBll = new B_Exam_Class();
        B_Exam_Version verBll = new B_Exam_Version();
        B_Exam_Answer ansBll = new B_Exam_Answer();
        B_Questions_Knowledge knowBll = new B_Questions_Knowledge();
        B_TempUser _tuBll = null;
        B_TempUser tuserBll
        {
            get
            {
                if (_tuBll == null) { _tuBll = new B_TempUser(HttpContext); }
                return _tuBll;
            }
        }
        B_Temp tempBll = new B_Temp();
        B_Admin badmin = new B_Admin();
        B_School schBll = new B_School();
        private int QType { get { return string.IsNullOrEmpty(RequestEx["qtype"]) ? 99 : DataConverter.CLng(RequestEx["qtype"]); } }//题目类型
        private int NodeID { get { return DataConverter.CLng(RequestEx["NodeID"]); } }
        public void Index()
        {
            RedirectToAction("QuestList");
        }
        #region 试卷相关
        public IActionResult AddPaper()
        {
            M_Exam_Sys_Papers paperMod = new M_Exam_Sys_Papers();
            if (Mid > 0)
            {
                paperMod = paperBll.GetSelect(Mid);
                if (mu.UserID != paperMod.UserID) { return WriteErr("你无权修改该试卷"); return Content(""); }
            }
            else
            {
                paperMod.p_BeginTime = DateTime.Now;
                paperMod.p_endTime = DateTime.Now.AddMonths(1);
            }
            C_TreeTlpDP treeMod = new C_TreeTlpDP()
            {
                F_ID = "C_id",
                F_Name = "C_ClassName",
                F_Pid = "C_Classid",
                dt = clsBll.Select_All(),
                seled = paperMod.p_class.ToString()
            };
            ViewBag.treeMod = treeMod;
            return View(paperMod);
        }
        public IActionResult Paper_Add()
        {
            M_Exam_Sys_Papers paperMod = new M_Exam_Sys_Papers();
            if (Mid > 0)
            {
                paperMod = paperBll.SelReturnModel(Mid);
                if (paperMod.UserID != mu.UserID) { return WriteErr("你无权修改该试卷");  }
            }
            paperMod.p_name = RequestEx["p_name"];
            paperMod.p_class = DataConvert.CLng(RequestEx["TreeTlp_Hid"]);
            paperMod.p_type = DataConverter.CLng(RequestEx["p_type"]);
            paperMod.p_Remark = RequestEx["p_Remark"];
            paperMod.p_UseTime = DataConverter.CDouble(RequestEx["p_UseTime"]);
            paperMod.p_BeginTime = DataConverter.CDate(RequestEx["p_BeginTime"]);
            paperMod.p_endTime = DataConverter.CDate(RequestEx["p_endTime"]);
            paperMod.p_Style = DataConverter.CLng(RequestEx["p_Style"]);
            paperMod.TagKey = RequestEx["tabinput"];
            if (paperMod.id > 0)
            {
                paperMod.UserID = mu.UserID;
                paperBll.UpdateByID(paperMod);
            }
            else
            {
                paperBll.Insert(paperMod);
            }
            return WriteOK("操作成功!", "Papers_System_Manage"); 
        }
        public IActionResult paper()
        {
            M_Exam_Sys_Papers paperMod = paperBll.SelReturnModel(Mid);
            DataTable QuestDT = questBll.SelByIDSForExam(paperMod.QIDS, paperMod.id);//获取问题,自动组卷则筛选合适的IDS
            DataTable typeDT = ansBll.GetTypeDT(QuestDT);
            ViewBag.pname = paperMod.p_name;
            ViewBag.questDt = QuestDT;
            ViewBag.typeDt = typeDT;
            return View();
        }
        //试卷下题目管理
        public IActionResult Paper_QuestionManage()
        {
            int Pid = DataConverter.CLng(RequestEx["pid"]);
            int QType = DataConverter.CLng(RequestEx["qtype"], 99); //题目类型
            M_Exam_Sys_Papers paperMod = paperBll.GetSelect(Pid);
            PageSetting setting = new PageSetting();
            if (!string.IsNullOrEmpty(paperMod.QIDS)) { setting = questBll.SelByIDS(CPage, PSize, paperMod.QIDS, QType, "*"); }
            else { setting.dt = new DataTable(); }
            ViewBag.paperMod = paperMod;
            ViewBag.QType = QType;
            return View(setting);
        }
        public IActionResult PaperCenter()
        {
            M_Temp tempMod = tempBll.SelModelByUid(mu.UserID, 10);
            DataTable questDt = questBll.SelByIDSForExam(tempMod.Str1);
            DataTable typeDt = ansBll.GetTypeDT(questDt);
            ViewBag.title = DateTime.Now.ToString("yyyy年MM月dd日" + mu.UserName + "的组卷");
            ViewBag.questDt = questDt;
            ViewBag.typeDt = typeDt;
            return View();
        }
        public IActionResult PaperCenter_Submit()
        {
            M_Temp tempMod = tempBll.SelModelByUid(mu.UserID, 10);
            M_Exam_Sys_Papers paperMod = new M_Exam_Sys_Papers();
            if (string.IsNullOrEmpty(tempMod.Str1)) { return WriteErr("试题蓝为空,无法生成试卷!"); return null; }
            paperMod.p_name = RequestEx["title_t"];
            paperMod.p_class = 0;
            paperMod.p_Remark = RequestEx["desc_t"];
            paperMod.p_UseTime = DataConverter.CLng(RequestEx["usetime_t"]);
            paperMod.p_BeginTime = DateTime.Now;
            paperMod.p_endTime = DateTime.Now.AddYears(1);
            paperMod.p_Style = 2;
            paperMod.UserID = mu.UserID;
            paperMod.QIDS = tempMod.Str1;
            paperMod.QuestList = RequestEx["qinfo_hid"];
            paperMod.Price = DataConverter.CLng(RequestEx["price_t"]);
            int newid = paperBll.Insert(paperMod);
            //-------------------------
            tempMod.Str1 = "";
            tempBll.UpdateByID(tempMod);
            ViewBag.pname = paperMod.p_name;
            ViewBag.newid = newid;
            ViewBag.step = 2;
            return View("PaperCenter");
        }
        public IActionResult Papers_System_Manage()
        {
            PageSetting setting = paperBll.SelPage(CPage, PSize, -100, 0);
            if (Request.IsAjaxRequest())
            {
                return PartialView("Papers_System_Manage_List", setting);
            }
            return View(setting);
        }
        public void DownPaper()
        {
           
        }
        public int Paper_Del(string ids)
        {
            paperBll.DelByIDS(ids, mu.UserID);
            return Success;
        }
        //试卷合并
        public IActionResult Paper_Merge()
        {
            string ids = RequestEx["idchk"];
            if (string.IsNullOrEmpty(ids)) { return WriteErr("请先选定需要合并的试卷");  }
            DataTable dt = paperBll.SelByIDS(ids);
            if (dt.Rows.Count < 1) { return WriteErr("选定的试卷不存在");  }
            M_Exam_Sys_Papers paperMod = new M_Exam_Sys_Papers();
            paperMod.UserID = mu.UserID;
            paperMod.p_name = DateTime.Now.ToString("yyyyMMdd") + "合并试卷";
            paperMod.QIDS = "";
            foreach (DataRow dr in dt.Rows)
            {
                paperMod.QIDS += dr["QIDS"] + ",";
            }
            if (string.IsNullOrEmpty(paperMod.QIDS.Replace(",", ""))) { return WriteErr("试卷中没有添加题目,取消合并");  }
            paperMod.p_type = Convert.ToInt32(dt.Rows[0]["p_type"]);
            paperMod.p_class = Convert.ToInt32(dt.Rows[0]["p_class"]);
            paperMod.QIDS = StrHelper.RemoveDupByIDS(paperMod.QIDS);
            paperMod.QIDS = StrHelper.PureIDSForDB(paperMod.QIDS);
            paperBll.Insert(paperMod);
            return WriteOK("试卷合并成功");
            
        }
        //为试卷添加试题
        public int Paper_AddQids(int pid, string ids)
        {
            M_Exam_Sys_Papers paperMod = paperBll.GetSelect(pid);
            if (paperMod.UserID != mu.UserID) { return Failed; }
            paperMod.QIDS = string.Join(",", ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            paperBll.UpdateByID(paperMod);
            return Success;
        }
        //为试卷添加大题
        public string Paper_AddLarge(string ids)
        {
            DataTable dt = questBll.SelByIDSForType(ids);
            return JsonConvert.SerializeObject(dt);
        }
        #endregion
        #region 试题
        public IActionResult QuestList()
        {
            PageSetting setting = questBll.U_SelByFilter(CPage, PSize, NodeID, QType, "", mu.UserID, 0);
            C_TreeView treeMod = new C_TreeView()
            {
                NodeID = "C_id",
                NodeName = "C_ClassName",
                NodePid = "C_Classid",
                DataSource = clsBll.Select_All(),
                SelectedNode = RequestEx["NodeID"]
            };
            ViewBag.treeMod = treeMod;
            ViewBag.QType = QType;
            ViewBag.NodeID = NodeID;
            return View(setting);
        }
        //普通与专业版组卷
        public IActionResult QuestionManage()
        {
            var model = new VM_QuestManage(HttpContext);
            return View(model);
        }
        public void QuestRPT()
        {
            //Server.Transfer("/BU/QuestRPT", true);
        }
        [HttpPost]
        public string QuestionManage_API()
        {
            string action = RequestEx["action"];
            string result = "";
            var model = new VM_QuestManage(HttpContext);
            switch (action)
            {
                case "getknows":
                    {
                        DataTable dt = knowBll.Select_All(DataConvert.CLng(RequestEx["nodeid"]), -1, 1, RequestEx["knowname"]);
                        result = model.GetTreeStr(model.FillKnows(dt), 0, "knows");
                    }
                    break;
                case "GetKnowsByVersion":
                    {
                        int version = Convert.ToInt32(RequestEx["version"]);
                        M_Exam_Version verMod = verBll.SelReturnModel(version);
                        if (verMod == null || string.IsNullOrEmpty(verMod.Knows)) { result = "{}"; }
                        else
                        {
                            DataTable dt = knowBll.SelByIDS(verMod.Knows);
                            dt = dt.DefaultView.ToTable(false, "k_id,k_name".Split(','));
                            result = JsonConvert.SerializeObject(dt);
                        }
                    }
                    break;
                default:
                    break;
            }
            return result;
        }
        //public IActionResult QuestionManage2() {return View(); }
        //public IActionResult AddQuestion() { return View(); }
        public PartialViewResult Quest_Data()
        {
            PageSetting setting = questBll.U_SelByFilter(CPage, PSize, NodeID, QType, RequestEx["skey"], mu.UserID, 0);
            return PartialView("_qlist", setting);
        }
        public int Quest_Del(string ids)
        {
            questBll.DelByIDS(ids);//,mu.UserID
            return Success;
        }
        public IActionResult QuestAPI()
        {
            string action = RequestEx["action"] ?? "";
            int qid = DataConverter.CLng(RequestEx["qid"]);
            int qtype = DataConverter.CLng(RequestEx["qtype"]);
            string qids = (RequestEx["qids"] ?? "").Trim(',');
            while (qids.Contains(",,")) { qids.Replace(",,", ","); }
            int result = Failed;
            M_Temp tempMod = new M_Temp();
            switch (action)
            {
                case "cart_add"://试题篮
                    tempMod = GetCartByUid(mu.UserID);
                    tempMod.Str1 = StrHelper.AddToIDS(tempMod.Str1, qid.ToString());
                    AddORUpdate(tempMod);
                    result = Success;
                    break;
                case "cart_adds":
                    if (string.IsNullOrEmpty(qids)) { break; }
                    tempMod = GetCartByUid(mu.UserID);
                    foreach (string q in qids.Split(','))
                    {
                        if (string.IsNullOrEmpty(q)) continue;
                        tempMod.Str1 = StrHelper.AddToIDS(tempMod.Str1, q);
                    }
                    AddORUpdate(tempMod);
                    result = Success;
                    break;
                case "cart_remove":
                    tempMod = GetCartByUid(mu.UserID);
                    tempMod.Str1 = StrHelper.RemoveToIDS(tempMod.Str1, qid.ToString());
                    AddORUpdate(tempMod);
                    result = Success;
                    break;
                case "cart_clear":
                    tempMod = GetCartByUid(mu.UserID);
                    tempMod.Str1 = "";
                    AddORUpdate(tempMod);
                    result = Success;
                    break;
                case "collect_add"://试题收藏与移除
                    break;
                case "collect_remove":
                    break;
            }
            return Content(result.ToString());
        }
        private M_Temp GetCartByUid(int uid, int usetype = 10)
        {
            M_Temp tempMod = tempBll.SelModelByUid(uid, usetype);
            if (tempMod == null) { tempMod = new M_Temp(); tempMod.UserID = uid; tempMod.UseType = usetype; }
            return tempMod;
        }
        private void AddORUpdate(M_Temp model)
        {
            if (model.ID > 0) { tempBll.UpdateByID(model); }
            else { tempBll.Insert(model); }
        }
        public IActionResult AddEngLishQuestion()
        {
            VM_Question model = new VM_Question(HttpContext);
            if (mu.UserID != model.questMod.UserID) {return WriteErr("你无权限修改该试题"); }
            model.treeMod = GetTreeMod();
            model.treeMod.seled = model.questMod.p_Class.ToString();
            return View(model);
        }
        public IActionResult AddSmallQuest()
        {
            int Pid = DataConverter.CLng(RequestEx["pid"]);
            M_Exam_Sys_Questions questMod = questBll.GetSelect(Mid);
            if (questMod.p_id <= 0 && Pid > 0)
            {
                M_Exam_Sys_Questions pMod = questBll.GetSelect(Pid);
                questMod.p_Difficulty = pMod.p_Difficulty;
                questMod.p_Class = pMod.p_Class;
                questMod.Tagkey = pMod.Tagkey;
                questMod.IsShare = pMod.IsShare;
                questMod.p_defaultScores = pMod.p_defaultScores;
                questMod.Jiexi = pMod.Jiexi;
                questMod.Version = pMod.Version;
            }
            return View(questMod);
        }
        [HttpPost]
        
        public IActionResult SmallQuest_Add()
        {
            M_Exam_Sys_Questions questMod = Question_FillMod();
            questMod.IsSmall = 1;
            if (Mid > 0)
            {
                questBll.GetUpdate(questMod);
            }
            else
            {
                questMod.p_id = questBll.GetInsert(questMod);
            }
            DataTable dt = questBll.SelByIDSForType(questMod.p_id.ToString());
            string json = JsonConvert.SerializeObject(dt);
            return Content("<Script>parent.SelQuestion(" + json + "," + Mid + ");</Script>");
        }
        public IActionResult Question_Class_Manage() { return View(); }
        public IActionResult QuestOption() { return View(); }
        public IActionResult QuestShow() { return View(); }
        public IActionResult QuestView()
        {
            M_Exam_Sys_Questions questMod = questBll.GetSelect(Mid);
            return View(questMod);
        }
        [HttpPost]
        
        public IActionResult Question_Add()
        {
            var model = new VM_Question(HttpContext);
            M_Exam_Sys_Questions questMod = Question_FillMod();
            if (Mid > 0)
            {
                questBll.GetUpdate(questMod);
            }
            else
            {
                questMod.p_id = questBll.GetInsert(questMod);
            }
            SafeSC.WriteFile(questMod.GetOPDir(), RequestEx["Optioninfo_Hid"]);
            return WriteOK("操作成功!", "QuestList?NodeID=" + model.NodeID);
        }
        private M_Exam_Sys_Questions Question_FillMod()
        {
            M_Exam_Sys_Questions questMod = null;
            if (Mid > 0)
            {
                questMod = questBll.GetSelect(Mid);
            }
            else
            {
                questMod = new M_Exam_Sys_Questions();
                M_UserInfo mu = buser.GetLogin();
                questMod.UserID = mu.UserID;
                questMod.p_Inputer = mu.UserName;
            }
            questMod.p_title = RequestEx["p_title"];
            questMod.p_Difficulty = DataConverter.CDouble(RequestEx["p_Difficulty"]);
            questMod.p_Class = DataConverter.CLng(RequestEx["TreeTlp_hid"]);
            questMod.p_Views = DataConverter.CLng(RequestEx["p_Views"]);
            questMod.p_Knowledge = DataConverter.CLng(RequestEx["knowname"]);
            string tagkey = RequestEx["tabinput"];
            if (string.IsNullOrEmpty(tagkey))
            {
                questMod.Tagkey = "";
            }
            else
            {
                int firstid = clsBll.SelFirstNodeID(questMod.p_Class);
                questMod.Tagkey = knowBll.AddKnows(firstid, tagkey, 0);
            }
            questMod.p_Type = DataConverter.CLng(RequestEx["qtype_rad"]);
            questMod.p_shuming = RequestEx["p_shuming"] ?? RequestEx["p_Answer"];
            questMod.p_Answer = RequestEx["p_Answer"];
            if (questMod.p_Type == 10) { questMod.p_Content = RequestEx["Qids_Hid"]; questMod.LargeContent = RequestEx["p_Content"]; }
            else { questMod.p_Content = RequestEx["p_Content"]; }
            questMod.Qinfo = RequestEx["Qinfo_Hid"];
            questMod.p_ChoseNum = DataConverter.CLng(RequestEx["p_ChoseNum_DP"]);
            questMod.IsBig = 0;
            questMod.IsShare = string.IsNullOrEmpty(RequestEx["IsShare"]) ? 1 : 0;
            questMod.p_defaultScores = DataConverter.CFloat(RequestEx["p_defaultScores"]);
            questMod.Jiexi = RequestEx["Jiexi"];
            questMod.Version = DataConverter.CLng(RequestEx["Version"]);
            return questMod;
        }
        #endregion
        public IActionResult SchoolShow() { return View(); }
        public IActionResult SelKnowledge() { return View(); }
        public IActionResult SelQuestion()
        {
            string s=Request.Query[""];
            int issmall = DataConverter.CLng(RequestEx["issmall"]);
            string skey = RequestEx["skey"];
            ViewBag.selids = RequestEx["selids"];
            PageSetting setting = new PageSetting();
            //if (badmin.CheckLogin())
            //{
            //    setting = questBll.U_SelByFilter(CPage, PSize, 0, QType, skey, 0, issmall);
            //}
            //else if (buser.IsTeach())
            //{
               setting = questBll.U_SelByFilter(CPage, PSize, 0, QType, skey, mu.UserID, issmall);
            //}
            //else { return WriteErr("当前页面只有教师才可以访问!"); return null; }
            if (Request.IsAjaxRequest()) { return PartialView("SelQuestion_List", setting); }
            ViewBag.qtype = QType;
            ViewBag.issmall = issmall;//checkbox已选择的 
            return View(setting);
        }
        public IActionResult SelTearcher()
        {
            //PageSetting setting = buser.SelByGroupType_SPage(CPage, PSize, "isteach", RequestEx["skey"]);
            //if (Request.IsAjaxRequest()) { return PartialView("SelTearcher_List", setting); }
            //return View(setting);
            return View();
        }
        public IActionResult StudentManage()
        {
            PageSetting setting = new PageSetting();
            setting.dt = new DataTable();
            return View(setting);
        }
        public IActionResult StudentPic() { return View(); }
        public IActionResult ToScore()
        {
            PageSetting setting = ansBll.SelPage(CPage, PSize);
            if (Request.IsAjaxRequest()) { return PartialView("ToScore_List", setting); }
            return View(setting);
        }
        public IActionResult ExamDetail()
        {
            //if (!buser.IsTeach() && !badmin.CheckLogin()) { return WriteErr("当前页面只有教师才可访问"); return null; }
            VM_ExamDetail model = ExamDetail_MyBind();
            if (model == null) { return WriteErr("试卷不存在,可能已被删除");  }
            return View(model);
        }
        private VM_ExamDetail ExamDetail_MyBind()
        {
            M_Exam_Sys_Papers paperMod = paperBll.SelReturnModel(Mid);
            if (paperMod == null) { return null; }
            if (string.IsNullOrEmpty(paperMod.QIDS)) { throw new Exception("该试卷没有添加题目"); }
            VM_ExamDetail model = new VM_ExamDetail(mu, paperMod, Request);
            return model;
        }
        [HttpPost]
        
        public IActionResult ExamDetail_Submit()
        {
            string FlowID = RequestEx["FlowID"] ?? "";
            JArray arr = JsonConvert.DeserializeObject<JArray>(RequestEx["Answer_Hid"]);
            foreach (JObject obj in arr)
            {
                M_Exam_Answer answerMod = ansBll.SelReturnModel(Convert.ToInt32(obj["ID"].ToString()));
                answerMod.TechID = mu.UserID;
                answerMod.TechName = mu.UserName;
                answerMod.IsRight = Convert.ToInt32(obj["IsRight"].ToString());
                answerMod.Score = DataConverter.CLng(obj["Score"]);
                answerMod.Remark = obj["Remark"].ToString();
                answerMod.RDate = DateTime.Now;
                ansBll.UpdateByID(answerMod);
            }
            ansBll.SumScore(FlowID);
            return RedirectToAction("ToScore");
        }
        #region 版本,班级,学校等
        public IActionResult AddVersion()
        {

            M_Exam_Version verMod = new M_Exam_Version();
            if (Mid > 0)
            {
                verMod = verBll.SelReturnModel(Mid);
            }
            C_TreeTlpDP treeMod = GetTreeMod();
            ViewBag.treeMod = treeMod;
            ViewBag.gradeDT = gradeBll.GetGradeList(6, 0);
            return View(verMod);
        }
        public IActionResult VersionList() { return View(); }
        public IActionResult ClassManage()
        {
            PageSetting setting = new PageSetting();
            setting.dt = new DataTable();
            return View(setting);
        }
        public IActionResult AddClass() { return View(); }
        public IActionResult ClassShow() { return View(); }
        public IActionResult SelSchool()
        {
            PageSetting setting = schBll.SelPage(CPage, PSize);
            if (Request.IsAjaxRequest()) { return PartialView("SelSchool_List", setting); }
            return View(setting);
        }
        //获取科目下拉模型
        private C_TreeTlpDP GetTreeMod()
        {
            return new C_TreeTlpDP()
            {
                F_ID = "C_id",
                F_Name = "C_ClassName",
                F_Pid = "C_Classid",
                dt = clsBll.Select_All(),
            };
        }
        #endregion
    }
}
