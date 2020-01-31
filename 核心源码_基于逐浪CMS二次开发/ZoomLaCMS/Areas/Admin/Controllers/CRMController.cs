using System;
using System.Data;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Client;
using ZoomLa.BLL.Content;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Client;
using ZoomLa.Model.Content;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.AdminMVC.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    public class CRMController : Ctrl_Admin
    {
        B_Content conBll = new B_Content();
        B_CRMS_Client clientBll = new B_CRMS_Client();
        B_CRMS_Contact contactBll = new B_CRMS_Contact();
        B_Model modBll = new B_Model();
        B_ModelField fieldBll = new B_ModelField();
        B_CRMS_Attr attrBll = new B_CRMS_Attr();
        public void Index()
        {
            Response.Redirect("ClientList");
        }
        #region 客户
        public IActionResult ClientListDiag()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) { return null; }
            int clientId = DataConvert.CLng(RequestEx["Cid"]);
            PageSetting setting = clientBll.SelPage(CPage, PSize, new F_CRMS_Client()
            {
                ctype = RequestEx["ctype"],
                ignoreCids = clientId.ToString()
            });
            if (Request.IsAjax())
            {
                return PartialView("ClientListDiag_List", setting);
            }
            else { return View(setting); }
        }
        public IActionResult ClientList()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) { return null; }
            PageSetting setting = clientBll.SelPage(CPage, PSize, new F_CRMS_Client() { ctype = RequestEx["ctype"] });
            if (Request.IsAjax())
            {
                return PartialView("ClientList_List", setting);
            }
            else { return View(setting); }
        }
        public IActionResult ClientView()
        {
            if (Mid < 1) { return WriteErr("未指定客户信息");return null; }
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) { return null; }
            M_CRMS_Client clientMod = new M_CRMS_Client();
            M_ModelInfo modMod = modBll.SelReturnModel(48);
            ViewBag.fieldDT = new B_ModelField().SelByModelID(modMod.ModelID, false, false);

            clientMod = clientBll.SelReturnModel(Mid);
            if (!string.IsNullOrEmpty(modMod.TableName) && clientMod.ItemID > 0)
            {
                ViewBag.valueDT = DBCenter.Sel(modMod.TableName, "ID=" + clientMod.ItemID);
            }
            ViewBag.conSetting = contactBll.SelPage(CPage, PSize, new F_CRMS_Contact()
            {
                ClientID = clientMod.ID
            });
            if (!string.IsNullOrEmpty(clientMod.LinkIds))
            {
                SafeSC.CheckIDSEx(clientMod.LinkIds);
                ViewBag.linkDT = DBCenter.SelWithField(clientMod.TbName, "id,ClientName AS name,phone", "ID IN (" + clientMod.LinkIds + ")");
            }
            return View(clientMod);
        }
        public IActionResult ClientAdd()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) { return null; }
            M_CRMS_Client clientMod = new M_CRMS_Client();
            M_ModelInfo modMod = modBll.SelReturnModel(48);
            ViewBag.fieldDT = new B_ModelField().SelByModelID(modMod.ModelID, false, false);
            if (Mid > 0)
            {
                clientMod = clientBll.SelReturnModel(Mid);
                if (!string.IsNullOrEmpty(modMod.TableName) && clientMod.ItemID > 0)
                {
                    ViewBag.valueDT = DBCenter.Sel(modMod.TableName, "ID=" + clientMod.ItemID);
                }
                ViewBag.conSetting = contactBll.SelPage(CPage, PSize, new F_CRMS_Contact()
                {
                    ClientID = clientMod.ID
                });
                if (!string.IsNullOrEmpty(clientMod.LinkIds))
                {
                    SafeSC.CheckIDSEx(clientMod.LinkIds);
                    ViewBag.linkDT = DBCenter.SelWithField(clientMod.TbName,"id,ClientName AS name,phone", "ID IN (" + clientMod.LinkIds + ")");
                }
            }
            else
            {
                ViewBag.valueDT = null;
                ViewBag.conSetting = null;
            }
            return View(clientMod);
        }
        public int Client_Del(string ids)
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) { return Failed; }
            clientBll.Del(ids);
            return Success;
        }
        
        public IActionResult Client_Add(M_CRMS_Client model)
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) {  }

            model.ClientType = StrHelper.IdsFormat(Request.Form["ClientType"]);
            model.ID = Mid;
            M_ModelInfo modMod = modBll.SelReturnModel(48);
            DataTable fieldDT = fieldBll.SelByModelID(modMod.ModelID, false);
            DataTable table = Call.GetDTFromMVC(fieldDT, Request);
            model.ModelID = modMod.ModelID;
            model.ModelTable = modMod.TableName;
            model.CAdminID = adminMod.AdminId;
            model.LinkIds = Request.Form["LinkIds"];
            if (model.ID < 1)
            {
                model.ID = clientBll.Insert(model, table);
            }
            else
            {
                M_CRMS_Client clientMod = clientBll.SelReturnModel(Mid);
                model.ItemID = clientMod.ItemID;
                model.CDate = clientMod.CDate;
                model.CUserID = clientMod.CUserID;
                model.Flow = clientMod.Flow;
                clientBll.UpdateByID(model, table);
            }
            return WriteOK("操作成功", "ClientList");
        }
        private string XMLPath_Client = function.VToP("/Config/Import/CRM_Client.xml");
        public IActionResult ClientImport()
        {
            return View();
        }
        public void Import_DownTlp()
        {
            //string csv = Import_Logical.CreateCSVByXML(XMLPath_Client);
            //SafeC.DownStr(csv, "客户模板.xls", Encoding.Default);
            Import_Logical.Excel_CreateByXML(XMLPath_Client, "ClientTlp");
        }
        public IActionResult Import_Client()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) {  }
            var file = Request.Form.Files["file_up"];
            if (file == null) { return WriteErr("未指定文件");  }
            if (file.Length < 100) { return WriteErr("文件为空");  }
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".xlsx") { return WriteErr("只允许上传xlsx文件");  }
            DataTable excelDT = Import_Logical.Excel_ToDT(XMLPath_Client, file.OpenReadStream());
            DataTable typeDT = attrBll.Sel("ctype");
            //将数据直接写入数据库,或将其转换为模型再写入
            foreach (DataRow dr in excelDT.Rows)
            {
                M_CRMS_Client model = new M_CRMS_Client().GetModelFromReader(dr);
                //clientType统一让用户填中文,转化为中文
                //model.ClientType = StrHelper.IdsFormat(model.ClientType);
                #region ClientType
                if (!string.IsNullOrEmpty(model.ClientType))
                {
                    model.ClientType = model.ClientType.Replace(" ", "");
                    string[] ctypeArr = model.ClientType.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    foreach (string ctype in ctypeArr)
                    {
                        typeDT.DefaultView.RowFilter = "Value='" + ctype + "'";
                        if (typeDT.DefaultView.ToTable().Rows.Count < 1)
                        {
                            //新增ClientType
                            M_CRMS_Attr attrMod = new M_CRMS_Attr();
                            attrMod.ZType = "ctype";
                            attrMod.Value = ctype;
                            attrBll.Insert(attrMod);
                            typeDT = attrBll.Sel("ctype");
                        }
                    }
                }
                #endregion
                clientBll.Insert(model);
            }
            return WriteOK("客户信息导入成功", "ClientImport");
        }
        #endregion
        #region 客户类别
        public IActionResult CType()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) { return null; }
            DataTable dt = attrBll.Sel("ctype");
            if (Request.IsAjax())
            {
                return PartialView("CType_List", dt);
            }
            return View(dt);
        }
        public IActionResult CTypeAdd()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) { return null; }
            M_CRMS_Attr attrMod = new M_CRMS_Attr();
            if (Mid > 0) { attrMod = attrBll.SelReturnModel(Mid); }
            return View(attrMod);
        }
        public IActionResult CType_Add()
        {
            M_CRMS_Attr attrMod = new M_CRMS_Attr();
            if (Mid > 0) { attrMod = attrBll.SelReturnModel(Mid); }
            attrMod.Value = Request.Form["value"];
            attrMod.Remark = Request.Form["remark"];

            if (attrMod.ID > 0) { attrBll.UpdateByID(attrMod); }
            else { attrMod.ZType = "ctype"; attrBll.Insert(attrMod); }
            return WriteOK("操作成功", "CType");
        }
        public string CType_Del(string ids)
        {
            attrBll.Del(ids);
            return Success.ToString();
        }
        #endregion
        #region 联系人
        private string XMLPath_Contact = function.VToP("/Config/Import/CRM_Contact.xml");
        public IActionResult Contact()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) { return null; }
            F_CRMS_Contact filter = new F_CRMS_Contact()
            {
                ClientID = DataConvert.CLng(RequestEx["Cid"], -100),
                Name = RequestEx["name"]
            };
            PageSetting setting = contactBll.SelPage(CPage, PSize, filter);
            if (Request.IsAjax()) { return PartialView("Contact_List", setting); }
            else { return View("Contact", setting); }
        }
        public IActionResult ContactAdd()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) { return null; }
            M_CRMS_Contact contactMod = new M_CRMS_Contact();
            if (Mid > 0)
            {
                contactMod = contactBll.SelReturnModel(Mid);
            }
            ViewBag.clientDT = clientBll.Sel();
            return View(contactMod);
        }
        public IActionResult Contact_Add(M_CRMS_Contact subModel)
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) { return null; }
            M_CRMS_Contact model = new M_CRMS_Contact();
            if (Mid > 0) { model = contactBll.SelReturnModel(Mid); }
            model.ClientID = subModel.ClientID;
            model.DepName = subModel.DepName;
            model.Name = subModel.Name;
            model.Sex = subModel.Sex;
            model.Post = subModel.Post;
            model.Mobile_Office = subModel.Mobile_Office;
            model.Mobile_Home = subModel.Mobile_Home;
            model.Mobile1 = subModel.Mobile1;
            model.Mobile2 = subModel.Mobile2;
            model.QQ = subModel.QQ;
            model.Wechat = subModel.Wechat;
            model.SinaBlog = subModel.SinaBlog;
            model.Email = subModel.Email;
            model.Address = subModel.Address;
            model.Remind = subModel.Remind;
            if (model.ID > 0)
            {
                contactBll.UpdateByID(model);
            }
            else
            {
               
                model.CAdminID = adminMod.AdminId;
                model.ID = contactBll.Insert(model);
            }
            return Content("<script>parent.ContactAddSuccess();</script>");
        }
        public int Contact_Del(string ids)
        {
            contactBll.Del(ids);
            return Success;
        }
        public IActionResult ContactImport() {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) { return null; }
            return View();
        }
        public void ContactImport_Down() {
            Import_Logical.Excel_CreateByXML(XMLPath_Contact, "ContactTlp");
        }
        public IActionResult ContactImport_Upload()
        {
            if (!B_ARoleAuth.AuthCheckEx(ZLEnum.Auth.office, "crm")) {  }
            var file = Request.Form.Files["file_up"];
            if (file == null) { return WriteErr("未指定文件");  }
            if (file.Length < 100) { return WriteErr("文件为空");  }
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".xlsx") { return WriteErr("只允许上传xlsx文件");  }
            DataTable excelDT = Import_Logical.Excel_ToDT(XMLPath_Contact, file.OpenReadStream());
            //将数据直接写入数据库,或将其转换为模型再写入
            foreach (DataRow dr in excelDT.Rows)
            {
                M_CRMS_Contact model = new M_CRMS_Contact().GetModelFromReader(dr);
                contactBll.Insert(model);
            }
            return WriteOK("联系人导入成功", "ClientImport");
        }
        #endregion        
    }
}
