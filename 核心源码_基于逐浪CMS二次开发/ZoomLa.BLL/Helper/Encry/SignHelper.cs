using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ZoomLa.BLL.Helper
{
    //提供签名辅助
    public class SignHelper
    {
        /*
         * 常见规则:
         *  1.将参数按A-Z排序,最后再加密为MD5并大写
         *  2.将参数按ASCII从小到大排序(微信支付后台),HMAC-SHA256签名方式
         *  3.将生成的XML或字符串提交(使用不同的方法,有些还要求带证书)
         *   //提交XML,可带证书
         *   public static string Post(string xml, string url, bool isUseCert, int timeout,M_WX_APPID appMod)
         *   //提交字符串
         *   APIHelper.GetWebResult("https://api.weixin.qq.com/sns/oauth2/access_token?appid=" + appMod.APPID + "&secret=" + appMod.Secret + "&code=" + code + "&grant_type=authorization_code");
         *   //可带文件,证书,cookie
         *   HttpHelper.Post()
         *  
         * *如规则特殊,或参数固定,可手动排序,方便维护(如支付相关)(wxpay_mp)
         * *参数名区分大小写
         * *sign不参与签名
         * *参数值为空,有些要求忽略不加入验证,有些则要求加入
         */

        /// <summary>
        /// 将参数按A-Z排序,最后再加密为MD5并大写(Alipay,微信支付前端)
        /// </summary>
        /// <returns></returns>
        public static string MakeSign1(SignData data)
        {
            string str = data.ToUrl();
            //在string后加入API KEY(根据文档,有些需要后置,有些直接算参数)
            //str += "&key=" + key;
            //MD5加密
            var md5 = MD5.Create();
            var bs = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
            var sb = new StringBuilder();
            foreach (byte b in bs)
            {
                sb.Append(b.ToString("x2"));
            }
            //所有字符转为大写
            return sb.ToString().ToUpper();
        }

        /// <summary>
        /// 获取1970-当前的时间戮(微信支付)
        /// </summary>
        /// <returns></returns>
        public static string TimeStamp_1970()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
    }
    //存储待签名的参数
    public class SignData
    {
        //采用排序的Dictionary的好处是方便对数据包进行签名，不用再签名之前再做一次排序
        private SortedDictionary<string, object> m_values = new SortedDictionary<string, object>();
        /// <summary>
        /// 设置某个字段的值
        /// </summary>
        public void SetValue(string key, object value)
        {
            m_values[key] = value;
        }
        public object GetValue(string key)
        {
            object o = null;
            m_values.TryGetValue(key, out o);
            return o;
        }
        /// <summary>
        /// 判断某个字段是否已设置
        /// </summary>
        /// <returns>若字段key已被设置，则返回true，否则返回false</returns>
        public bool IsSet(string key)
        {
            object o = null;
            m_values.TryGetValue(key, out o);
            if (null != o)
                return true;
            else
                return false;
        }
        /// <summary>
        /// 将参数转换为XML,某些接口要求XML格式
        /// </summary>
        /// <returns></returns>
        public string ToXml()
        {
            string xml = "<xml>";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                //字段值不能为null，会影响后续流程
                if (pair.Value == null)
                {
                    throw new Exception("ToXml()内部[" + pair.Key + "]值为null的字段!");
                }

                if (pair.Value.GetType() == typeof(int))
                {
                    xml += "<" + pair.Key + ">" + pair.Value + "</" + pair.Key + ">";
                }
                else if (pair.Value.GetType() == typeof(string))
                {
                    xml += "<" + pair.Key + ">" + "<![CDATA[" + pair.Value + "]]></" + pair.Key + ">";
                }
                else//除了string和int类型不能含有其他数据类型
                {
                    throw new Exception("Toxml()字段[" + pair.Key + "]数据类型错误!");
                }
            }
            xml += "</xml>";
            return xml;
        }
        /// <summary>
        /// 便于API传参的字符串,a=1&b=2
        /// </summary>
        /// <returns></returns>
        public string ToUrl()
        {
            string buff = "";
            foreach (KeyValuePair<string, object> pair in m_values)
            {
                if (pair.Value == null)
                {
                    throw new Exception("ToUrl()内部[" + pair.Key + "]值为null的字段!");
                }
                if (pair.Key != "sign" && pair.Value.ToString() != "")
                {
                    buff += pair.Key + "=" + pair.Value + "&";
                }
            }
            buff = buff.Trim('&');
            return buff;
        }
        /// <summary>
        /// 前端提交的Form表单(Alipay)
        /// </summary>
        /// <param name="url">需要提交的URL</param>
        /// <returns></returns>
        public void ToForm(string url)
        {
            //见PayHelper.BuildForm
        }
    }
}
