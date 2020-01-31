using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ZoomLa.BLL.AdSystem;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLaCMS.Plugins.Third.Logo
{
    public partial class Default1 : System.Web.UI.Page
    {
        B_Logo_Design desBll = new B_Logo_Design();
        protected void Page_Load(object sender, EventArgs e)
        {
            RPT.SPage = SelPage;
            if (!IsPostBack)
            {
                RPT.DataBind();
            }
        }
        public DataTable SelPage(int pageSize, int pageIndex)
        {
            PageSetting config = new PageSetting();
            config=desBll.SelPage(pageIndex, pageSize, new F_Logo_Design()
            {
                ztype = 1
            });
            RPT.ItemCount = config.itemCount;
            return config.dt;
        }
    }
}