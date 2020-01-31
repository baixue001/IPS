using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZoomLa.BLL;
using ZoomLa.BLL.AdSystem;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.AdSystem;
using ZoomLa.SQLDAL;

namespace ZoomLaCMS.Plugins.Third.Logo
{
    public partial class Design : System.Web.UI.Page
    {
        public B_Logo_Icon iconBll = new B_Logo_Icon();
        B_Logo_Design desBll = new B_Logo_Design();
        B_User buser = new B_User();
        public int TlpID { get { return DataConvert.CLng(Request.QueryString["TlpID"]); } }
        public int Mid { get { return DataConvert.CLng(Request.QueryString["ID"]); } }
        M_Logo_Design desMod = null;
        M_UserInfo mu = null;
        protected void Page_Load(object sender, EventArgs e)
        {
            B_User.CheckIsLogged(Request.RawUrl);
            mu = buser.GetLogin();
            if (!IsPostBack)
            {
               
                if (TlpID > 0)
                {
                    desMod = desBll.SelReturnModel(TlpID);
                    if (desMod==null|| desMod.ZType != 1) { function.WriteErrMsg("模板不存在"); }
                }
                if (Mid > 0)
                {
                    desMod = desBll.SelReturnModel(Mid);
                    if (desMod == null) { function.WriteErrMsg("目标不存在"); }
                    if (desMod.UserID != mu.UserID) { function.WriteErrMsg("你无权修改"); }
                }
                if (desMod == null) { function.WriteErrMsg("未指定信息"); }
                function.Script(this, "fabHelper.init(" + StrHelper.DecompressString(desMod.LogoContent) + ");");

            }
        }
        protected void Save_Btn_Click(object sender, EventArgs e)
        {
            if (Mid > 0)
            {
                desMod = desBll.SelReturnModel(Mid);
            }
            else { desMod = new M_Logo_Design(); }
            desMod.LogoContent = StrHelper.CompressString(Save_Hid.Value);
            desMod.PreviewImg = Base64_Hid.Value;
            if (desMod.ID > 0) { desBll.UpdateByID(desMod); }
            else
            {
                desMod.ZType = 0;
                desMod.ZStatus = 99;
                desMod.UserID = mu.UserID;
                desMod.UserName = mu.UserName;
                desMod.ID= desBll.Insert(desMod);
            }
            function.WriteSuccessMsg("操作成功", "Design.aspx?ID="+desMod.ID);
        }
    }
}