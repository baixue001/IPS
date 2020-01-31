using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using ZoomLa.Model;
using ZoomLa.Components;
using ZoomLa.Common;
using System.Web;
using System.Data;
namespace ZoomLa.BLL
{


    /// <summary>
    /// B_ADZoneJs 的摘要说明
    /// </summary>
    public class B_ADZoneJs
    {
        private M_Advertisement advertisementInfo;
        private Dictionary<int, string> zoneConfig = new Dictionary<int, string>();
        private M_Adzone zoneInfo;
        public const string ADJSTlpDir = "/Manage/PLUS/ADTemplate/";
        
        public B_ADZoneJs()
        {
            this.zoneConfig.Add(0, "Banner");
            this.zoneConfig.Add(1, "Pop");
            this.zoneConfig.Add(2, "Move");
            this.zoneConfig.Add(3, "Fixed");
            this.zoneConfig.Add(4, "Float");
            this.zoneConfig.Add(5, "Code");
            this.zoneConfig.Add(6, "Couplet");
            this.zoneConfig.Add(7, "Empty");//静默
        }
        private static bool CheckJSName(string name)
        {
            Regex regex = new Regex(@"^[\w-]+/?\w+\.js$");
            bool flag = false;
            if (regex.IsMatch(name))
            {
                flag = true;
            }
            return flag;
        }
        /// <summary>
        /// 生成广告对象的JS代码
        /// </summary>
        /// <returns></returns>
        private string CreatAdvertisementJS()
        {

            return "";
        }
        /// <summary>
        /// 生成JS代码版位对象属性JS代码
        /// </summary>    
        /// <returns></returns>
        private string CreateADZoneJS()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".ZoneID=", this.zoneInfo.ZoneID, ";" }));
            builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".ZoneWidth=", this.zoneInfo.ZoneWidth, ";" }));
            builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".ZoneHeight=", this.zoneInfo.ZoneHeight, ";" }));
            builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".ShowType=", this.zoneInfo.ShowType, ";" }));
            builder.Append(this.GetZoneTypeJS());
         
            builder.Append("ZoneAD_" + this.zoneInfo.ZoneID + ".Show();");
            if (this.zoneInfo.Active==false)
                builder.Append("}");
            return builder.ToString();
        }
        /// <summary>
        /// 生成版位设置属性JS代码
        /// </summary>
        /// <returns></returns>
        private string GetZoneTypeJS()
        {
            StringBuilder builder = new StringBuilder();
            string[] strArray = (this.zoneInfo.ZoneSetting + ",,,,,").Split(new char[] { ',' });
            switch (this.zoneInfo.ZoneType)
            {
                case 1:
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".PopType = \"", strArray[0], "\";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".Left= ", DataConverter.CLng(strArray[1]).ToString(), ";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".Top= ", DataConverter.CLng(strArray[2]).ToString(), ";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".CookieHour  = \"", strArray[3], "\";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".LocalityType = \"", strArray[4], "\";" }));
                    break;
                case 2:
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".Left=", DataConverter.CLng(strArray[0]).ToString(), ";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".Top=", DataConverter.CLng(strArray[1]).ToString(), ";" }));
                    if (!string.IsNullOrEmpty(strArray[2]))
                    {
                        builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".Delta=\"", strArray[2], "\";" }));
                    }
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".ShowCloseAD=\"", strArray[3], "\";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".CloseFontColor=\"", strArray[4], "\";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".LocalityType = \"", strArray[5], "\";" }));
                    break;

                case 3:
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".Left= ", DataConverter.CLng(strArray[0]).ToString(), ";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".Top= ", DataConverter.CLng(strArray[1]).ToString(), ";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".ShowCloseAD=\"", strArray[2], "\";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".CloseFontColor=\"", strArray[3], "\";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".LocalityType = \"", strArray[4], "\";" }));
                    break;

                case 4:
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".FloatType= \"", strArray[0], "\";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".Left= ", DataConverter.CLng(strArray[1]).ToString(), ";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".Top= ", DataConverter.CLng(strArray[2]).ToString(), ";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".ShowCloseAD=\"", strArray[3], "\";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".CloseFontColor=\"", strArray[4], "\";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".LocalityType = \"", strArray[5], "\";" }));
                    break;

                case 6:
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".Left=", DataConverter.CLng(strArray[0]).ToString(), ";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".Top=", DataConverter.CLng(strArray[1]).ToString(), ";" }));
                    if (!string.IsNullOrEmpty(strArray[2]))
                    {
                        builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".Delta=\"", strArray[2], "\";" }));
                    }
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".ShowCloseAD=\"", strArray[3], "\";" }));
                    builder.Append(string.Concat(new object[] { "ZoneAD_", this.zoneInfo.ZoneID, ".CloseFontColor=\"", strArray[4], "\";" }));
                    break;
            }
            return builder.ToString();
        }
        /// <summary>
        /// 创建广告版位JS
        /// </summary>
        /// <param name="adZoneInfo"></param>
        /// <param name="advertisementInfoList"></param>
        public void CreateJS(M_Adzone adZoneInfo, DataTable dt)
        {

        }


        /// <summary>
        /// 根据版位的类型获取相应模板的内容
        /// </summary>
        /// <param name="zoneType"></param>
        /// <returns></returns>
        public string GetADZoneJSTemplateContent(int zoneType)
        {
            string templateName = this.GetTemplateName(zoneType);
            return SafeSC.ReadFileStr(ADJSTlpDir + templateName);
        }
        /// <summary>
        /// 根据类型获取版位的模板文件名
        /// </summary>
        /// <param name="zoneType"></param>
        /// <returns></returns>
        public string GetTemplateName(int zoneType)
        {
            return ("Template_" + this.zoneConfig[zoneType] + ".js");
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public bool SaveJSTemplate(string template,int zoneType)
        {
            string templateName = this.GetTemplateName(zoneType);
            try
            {
                SafeSC.WriteFile(ADJSTlpDir + templateName,template);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}