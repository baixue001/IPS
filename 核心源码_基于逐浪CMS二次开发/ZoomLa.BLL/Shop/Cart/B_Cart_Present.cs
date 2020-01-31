using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ZoomLa.BLL.CreateJS;
using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.Shop
{
    public class B_Cart_Present
    {
        public string TbName = "ZL_Cart_Present", PK = "ID";
        B_CodeModel codeBll = null;
        public B_Cart_Present() { codeBll = new B_CodeModel(TbName); }
        /// <summary>
        /// 批量插入记录,并扣减库存??
        /// </summary>
        public void BatInsert(int cartID, DataTable ptDT)
        {
            if (ptDT.Rows.Count < 1) { return; }
            DataTable dt = codeBll.SelStruct();
            string[] ignores = "id,cartid".Split(',');
            foreach (DataRow ptDR in ptDT.Rows)
            {
                DataRow dr = dt.NewRow();
                foreach (DataColumn dc in dt.Columns)
                {
                    string cname = dc.ColumnName.ToLower();
                    if (cname.Equals("id") || cname.Equals("cartid")) { continue; }
                    if (!ptDT.Columns.Contains(cname)) { continue; }
                    dr[cname] = ptDR[cname];
                }
                dr["CartID"] = cartID;
                dr["Remind"] = "";
                codeBll.Insert(dr);
            }
        }
        public DataTable Sel(int cartid)
        {
            return DBCenter.Sel(TbName, "CartID=" + cartid);
        }
    }
}
