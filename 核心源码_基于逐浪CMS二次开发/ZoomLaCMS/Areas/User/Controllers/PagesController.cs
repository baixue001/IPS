using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Page;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.Model.Page;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;
using ZoomLa.BLL.Content;
using Microsoft.AspNetCore.Authorization;
using ZoomLa.Model.Content;

namespace ZoomLaCMS.Areas.User.Controllers
{
    [Area("User")]
    [Authorize(Policy = "User")]
    public class PagesController : Ctrl_User
    {
        B_ModelField fieldBll = new B_ModelField();
        B_Model modBll = new B_Model();
        B_Node nodeBll = new B_Node();
        B_Content conBll = new B_Content();
        B_PageReg regBll = new B_PageReg();
        B_Pub pubBll = new B_Pub();
        //黄页栏目节点
        B_PageTemplate tempBll = new B_PageTemplate();
        B_PageStyle styleBll = new B_PageStyle();
        public int NodeID
        {
            get
            {
                if (ViewBag.NodeID == null) { ViewBag.NodeID = DataConvert.CLng(RequestEx["NodeID"]); };
                return ViewBag.NodeID;
            }
            set { ViewBag.NodeID = value; }
        }
        public int ModelID
        {
            get
            {
                if (ViewBag.ModelID == null) { ViewBag.ModelID = DataConvert.CLng(RequestEx["ModelID"]); };
                return ViewBag.ModelID;
            }
            set { ViewBag.ModelID = value; }
        }
        public void Index() { Response.Redirect("Default");  }
        public IActionResult Default()
        {
            M_PageReg prMod = regBll.SelModelByUid(mu.UserID);
            if (prMod == null || prMod.Status == 0) { return RedirectToAction("PageInfo"); }
            return View("Default");
        }
        //黄页申请
        public IActionResult PageInfo()
        {
            bool ShowRegPage = false;//新用户,或修改信息时允许显示,审核中则不显示
            M_PageReg regMod = regBll.SelModelByUid(mu.UserID);
            DataTable applyModDT = modBll.GetListPage();
            DataTable styleDt = styleBll.Sel();
            styleDt.Columns["PageNodeid"].ColumnName = "TemplateID";
            styleDt.Columns["TemplateIndex"].ColumnName = "TemplateUrl";
            styleDt.Columns["TemplateIndexPic"].ColumnName = "TemplatePic";
            styleDt.Columns["PageNodeName"].ColumnName = "rname";
            //------------------------------------------------------
            //申请模型提交后不可再次修改
            if (applyModDT.Rows.Count < 1) { return WriteErr("未定义黄页申请模型");  }
            if (regMod == null)
            {
                regMod = new M_PageReg();
                regMod.UserID = mu.UserID;
                regMod.UserName = mu.UserName;
                if (ModelID > 0) { regMod.ModelID = ModelID; }
                else { regMod.ModelID = Convert.ToInt32(applyModDT.Rows[0]["ModelID"]); }
                ShowRegPage = true;
            }
            if (regMod.Status == (int)ZLEnum.ConStatus.Audited) { Response.Redirect("Modifyinfo");  }
            if (DataConvert.CStr(RequestEx["menu"]).Equals("edit")) { ShowRegPage = true; }
            //InfoID==GeneralID???  修改,黄页申请不在CommonModel中留信息

            
            ViewBag.valuedr=regBll.SelUserApplyInfo(regMod);
            ViewBag.ShowRegPage = ShowRegPage;
            ViewBag.applyModDT = applyModDT;
            ViewBag.styleDt = styleDt;
            return View(regMod);
        }
        
