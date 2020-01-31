using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZoomLa.BLL;
using ZoomLa.BLL.AdSystem;
using ZoomLa.BLL.User;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Model.AdSystem;
using ZoomLa.Model.User;
using ZoomLa.SQLDAL;

namespace ZoomLaCMS.Plugins.Third.Logo
{
    public partial class Default : System.Web.UI.Page
    {
        B_User buser = new B_User();
        B_Logo_Icon iconBll = new B_Logo_Icon();
        B_Logo_Design desBll = new B_Logo_Design();
        public string baseDir = "";
        protected void Page_Load(object sender, EventArgs e)
        {
            baseDir = iconBll.PlugDir;
            if (!IsPostBack)
            {
                B_User.CheckIsLogged(Request.RawUrl);
                RPT.DataSource = iconBll.Sel();
                RPT.DataBind();
            }
        }

        protected void Save_Btn_Click(object sender, EventArgs e)
        {
            //M_UserInfo mu = buser.GetLogin();
            //M_Logo_Design desMod = new M_Logo_Design();
            //desMod.CompName = CompName_T.Text.Trim();
            //desMod.SubTitle = SubTitle_T.Text.Trim();
            //desMod.LogoID = DataConvert.CLng(LogoID_Hid.Value);
            //if (string.IsNullOrEmpty(desMod.CompName)) { function.WriteErrMsg("未设定公司名称"); }
            //if (desMod.LogoID < 1) { function.WriteErrMsg("未指定Logo"); }
            //desMod.UserID = mu.UserID;
            //desMod.UserName = mu.UserName;
            //desMod.ID = desBll.Insert(desMod);
            //Response.Redirect("Third.aspx?ID=" + desMod.ID);
        }

    }
}