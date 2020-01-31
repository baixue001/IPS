using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZoomLa.BLL;
using ZoomLa.BLL.AdSystem;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.AdSystem;
using ZoomLa.Model.User;
using ZoomLa.SQLDAL;

namespace ZoomLaCMS.LOGO
{
    public partial class Third : System.Web.UI.Page
    {
        B_User buser = new B_User();
        public B_Logo_Icon iconBll = new B_Logo_Icon();
        B_Logo_Design desBll = new B_Logo_Design();
        public M_Logo_Design desMod = null;
        public int Mid { get { return DataConvert.CLng(Request.QueryString["ID"]); } }
        public string Mode { get { return DataConvert.CStr(Request.QueryString["Mode"]); } }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                desMod = desBll.SelReturnModel(Mid);
                Save_Hid.Value =StrHelper.DecompressString(desMod.LogoContent);
                if (Mode == "admin")
                {
                    B_Admin.CheckIsLogged(Request.RawUrl);
                }
                else
                {
                    B_User.CheckIsLogged(Request.RawUrl);
                    M_UserInfo mu = buser.GetLogin();
                    if (desMod.UserID != mu.UserID) { function.WriteErrMsg("你无权修改记录"); }
                }
            }
        }

        protected void Save_Btn_Click(object sender, EventArgs e)
        {
            desMod = desBll.SelReturnModel(Mid);
            desMod.LogoContent = StrHelper.CompressString(Save_Hid.Value);
            desBll.UpdateByID(desMod);
            function.WriteSuccessMsg("操作成功,已保存至服务端");
        }
    }
}