﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using ZoomLa.Model;

using ZoomLa.SQLDAL;

namespace ZoomLa.BLL.User
{
    public class B_User_BindPro
    {
        private string TbName, PK;
        private M_User_BindPro initMod = new M_User_BindPro();
        public B_User_BindPro()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public int Insert(M_User_BindPro model)
        {
            //不允许重复,自动读取值
            if (DBCenter.IsExist(TbName, "UserID=" + model.UserID))
            {
                M_User_BindPro existMod = SelModelByUid(model.UserID);
                model.ID = existMod.ID;
                UpdateByID(model);
                return model.ID;
            }
            return DBCenter.Insert(model);
        }
        public bool UpdateByID(M_User_BindPro model)
        {
            return DBCenter.UpdateByID(model, model.ID);
        }
        public bool Del(int ID)
        {
            return DBCenter.Del(TbName, PK, ID);
        }
        public M_User_BindPro SelModelByUid(int uid)
        {
            using (DbDataReader reader = DBCenter.SelReturnReader(TbName, "UserID=" + uid))
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
        public M_User_BindPro SelReturnModel(int ID)
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
    }
}
