using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.BLL.Helper;
using ZoomLa.Components;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{
    public class B_QrCode : ZL_Bll_InterFace<M_QrCode>
    {
        public string PK, TbName,strTableName;
        private M_QrCode initmod = new M_QrCode();
        public B_QrCode()
        {
            PK = initmod.PK;
            TbName = initmod.TbName;
            strTableName = initmod.TbName;
        }
        public DataTable Sel(int ID)
        {
            return Sql.Sel(TbName, PK, ID);
        }
        public M_QrCode SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, PK, ID))
            {
                if (reader.Read())
                {
                    return initmod.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        public M_QrCode SelModelByAppID(int appid)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("@flag", appid) };
            string sql = "SELECT * FROM " + TbName + " WHERE AppID=@flag";
            using (DbDataReader reader=SqlHelper.ExecuteReader(CommandType.Text,sql,sp))
            {
                if (reader.Read())
                {
                    return initmod.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName,"",PK+" DESC");
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_QrCode model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public void DelByIDS(string ids)
        {
            if (string.IsNullOrEmpty(ids)) return;
            SafeSC.CheckIDSEx(ids);
            string sql = "DELETE FROM " + TbName + " WHERE ID IN (" + ids + ")";
            SqlHelper.ExecuteSql(sql);
        }
        public string GetUrlByAgent(DeviceHelper.Agent agent, M_QrCode model)
        {
            string[] urls = model.Urls.Split(',');
            foreach (string urlstr in urls)
            {
                string name = urlstr.Split('$')[0];
                string url = urlstr.Split('$')[1];
                if (agent.ToString().Equals(name)) return url;
            }
            return "";
        }
        public int Insert(M_QrCode model)
        {
            return DBCenter.Insert(model);
        }
        //-----------------------
        public string GetUrl(int id)
        {
            return StrHelper.UrlDeal(SiteConfig.SiteInfo.SiteUrl + "/app/url?id=" + id);
        }
    }
}
