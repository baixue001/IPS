using System;
using System.Collections.Generic;
using System.Text;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.Content;

namespace ZoomLa.BLL.Content
{
    public class ContentHelper
    {
        B_Model modelBll = new B_Model();
        public string GetElite(string Elite)
        {
            if (DataConverter.CLng(Elite) > 0)
                return "推荐";
            else
                return "未推荐";
        }
        //根据模型ID获取图标
        public string GetPic(object modeid)
        {
            M_ModelInfo model = modelBll.SelReturnModel(DataConverter.CLng(modeid));
            if (model == null) { return ""; }
            if (!string.IsNullOrEmpty(model.ItemIcon))
            {
                return "<span class=\"" + model.ItemIcon + "\" />";
            }
            else
            {
                return "";
            }
        }
        public string GetStatus(int status)
        {
            switch (status)
            {
                case (int)ZLEnum.ConStatus.Audited:
                    return "已审核";
                case (int)ZLEnum.ConStatus.UnAudit:
                    return "未审核";
                case (int)ZLEnum.ConStatus.Draft:
                    return "草稿";
                case (int)ZLEnum.ConStatus.Recycle:
                    return "回收站";
                case (int)ZLEnum.ConStatus.Reject:
                    return "退稿";
                default:
                    return status.ToString();
            }
        }
    }
}
