using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using ZoomLa.Common;

namespace ZoomLa.Extend
{
    public class ExHelper
    {
        public static void Alert(string msg, string url = "")
        {

        }
        /// <summary>
        /// WEUI存储的json数据-->图片1|图片2
        /// </summary>
        /// <param name="json">weui uploader 数据</param>
        /// <returns></returns>
        public static string Json_ToImages(string json)
        {
            if (string.IsNullOrEmpty(json) || json.Equals("[]")) { return ""; }
            DataTable imgDT = JsonConvert.DeserializeObject<DataTable>(json);
            string result = "";
            foreach (DataRow dr in imgDT.Rows)
            {
                result += dr["url"] + "|";
            }
            return result.Trim('|');
        }
        /// <summary>
        /// 取json数组中的第一张图
        /// </summary>
        public static string Json_GetFirstImage(string json)
        {
            string result = "";
            if (string.IsNullOrEmpty(json) || json.Equals("[]")) { return ""; }
            try
            {
                json = json.Replace("[", "").Replace("]", "");
                JObject jobj = JsonConvert.DeserializeObject<JObject>(json);
                result = jobj["url"].ToString();
            }
            catch (Exception) { result = ""; }
            return result;
        }
    }
}
