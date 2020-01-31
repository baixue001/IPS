using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using System.Text;
using System.Data.SqlClient;
using ZoomLa.SQLDAL;
using ZoomLa.Components;
using System.Collections.Generic;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;
using ZoomLa.BLL.Content;

namespace ZoomLa.BLL
{

    public class B_UserBaseField
    {
        string TbName = "", PK = "";
        M_UserBaseField initMod = new M_UserBaseField();
        public B_UserBaseField()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public int GetInsert(M_UserBaseField model)
        {
            return DBCenter.Insert(model);
        }
        public bool GetDelete(int ID)
        {
            M_UserBaseField fieldMod = GetSelect(ID);
            DBCenter.Del(TbName, PK, ID);
            new B_ModelField().DelField("ZL_UserBase", fieldMod.FieldName);
            return true;
        }
        public M_UserBaseField getUserBaseFieldByFieldName(string fieldName)
        {
            List<SqlParameter> spList = new List<SqlParameter>() { new SqlParameter("fieldName", fieldName) };
            return initMod.GetInfoFromDataTable(DBCenter.Sel("ZL_UserBaseField", "fieldName=@fieldName", "", spList));
        }
        public bool GetUpdate(M_UserBaseField model)
        {
            return DBCenter.UpdateByID(model, model.FieldID); ;
        }
        public bool InsertUpdate(M_UserBaseField model)
        {
            if (model.FieldID > 0)
                return GetUpdate(model);
            else
                return GetInsert(model) > 0;
        }
        public bool GetDeletes(int FieldID)
        {
            return GetDelete(FieldID);
        }
        public M_UserBaseField GetSelect(int ID)
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
        public DataTable SelByFieldName(string fieldName)
        {
            List<SqlParameter> spList = new List<SqlParameter>() { new SqlParameter("fieldName", fieldName) };
            return DBCenter.Sel(TbName, "FieldName=@fieldName", "", spList);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }
        #region 排序
        public int GetMaxID()
        {
            return DataConvert.CLng(DBCenter.ExecuteScala(TbName, "Max(FieldID)", ""));
        }
        public M_UserBaseField GetPreField(int CurrentID)
        {
            int FieldID = GetPreID(CurrentID);
            return GetSelect(FieldID);
        }
        public int GetPreID(int CurrentID)
        {
            return DataConverter.CLng(DBCenter.ExecuteScala(TbName, "FieldID", "OrderId<" + CurrentID, "OrderId DESC"));
        }
        public int GetNextID(int CurrentID)
        {
            return DataConverter.CLng(DataConvert.CLng(DBCenter.ExecuteScala("ZL_UserBaseField", "FieldID", "OrderId>" + CurrentID, "OrderId Asc")));
        }
        public M_UserBaseField GetNextField(int CurrentID)
        {
            int FieldID = GetNextID(CurrentID);
            return GetSelect(FieldID);
        }
        #endregion
        /// <summary>
        /// 返回UserID,TrueName,HoneyName,用于工作流等显示用户信息
        /// </summary>
        /// <returns></returns>
        public DataTable SelAll()
        {
            return DBCenter.SelWithField("ZL_UserBase", "UserID,HoneyName,TrueName");
        }
        public DataTable Select_All()
        {
            return DBCenter.SelWithField(TbName, "*,IsShow=1,ModelId=0,NodeID=0,IsSearchForm=0,IsView=1", "","OrderId ASC");
        }
    }
}
