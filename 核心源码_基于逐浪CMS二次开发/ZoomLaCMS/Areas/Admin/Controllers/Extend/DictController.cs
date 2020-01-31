using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Other;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    [Route("[area]/Extend/[controller]/[action]")]
    public class DictController : Ctrl_Admin
    {
        B_DataDicCategory dCateBll = new B_DataDicCategory();
        B_DataDictionary dItemBll = new B_DataDictionary();
        B_GradeOption gradeBll = new B_GradeOption();
        B_GradeCate gcateBll = new B_GradeCate();
        public int CateID { get { return DataConverter.CLng(RequestEx["CateID"]); } }
        public int ParentID { get { return DataConverter.CLng(RequestEx["ParentID"]); } }
        //DictionaryManage
        public IActionResult DicCateList()
        {
            PageSetting setting = dCateBll.SelPage(CPage, PSize, new Com_Filter()
            {
                //storeId = CateID,
                skey=GetParam("skey")
            });
            if (Request.IsAjax())
            {
                return PartialView("DicCate_List",setting);
            }
            return View(setting);
        }
        public ContentResult DictCate_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "del":
                    B_DataDicCategory.DelCate(ids);
                    break;
                case "use":
                    B_DataDicCategory.SetUsedByArr(ids, true);
                    break;
                case "unuse":
                    B_DataDicCategory.SetUsedByArr(ids, false);
                    break;
                case "save"://新增或修改
                    {
                        string name = GetParam("name");
                        if (string.IsNullOrEmpty(name)) { return Content(Failed.ToString()); }
                        if (Mid > 0)
                        {
                            M_DicCategory info = B_DataDicCategory.GetDicCate(Mid);
                            info.CategoryName = name;
                            B_DataDicCategory.Update(info);
                        }
                        else
                        {
                            M_DicCategory info = new M_DicCategory();
                            info.DicCateID = 0;
                            info.CategoryName = name;
                            info.IsUsed = true;
                            B_DataDicCategory.AddCate(info);
                        }
                    }
                    break;
            }
            return Content(Success.ToString());
        }
        //Dictionary
        public IActionResult DicItemList()
        {
            if (CateID < 1) { return WriteErr("缺少字典分类ID参数！", "DicCateList"); }
            PageSetting setting = dItemBll.SelPage(CPage,PSize,new Com_Filter(){
                storeId=CateID,
                skey=GetParam("skey")
            });
            if (Request.IsAjax())
            {
                return PartialView("DicItem_List", setting);
            }
            else { return View(setting); }
            //cateMod = B_DataDicCategory.GetDicCate(CateID);
            //btnSave.Text = Mid > 0 ? "修改" : "添加";
            //if (Mid > 0)
            //{
            //    M_Dictionary info = B_DataDictionary.GetModel(Mid);
            //    txtDicName.Text = info.DicName;
            //}
        }
        public ContentResult DicItem_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "del":
                    dItemBll.DelByIds(ids);
                    break;
                case "save":
                    {
                        string name = GetParam("name");
                        if (string.IsNullOrEmpty(name)) { return Content(Failed.ToString()); }
                        M_Dictionary info = new M_Dictionary();
                        if (Mid > 0) { info = B_DataDictionary.GetModel(Mid); }
                        info.DicName = name;
                        if (info.DicID < 1)
                        {
                            info.DicCate = CateID;
                            info.IsUsed = true;
                            B_DataDictionary.AddDic(info);
                        }
                        else { B_DataDictionary.Update(info); }
                    }
                    break;
            }

            return Content(Success.ToString());
        }
        //-----------------多级数据字典(ZL_GradeCate,ZL_Grade)
        public IActionResult GradeCateManage()
        {
            PageSetting setting = gcateBll.SelPage(CPage, PSize, new Com_Filter() { });
            if (Request.IsAjax())
            {
                return PartialView("GradeCate_List", setting);
            }
            return View(setting);
        }
        public IActionResult GCateAdd()
        {
            M_GradeCate cateMod = new M_GradeCate();
            if (Mid > 0) { cateMod = gcateBll.SelReturnModel(Mid); }
            return View(cateMod);
        }
        public ContentResult GCateAdd_Submit(M_GradeCate model)
        {
            M_GradeCate cateMod = new M_GradeCate();
            if (Mid > 0) { cateMod = gcateBll.SelReturnModel(Mid); }
            cateMod.CateName = model.CateName;
            cateMod.Remark = model.Remark;
            cateMod.GradeAlias = model.GradeAlias;
            if (Mid > 0)
            {
                gcateBll.UpdateByID(cateMod);
            }
            else { gcateBll.insert(cateMod); }
            return JavaScript("<script>parent.mybind();</script>");
        }
        [HttpPost]
        public ContentResult GCate_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "del":
                    gcateBll.DelByIDS(ids);
                    break;
                case "save":
                    {
                        string name = GetParam("name");
                        string alias = GetParam("alias");
                        string remark = GetParam("remark");
                        if (string.IsNullOrEmpty(name)||string.IsNullOrEmpty(alias)) { return Content(Failed.ToString()); }
                        //int CateID = DataConverter.CLng(this.HdnCateID.Value);
                        if (Mid> 0)
                        {
                            M_GradeCate info = gcateBll.GetCate(CateID);
                            info.CateID = CateID;
                            info.CateName = name;
                            info.Remark = remark;
                            info.GradeAlias = alias;
                            gcateBll.UpdateCate(info);
                        }
                        else
                        {
                            M_GradeCate info = new M_GradeCate();
                            info.CateName = name;
                            info.Remark = remark;
                            info.GradeAlias = alias;
                            gcateBll.AddCate(info);
                        }

                    }
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult GradeOption()
        {
            if (CateID <= 0) { return WriteErr("没有指定多级数据字典分类ID", "GradeCateManage"); }
            M_GradeCate cateMod = gcateBll.GetCate(CateID);
            ViewBag.cateMod = cateMod;
            PageSetting setting = gradeBll.SelPage(CPage, PSize, new Com_Filter()
            {
                storeId = CateID,
                pid = ParentID,
                skey = GetParam("skey")
            });
            if (Request.IsAjax())
            {
                return PartialView("GradeOption_List",setting);
            }
            return View(setting);
        }
        public IActionResult GOptionAdd()
        {
            M_Grade model = new M_Grade();
            if (Mid > 0)
            {
                model = gradeBll.GetGradeOption(Mid);
            }
            else
            {
                model.Cate = CateID;
                model.ParentID = ParentID;
                if (model.Cate < 1) { return WriteErr("未指定所属分类"); }
            }
            M_GradeCate cateMod = gcateBll.SelReturnModel(model.Cate);
            M_Grade parentMod = new M_Grade() { GradeName = "无" };
            if (model.ParentID > 0) { parentMod = gradeBll.GetGradeOption(model.ParentID); }
            ViewBag.cateMod = cateMod;
            ViewBag.parentMod = parentMod;
            return View(model);
        }
        [HttpPost]
        public ContentResult GOptionAdd_Submit()
        {
            M_Grade model = new M_Grade();
            if (Mid > 0) { model = gradeBll.GetGradeOption(Mid); }
            model.GradeName = GetParam("GradeName");
            if (model.GradeID > 0) { gradeBll.UpdateDic(model); }
            else
            {
                model.Cate = CateID;
                model.ParentID = ParentID;
                if (ParentID == 0) { model.Grade = 1; }
                else { model.Grade = gradeBll.GetGradeOption(model.ParentID).Grade + 1; }
                gradeBll.AddGradeOption(model);
            }
          
            return JavaScript("<script>parent.mybind();</script>");
        }
        [HttpPost]
        public ContentResult GOption_API()
        {
            string action = GetParam("action");
            string ids = GetParam("ids");
            switch (action)
            {
                case "del":
                    gradeBll.DelByIds(ids);
                    break;
                case "save":
                    {
                        string name = GetParam("name");

                        if (string.IsNullOrEmpty(name)) { return Content(Failed.ToString()); }
                        if (Mid > 0)
                        {
                            M_Grade info =gradeBll.GetGradeOption(Mid);
                            info.GradeName = name;
                            gradeBll.UpdateDic(info);
                        }
                        else
                        {
                            M_Grade info = new M_Grade();
                            info.GradeName = name;
                            info.ParentID = DataConverter.CLng(RequestEx["ParentID"]);
                            info.Cate = CateID;
                            info.Grade = DataConverter.CLng(GetParam("level"));
                            gradeBll.AddGradeOption(info);
                        }
                        break;
                    }
            }
            return Content(Success.ToString());
        }
        }
}