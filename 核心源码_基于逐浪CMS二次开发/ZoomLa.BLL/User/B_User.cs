using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using ZoomLa.BLL.Helper;
using ZoomLa.BLL.SYS;
using ZoomLa.BLL.System.Security;
using ZoomLa.Common;
using ZoomLa.Model;
using ZoomLa.Safe;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;

namespace ZoomLa.BLL
{
    public class B_User
    {
        public const string preFix = "User_";
        private string strTableName = "ZL_User", PK = "UserID";
        private string strTableName2 = "ZL_UserBase";
        private M_UserInfo initMod = new M_UserInfo();
        //M_UserBaseField fieldMod = new M_UserBaseField();
        HttpContext curReq;
        public string SessionID
        {
            get
            {
                try
                {
                    return curReq.Session.Id;
                }
                catch (InvalidOperationException) { return ""; }
            }
        }
        public B_User()
        {
          
        }
        public B_User(HttpContext current)
        {
            curReq = current;
        }

        //-------------Insert
        public int Add(M_UserInfo model)
        {
            return AddWithCheck(model);
        }
        //添加会员信息
        private int AddWithCheck(M_UserInfo model)
        {
            model.UserName = model.UserName.Replace(" ", "");
            model.Email = string.IsNullOrEmpty(model.Email) ? "" : model.Email.Replace(" ", "");
            CommonReturn retMod = CheckRegInfo(model);
            if (!retMod.isok) { throw new Exception(retMod.err); }
            //信息补充
            if (string.IsNullOrEmpty(model.Question)) { model.Question = function.GetRandomString(10); }
            if (string.IsNullOrEmpty(model.Answer)) { model.Answer = function.GetRandomString(6); }
            if (string.IsNullOrEmpty(model.UserFace)) { model.UserFace = "/Images/userface/noface.png"; }
            if (string.IsNullOrEmpty(model.Email)) { model.Email = function.GetRandomEmail(); }
            //推荐人信息不存在,则重置为空
            if (!string.IsNullOrEmpty(model.ParentUserID)) { bool flag = IsExit(DataConvert.CLng(model.ParentUserID)); if (!flag) model.ParentUserID = ""; }
            if (string.IsNullOrEmpty(model.HoneyName)) { model.HoneyName = model.UserName; }
            int uid = DBCenter.Insert(model);
            //[c]
            //try
            //{
            //    //model.SiteID = function.StrToASCII(function.GetpyChar(model.UserName).ToUpper()[0].ToString());//获取首字母
            //    //model.PayPassWord = curReq.Request.RawUrl;
            //    List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("url", curReq.Request.RawUrl) };
            //    DBCenter.UpdateSQL("ZL_User", "RegUrl=@url", "UserID=" + uid, sp);
            //}
            //catch (Exception ex) { ZLLog.L("用户注册链接获取失败:[" + uid + "]" + ex.Message); }
            return uid;
        }
        //-------------SELECT
        public DataTable SelPage(F_User filter) { return SelPage(1, int.MaxValue, filter).dt; }
        //new 与userbase联接查询
        public PageSetting SelPage(int cpage, int psize, F_User filter)
        {
            //int psize, int cpage, out int itemCount, string action, string value, string order, string charstr, int status
            string where = "1=1 ";
            List<SqlParameter> sp = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(filter.groupIds) && filter.groupIds != "0")
            {
                SafeSC.CheckIDSEx(filter.groupIds);
                where += " AND A.GroupID IN (" + filter.groupIds + ")";
            }
            if (!string.IsNullOrEmpty(filter.structIds) && filter.structIds != "0")
            {
                where += " AND StructureID LIKE '%," + DataConvert.CLng(filter.structIds) + ",%' ";
            }
            if (!string.IsNullOrEmpty(filter.roleIds) && filter.roleIds != "0")
            {
                where += " AND UserRole LIKE '%," + DataConvert.CLng(filter.roleIds) + ",%' ";
            }
            if (!string.IsNullOrEmpty(filter.uids))
            {
                SafeSC.CheckIDSEx(filter.uids);
                where += " AND A.UserID IN (" + filter.uids + ")";
            }
            DateTime date = DateTime.Now;
            if (!string.IsNullOrEmpty(filter.regSDate) && DateTime.TryParse(filter.regSDate, out date))
            {
                where += " AND A.RegTime>=@regSDate";
                sp.Add(new SqlParameter("regSDate", date));
            }
            if (!string.IsNullOrEmpty(filter.regEDate) && DateTime.TryParse(filter.regEDate, out date))
            {
                where += " AND A.RegTime<@regEDate";
                sp.Add(new SqlParameter("regEDate", date));
            }
            if (filter.siteId > 0)
            {
                where += " AND A.SiteId=" + filter.siteId;
            }
            //------------
            if (!string.IsNullOrEmpty(filter.uname))
            {
                // OR A.[PERMISSIONS] LIKE @uname
                where += " AND (A.UserName LIKE @uname OR A.HoneyName LIKE @uname)";
                sp.Add(new SqlParameter("uname", "%" + filter.uname + "%"));
            }
            if (!string.IsNullOrEmpty(filter.status)) { SafeSC.CheckIDSEx(filter.status); where += " AND A.Status IN (" + filter.status + ")"; }
            string orderBy = StrHelper.SQL_OrderBy(filter.orderBy,"a.userid,regtime,purse,lastlogintime,userexp","A.UserID DESC");
            //为兼容可改为视图
            //string mtable = "(SELECT U.*,UB.Mobile FROM ZL_User U LEFT JOIN ZL_UserBase UB ON U.UserID=UB.UserID)";
            string fields = "A.*,B.*,(SELECT GroupName FROM ZL_Group WHERE GroupID=A.GroupID) AS GroupName";
            PageSetting setting = new PageSetting()
            {
                psize = psize,
                cpage = cpage,
                fields = fields,
                pk = "A.UserID",
                t1 = "ZL_User",
                t2 = "ZL_UserBase",
                on = "A.UserID=B.UserID",
                where = where,
                order = orderBy,
                spList = sp
            };
            DBCenter.SelPage(setting);
            return setting;
        }
        public DataTable Sel()
        {
            return DBCenter.Sel(strTableName);
        }
        /// <summary>
        /// 返回所有会员，并包含真实名称，会员组
        /// </summary>
        public DataTable SelAll()
        {
            //string sql = "Select u.*,g.GroupName,ub.Mobile,ub.TrueName,ub.UserFace From ZL_User as u Left Join ZL_Group as g On u.GroupID=g.GroupID Left Join ZL_UserBase as ub On u.UserID=ub.UserID";
            return DBCenter.JoinQuery(new PageSetting()
            {
                fields = "A.*,B.GroupName",
                t1 = strTableName,
                t2 = "ZL_Group",
                on = "A.GroupID=B.GroupID",
            });
        }
        //根据ids查询用户集合
        public DataTable SelectUserByIds(string ids)
        {
            ids = FormatIDS(ids);
            if (string.IsNullOrEmpty(ids)) { return null; }
            return DBCenter.SelWithField(strTableName, "*,Salt AS UserFace,HoneyName AS TrueName", "UserID IN(" + ids + ")", "UserID DESC");
        }
        /// <summary>
        /// 仅返回少量数据,专用于配合选择用户
        /// </summary>
        public string SelByIDS(string ids)
        {
            ids = FormatIDS(ids);
            if (string.IsNullOrEmpty(ids)) { return null; }
            DataTable dt = DBCenter.SelWithField(strTableName, "UserID,UserName,HoneyName,Salt AS UserFace", "UserID IN(" + ids + ")");
            return JsonConvert.SerializeObject(dt);
            //return DBCenter.JoinQuery("A.UserID,A.UserName,A.salt AS UserFace,B.TrueName", strTableName, strTableName2, "A.UserID=B.UserID", "A.UserID IN(" + ids + ")", "A.UserID DESC");
        }
        /// <summary>
        /// 推广用户列表
        /// </summary>
        /// <returns></returns>
        public DataTable SelPromoUser(int puserid = 0, string search = "", string startdate = "", string enddate = "")
        {
            PageSetting setting = new PageSetting();
            setting.fields = "A.*,B.UserName AS PromoUser";
            setting.t1 = "ZL_User";
            setting.t2 = "ZL_User";
            setting.on = "A.ParentUserID=B.UserID";
            setting.order = "A.RegTime";
            setting.where = "A.ParentUserID>0";
            if (puserid > 0)
            {
                setting.where += " AND A.ParentUserID=" + puserid;
            }
            if (!string.IsNullOrEmpty(search))
            {
                setting.spList.Add(new SqlParameter("search", "%" + search + "%"));
                setting.where += " AND A.UserName LIKE @search";
            }
            if (!string.IsNullOrEmpty(startdate))
            {
                setting.spList.Add(new SqlParameter("startdate", startdate));
                setting.where += " AND A.RegTime>@startdate";
            }
            if (!string.IsNullOrEmpty(enddate))
            {
                setting.spList.Add(new SqlParameter("enddate", enddate));
                setting.where += " AND A.RegTime<@enddate";
            }
            setting.sp = setting.spList.ToArray();
            //string sql = "SELECT A.*,B.UserName AS PromoUser FROM ZL_User A LEFT JOIN ZL_User B ON A.ParentUserID=B.UserID WHERE A.ParentUserID>0"+strwhere+" ORDER BY A.RegTime";
            //return SqlHelper.ExecuteTable(sql,sp);
            return DBCenter.JoinQuery(setting);
        }
        /// <summary>
        /// 获取当前用户总数
        /// </summary>
        public int GetUserNameListTotal(string keyword = "")
        {
            string where = "";
            List<SqlParameter> splist = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(keyword))
            {
                where = "UserName LIKE @UserName";
                splist.Add(new SqlParameter("UserName", "%" + keyword + "%"));
            }
            return DBCenter.Count(strTableName, where, splist);
        }
        /// <summary>
        /// 用于导出Excel和生成Html
        /// </summary>
        public DataTable GetUserBaseByuserid(string uids)
        {
            SafeSC.CheckIDSEx(uids);
            return DBCenter.JoinQuery("*", strTableName, strTableName2, "A.UserID=B.UserID", "B.UserID IN (" + uids + ")");
        }
        public bool IsExit(int userID)
        {
            return IsExist("uid", userID.ToString());
        }
        /// <summary>
        /// 用户名是否存在，用于注册等时判断
        /// </summary>
        public bool IsExistUName(string uname)
        {
            return IsExist("uname", uname);
        }
        //根据会员名判断身份证是否存在
        public bool IsExitcard(string idcard)
        {
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("IDCard", idcard) };
            return DBCenter.IsExist(strTableName2, "IDCard=@IDCard", sp);
        }
        /// <summary>
        /// 邮箱是否存在,如为空也为true
        /// </summary>
        public bool IsExistMail(string email)
        {
            return IsExist("email", email);
        }
        /// <summary>
        /// 用于社会化登录,检测是否有同值的openID存在
        /// </summary>
        public bool IsExistByOpenID(string openID)
        {
            return IsExist("openid", openID);
        }
        /// <summary>
        /// 指定字段是否重复,为空也重复,true:重复
        /// openid,email,uname,mobile,uid
        /// </summary>
        public bool IsExist(string type, string val)
        {
            val = (val ?? "").Replace(" ", "");
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(val)) { return true; }
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("val", val) };
            switch (type)
            {
                case "email":
                    return DBCenter.IsExist(strTableName, "Email=@val", sp);
                case "mobile":
                    return DBCenter.IsExist(strTableName2, "Mobile=@val", sp);
                case "uname":
                    return DBCenter.IsExist(strTableName, "UserName=@val", sp);
                case "uid":
                    return DBCenter.IsExist(strTableName, "UserID=@val", sp);
                case "openid":
                    return DBCenter.IsExist("ZL_UserApp", "AppUid=@val", sp);
                case "ume"://手机,用户名,邮箱
                    DataTable dt = DBCenter.JoinQuery("A.*,B.Mobile", strTableName, strTableName2, "A.UserID=B.UserID", "A.UserName=@val OR A.Email=@val OR B.Mobile=@val", "", sp.ToArray());
                    return dt.Rows.Count > 0;
                default:
                    throw new Exception("指定的验证方式不存在");
            }
        }
        //----------UPDATE
        public bool UpdateByID(M_UserInfo model) { ZLCache.ClearByIDS(model.UserID.ToString()); return UpDateUser(model); }
        public bool UpDateUser(M_UserInfo model)
        {
            DBCenter.UpdateByID(model, model.UserID);
            ZLCache.ClearByIDS(model.UserID.ToString());
            return true;
        }
        public bool UpdateGroupId(string ids, int groupId)
        {
            ids = StrHelper.PureIDSForDB(ids);
            SafeSC.CheckIDSEx(ids);
            DBCenter.UpdateSQL(strTableName, "GroupID=" + groupId, "UserID IN (" + ids + ")", null);
            ZLCache.ClearByIDS(ids);
            return true;
        }
        //开通云盘
        public bool UpdateIsCloud(int userid, int value)
        {
            UpdateField("IsCloud", value.ToString(), userid);
            return true;
        }
        public static void UpdateField(string field, string value, int uid, bool clear = true)
        {
            SafeSC.CheckDataEx(field);
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("value", value) };
            DBCenter.UpdateSQL("ZL_User", field + " = @value", "UserID=" + uid, sp);
            if (clear) { ZLCache.ClearByIDS(uid); }
        }
        /// <summary>
        /// 批量审核,或禁用  0:启用,1:禁用
        /// </summary>
        public bool BatAudit(string ids, int type = 0)
        {
            SafeSC.CheckIDSEx(ids);
            List<SqlParameter> sp = new List<SqlParameter>();
            switch (type)
            {
                case 0://解锁用户
                    DBCenter.UpdateSQL(strTableName, "Status=0", "UserID IN(" + ids + ")", null);
                    break;
                case 1://锁定用户
                    sp.Add(new SqlParameter("LastLockTime", DateTime.Now));
                    DBCenter.UpdateSQL(strTableName, "Status=1,LastLockTime=@LastLockTime", "UserID IN(" + ids + ")", sp);
                    break;
                case 2://认证店铺
                    sp.Add(new SqlParameter("EDate", DateTime.Now.AddYears(3)));
                    DBCenter.UpdateSQL(strTableName, "State=2,CerificateDeadLine=@EDate", "UserID IN(" + ids + ")", sp);
                    break;
                case 3: //取消认证
                    DBCenter.UpdateSQL(strTableName, "State=0", "UserID IN(" + ids + ")", null);
                    break;
            }
            ZLCache.ClearByIDS(ids);
            return true;
        }
        //-----------------------------------Delete
        public bool DelUserById(int userID)
        {
            DelUserById(userID.ToString());
            return true;
        }
        public void DelUserById(string uids)
        {
            if (string.IsNullOrEmpty(uids)) { return; }
            SafeSC.CheckIDSEx(uids);
            DBCenter.DelByWhere(strTableName, "UserID IN (" + uids + ")");
            DBCenter.DelByWhere(strTableName2, "UserID IN (" + uids + ")");
            DBCenter.DelByWhere("ZL_UserAPP", "UserID IN (" + uids + ")");

        }
        //-----------------------------------直接获取
        public M_UserInfo GetUserByUserID(int UserID)
        {
            return SelReturnModel(UserID);
        }
        public M_UserInfo SelReturnModel(int id)
        {
            if (id < 1) { return new M_UserInfo(true); }
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, PK, id))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_UserInfo(true);
                }
            }
        }
        public M_UserInfo GetSelectByEmail(string email)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("Email", email) };
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, "Email=@Email", sp))
            {
                if (reader.Read())
                    return initMod.GetModelFromReader(reader);
                else
                    return new M_UserInfo(true);
            }
        }
        //仅用于能力中心,注册用户时使用,彼时，该字段存校验码
        public M_UserInfo GetSelectByRemark(string remark)
        {
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("Remark", remark) };
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName,
               "Remark=@Remark", sp))
            {
                if (reader.Read())
                    return initMod.GetModelFromReader(reader);
                else
                    return new M_UserInfo(true);
            }
        }
        public M_UserInfo GetUserByName(string username)
        {
            return GetUserByName(username, "");
        }
        public M_UserInfo GetUserByName(string username, string upwd)
        {
            List<SqlParameter> sp = new List<SqlParameter>() { new SqlParameter("uname", username) };
            string where = "UserName=@uname";
            if (!string.IsNullOrEmpty(upwd)) { sp.Add(new SqlParameter("upwd", upwd)); where += " AND UserPwd=@upwd"; }
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, where, sp.ToArray()))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_UserInfo(true);
                }
            }
        }
        public M_UserInfo GetUserByWorkNum(string worknum)
        {
            if (string.IsNullOrWhiteSpace(worknum)) { return new M_UserInfo(true); }
            string strSql = "select * from ZL_User where WorkNum=@worknum";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("worknum", worknum) };
            using (DbDataReader rdr = SqlHelper.ExecuteReader(CommandType.Text, strSql, sp))
            {
                if (rdr.Read())
                {
                    return initMod.GetModelFromReader(rdr);
                }
                else
                {
                    return new M_UserInfo(true);
                }
            }
        }
        public M_UserInfo GetUserByUME(string uname)
        {
            M_UserInfo mu = new M_UserInfo(true);
            uname = (uname ?? "").Replace(" ", "");
            if (string.IsNullOrEmpty(uname)) { return mu; }
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("uname", uname) };
            DataTable dt = DBCenter.JoinQuery("A.*,B.Mobile", strTableName, strTableName2, "A.UserID=B.UserID", "A.UserName=@uname OR A.Email=@uname OR B.Mobile=@uname", "", sp);
            if (dt.Rows.Count > 0) { mu = mu.GetModelFromReader(dt.Rows[0]); }
            return mu;
        }
        //用于邮箱验证,手机验证
        public M_UserInfo GetUserByCheckNum(string uname, string checkNum)
        {
            if (string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(checkNum)) { return new M_UserInfo(true); }
            List<SqlParameter> sp = new List<SqlParameter>() {
                new SqlParameter("uname", uname.Trim()),
                new SqlParameter("checkNum",checkNum.Trim())
            };
            string where = " checkNum=@checkNum AND UserName=@uname";
            using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, where, sp.ToArray()))
            {
                if (reader.Read())
                {
                    return initMod.GetModelFromReader(reader);
                }
                else
                {
                    return new M_UserInfo(true);
                }
            }
        }
        //-----------------------------------前端登录
        public M_UserInfo LoginUser(string username, string userpwd, bool ismd5 = false)
        {
            return Authenticate(username, userpwd, "username", ismd5);
        }
        public M_UserInfo AuthenticateUser(string uname, string upwd, bool ismd5 = false)
        {
            return Authenticate(uname, upwd, "username", ismd5);
        }
        public M_UserInfo AuthenticateEmail(string email, string upwd)
        {
            return Authenticate(email, upwd, "email");
        }
        public M_UserInfo AuthenticateID(int UserID, string upwd)
        {
            return Authenticate(UserID.ToString(), upwd, "userid");
        }
        public M_UserInfo AuthenByMobile(string mobile, string upwd)
        {
            return Authenticate(mobile, upwd, "mobile");
        }
        /// <summary>
        /// 同时校验用户名,邮箱,手机号,任一皆可登录
        /// </summary>
        public M_UserInfo AuthenByUME(string uname, string upwd)
        {
            return Authenticate(uname, upwd, "ume");
        }
        /// <summary>
        /// 用户登录,支持ID,Email,用户名
        /// userid,email,username,worknum,mobile,ume
        /// </summary>
        private M_UserInfo Authenticate(string uname, string upwd, string type, bool ismd5 = false)
        {
            M_UserInfo mu = new M_UserInfo(true);
            if (string.IsNullOrEmpty(uname) || string.IsNullOrEmpty(upwd)) { return mu; }
            if (!ismd5) { upwd = StringHelper.MD5(upwd); }
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("uname", uname), new SqlParameter("upwd", upwd) };
            string where = "UserPwd=@upwd AND ";
            switch (type)
            {
                case "userid":
                    where += "UserID=@uname";
                    break;
                case "email":
                    where += "Email=@uname";
                    break;
                case "worknum":
                    where += "WorkNum=@uname";
                    break;
                case "mobile":
                case "ume":
                    break;
                case "username":
                default:
                    where += "UserName=@uname";
                    break;
            }
            if (type.Equals("mobile"))
            {
                DataTable dt = SqlHelper.JoinQuery("TOP 1 A.*,B.Mobile", strTableName, strTableName2, "A.UserID=B.UserID", "B.Mobile=@uname AND UserPwd=@upwd", "", sp);
                if (dt.Rows.Count > 0) { mu = mu.GetModelFromReader(dt.Rows[0]); }
            }
            else if (type.Equals("ume"))
            {
                DataTable dt = DBCenter.JoinQuery("A.*,B.Mobile", strTableName, strTableName2, "A.UserID=B.UserID", "(A.UserName=@uname OR A.Email=@uname OR B.Mobile=@uname) AND UserPwd=@upwd", "", sp);
                if (dt.Rows.Count > 0) { mu = mu.GetModelFromReader(dt.Rows[0]); }
            }
            else
            {
                using (DbDataReader reader = DBCenter.SelReturnReader(strTableName, where, sp))
                {
                    if (reader.Read()) { mu = initMod.GetModelFromReader(reader); }
                }
            }
            if (!mu.IsNull)
            {
                List<SqlParameter> usp = new List<SqlParameter>() { new SqlParameter("LastLoginTime", DateTime.Now) };
                //LastLoginIP='" + IPScaner.GetUserIP() + "',
                DBCenter.UpdateSQL(strTableName, "LastLoginTime=@LastLoginTime,LoginTimes=" + (mu.LoginTimes + 1), "UserID=" + mu.UserID, usp);
            }
            return mu;
        }
        //-----------------------------------登录检测
        /// <summary>
        /// False则重取数据
        /// </summary>
        public M_UserInfo GetLogin(bool flag = true)
        {
            M_UserInfo mu = ZLCache.GetUser(SessionID);
            string loginName =CookieGet("LoginName"), password = CookieGet("Password");
            if (mu != null && flag)
            {
                return mu;
            }
            else if (string.IsNullOrEmpty(loginName))
            {
                return new M_UserInfo(true);
            }
            else
            {
              
                if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(password)) return new M_UserInfo(true);
                mu = AuthenticateUser(StringHelper.Base64StringDecode(loginName), password, true);
                if (mu != null && !mu.IsNull) { ZLCache.AddUser(SessionID, mu); }
                return mu;
            }
        }
        public bool CheckUserStatus(M_UserInfo mu, ref string err)
        {
            bool flag = true;
            if (mu.IsNull) { return false; }
            if (mu.Status != 0)
            {
                flag = false;
                switch (mu.Status)
                {
                    case 1:
                        err = "该用户帐号已被锁定";
                        break;
                    case 2:
                    case 5:
                        err = "该用户帐号尚未认证";
                        break;
                    case 3:
                    case 4:
                        err = "该用户账号尚未邮件认证";
                        break;
                }
            }
            return flag;
        }
        //[控制器中检测]或以属性的方式
        //public bool CheckIsLogged(HttpContext ctx, string returnUrl = "")
        //{
        //    B_User buser = new B_User(ctx);
        //    M_UserInfo mu = ZLCache.GetUser(buser.SessionID);
        //    if (mu != null && !mu.IsNull) { return true; }
        //    string url = "~/User/Login?ReturnUrl=" + returnUrl.Replace("&", HttpUtility.UrlEncode("&"));
        //    if (HttpContext.Current.Request.Cookies[ckName] == null)
        //    {
        //        HttpContext.Current.Response.Redirect(url); return false;
        //    }
        //    else
        //    {
        //        string loginName = CookieGet("LoginName");
        //        string password = HttpContext.Current.Request.Cookies[ckName]["Password"];
        //        if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(password)) { HttpContext.Current.Response.Redirect(url); return false; }
        //        SqlParameter[] sp = new SqlParameter[] { new SqlParameter("UserName", StringHelper.Base64StringDecode(loginName)), new SqlParameter("UserPwd", password) };
        //        object o = SqlHelper.ExecuteScalar(CommandType.Text, "Select UserID From ZL_User Where UserName=@UserName And UserPwd=@UserPwd", sp);
        //        if (o == null) { HttpContext.Current.Response.Redirect(url); return false; }
        //        return true;
        //    }

        //}
        private string CookieGet(string key) { return CookieHelper.Get(curReq, preFix + key); }
        private void CookieSet(string key, string value) { CookieHelper.Set(curReq, preFix + key, value); }


        public bool CheckLogin()
        {
            M_UserInfo mu = ZLCache.GetUser(SessionID);
            if (mu != null && !mu.IsNull) return true;
            try
            {
                if (string.IsNullOrEmpty(CookieGet("")))
                {
                    return false;
                }
                else
                {
                    string loginName = CookieGet("LoginName");
                    string password = CookieGet("Password");
                    if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(password)) return false;
                    loginName = StringHelper.Base64StringDecode(loginName);
                    return (!AuthenticateUser(loginName, password, true).IsNull);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public void ClearCookie()
        {
            ZLCache.ClearByKeys(curReq.Session.Id);
            CookieHelper.ClearAll(curReq);
        }
        /// <summary>
        /// 设定登录状态
        /// </summary>
        public void SetLoginState(M_UserInfo model, string CookieStatus = "day")
        {
            if (model == null || string.IsNullOrEmpty(model.UserName)) { throw new Exception("登录的用户信息为空"); }
            CookieStatus = CookieStatus.ToLower();
            ZLCache.AddUser(SessionID, model);
            //curReq.Response.Cookies["UserState"]["WorekNum"] = model.WorkNum;
            CookieSet("UserID",model.UserID.ToString());
            CookieSet("LoginName", StringHelper.Base64StringEncode(model.UserName));
            CookieSet("Password",model.UserPwd);
            //switch (CookieStatus)
            //{
            //    case "none"://即时关闭浏览器失效
            //        break;
            //    case "minute":
            //        curReq.Response.Cookies[ckName].Expires = DateTime.Now.AddMinutes(30);
            //        break;
            //    case "day":
            //        curReq.Response.Cookies[ckName].Expires = DateTime.Now.AddDays(7);
            //        break;
            //    case "year":
            //        curReq.Response.Cookies[ckName].Expires = DateTime.Now.AddYears(1);
            //        break;
            //    case "month":
            //    default:
            //        curReq.Response.Cookies[ckName].Expires = DateTime.Now.AddMonths(1);
            //        break;
            //}
        }
        /*-----------Not Deal-------------*/
        #region UserBase
        /// <summary>
        /// 添加用户基本信息
        /// </summary>
        public bool AddBase(M_Uinfo model)
        {
            if (model.UserId < 1) { throw new Exception("错误,未指定用户身份"); }
            model._pk = "";
            DBCenter.Insert(model);
            return true;
        }
        public M_Uinfo GetUserBaseByuserid(int userid)
        {
            if (userid < 1) { return new M_Uinfo(true); }
            using (DbDataReader rdr = DBCenter.SelReturnReader("ZL_UserBase", " UserID=" + userid))
            {
                if (rdr.Read())
                {
                    return new M_Uinfo().GetModelFromReader(rdr);
                }
                else
                {
                    M_Uinfo model = new M_Uinfo();
                    if (model.UserId < 1)
                    {
                        model.UserId = userid;
                        AddBase(model);
                    }
                    return model;
                }
            }
        }
        public bool UpdateBase(M_Uinfo model)
        {
            return DBCenter.UpdateByID(model, model.UserId);
        }
        /// <summary>
        /// 更新用户自定义字段基本信息,不远程
        /// </summary>
        public bool UpdateUserFile(int uid, DataTable dt)
        {

            DBCenter.UpdateSQL("ZL_UserBase", BLLCommon.GetFieldAndPara(dt),"UserID="+uid, BLLCommon.GetParameters(dt));
            return true;
        }
        #endregion
        
        #region 模型
        public void InsertModel(DataTable modelinfo, string tablename)
        {
            //SafeSC.CheckDataEx(tablename);
            //string dxsql = "insert into " + tablename + " " + Sql.InsertSql(modelinfo);
            //SqlParameter[] parameter = Sql.ContentPara(modelinfo);
            //try
            //{
            //    SqlHelper.ExistsSql(dxsql, parameter);
            //}
            //catch (Exception) { }
            DBCenter.Insert(tablename,BLLCommon.GetFields(modelinfo),BLLCommon.GetParas(modelinfo),BLLCommon.GetParameters(modelinfo).ToArray());
        }
        /// <summary>
        /// 获取会员模型信息
        /// </summary>
        /// <param Name="TableName">存储会员模型信息的表名</param>
        /// <param Name="ID">记录ID或会员ID由type决定</param>
        /// <param Name="Type">0-会员ID 1-记录ID</param>
        public DataTable GetUserModeInfo(string TableName, int ID, int Type)
        {
            // if (StationGroup.RemoteUser) { umod.str = TableName; umod.uid = ID; umod.gid = Type; return APIHelper.UserApi_DT("GetUserModeInfo", umod); }
            string strSql = "select * from " + TableName;
            SqlParameter[] sp;
            switch (Type)
            {
                case 0:
                    strSql = strSql + " where UID=@ID and Status=0";
                    strSql = strSql + " order by UpdateTime desc,id desc";
                    sp = new SqlParameter[] { new SqlParameter("@ID", SqlDbType.Int) };
                    sp[0].Value = ID;
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, sp);
                case 3:
                    strSql = strSql + " order by UpdateTime desc,id desc";
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, null);
                case 4:
                    strSql = strSql + " order by id desc";
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, null);
                /// jc:type=9为取出整表信息
                case 9:
                    strSql = strSql + " where Status=0 order by UpdateTime desc,id desc";
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, null);
                case 10:
                    strSql = strSql + " where Status=1 order by UpdateTime desc,id desc";
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, null);
                case 11:
                    strSql = strSql + " order by Parentid asc,ID desc";
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, null);
                case 12:
                    strSql = strSql + " where ID=@ID ";
                    sp = new SqlParameter[] {
                    new SqlParameter("@ID",SqlDbType.Int)
                      };
                    sp[0].Value = ID;
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, sp);
                case 13:
                    strSql = strSql + " where Parentid=@ID order by Parentid asc,ID desc";
                    sp = new SqlParameter[] {
                         new SqlParameter("@ID",SqlDbType.Int)
                       };
                    sp[0].Value = ID;
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, sp);
                case 14:
                    strSql = strSql + " where Pubstart=0 and Parentid=0 order by Parentid asc,ID desc";
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, null);
                case 15:
                    strSql = strSql + " where Pubstart=1 and Parentid=0 order by Parentid asc,ID desc";
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, null);
                case 16:
                    sp = new SqlParameter[] { new SqlParameter("@ID", SqlDbType.Int) };
                    sp[0].Value = ID;
                    strSql = strSql + " where PubUserID=@ID AND Parentid=0 order by ID desc";
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, sp);
                case 17:
                    strSql = strSql + " order by Parentid asc,ID desc";
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, null);
                case 18:
                    sp = new SqlParameter[] { new SqlParameter("@ID", SqlDbType.Int) };
                    sp[0].Value = ID;
                    strSql = strSql + " where ID=@ID order by ID desc";
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, sp);
                case 19:
                    strSql = strSql + " where Parentid=@ID and Pubstart=1 order by Parentid asc,ID desc";
                    sp = new SqlParameter[] {
                         new SqlParameter("@ID",SqlDbType.Int)
                       };
                    sp[0].Value = ID;
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, sp);
                case 20:
                    strSql = strSql + " where Parentid=@ID and Pubstart=0 order by Parentid asc,ID desc";
                    sp = new SqlParameter[] {
                         new SqlParameter("@ID",SqlDbType.Int)
                       };
                    sp[0].Value = ID;
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, sp);
                case 111:
                    strSql = strSql + " where Parentid=0 order by Parentid asc,ID desc";
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, null);
                default:
                    strSql = strSql + " where [ID]=@ID and Recycler=0";
                    sp = new SqlParameter[] {
                         new SqlParameter("@ID",SqlDbType.Int)
                       };
                    sp[0].Value = ID;
                    return SqlHelper.ExecuteTable(CommandType.Text, strSql, sp);
            }
        }
        /// <summary>
        /// 获取会员模型信息
        /// </summary>
        /// <param Name="TableName">存储会员模型信息的表名</param>
        /// <param Name="ID">记录ID或会员ID由type决定</param>
        /// <param Name="Type">0-会员ID 1-记录ID</param>
        public PageSetting GetUserModeInfo_Page(int cpage, int psize, string TableName, int ID, int Type)
        {
            string where = "1=1";
            string order = "UpdateTime DESC,ID DESC";
            switch (Type)
            {
                case 0:
                    where += " AND UID=" + ID + " AND Status=0";
                    break;
                case 3: break;
                case 4:
                    order += "ID DESC";
                    break;
                /// jc:type=9为取出整表信息
                case 9:
                    where += " AND Status=0";
                    break;
                case 10:
                    where += " AND Status=1";
                    break;
                case 11:
                    order += " AND Parentid ASC,ID DESC";
                    break;
                case 12:
                case 18:
                    where += " AND ID=" + ID;
                    break;
                case 13:
                    where += " AND Parentid=" + ID;
                    order = " Parentid ASC,ID DESC";
                    break;
                case 14:
                    where += " AND Pubstart=0 AND Parentid=0";
                    break;
                case 15:
                    where += " AND Pubstart=1 and Parentid=0";
                    order = "Parentid ASC,ID DESC";
                    break;
                case 16:
                    where += " AND PubUserID=" + ID + " AND Parentid=0";
                    break;
                case 17:
                    order = "Parentid ASC,ID DESC";
                    break;
                case 19:
                    where += " AND Parentid=" + ID + " AND Pubstart=1";
                    order = "Parentid ASC,ID DESC";
                    break;
                case 20:
                    where += " AND Parentid=" + ID + " AND Pubstart=0";
                    order = "Parentid ASC,ID DESC";
                    break;
                case 111:
                    where += " AND Parentid=0";
                    order = "Parentid ASC,ID DESC";
                    break;
                default:
                    where += " AND ID=" + ID + " AND Recycler=0";
                    break;
            }
            PageSetting setting = PageSetting.Single(cpage, psize, TableName, "ID", where, order);
            DBCenter.SelPage(setting);
            return setting;
        }
        /// <summary>
        /// 添加会员模型信息
        /// </summary>
        public bool AddUserModel(DataTable dt, string TableName)
        {
            // if (StationGroup.RemoteUser) { umod.dt = UserData; umod.uname = TableName; return APIHelper.UserApi_Bool("AddUserModel", umod); }
            //string strSql = "Insert Into " + TableName + Sql.InsertSql(UserData);
            //SqlParameter[] sp = Sql.ContentPara(UserData);
            //return SqlHelper.ExecuteSql(strSql, sp);
            DBCenter.Insert(TableName,BLLCommon.GetFields(dt),BLLCommon.GetParas(dt),BLLCommon.GetParameters(dt).ToArray());
            return true;
        }
        /// <summary>
        /// 更新会员模型信息
        /// </summary>
        public bool UpdateModelInfo(DataTable UserData, string TableName, int ID)
        {
            DBCenter.UpdateSQL(TableName,BLLCommon.GetFieldAndPara(UserData),"ID="+ID,BLLCommon.GetParameters(UserData));
            return true;
        }
        /// <summary>
        /// 删除会员模型信息
        /// </summary>
        public bool DelModelInfo(string TableName, int ID)
        {
            string strSql = "delete from " + TableName + " Where [ID]=@ID";
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("@ID", SqlDbType.Int) };
            sp[0].Value = ID;
            return SqlHelper.ExecuteSql(strSql, sp);
        }
        /// <summary>
        /// 删除所有回收站信息
        /// </summary>
        public bool DelModelInfoAll(string TableName)
        {
            return DBCenter.DelByWhere(TableName, "Recycler=1");
        }
        /// <summary>
        /// 删除会员模型信息进入回收站
        /// </summary>
        /// <param Name="type">1进入回收站，88表示生成页面，89表示取消生成</param>
        public bool DelModelInfo2(string TableName, int ID, int type)
        {
            string sqlStr = "";
            switch (type)
            {
                case -1:
                    sqlStr = "update " + TableName + " set NewTime='" + DateTime.Now.ToString() + "' Where [UserID]=@ID and Recycler=0";
                    break;
                case 1:
                    sqlStr = "update " + TableName + " set Recycler=1 Where [ID]=@ID";
                    break;
                case 11:
                    sqlStr = "update " + TableName + " set Recycler=0 Where Recycler=1";
                    return SqlHelper.ExecuteSql(sqlStr, null);
                case 12:
                    sqlStr = "update " + TableName + " set Pubstart=1 Where [ID]=@ID";
                    break;
                case 13:
                    sqlStr = "update " + TableName + " set Pubstart=0 Where [ID]=@ID";
                    break;
                case 88:
                    sqlStr = "update " + TableName + " set IsCreate=1 Where [ID]=@ID";
                    break;
                case 89:
                    sqlStr = "update " + TableName + " set IsCreate=0 Where [ID]=@ID";
                    break;
                default:
                    sqlStr = "update " + TableName + " set Recycler=0 Where [ID]=@ID";
                    break;
            }
            SqlParameter[] sp = new SqlParameter[] { new SqlParameter("@ID", SqlDbType.Int, 4) };
            sp[0].Value = ID;
            return SqlHelper.ExecuteSql(sqlStr, sp);
        }
        /// <summary>
        /// 获取某个用户的模型信息数据的ID
        /// </summary>
        /// <param Name="TableName">存储会员模型信息的表名</param>
        /// <param Name="UserID">用户ID</param>
        /// <returns>信息ID</returns>
        public int UserModeInfoID(string TableName, int UserID)
        {
            // if (StationGroup.RemoteUser) { umod.uname = TableName; umod.uid = UserID; return int.Parse(APIHelper.UserApi_Str("UserModeInfoID", umod)); }
            string strSql = "select ID from " + TableName + " where UserID=@UserID";
            SqlParameter[] sp = new SqlParameter[] {
                new SqlParameter("@UserID",SqlDbType.Int)
            };
            sp[0].Value = UserID;
            return SqlHelper.ObjectToInt32(SqlHelper.ExecuteScalar(CommandType.Text, strSql, sp));
        }
        #endregion
        //public DataTable SelBarAuth(int barid)
        //{
        //    string sql = "Select A.UserID,A.UserName,b.* From " + strTableName + " A Left Join (Select * From ZL_Guest_BarAuth Where BarID=" + barid + ") B ON A.UserID=B.Uid";
        //    return SqlHelper.ExecuteTable(CommandType.Text, sql);
        //}
        public PageSetting SelBarAuth(int cpage, int psize, int cateId,string view)
        {
            string fields = " A.UserID,A.UserName,b.*";
            string tbname2 = "(Select * From ZL_Guest_BarAuth WHERE BarID=" + cateId + ")";
            string where = "1=1 ";
            switch (view)
            {
                case "leastone":
                    where += " AND (Look = 1 OR Send = 1 OR Reply = 1)";
                    break;
                default:
                    break;
            }
            PageSetting setting = PageSetting.Double(cpage, psize, strTableName, tbname2, "A.UserID", "A.UserID=B.Uid", where, "", null, fields);
            DBCenter.SelPage(setting);
            return setting;
        }
        //------Tools
        /// <summary>
        /// 支付密码检测
        /// </summary>
        public CommonReturn CheckPayPwd(M_UserInfo mu, string pwd)
        {
            if (string.IsNullOrEmpty(mu.PayPassWord)) { return CommonReturn.Failed("用户未设定支付密码"); }
            if (string.IsNullOrEmpty(pwd)) { return CommonReturn.Failed("支付密码值不能为空"); }
            if (!mu.PayPassWord.Equals(StringHelper.MD5(pwd))) { return CommonReturn.Failed("支付密码不正确"); }
            return CommonReturn.Success();
        }
        /// <summary>
        /// 用户是否填写了必须信息,是否允许注册
        /// </summary>
        public CommonReturn CheckRegInfo(M_UserInfo model)
        {
            if (string.IsNullOrEmpty(model.UserName)) { return CommonReturn.Failed("用户名不能为空"); }
            if (string.IsNullOrEmpty(model.UserPwd)) { return CommonReturn.Failed("密码不能为空"); }
            if (model.UserPwd.Length < 6) { return CommonReturn.Failed("密码最少需要6位"); }
            if (!SafeSC.CheckUName(model.UserName)) { return CommonReturn.Failed("用户名含有非法字符!"); }
            if (IsExistUName(model.UserName)) { return CommonReturn.Failed("用户名" + model.UserName + "已存在!"); }
            return CommonReturn.Success();
        }
        /// <summary>
        /// 生成一个新用户,但并不插入数据库
        /// </summary>
        public M_UserInfo NewUser(string uname, string upwd, string email = "")
        {
            M_UserInfo mu = new M_UserInfo();
            mu.UserName = uname;
            mu.HoneyName = uname;
            mu.UserPwd = StringHelper.MD5(upwd);
            mu.Email = string.IsNullOrEmpty(email) ? function.GetRandomEmail() : email;
            mu.DeadLine = DateTime.MaxValue;
            return mu;
        }
        /// <summary>
        /// 第三方平台(微信等新建用户) [c]
        /// </summary>
        public M_UserInfo AddUserByOpenID(M_UserInfo mu, string source)
        {
            //ZoomLa.BLL.User.Addon.B_UserAPP appBll = new User.Addon.B_UserAPP();
            ////openid
            //if (string.IsNullOrEmpty(mu.UserName)) { mu.UserName = source + function.GetRandomString(8); }
            //if (string.IsNullOrEmpty(mu.UserPwd)) { mu.UserPwd = StringHelper.MD5(function.GetRandomString(6)); }
            //else { mu.UserPwd = StringHelper.MD5(mu.UserPwd); }
            //if (string.IsNullOrEmpty(mu.Email)) { mu.Email = function.GetRandomEmail(); }
            //mu.DeadLine = DateTime.MaxValue;
            //if (string.IsNullOrEmpty(mu.OpenID)) { throw new Exception("未指定OpenID"); }
            //mu.UserID = Add(mu);
            //M_Uinfo mubase = new M_Uinfo();
            //mubase.UserId = mu.UserID;
            //AddBase(mubase);
            //appBll.LinkToUser(mu.OpenID, source, mu.UserID);
            //return mu;
            return null;
        }
        /// <summary>
        /// 根据管理员信息,创建关联的用户,同步更新管理员AddUserID
        /// </summary>
        public M_UserInfo NewUserByAdmin(M_AdminInfo adminMod)
        {
            string uname = adminMod.AdminName;
            if (adminMod.AdminId < 1) { throw new Exception("管理员未指定ID"); }
            if (string.IsNullOrEmpty(adminMod.AdminName) || string.IsNullOrEmpty(adminMod.AdminPassword)) { throw new Exception("管理员未指定名称或密码"); }
            if (IsExist("uname", uname)) { uname = uname + function.GetRandomString(4, 2); }
            //----------------------------------
            M_UserInfo mu = NewUser(uname, adminMod.AdminPassword);
            mu.GroupID = new B_Group().DefaultGroupID();
            //mu.RegisterIP = IPScaner.GetUserIP();
            mu.UserID = Add(mu);
            //----------------------------------
            DBCenter.UpdateSQL(adminMod.TbName, "AddUserID=" + mu.UserID, "AdminID=" + adminMod.AdminId);
            adminMod.AddUserID = mu.UserID;
            return mu;
        }
        public M_Uinfo NewBase(M_UserInfo mu)
        {
            M_Uinfo basemu = new M_Uinfo();
            basemu.UserId = mu.UserID;
            basemu.UserFace = mu.UserFace;
            basemu.TrueName = mu.TrueName;
            basemu.HoneyName = mu.HoneyName;
            return basemu;
        }
        public string GetRegion(int uid)
        {
            M_Uinfo basemu = GetUserBaseByuserid(uid);
            if (basemu == null || basemu.IsNull || string.IsNullOrEmpty(basemu.Province + basemu.City + basemu.County)) { return "none"; }
            else { return basemu.Province + "|" + basemu.City + "|" + basemu.County; }
        }
        /// <summary>
        /// 字符串是否符号密码规范,为空表示通过
        /// </summary>
        /// <returns></returns>
        public string CheckPwdRegular(string pwd)
        {
            string err = "";
            if (string.IsNullOrEmpty(pwd)) { err = "密码不能为空"; }
            else if (pwd.Contains(" ")) { err = "密码不能包含空格"; }
            else if (pwd.Length < 6) { err = "密码不能少于6位"; }
            else if (pwd.Length > 30) { err = "密码不能长于30位"; }
            else if (pwd.Contains(":")) { err = "密码不能包含特殊符号"; }
            return err;
        }
        /// <summary>
        /// 返回第一个有信息的名称,真实名称|昵称|用户名|默认名称
        /// </summary>
        public static string GetUserName(params object[] names)
        {
            string result = "";
            for (int i = 0; i < names.Length; i++)
            {
                if (!string.IsNullOrEmpty(DataConvert.CStr(names[i]))) { result = names[i].ToString(); break; }
            }
            return result;
        }
        ///// <summary>
        ///// WX-->Honey-->TrueName-->UserName
        ///// </summary>
        //public static string GetUserHoney(int uid = 0)
        //{
        //    if (uid == 0) { uid = new B_User().GetLogin().UserID; }
        //    if (uid < 1) { return ""; }
        //    string fields = "A.UserName,A.HoneyName,A.PERMISSIONS AS TrueName";
        //    fields += ",(SELECT TOP 1 WX.Name FROM ZL_UserApp UA LEFT JOIN ZL_WX_User WX ON UA.OpenID=WX.OpenID WHERE UA.UserID=" + uid + " AND UA.SourcePlat='wechat')AS WXName";

        //    // DataTable dt = DBCenter.JoinQuery(fields, "ZL_User", "ZL_WX_User", "A.UserID=B.UserID", "A.UserID=" + uid);
        //    DataTable dt = DBCenter.SelWithField("ZL_User", fields, "UserID=" + uid);
        //    if (dt.Rows.Count < 1) { return uid.ToString(); }
        //    DataRow dr = dt.Rows[0];
        //    return GetUserName(DataConvert.CStr(dr["WXName"]), DataConvert.CStr(dr["HoneyName"]),
        //        DataConvert.CStr(dr["TrueName"]), DataConvert.CStr(dr["UserName"]));
        //}
        /// <summary>
        /// 检测推荐人
        /// </summary>
        /// <param name="puid">推荐人ID</param>
        /// <param name="uid">用户ID,未注册用户为0</param>
        /// <param name="err">错误信息</param>
        /// <returns>true:推荐人信息正常</returns>
        public bool CheckParentUser(int puid, int uid, ref string err)
        {
            //1,数据库中旧数据都是检测过的,不需要重检,所以只需要把好入口,对新用户的检测即可,即新用户的ParentUserID链中,不能有新用户ID
            //2,父ID是单线的不会有枝丫
            //3,子级仅需检测父ID是否包含在其子链当中即可
            bool isok = false;
            try
            {
                M_UserInfo pmu = SelReturnModel(puid);
                M_UserInfo mu = SelReturnModel(uid);
                if (pmu.IsNull) { err = "推荐人不存在"; return isok; }
                if (uid > 0 && puid == uid) { err = "推荐人ID不能同于用户ID"; return isok; }
                if (!mu.IsNull)//在数据库中已有记录
                {
                    //puid的父级链条中不能有该uid存在
                    string puids = SelParentTree("ZL_User", "UserID", "ParentUserID", puid);
                    string cuids = SelChildTree("ZL_User", "UserID", "ParentUserID", mu.UserID);
                    //父级链条中不能包含当前用户ID
                    if (!(puids.Split(',').FirstOrDefault(p => p.Equals(mu.UserID.ToString())) == null)) { err = "父级链中有用户[" + mu.UserName + "]存在"; return false; }
                    if (!(cuids.Split(',').FirstOrDefault(p => p.Equals(mu.UserID.ToString())) == null)) { err = "子级链中有用户[" + pmu.UserName + "]存在"; return false; }
                    isok = true;
                }
            }
            catch (Exception ex) { isok = false; err = ex.Message; }
            return isok;
        }
        private string SelParentTree(int uid)
        {
            return SelParentTree(strTableName, "UserID", "ParentUserID", uid);
        }
        /// <summary>
        /// 返回父级链,包含起始ID
        /// </summary>
        /// <param name="tbname">ZL_Node</param>
        /// <param name="pk">NodeID</param>
        /// <param name="pfield">示例:ParentID</param>
        /// <param name="startid">起始的ID值,如UserID的值</param>
        /// <param name="ids">返回的层级IDS</param>
        /// <returns></returns>
        private string SelParentTree(string tbname, string pk, string pfield, int startID)
        {
            string ids = "";
            try
            {
                if (startID < 1) { return ids; }
                string sql = "WITH f AS(SELECT * FROM {ZL_Node} WHERE {NodeID}=" + startID + " UNION ALL SELECT A.* FROM {ZL_Node} A, f WHERE a.{NodeID}=f.{ParentID}) SELECT * FROM {ZL_Node} WHERE {NodeID} IN(SELECT {NodeID} FROM f)";
                string oracle = "SELECT * FROM {tbname} START WITH {NodeID} =" + startID + " CONNECT BY PRIOR {ParentID} = {NodeID}";
                sql = sql.Replace("{ZL_Node}", tbname).Replace("{NodeID}", pk).Replace("{ParentID}", pfield);
                DataTable dt = DBCenter.ExecuteTable(DBCenter.GetSqlByDB(sql, oracle));
                foreach (DataRow dr in dt.Rows)
                {
                    ids += dr[pk] + ",";
                }
                return ids.Trim(',');
            }
            catch (Exception ex) { throw new Exception(ex.Message + "|" + ids); }
        }
        /// <summary>
        /// 获取子级链,包含起始ID
        /// </summary>
        private string SelChildTree(string tbname, string pk, string pfield, int startID)
        {
            string ids = "";
            string sql = "WITH TREE as(SELECT * FROM {ZL_Node} WHERE {ParentID}=" + startID + " UNION ALL SELECT a.* FROM {ZL_Node} a JOIN Tree b on a.{ParentID}=b.{NodeID}) SELECT {NodeID} FROM Tree AS A";
            sql = sql.Replace("{ZL_Node}", tbname).Replace("{NodeID}", pk).Replace("{ParentID}", pfield);
            DataTable dt = DBCenter.ExecuteTable(sql);
            foreach (DataRow dr in dt.Rows)
            {
                ids += dr[pk] + ",";
            }
            return ids.Trim(',');
        }
        //格式化用户IDS,不通过则返回空值
        private string FormatIDS(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return ids; }
            ids = StrHelper.PureIDSForDB(ids.Replace("undefined", ""));
            if (SafeSC.CheckIDS(ids)) { return ids; }
            else { return ""; }
        }
        public bool IsTeach(M_UserInfo mu = null)
        {
            if (mu == null) { mu = GetLogin(); }
            M_Group gpMod = new B_Group().GetByID(mu.GroupID);
            return gpMod.Enroll != null && gpMod.Enroll.Contains("isteach");
        }
        //-----------New
        public int GetTypeByStr(string ptype)
        {
            ptype = ptype.ToLower();
            switch (ptype)
            {
                case "purse":
                    return 1;
                case "sicon":
                    return 2;
                case "point":
                    return 3;
                default:
                    throw new Exception("未知币种" + ptype);
            }
        }
        #region Update区,该区所有操作需要刷新缓存
        /// <summary>
        /// 更改虚拟币的值,上层应该做好检测,这层只负责更新与写入记录
        /// </summary>
        public void ChangeVirtualMoney(int uid, M_UserExpHis hisMod)
        {
            if (hisMod.score == 0) { return; }
            //string sql = "Update " + strTableName + " SET {0}={0}+" + hisMod.score + " Where UserID=" + uid;
            string field = "";
            //-为扣减,
            switch (hisMod.ScoreType)
            {
                case 1:
                    field = "Purse";
                    break;
                case 2:
                    field = "SilverCoin";
                    break;
                case 3:
                    field = "UserExp";
                    break;
                case 4:
                    field = "UserPoint";
                    break;
                case 5:
                    field = "DummyPurse";
                    break;
                case 6:
                    field = "UserCreit";
                    break;
                default:
                    throw new Exception("虚拟币类型不存在");
            }
            DataTable dt = DBCenter.SelWithField(strTableName, "UserID," + field, "UserID=" + uid);
            if (dt.Rows[0][field] == DBNull.Value) { DBCenter.UpdateSQL(strTableName, field + "=0", "UserID=" + uid); }
            DBCenter.UpdateSQL(strTableName, field + "=" + field + "+" + hisMod.score, "UserID=" + uid);
            hisMod.UserID = uid;
            ZLCache.ClearByIDS(uid.ToString());
            B_History.Insert(hisMod);
        }
        public void MinusVMoney(int uid, double money, M_UserExpHis.SType stype, string detail)
        {
            if (money <= 0) { throw new Exception("数值不正确,必须大于0"); }
            ChangeVirtualMoney(uid, new M_UserExpHis()
            {
                UserID = uid,
                score = -money,
                ScoreType = (int)stype,
                detail = detail
            });
        }
        public void AddMoney(int uid, double money, M_UserExpHis.SType stype, string detail)
        {
            if (uid < 1) { throw new Exception("AddMoney未指定uid"); }
            if (money <= 0) { throw new Exception("[" + stype.ToString() + "]数值不正确,必须大于0"); }
            ChangeVirtualMoney(uid, new M_UserExpHis()
            {
                UserID = uid,
                score = money,
                ScoreType = (int)stype,
                detail = detail
            });
        }
        /// <summary>
        /// 获得虚拟币的名称
        /// </summary>
        /// <param name="stype"></param>
        /// <returns></returns>
        public string GetVirtualMoneyName(int stype)
        {
            M_UserExpHis.SType ExpType = (M_UserExpHis.SType)stype;
            switch (ExpType)
            {
                case M_UserExpHis.SType.Purse:
                    return "资金";
                case M_UserExpHis.SType.SIcon:
                    return "银币";
                case M_UserExpHis.SType.Point:
                    return "积分";
                case M_UserExpHis.SType.UserPoint:
                    return "点券";
                case M_UserExpHis.SType.Credit:
                    return "信誉值";
                case M_UserExpHis.SType.DummyPoint:
                    return "虚拟币";
                default:
                    return "未知类型";
            }
        }
        /// <summary>
        /// 按虚拟币类型获得相应值
        /// </summary>
        /// <param name="mu"></param>
        /// <returns></returns>
        public double GetVirtualMoney(M_UserInfo mu, int stype)
        {
            M_UserExpHis.SType ExpType = (M_UserExpHis.SType)stype;
            switch (ExpType)
            {
                case M_UserExpHis.SType.Purse:
                    return mu.Purse;
                case M_UserExpHis.SType.SIcon:
                    return mu.SilverCoin;
                case M_UserExpHis.SType.Point:
                    return mu.UserExp;
                case M_UserExpHis.SType.UserPoint:
                    return mu.UserPoint;
                case M_UserExpHis.SType.DummyPoint:
                    return mu.DummyPurse;
                case M_UserExpHis.SType.Credit:
                    return mu.UserCreit;
                default:
                    return mu.UserExp;
            }
        }
        public bool Audit(string uids, int type, int GroupID)
        {
            // if (StationGroup.RemoteUser) { umod.str = userids; umod.uid = type; umod.gid = GroupID; return APIHelper.UserApi_Bool("Audit", umod); }
            SafeSC.CheckIDSEx(uids);
            string strSql = "";
            if (type == 1)
            {
                strSql = "UPDATE [ZL_Page] SET [Status] = 99 WHERE ID IN(" + uids + ")";
                SqlHelper.ExecuteSql(strSql);
                strSql = "update ZL_CommonModel set Status = 99 where TableName like 'ZL_Reg_%' and (GeneralID in(" + uids + "))";
            }
            else
            {
                strSql = "UPDATE [ZL_Page] SET [Status] = 0 WHERE ID IN(" + uids + ")";
                SqlHelper.ExecuteSql(strSql);
                strSql = "update ZL_CommonModel set Status = 0 where TableName like 'ZL_Reg_%' and (GeneralID in(" + uids + "))";
            }
            ZLCache.ClearByIDS(uids);
            return SqlHelper.ExecuteSql(strSql);
        }
        #endregion

        /// <summary>
        /// 用于OA，传入1,2,3，返回用户名(有HoneyName的时候优先使用HoneyName)
        /// </summary>
        public string GetUserNameByIDS(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return ""; }
            ids = StrHelper.PureIDSForDB(ids);
            SafeSC.CheckIDSEx(ids);
            if (ids.Contains("|")) { ids = ids.Replace("|", ","); }
            string result = "";
            try
            {
                DataTable dt = DBCenter.SelWithField(strTableName, "UserName,HoneyName", "UserID IN (" + ids + ")");
                foreach (DataRow dr in dt.Rows)
                {
                    result += B_User.GetUserName(dr["HoneyName"], dr["UserName"]) + ",";
                }
            }
            //有此旧逻辑,以用户名作为收件人
            catch { result = ids; }
            return result.TrimEnd(',');
        }
    }
    public class F_User
    {
        //honeyName,trueName,userName
        public string uname = "";
        public string uids = "";
        // 根据推荐人筛选
        public int puid = -100;
        //多个推荐人的情况,用于二级筛选
        public string puids = "";
        //会员组筛选
        public string groupIds = "";
        //部门与角色筛选,暂只支持单ID
        public string structIds = "";
        public string roleIds = "";
        public string compIds = "";
        //根据哪个字段排序
        public string orderBy = "";
        // 根据用户状态筛选
        public string status = "";
        //注册的起始与结束时间筛选
        public string regSDate = "";
        public string regEDate = "";
        public string mobile = "";
        //店铺|分组
        public int siteId = 0;
        //-------------------预留
        public string extend = "";
        public string addon = "";
    }
}