        public IActionResult Page_Apply()
        {
            M_PageReg regMod = regBll.SelModelByUid(mu.UserID);
            if (regMod == null)
            {
                regMod = new M_PageReg();
                regMod.ModelID = DataConvert.CLng(RequestEx["ApplyModel_Rad"]);
            }
            DataTable dt = fieldBll.GetModelFieldList(regMod.ModelID);
            DataTable table = Call.GetDTFromMVC(dt, Request);
            regMod.CompanyName = RequestEx["CompanyName"];
            regMod.PageTitle = mu.UserName + "的黄页信息";
            regMod.PageInfo = RequestEx["PageInfo"];
            regMod.LOGO = RequestEx["LOGO_t"];
            regMod.NodeStyle = DataConverter.CLng(RequestEx["TempleID_Hid"]);//样式与首页模板??,首页模板可动态以styleMod中为准
            regMod.Template = RequestEx["TempleUrl_Hid"];
            regMod.Status = SiteConfig.SiteOption.RegPageStart ? 0 : 99;
            if (regMod.ID > 0)
            {
                conBll.Page_Update(table, regMod);
                return WriteOK("修改提交成功", "PageInfo"); 
            }
            else
            {
                M_ModelInfo applyMod = modBll.SelReturnModel(regMod.ModelID);
                regMod.TableName = applyMod.TableName;
                regMod.UserName = mu.UserName;
                regMod.UserID = mu.UserID;
                regMod.ID = conBll.Page_Add(table, regMod);
                return WriteOK("申请提交成功", "PageInfo"); 
            }
        }
        public IActionResult NodeTree()
        {
            string hasChild = "<a href='MyContent?NodeID={0}' id='a{0}' class='list1' target='main_right1'>{2}<span class='list_span'>{1}</span></a>";
            string noChild = "<a href='MyContent?NodeID={0}' target='main_right1'>{2}{1}</a>";
            M_PageReg regMod = regBll.SelModelByUid(mu.UserID);
            DataTable nodeDT = tempBll.Sel(mu.UserID, true);
            //nodeDT.DefaultView.RowFilter = "NodeType IN (0,1)";
            //nodeDT = nodeDT.DefaultView.ToTable();
            ViewBag.NodeHtml = "<ul class='list-unstyled'><li><a class='list1' id='a0' href='MyContent' target='main_right1'><span class='list_span'>全部内容</span><span class='zi zi_list'></span></a>" + B_Node.GetLI(nodeDT, hasChild, noChild) + "</li></ul>";
            return View();
        }
        #region 黄页内容
        public IActionResult AddContent()
        {
            M_CommonData Cdata = new M_CommonData();
            if (Mid > 0)
            {
                Cdata = conBll.GetCommonData(Mid);
                NodeID = Cdata.NodeID;
                ModelID = Cdata.ModelID;
            }
            else
            {
                Cdata.ModelID = ModelID;
                Cdata.NodeID = NodeID;
            }
            if ((ModelID < 1 && NodeID < 1) && Mid < 1) { return WriteErr("参数错误");  }
            M_ModelInfo model = modBll.SelReturnModel(ModelID);
            M_Templata nodeMod = tempBll.SelReturnModel(NodeID);
            if (Mid > 0)
            {
                if (mu.UserName != Cdata.Inputer) { return WriteErr("不能编辑不属于自己输入的内容!");  }
                DataTable dtContent = conBll.GetContent(Mid);
            }
            ViewBag.nodeMod = nodeMod;
            ViewBag.ds = fieldBll.SelByModelID(ModelID, true);
            ViewBag.op = (Mid < 1 ? "添加" : "修改") + model.ItemName;
            ViewBag.tip = "向 <a href='MyContent?NodeId=" + nodeMod.TemplateID + "'>[" + nodeMod.TemplateName + "]</a> 节点" + ViewBag.op;
            return View(Cdata);
        }
        public IActionResult EditContent()
        {
            int gid = DataConvert.CLng(RequestEx["GeneralID"]);
            if (gid < 1) { gid = DataConvert.CLng(RequestEx["ID"]); }
            if (gid < 1) { return WriteErr("未指定需要修改的内容");  }
            return RedirectToAction("AddContent",new {id=gid });
        }
        [HttpPost]
        
