using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using ZoomLa.Components;
using ZoomLa.Model.AdSystem;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL.AdSystem
{
    public class B_AD_Banner
    {
        private M_AD_Banner initMod = new M_AD_Banner();
        public string TbName = "", PK = "";
        //Banner图片与JS的存储根路径
        public string BannerDir
        {
            get
            {
                string dir = "";
                dir = SiteConfig.SiteOption.AdvertisementDir.TrimEnd('/');
                dir += "/Banner/";
                return dir;
            }
        }
        public B_AD_Banner()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", PK + " DESC");
        }
        public M_AD_Banner SelReturnModel(int ID)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, PK, ID))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        public int Insert(M_AD_Banner model)
        {
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_AD_Banner model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            string where = "ID IN (" + ids + ") ";
            DBCenter.DelByWhere(TbName, where);
        }
        public PageSetting SelPage(int cpage, int psize, Com_Filter filter)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            PageSetting setting = PageSetting.Double(cpage, psize, TbName, "ZL_AdZone", "A.ID", "A.ADID=B.ZoneID", where, PK + " DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
    }
}
