using System;
using System.Collections.Generic;
using System.Text;
using ZoomLa.Model;
using ZoomLa.Common;
using System.Data;
using ZoomLa.SQLDAL;
using System.Data.SqlClient;
using ZoomLa.SQLDAL.SQL;
using System.Data.Common;

namespace ZoomLa.BLL
{
    public class B_GradeOption
    {
        public string PK = "", TbName = "";
        public M_Grade initMod = new M_Grade();
        public B_GradeOption()
        {
            PK = initMod.PK;
            TbName = initMod.TbName;
        }
        /// <summary>
        /// 添加分级选项
        /// </summary>
        /// <param name="GradeOption">分级选项实例</param>
        /// <returns>成功状态</returns>
        public bool AddGradeOption(M_Grade model)
        {
            if (model.GradeID > 0)
                return DBCenter.UpdateByID(model,model.GradeID);
            else
                return DBCenter.Insert(model) > 0;
        }
        /// <summary>
        /// 更新分级选项
        /// </summary>
        /// <param name="GradeOption">分级选项实例</param>
        /// <returns>成功状态</returns>
        public bool UpdateDic(M_Grade model)
        {
            if (model.GradeID > 0)
                return DBCenter.UpdateByID(model, model.GradeID);
            else
                return DBCenter.Insert(model) > 0;
        }
        /// <summary>
        /// 删除分级选项
        /// </summary>
        /// <param name="GradeID">选项ID</param>
        /// <returns></returns>
        public bool DelGradeOption(int ID)
        {
            M_Grade model = new M_Grade();
            return Sql.Del(model.TbName, model.PK + "=" + ID);
        }
        public void DelOptioinsByIDS(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            SqlHelper.ExecuteSql("DELETE FROM ZL_Grade WHERE " + PK + " IN(" + ids + ")");
        }

        /// <summary>
        /// 分级选项列表
        /// </summary>
        /// <param name="CateID">分类ID</param>
        /// <param name="ParentID">父选项ID</param>
        /// <returns></returns>
        public DataTable GetGradeList(int CateID, int ParentID = -1)
        {
            return SelPage(1, int.MaxValue, new Com_Filter()
            {
                storeId = CateID,
                pid = ParentID
            }).dt;
        }
        //public PageSetting GetGradeList_SPage(int cpage, int psize, int cateid, int ParentID = -1)
        //{
        //    string where = "Cate=" + cateid;
        //    if (ParentID > -1) { where += " AND ParentID=" + ParentID; }
        //    PageSetting setting = PageSetting.Single(cpage,psize, "ZL_Grade","GradeID",where,"");
        //    DBCenter.SelPage(setting);
        //    return setting;
        //}
        ///// <summary>
        ///// 分级选项列表
        ///// </summary>
        ///// <param name="CateID">分类ID</param>
        ///// <param name="ParentID">父选项ID</param>
        ///// <returns></returns>
        //public DataTable GetGradeListTop(int CateID, int ParentID, int num)
        //{
        //    string strSql = "select top " + num + " * from ZL_Grade where Cate=@CateID and ParentID=@ParentID Order by GradeID Asc";
        //    SqlParameter[] sp = new SqlParameter[] {
        //        new SqlParameter("@CateID",SqlDbType.Int),
        //        new SqlParameter("@ParentID",SqlDbType.Int)
        //    };
        //    sp[0].Value = CateID;
        //    sp[1].Value = ParentID;
        //    return SqlHelper.ExecuteTable(CommandType.Text, strSql, sp);
        //}
        public bool DelByIds(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            string strsql = "DELETE FROM ZL_Grade WHERE GradeID IN (" + ids + ")";
            return SqlHelper.ExecuteSql(strsql);
        }
     