        public IActionResult Content_Add()
        {
            M_CommonData CData = new M_CommonData();
            if (Mid > 0)
            {
                CData = conBll.SelReturnModel(Mid);
                if (CData.SuccessfulUserID != mu.UserID) { return WriteErr("你无权修改该内容");  }
                ModelID = CData.ModelID;
                NodeID = CData.NodeID;
            }
            Call commonCall = new Call();
            DataTable dt = fieldBll.SelByModelID(ModelID, false);
            DataTable table;
            try
            {
                table = Call.GetDTFromMVC(dt, Request);
            }
            catch (Exception e)
            {
                return WriteErr(e.Message); 
            }
            CData.Title = RequestEx["Title"];
            CData.TopImg=RequestEx["ThumImg_Hid"];
            if (CData.GeneralID > 0)
            {
                conBll.UpdateContent(table, CData);
            }
            else
            {
                CData.Status = SiteConfig.YPage.IsAudit ? 99 : 0;
                CData.Inputer = mu.UserName;
                CData.SuccessfulUserID=mu.UserID;
                CData.NodeID = NodeID;
                CData.ModelID = ModelID;
                CData.TableName = modBll.SelReturnModel(ModelID).TableName;
                CData.FirstNodeID = GetFriestNode(NodeID);
                CData.GeneralID = conBll.AddContent(table, CData);
            }
            return WriteOK("添加成功", "Default"); 
        }
        // 获得第一级节点ID
        private int GetFriestNode(int NodeID)
        {
            M_Node nodeinfo = nodeBll.GetNodeXML(NodeID);
            int ParentID = nodeinfo.ParentID;
            if (DataConverter.CLng(nodeinfo.ParentID) > 0)
            {
                return GetFriestNode(nodeinfo.ParentID);
            }
            else
            {
                return nodeinfo.NodeID;
            }
        }
        public IActionResult MyContent()
        {
            string Status = DataConverter.CStr(RequestEx["status"]);
            if (NodeID != 0)
            {
                M_Node nod = nodeBll.SelReturnModel(NodeID);
                M_Templata tempMod = tempBll.SelReturnModel(NodeID);
                B_Page pageBll = new B_Page(tempMod.Modelinfo);
                string AddContentlink = "";
                foreach (DataRow dr in pageBll.modeldt.Rows)
                {
                    M_ModelInfo infoMod = modBll.SelReturnModel(Convert.ToInt32(dr["modelid"]));
                    if (infoMod.IsNull) { continue; }
                    AddContentlink += "<a href='AddContent?NodeID=" + NodeID + "&ModelID=" + infoMod.ModelID + "' target='_parent' class='btn btn-info'><i class='zi zi_plus'></i> 添加" + infoMod.ItemName + "</a>";
                }
                ViewBag.addhtml = AddContentlink;
            }
            M_PageReg prMod = regBll.SelModelByUid(mu.UserID);
            //ViewBag.pageid = prMod.ID;
            //ViewBag.PageID = prMod.ID;
            ViewBag.NodeID = NodeID;
            ViewBag.Status = Status;
           
            PageSetting setting = conBll.Page_Sel(CPage,PSize,NodeID,Status,mu.UserName,RequestEx["skey"]);
            return View(setting);
        }
        public PartialViewResult Content_Data()
        {
            PageSetting setting = null;
            //string action = RequestEx["action"] ?? "";
            //string status = action.Equals("recycle") ? ((int)ZLEnum.ConStatus.Recycle).ToString() : "";
            string status = DataConverter.CStr(RequestEx["status"]);
            setting = conBll.SelContent(CPage, PSize, NodeID, status, mu.UserName, RequestEx["skey"], "ZL_Page");
            ViewBag.PageID = regBll.SelModelByUid(mu.UserID).ID;
            return PartialView("MyContent_List", setting);
        }
        //移入回收站
        public int Content_Del(string ids)
        {
            string[] idArr = ids.Split(',');
            for (int i = 0; i < idArr.Length; i++)
            {
                conBll.SetDel(Convert.ToInt32(idArr[i]));
            }
            return 1;
        }
        //彻底删除
        public int Content_RealDel(string ids)
        {
            if (conBll.DelByIDS(ids)) return 1;
            else return 0;
        }
        //还原
        public int Content_Recover(string ids)
        {
            conBll.Reset(ids);
            return 1;
        }
        #endregion
        #region 栏目管理
        public IActionResult ClassManage()
        {
            M_PageReg regMod = regBll.SelModelByUid(mu.UserID);
            if (regMod == null || regMod.Status != (int)ZLEnum.ConStatus.Audited)
            {
                Response.Redirect("PageInfo"); 
            }
            PageSetting setting = tempBll.SelPage(CPage, PSize, 0, mu.UserID);
            return View(setting);
        }
        public IActionResult AddNode()
        {
            if (!SiteConfig.YPage.UserCanNode) { return WriteErr("不允许自建栏目!");  }
            M_Templata model = new M_Templata();
            B_Page pageBll = new B_Page();
            string op = "添加栏目";
            if (Mid > 0)
            {
                op = "修改栏目";
                model = tempBll.SelReturnModel(Mid);
                pageBll = new B_Page(model.Modelinfo);
            }
            pageBll.moddt= modBll.SelByType("4");
            ViewBag.op = op;
            ViewBag.pageBll = pageBll;
            return View(model);
        }
        [HttpPost]
        
