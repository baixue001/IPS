﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{
    public class B_User_Cloud : ZL_Bll_InterFace<M_User_Cloud>
    {
        public string TbName, PK;
        public M_User_Cloud initMod = new M_User_Cloud();

        public B_User_Cloud()
        {
            TbName = initMod.TbName;
            PK = initMod.PK;
        }
        public bool Del(int ID)
        {
            return Sql.Del(TbName, ID);
        }
        public bool DelByFile(string guid)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("@guid", guid) };
            string sql = "DELETE FROM " + TbName + " WHERE Guid=@guid";
            return SqlHelper.ExecuteSql(sql, sp);
        }

        public int Insert(M_User_Cloud model)
        {
            return DBCenter.Insert(model);
        }

        public DataTable Sel()
        {
            return Sql.Sel(TbName);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, "");
            DBCenter.SelPage(setting);
            return setting;
        }

        public M_User_Cloud SelReturnModel(string guid)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("guid", guid) };
            using (DbDataReader reader = Sql.SelReturnReader(TbName, " Where Guid=@guid", sp))
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
        public M_User_Cloud SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(TbName, "ID", ID))
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
        public bool UpdateByID(M_User_Cloud model)
        {
            return DBCenter.UpdateByID(model,model.ID);
        }

        public PageSetting SelByPath(int cpage, int psize, string path, int userid)
        {
            string where = "VPath=@url AND UserID=" + userid;
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("url", path) };
            PageSetting setting = PageSetting.Single(cpage, psize, TbName, PK, where, "", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        public string H_GetFolderByFType(string ftype, M_UserInfo mu)
        {
            string folder = "";
            ftype = string.IsNullOrEmpty(ftype) ? "" : ftype;
            switch (ftype.ToLower())
            {
                case "photo":
                    folder = "我的相册/";
                    //UploadType = "jpg|gif|bmp|png";
                    break;
                case "music":
                    folder = "我的音乐/";
                    //UploadType = "mp3|wma|wav|midi|flac";
                    break;
                case "video":
                    folder = "我的视频/";
                    //UploadType = "avi|mp4|f4v|m4v|rmvb|rm|flv|wm|ram|asf|wmv";
                    break;
                //case "PF":
                //    folder = "公共文件";
                //    UploadType = SiteConfig.SiteOption.UploadFileExts;
                //    break;
                case "file":
                default:
                    folder = "我的文档/";
                    //UploadType = SiteConfig.SiteOption.UploadFileExts;
                    break;
            }
            return ZLHelper.GetUploadDir_User(mu, "YunPan", folder, "");
        }
    }
}
