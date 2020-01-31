using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ZoomLa.BLL;
using ZoomLa.BLL.CreateJS;
using ZoomLa.SQLDAL;

namespace ZoomLa.Extend
{
    public class TaskCode
    {
        //微信推送消息
        public void WX_PushMsg()
        {
            WxAPI api = WxAPI.Code_Get();
            api.SendAllBySingle("这是消息");
        }
        //public void PointToUser()
        //{
        //    try
        //    {
        //        B_User buser = new B_User();
        //        B_CodeModel logBll = new B_CodeModel("ZL_T_PointLog");
        //        DateTime sdate = DateTime.Now.AddDays(-PlugConfig.Info.NeedDay);
        //        //注册日期必须大于分成周期,且有可分成记录的用户(筛选掉新注册用户&&废弃用户&&周期内没消费的用户)
        //        string where = "RegTime<='" + sdate.ToString("yyyy/MM/dd 00:00:00") + "' AND UserID IN (SELECT UserID FROM ZL_T_Point WHERE ZStatus=0 GROUP BY UserID)";
        //        DataTable userdt = DBCenter.SelWithField("ZL_User", "UserID,UserName,RegTime", where);
        //        if (userdt.Rows.Count < 1) { return; }
        //        foreach (DataRow dr in userdt.Rows)
        //        {
        //            int uid = Convert.ToInt32(dr["UserID"]);
        //            string remind = "";
        //            int point = GetUserPoint(uid, Convert.ToDateTime(dr["RegTime"]), ref remind);
        //            if (point > 0)
        //            {
        //                //超过指定的积分数额才可入账,否则重置
        //                if (point >= PlugConfig.Info.NeedPoint)
        //                {
        //                    remind = "验证通过积分入账," + remind;
        //                    buser.AddMoney(uid, point, ZoomLa.Model.M_UserExpHis.SType.Point, remind);
        //                }
        //                else
        //                {
        //                    remind = "验证未通过积分重置," + remind;
        //                }
        //                DBCenter.UpdateSQL("ZL_T_Point", "ZStatus=1", "UserID=" + uid + " AND ZStatus=0");
        //                DataRow logdr = logBll.NewModel();
        //                logdr["CDate"] = DateTime.Now;
        //                logdr["UserID"] = uid;
        //                logdr["Point"] = point;
        //                logBll.Insert(logdr);
        //            }
        //        }
        //    }
        //    catch (Exception ex) { ZLLog.L("PointToUser:" + ex.Message); }
        //}
        //private int GetUserPoint(int uid, DateTime regTime, ref string remind)
        //{
        //    //为确保稳定与有效,将信息日志存档,便于确定周期,而不采用动态计算的方法
        //    DataTable lastBonus = DBCenter.Sel("ZL_T_PointLog", "UserID=" + uid, "ID DESC");
        //    //如果有分红记录,则按上次分红的时间来计算周期
        //    if (lastBonus.Rows.Count > 0)
        //    {
        //        regTime = Convert.ToDateTime(lastBonus.Rows[0]["CDate"]);
        //    }
        //    //判断距现在是否满了一个周期,未满则忽略
        //    if (regTime.AddDays(PlugConfig.Info.NeedDay) > DateTime.Now) { return 0; }
        //    //取应该入账的积分记录,给予入账
        //    string where = " UserID=" + uid + " AND ZStatus=0";
        //    DataTable pointdt = DBCenter.Sel("ZL_T_Point", where);
        //    int point = 0;
        //    if (pointdt.Rows.Count > 0)
        //    {
        //        point = Convert.ToInt32(pointdt.Compute("SUM(Point)", ""));
        //    }
        //    remind = "周期[" + regTime.ToString("yyyy/MM/dd") + "]--[" + DateTime.Now.ToString("yyyy/MM/dd") + "]";
        //    return point;
        //}
    }
}