        public IActionResult Node_Add()
        {
            B_Page pageBll=new B_Page();
            M_PageReg pageinfo = regBll.SelModelByUid(mu.UserID);
            M_Templata model = new M_Templata();
            if (Mid > 0) { model = tempBll.SelReturnModel(Mid); }
            model.OrderID = DataConverter.CLng(RequestEx["OrderID"]);
            model.PageMetakeyinfo = RequestEx["PageMetakeyinfo"];
            model.PageMetakeyword = RequestEx["PageMetakeyword"];
            model.TemplateName = RequestEx["TemplateName"];
            model.ParentID = 0;//用户暂不可建多级栏目
            model.Modelinfo = nodeBll.GetNodeXML(model.ParentNode).ContentModel;
            model.UserGroup = pageinfo.NodeStyle.ToString();
            model.Nodeimgtext = RequestEx["Nodeimgtext"];
            model.TemplateUrl = RequestEx["TemplateUrl_hid"];
            model.Modelinfo=pageBll.GetSubmitModelChk(Request);
            if (model.TemplateID > 0)
            {
                tempBll.UpdateByID(model);
            }
            else
            {
                model.UserID = mu.UserID;
                model.Username = mu.UserName;
                tempBll.insert(model);
            }
            return WriteOK("操作成功!", "ClassManage"); 
        }
        public PartialViewResult Node_Data()
        {
            PageSetting setting = tempBll.SelPage(CPage, PSize, 0, mu.UserID);
            return PartialView("ClassManage_List", setting);
        }
        //黄页栏目批量删除
        public int Node_Del(string id)
        {
            if (tempBll.DelByIDS(id)) return 1;
            else return 0;
        }
        ////黄页栏目批量隐藏
        //public int Node_Hide(string id)
        //{
        //    if (tempBll.ChangeStatus(id, 0)) return 1;
        //    else return 0;
        //}
        #endregion
        #region 互动模块
        //注册企业黄页
        public IActionResult Modifyinfo()
        {
            M_PageReg regMod = regBll.SelModelByUid(mu.UserID);
            if (regMod == null || regMod.Status != (int)ZLEnum.ConStatus.Audited) { Response.Redirect("PageInfo");  }
            return View(regMod);
        }
        [HttpPost]
        public IActionResult Modifyinfo_Edit()
        {
            M_PageReg regMod = regBll.SelModelByUid(mu.UserID);
            regMod.Keyword = RequestEx["Keyword"];
            regMod.PageTitle = RequestEx["PageTitle"];
            regMod.Description = RequestEx["Description"];
            regMod.NavColor = RequestEx["NavColor"];
            regMod.NavBackground = RequestEx["NavBackground"];
            regMod.NavHeight = RequestEx["NavHeight"];
            regMod.TopWords = RequestEx["TopWords"];
            regMod.BottonWords = RequestEx["BottonWords"];
            regBll.UpdateByID(regMod);
            return WriteOK("修改成功", "Modifyinfo"); 
        }
        public IActionResult PageApply()
        {
            return View();
        }
        public IActionResult PubView()
        {
            int TopID = DataConverter.CLng(RequestEx["menu"]);
            int PubID = DataConverter.CLng(RequestEx["pubid"]);
            string Small = RequestEx["small"] ?? "";
            int ID = DataConverter.CLng(RequestEx["ID"]);
            if (ID <= 0) { return WriteErr("缺少ID参数");  }
            M_Pub pubMod = pubBll.GetSelect(PubID);
            int ModelID = pubMod == null ? 0 : pubMod.PubModelID;
            if (ModelID <= 0) { return WriteErr("缺少用户模型ID参数!");  }
            M_ModelInfo model = modBll.SelReturnModel(DataConverter.CLng(ModelID));
            DataTable DataDt = buser.GetUserModeInfo(model.TableName, ID, 18);
            DataTable newDt = new DataTable();
            if (DataDt.Rows.Count > 0)
            {
                DataRow DataDr = DataDt.Rows[0];
                DataTable FieldDt = fieldBll.GetModelFieldList(ModelID);
                newDt.Columns.Add("Title");
                newDt.Columns.Add("Content");
                DataRow dr1 = newDt.NewRow(); dr1["Title"] = "ID"; dr1["Content"] = DataDr["ID"]; newDt.Rows.Add(dr1);
                DataRow dr2 = newDt.NewRow(); dr2["Title"] = "用户名"; dr2["Content"] = DataDr["PubUserName"]; newDt.Rows.Add(dr2);
                DataRow dr3 = newDt.NewRow(); dr3["Title"] = "标题"; dr3["Content"] = DataDr["PubTitle"]; newDt.Rows.Add(dr3);
                DataRow dr4 = newDt.NewRow(); dr4["Title"] = "内容"; dr4["Content"] = DataDr["PubContent"]; newDt.Rows.Add(dr4);
                DataRow dr5 = newDt.NewRow(); dr5["Title"] = "IP地址"; dr5["Content"] = DataDr["PubIP"]; newDt.Rows.Add(dr5);
                DataRow dr6 = newDt.NewRow(); dr6["Title"] = "添加时间"; dr6["Content"] = DataDr["PubAddTime"]; newDt.Rows.Add(dr6);
                foreach (DataRow dr in FieldDt.Rows)
                {
                    DataRow row = newDt.NewRow();
                    row["Title"] = dr["FieldAlias"];
                    row["Content"] = DataDr[dr["FieldName"].ToString()];
                    newDt.Rows.Add(row);
                }
            }
            ViewBag.Details = newDt;
            ViewBag.ReturnUrl = Small.ToLower().Equals("small") ? "ViewSmallPub?pubid=" + PubID + "&ID=" + TopID : "ViewPub?pubid=" + PubID;
            ViewBag.AddUrl = "AddPub?Pubid=" + PubID + "&Parentid=" + ID;
            return View();
        }
        public IActionResult ViewPub()
        {
            int PubID = DataConverter.CLng(RequestEx["pubid"]);
            M_Pub pubMod = pubBll.GetSelect(PubID);
            int ModelID = pubMod == null ? 0 : pubMod.PubModelID;
            M_ModelInfo model = modBll.SelReturnModel(ModelID);
            PageSetting setting = new PageSetting();
            setting = buser.GetUserModeInfo_Page(CPage, PSize, model.TableName, mu.UserID, 16);
            ViewBag.PubID = PubID;
            ViewBag.PubType = pubMod.PubType;
            ViewBag.TableName = model.TableName;
            if (Request.IsAjaxRequest())
            {
                return PartialView("ViewPub_List", setting);
            }
            return View(setting);
        }
        public IActionResult ViewSmallPub()
        {
            int PubID = DataConverter.CLng(RequestEx["pubid"]);
            int ID = DataConverter.CLng(RequestEx["ID"]);
            int Type = DataConverter.CLng(RequestEx["type"] ?? "1");
            M_Pub pubMod = pubBll.GetSelect(PubID);
            int ModelID = pubMod == null ? 0 : pubMod.PubModelID;
            M_ModelInfo model = modBll.SelReturnModel(ModelID);
            PageSetting setting = new PageSetting();
            switch (Type)
            {
                case 2:
                    setting = buser.GetUserModeInfo_Page(CPage, PSize, model.TableName, ID, 19);
                    break;
                case 3:
                    setting = buser.GetUserModeInfo_Page(CPage, PSize, model.TableName, ID, 20);
                    break;
                default:
                    setting = buser.GetUserModeInfo_Page(CPage, PSize, model.TableName, ID, 13);
                    break;
            }
            ViewBag.PubID = PubID;
            ViewBag.PubType = pubMod.PubType;
            ViewBag.ID = ID;
            ViewBag.TableName = model.TableName;
            if (Request.IsAjaxRequest())
            {
                return PartialView("ViewSmallPub_List", setting);
            }
            return View(setting);
        }
        //审核互动信息
        public IActionResult EditPubstart()
        {
            int ID = DataConverter.CLng(RequestEx["ID"]);
            int PubID = DataConverter.CLng(RequestEx["PubID"]);
            string menu = RequestEx["menu"] ?? "";
            string small = RequestEx["small"] ?? "";
            if (PubID < 1) { return WriteErr("缺少用户ID参数！");  }
            M_Pub pubMod = pubBll.GetSelect(PubID);
            int ModelID = pubMod.PubModelID;
            M_ModelInfo model = modBll.SelReturnModel(ModelID);

            string returnUrl = "ViewPub?PubID=" + PubID;
            if (small.Equals("small")) { returnUrl = "ViewSmallPub?PubID=" + PubID + "&ID=" + ID; }
            if (menu.ToLower().Equals("false"))
            {
                if (buser.DelModelInfo2(model.TableName, ID, 12)) { return WriteOK("审核成功!", returnUrl);  }
                else { return WriteErr("审核失败", returnUrl);  }
            }
            else
            {
                if (buser.DelModelInfo2(model.TableName, ID, 13)) { return WriteOK("取消审核成功!", returnUrl);  }
                else { return WriteErr("取消审核失败!", returnUrl);  }
            }
        }
        public IActionResult AddPub()
        {
            int PubID = DataConverter.CLng(RequestEx["pubid"]);
            int ParentID = DataConverter.CLng(RequestEx["parentid"]);
            int ID = DataConverter.CLng(RequestEx["ID"]);
            if (PubID < 1 || ParentID < 1) { return WriteErr("缺少用户参数");  }
            M_Pub pubMod = pubBll.GetSelect(PubID);
            int ModelID = pubMod.PubModelID;
            M_ModelInfo model = modBll.SelReturnModel(ModelID);
            //ViewBag.ModelHtml = fieldBll.InputallHtml(ModelID, 0, new ModelConfig() { Source = ModelConfig.SType.UserBase });
            ViewBag.ModelName = "添加" + model.ModelName;
            return View();
        }
        //删除互动信息
        public IActionResult Delpub()
        {
            int ID = DataConverter.CLng(RequestEx["id"]);
            int pubid = DataConverter.CLng(RequestEx["pubid"]);
            string small = RequestEx["small"] ?? "";
            if (pubid <= 0) { return WriteErr("缺少用户ID参数！");  }
            int ModelID = pubBll.GetSelect(pubid).PubModelID;
            if (buser.DelModelInfo(modBll.SelReturnModel(ModelID).TableName, ID))
            {

                if (!string.IsNullOrEmpty(small)) { return RedirectToAction("ViewSmallPub", new { pubid = pubid, id = small }); }
                else { return WriteOK("删除成功!", "ViewPub?pubid=" + pubid);  }
            }
            else
            {
                if (!string.IsNullOrEmpty(small)) { return RedirectToAction("ViewSmallPub", new { modelid = ModelID, id = small }); }
                else { return WriteErr("删除失败!", "ViewPub?id=" + ModelID);  }
            }
        }
        public IActionResult Pub_Add()
        {
            int PubID = DataConverter.CLng(RequestEx["pubid"]);
            int ParentID = DataConverter.CLng(RequestEx["parentid"]);
            int ID = DataConverter.CLng(RequestEx["ID"]);
            M_Pub pubMod = pubBll.GetSelect(PubID);
            int ModelID = pubMod.PubModelID;
            M_ModelInfo model = modBll.SelReturnModel(ModelID);
            DataTable fieldDt = fieldBll.GetModelFieldListall(ModelID);
            DataTable userDt = buser.GetUserModeInfo(model.TableName, ParentID, 18);
            string ContentID = "1";
            if (userDt.Rows.Count > 0) { ContentID = userDt.Rows[0]["PubContent"].ToString(); }
            else { return WriteErr("输入无效参数！");  }

            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("FieldName", typeof(string)));
            table.Columns.Add(new DataColumn("FieldType", typeof(string)));
            table.Columns.Add(new DataColumn("FieldValue", typeof(string)));

