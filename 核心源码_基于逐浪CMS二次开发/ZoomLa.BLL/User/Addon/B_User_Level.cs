using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using ZoomLa.Model;

using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.User
{
    public class B_User_Level
    {
        private M_User_Level initMod = new M_User_Level();
        public string TbName = "", PK = "";
        public B_User_Level()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(TbName, "", "OrderId DESC");
        }
        public M_User_Level SelReturnModel(int ID)
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
        public M_User_Level SelModelByLevel(int level)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, "OrderID",level))
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
        /// <summary>
        /// 获取等级信息
        /// </summary>
        public M_User_Level GetLevel(int vip)
        {
            if (vip <= 0) { vip = 1; };
            M_User_Level model = SelModelByLevel(vip);
            if (model == null)
            {
                model = new M_User_Level() { Alias = "普通会员" };
            }
            return model;
        }
        public int Insert(M_User_Level model)
        {
            if (model.OrderID < 1)
            {
                model.OrderID = DataConvert.CLng(DBCenter.ExecuteScala(TbName, "MAX(OrderID)", ""));
                model.OrderID++;
            }
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_User_Level model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public void Del(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return; }
            SafeSC.CheckIDSEx(ids);
            DBCenter.DelByIDS(TbName, PK, ids);
        }
    }
}