        /// <summary>
        /// 获取分级选项实例
        /// </summary>
        /// <param name="GradeID">选项ID</param>
        /// <returns></returns>
        public M_Grade GetGradeOption(int GradeID)
        {
            string strSql = "select * from ZL_Grade where GradeID=@GradeID";
            SqlParameter[] cmdParams = new SqlParameter[] {
                new SqlParameter("@GradeID",GradeID)
            };
            using (DbDataReader sdr = SqlHelper.ExecuteReader(CommandType.Text, strSql, cmdParams))
            {
                if (sdr.Read())
                {
                    return new M_Grade().GetModelFromReader(sdr);
                }
                else
                {
                    return new M_Grade();
                }
            }
        }
        /// <summary>
        /// 同级选项是否重名
        /// </summary>
        /// <param name="CateID"></param>
        /// <param name="ParentID"></param>
        /// <param name="GradeName"></param>
        /// <returns></returns>
        public bool IsExsitGrade(int CateID, int ParentID, string GradeName)
        {
            string strSql = "select GradeID from ZL_Grade where Cate=@CateID and ParentID=@ParentID and GradeName=@GradeName";
            SqlParameter[] sp = new SqlParameter[] {
                new SqlParameter("@CateID",SqlDbType.Int),
                new SqlParameter("@ParentID",SqlDbType.Int),
                new SqlParameter("@GradeName",SqlDbType.NVarChar,50)
            };
            sp[0].Value = CateID;
            sp[1].Value = ParentID;
            sp[2].Value = GradeName;
            return DataConvert.CLng(SqlHelper.ExecuteScalar(CommandType.Text, strSql, sp)) > 0;
        }
        /// <summary>
        /// 某级选项名称是否存在，存在则返回选项ID，否咋返回0
        /// </summary>
        /// <param name="CateID">分类ID</param>
        /// <param name="ParentID">父选项ID</param>
        /// <param name="GradeName">选项名称</param>
        /// <returns>返回存在的选项ID</returns>
        public int GradeIDByName(int CateID, int ParentID, string GradeName)
        {
            string strSql = "select GradeID from ZL_Grade where Cate=@CateID and ParentID=@ParentID and GradeName=@GradeName";
            SqlParameter[] sp = new SqlParameter[] {
                new SqlParameter("@CateID",SqlDbType.Int),
                new SqlParameter("@ParentID",SqlDbType.Int),
                new SqlParameter("@GradeName",SqlDbType.NVarChar,50)
            };
            sp[0].Value = CateID;
            sp[1].Value = ParentID;
            sp[2].Value = GradeName;
            return DataConvert.CLng(SqlHelper.ExecuteScalar(CommandType.Text, strSql, sp));
        }
        /// <summary>
        /// 获取下级选项组合字符串 各选项由","隔开
        /// </summary>
        /// <param name="CateID"></param>
        /// <param name="GradeID"></param>
        /// <returns></returns>
        public string GetNextGrade(int CateID, int GradeID)
        {
            string strSql = "select GradeName from ZL_Grade where Cate=@CateID and ParentID=@ParentID Order by GradeID Asc";
            SqlParameter[] sp = new SqlParameter[] {
                new SqlParameter("@CateID",SqlDbType.Int),
                new SqlParameter("@ParentID",SqlDbType.Int)
            };
            sp[0].Value = CateID;
            sp[1].Value = GradeID;
            string gradestr = "";
            using (DbDataReader sdr = SqlHelper.ExecuteReader(CommandType.Text, strSql, sp))
            {
                while (sdr.Read())
                {
                    if (string.IsNullOrEmpty(gradestr))
                        gradestr = sdr[0].ToString();
                    else
                        gradestr = gradestr + "," + sdr[0].ToString();
                }
                sdr.Close();
                sdr.Dispose();
            }
            return gradestr;
        }

        //---------------
        public PageSetting SelPage(int cpage, int psize, Com_Filter filter)
        {
            string where = "1=1";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (filter.storeId != -100) { where += " AND Cate=" + filter.storeId; }
            if (filter.pid != -100) { where += " AND ParentID=" + filter.pid; }
            if (!string.IsNullOrEmpty(filter.skey))
            {
                where += " AND GradeName LIKE @skey";
                sp.Add(new SqlParameter("skey", "%" + filter.skey + "%"));
            }
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, PK + " DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
    }
}