            DataRow rowa = table.NewRow();
            rowa[0] = "PubTitle";
            rowa[1] = "TextType";
            rowa[2] = RequestEx["PubTitle"];
            table.Rows.Add(rowa);

            DataRow rowa1 = table.NewRow();
            rowa1[0] = "PubContent";
            rowa1[1] = "MultipleTextType";
            rowa1[2] = RequestEx["PubContent"];
            table.Rows.Add(rowa1);

            DataRow rowa2 = table.NewRow();
            rowa2[0] = "PubIP";
            rowa2[1] = "TextType";
            table.Rows.Add(rowa2);

            DataRow rowa3 = table.NewRow();
            rowa3[0] = "PubAddTime";
            rowa3[1] = "DateType";
            table.Rows.Add(rowa3);

            DataRow rowa4 = table.NewRow();
            rowa4[0] = "Pubupid";
            rowa4[1] = "int";
            rowa4[2] = PubID;
            table.Rows.Add(rowa4);

            DataRow rowa5 = table.NewRow();
            rowa5[0] = "PubContentid";
            rowa5[1] = "int";
            rowa5[2] = ContentID;
            table.Rows.Add(rowa5);

            DataRow rowa6 = table.NewRow();
            rowa6[0] = "Parentid";
            rowa6[1] = "int";
            rowa6[2] = ParentID;
            table.Rows.Add(rowa6);

