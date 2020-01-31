using System;
using System.Data;
using ZoomLa.Model;
using ZoomLa.Common;
using ZoomLa.SQLDAL;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Data.SqlClient;
using ZoomLa.Components;
using System.Collections.Generic;
using System.Web;
using System.Data.Common;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{

    public class B_CollectionItem
    {
        public B_CollectionItem()
        {
            PK = initMod.PK;
            strTableName = initMod.TbName;
        }
        private string PK, strTableName;
        private M_CollectionItem initMod = new M_CollectionItem();
        public DataTable Sel(int ID)
        {
            return Sql.Sel(strTableName, PK, ID);
        }
        public M_CollectionItem SelReturnModel(int ID)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, PK, ID))
            {
                if (reader.Read())
                {
                    return new M_CollectionItem().GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        private M_CollectionItem SelReturnModel(string strWhere)
        {
            using (DbDataReader reader = Sql.SelReturnReader(strTableName, strWhere))
            {
                if (reader.Read())
                {
                    return new M_CollectionItem().GetModelFromReader(reader);
                }
                else
                {
                    return null;
                }
            }
        }
        public DataTable Sel()
        {
            return Sql.Sel(strTableName);
        }
        public PageSetting SelPage(int cpage, int psize)
        {
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            PageSetting setting = PageSetting.Single(cpage, psize, strTableName, PK, where, PK + " DESC", sp);
            DBCenter.SelPage(setting);
            return setting;
        }
        public bool UpdateByID(M_CollectionItem model)
        {
            return DBCenter.UpdateByID(model, model.CItem_ID);
        }
        public bool GetDelete(int ID)
        {
            return Sql.Del(strTableName, "CItem_ID=" + ID);
        }
        public int insert(M_CollectionItem model)
        {
            return DBCenter.Insert(model);
        }
        public int GetInsert(M_CollectionItem model)
        {
            return insert(model);
        }
        public bool GetUpdate(M_CollectionItem model)
        {
            return UpdateByID(model);
        }
        public bool InsertUpdate(M_CollectionItem model)
        {
            if (model.CItem_ID > 0)
                UpdateByID(model);
            else
                insert(model);
            return true;
        }
        public bool DelByIds(string ids)
        {
            SafeSC.CheckIDSEx(ids);
            return Sql.Del(strTableName, "CItem_ID in (" + ids + ")");
        }
        public M_CollectionItem GetSelect(int ID)
        {
            return SelReturnModel(ID);
        }
        public DataTable SelBySwitch(int status)
        {
            string sql = "Select * From " + strTableName + " Where Switch=" + status + " Order BY AddTime DESC";
            return SqlHelper.ExecuteTable(CommandType.Text, sql);
        }

        public DataTable Select_All()
        {
            return Sel();
        }
        private string Fromatstr(string str)
        {
            str = Regex.Replace(str, @"[\r\n]|[ \t]*", "");
            return HttpUtility.HtmlEncode(str);
        }

        private string checkList(string stext, string etext, string htmlstr)
        {
            return "";
        }



        //执行采集
        public int ExeColl(bool readto)
        {
            int scnum = 0;
            //B_CollectionItem bc = new B_CollectionItem();
            //DataTable dtUrl = new DataTable();
            //dtUrl.Columns.Add(new DataColumn("url", System.Type.GetType("System.String")));
            ////查询所有开始执行采集的项目
            //DataTable dt = bc.SelBySwitch(1);
            //foreach (DataRow dr in dt.Rows)
            //{
            //    if (dr["LinkList"] != null) //判断是否有规则，满足条件
            //    {
            //        scnum = dr["LinkList"].ToString().Split(',').Length;
            //    }
            //    //结束采集
            //    if (readto)
            //    {
            //        M_CollectionItem mc = bc.GetSelect(DataConverter.CLng(dr["CItem_ID"]));
            //        //停止采集
            //        mc.Switch = 2;
            //        bc.GetUpdate(mc);
            //    }
            //}
            return scnum;
        }


        /// <summary>
        /// 写入字段
        /// </summary>
        /// <param name="info"></param>
        /// <param name="strhtml"></param>
        /// <param name="dt"></param>
        /// <param name="tablefiled"></param>
        private string Insertinfo(string info, string strhtml, string tablefiledname)
        {
            string s = "";
            //获取列表规则
            if (!string.IsNullOrEmpty(info))
            {
                DataSet ds = function.XmlToTable(info);
                if (ds.Tables.Count > 0)
                {
                    foreach (DataTable dt in ds.Tables)//规则里的数据
                    {
                        if (dt.TableName == tablefiledname)
                        {
                            //是否是使用默认值
                            if (dt.Columns[0].ColumnName == tablefiledname + "_Default")
                            {

                            }
                            //是否是指定值
                            if (dt.Columns[0].ColumnName == tablefiledname + "_Appoint")
                            {
                                foreach (DataRow dr in dt.Rows)
                                {
                                    s = dr[tablefiledname + "_Appoint"].ToString();
                                }
                            }
                            //是否是使用规则
                            if (dt.Columns[0].ColumnName == tablefiledname + "_Id")
                            {
                                s = SetField(info, tablefiledname, strhtml);
                            }
                            /*结束*/
                        }
                    }
                }
            }
            return s;
        }


        private string SetUrl(string Url, string str)
        {
            string strurl = "";
            //切割字符串地址
            string[] UrlArr = str.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            if (UrlArr.Length > 0)
            {
                //循环地址
                foreach (string ustr in UrlArr)
                {
                    strurl += GetStr(Url, ustr);
                }
            }
            else
            {
                strurl = GetStr(Url, str);
            }

            return strurl;
        }

        private static string GetStr(string Url, string ustr)
        {
            string strurl = "";
            if (ustr.IndexOf("http://") < 0)
            {
                //切割地址
                string[] urlinfo = ustr.Split(new char[] { '/' });
                int i = 0;
                //循环切割后的地址字符
                foreach (string s in urlinfo)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        i++;
                    }
                }
                if (i > 1)
                {
                    i--;
                }
                //切割当前页面地址
                string[] infoarr = Url.Split(new char[] { '/' });
                for (int j = 0; j < infoarr.Length - i; j++)
                {
                    strurl += infoarr[j].ToString() + "/";
                }
                strurl += urlinfo[urlinfo.Length - 1] + "\n\r";

            }
            else
            {
                strurl += ustr + "\n\r";
            }
            return strurl;
        }
        /// <summary>
        /// 判断URL列表里的URL是否重复。如果重复就选择第一个
        /// </summary>
        /// <param name="strurl">url列表</param>
        /// <returns></returns>
        private string orderUrl(string strurl)
        {
            string[] str = strurl.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            string[] str2 = new string[str.Length];
            strurl = "";
            for (int i = 0; i < str.Length; i++)
            {
                bool b = true;
                for (int j = 0; j < str2.Length; j++)
                {
                    if (str[i] == str2[j])
                    {
                        b = false;
                    }
                }
                if (b)
                {
                    str2[i] = str[i];
                    strurl += str[i] + "\n\r";
                }
            }
            return strurl;
        }


        public string GetRemoteHtmlCode(string Url, Encoding code)
        {
            //EC.GetRemoteObj ecd = new EC.GetRemoteObj();
            //return ecd.Url(Url,code);
            return "";
        }


        //获取设置规则
        private string SetField(string config, string IName, string htmlstr)
        {
            string sf = "";
            //将XML设置成DataSet
            if (!string.IsNullOrEmpty(config))
            {
                DataSet ds = function.XmlToTable(config);
                if (ds.Tables.Count > 0)
                {
                    //获得表
                    foreach (DataTable dt in ds.Tables)
                    {
                        //是否是当前字段设置的XML节点
                        if (dt.TableName == IName)
                        {
                            //是否是使用规则
                            if (dt.Columns[0].ColumnName == IName + "_Id")
                            {
                                foreach (DataTable dtx in ds.Tables)
                                {
                                    if (dtx.TableName == IName + "_CollConfig")
                                    {
                                        foreach (DataRow dr in dtx.Rows)
                                        {
                                            string filestr = dr["FieldStart"].ToString();
                                            string filend = dr["FieldEnd"].ToString();
                                            sf = checkList(filestr.Replace("&lt;", "<").Replace("&gt;", ">"), filend.Replace("&lt;", "<").Replace("&gt;", ">"), htmlstr);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return sf;
            //}
        }
    }
}