            M_UserInfo aa = buser.GetLogin();

            DataRow rowa7 = table.NewRow();
            rowa7[0] = "PubUserName";
            rowa7[1] = "TextType";
            rowa7[2] = aa.UserName;
            table.Rows.Add(rowa7);

            DataRow rowa8 = table.NewRow();
            rowa8[0] = "PubUserID";
            rowa8[1] = "TextType";
            rowa8[2] = aa.UserID;
            table.Rows.Add(rowa8);

            foreach (DataRow dr in fieldDt.Rows)
            {
                if (DataConverter.CBool(dr["IsNotNull"].ToString()))
                {
                    if (string.IsNullOrEmpty(RequestEx["txt_" + dr["FieldName"].ToString()]))
                    {
                        return WriteErr(dr["FieldAlias"].ToString() + "不能为空!"); 
                    }
                }
                if (dr["FieldType"].ToString() == "FileType")
                {
                    string[] Sett = dr["Content"].ToString().Split(new char[] { ',' });
                    bool chksize = DataConverter.CBool(Sett[0].Split(new char[] { '=' })[1]);
                    string sizefield = Sett[1].Split(new char[] { '=' })[1];
                    if (chksize && sizefield != "")
                    {
                        DataRow row2 = table.NewRow();
                        row2[0] = sizefield;
                        row2[1] = "FileSize";
                        row2[2] = RequestEx["txt_" + sizefield];
                        table.Rows.Add(row2);
                    }
                }
                if (dr["FieldType"].ToString() == "MultiPicType")
                {
                    string[] Sett = dr["Content"].ToString().Split(new char[] { ',' });
                    bool chksize = DataConverter.CBool(Sett[0].Split(new char[] { '=' })[1]);
                    string sizefield = Sett[1].Split(new char[] { '=' })[1];
                    if (chksize && sizefield != "")
                    {
                        if (string.IsNullOrEmpty(RequestEx["txt_" + sizefield]))
                        {
                            return WriteErr(dr["FieldAlias"].ToString() + "的缩略图不能为空!"); 
                        }
                        DataRow row1 = table.NewRow();
                        row1[0] = sizefield;
                        row1[1] = "ThumbField";
                        row1[2] = RequestEx["txt_" + sizefield];
                        table.Rows.Add(row1);
                    }
                }
                DataRow row = table.NewRow();
                row[0] = dr["FieldName"].ToString();
                string ftype = dr["FieldType"].ToString();
                if (ftype == "NumType")
                {
                    string[] fd = dr["Content"].ToString().Split(new char[] { ',' });
                    string[] fdty = fd[1].Split(new char[] { '=' });

                    int numstyle = DataConverter.CLng(fdty[1]);
                    if (numstyle == 1)
                        ftype = "int";
                    if (numstyle == 2)
                        ftype = "float";
                    if (numstyle == 3)
                        ftype = "money";
                }
                row[1] = ftype;
                string fvalue = RequestEx["txt_" + dr["FieldName"].ToString()];
                row[2] = fvalue;
                table.Rows.Add(row);
            }
            try
            {
                if (buser.AddUserModel(table, model.TableName))
                {
                    return WriteOK("添加成功!", "ViewSmallPub?Pubid=" + PubID + "&ID=" + ParentID); 
                }
            }
            catch
            {
                //?Pubid=" + PubID + "&ID=" + ParentID
                return RedirectToAction("ViewSmallPub",new { Pubid=PubID,ID=ParentID }); 
            }
            return View();
        }
        public int Pub_Del(string id)
        {
            string TableName = RequestEx["TableName"] ?? "";
            if (!string.IsNullOrEmpty(TableName)) { return 1; }
            else { return 0; }
        }
        #endregion
    }
}
