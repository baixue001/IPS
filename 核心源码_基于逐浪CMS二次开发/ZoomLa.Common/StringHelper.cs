namespace ZoomLa.Common
{
    using Microsoft.VisualBasic;
    using System;
    using System.Globalization;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    public static class StringHelper
    {
        #region old
        public static void AppendString(StringBuilder sb, string append, string split = ",")
        {
            if (sb.Length == 0)
            {
                sb.Append(append);
            }
            else
            {
                sb.Append(split);
                sb.Append(append);
            }
        }
        public static string Base64StringDecode(string input)
        {
            input = input.Replace(" ", "+");//使用HttpPost会将+给格式化为 .
            byte[] bytes = Convert.FromBase64String(input);
            return Encoding.UTF8.GetString(bytes);
        }
        public static string Base64StringEncode(string input)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
        }
        public static bool FoundCharInArr(string checkStr, string findStr)
        {
            return FoundCharInArr(checkStr, findStr, ",");
        }
        public static bool FoundCharInArr(string checkStr, string findStr, string split)
        {
            bool flag = false;
            if (string.IsNullOrEmpty(split))
            {
                split = ",";
            }
            if (string.IsNullOrEmpty(checkStr))
            {
                return false;
            }
            if (checkStr.IndexOf(split) != -1)
            {
                string[] strArray;
                if (findStr.IndexOf(split) != -1)
                {
                    strArray = checkStr.Split(new char[] { Convert.ToChar(split) });
                    string[] strArray2 = findStr.Split(new char[] { Convert.ToChar(split) });
                    foreach (string str in strArray)
                    {
                        foreach (string str2 in strArray2)
                        {
                            if (string.Compare(str, str2) == 0)
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (flag)
                        {
                            return flag;
                        }
                    }
                    return flag;
                }
                strArray = checkStr.Split(new char[] { Convert.ToChar(split) });
                foreach (string str in strArray)
                {
                    if (string.Compare(str, findStr) == 0)
                    {
                        return true;
                    }
                }
                return flag;
            }
            if (string.Compare(checkStr, findStr) == 0)
            {
                flag = true;
            }
            return flag;
        }
        public static bool FoundInArr(string checkStr, string findStr, string split)
        {
            bool flag = false;
            if (checkStr.IndexOf(findStr) != -1)
            {
                string[] strArray = checkStr.Split(new string[] { split }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string str in strArray)
                {
                    if (string.Compare(str, findStr) == 0)
                    {
                        return true;
                    }
                }
                return flag;
            }
            if (string.Compare(checkStr, findStr) == 0)
            {
                flag = true;
            }
            return flag;
        }
        #endregion
        /// <summary>
        /// 清除html标记只保留文本内容
        /// </summary>
        public static string StripHtml(string html, int length=0)
        {
            if (string.IsNullOrEmpty(html)) {return html; }
            //删除脚本
            //html = Regex.Replace(html, @"<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<script>[\s\S]+</script>", "", RegexOptions.IgnoreCase);
            //删除HTML
            html = Regex.Replace(html, @"<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"-->", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"<!--.*", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(amp|#38);", "&", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(lt|#60);", "<", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(gt|#62);", ">", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(nbsp|#160);", " ", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(iexcl|#161);", "\xa1", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(cent|#162);", "\xa2", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(pound|#163);", "\xa3", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&(copy|#169);", "\xa9", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&#(\d+);", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"&ldquo;", "\"", RegexOptions.IgnoreCase);//保留【 “ 】的标点符合
            html = Regex.Replace(html, @"&rdquo;", "\"", RegexOptions.IgnoreCase);//保留【 ” 】的标点符合
            html.Replace("<", "");
            html.Replace(">", "");
            html.Replace("\r\n", "");
            html = HttpUtility.HtmlEncode(html).Trim();
            if (length > 0 && !string.IsNullOrWhiteSpace(html)) { html = html.Substring(0, length > html.Length ? (html.Length - 1) : (length - 1)); }
            return html;
        }
        /// <summary>
        /// 去html中的注释,包括注释中的内容
        /// </summary>
        /// <param name="schar"><!--</param>
        /// <param name="echar">--></param>
        public static string StripHtml_SE(string content, string schar = "<!--", string echar = "-->")
        {
            if (string.IsNullOrEmpty(content)) { return content; }
            if (!content.Contains(schar) || !content.Contains(echar)) { return content; }

            while (content.IndexOf(schar) > -1 && content.IndexOf(echar) > -1)
            {
                int start = content.IndexOf(schar);
                int end = content.IndexOf(echar);

                content = content.Remove(start, ((end + echar.Length) - start));
            }
            return content;
        }
        /// <summary>
        /// 过滤值中的js脚本,避免注入
        /// </summary>
        public static string Strip_Script(string html)
        {
            if (string.IsNullOrEmpty(html) || DataConverter.CLng(html) != 0) { return html; }
            html = Regex.Replace(html, @"<script>[\s\S]+</script>", "", RegexOptions.IgnoreCase);
            html = Regex.Replace(html, @"javascript:", "", RegexOptions.IgnoreCase);
            return html;
        }
        public static string MD5(string input)
        {
            using (MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider())
            {
                return BitConverter.ToString(provider.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", "").ToLower();
            }
        }
        public static string MD5DZ(string str)
        {
            byte[] bytes = Encoding.Default.GetBytes(str);
            bytes = new MD5CryptoServiceProvider().ComputeHash(bytes);
            string str2 = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                str2 = str2 + bytes[i].ToString("x").PadLeft(2, '0');
            }
            return str2;
        }
        public static string StripTags(string input)
        {
            Regex regex = new Regex("<([^<]|\n)+?>");
            return regex.Replace(input, "");
        }
        public static string SubStr(object obj, int len = 60, string sub = "...")
        {
            string str = obj.ToString();
            if (string.IsNullOrEmpty(str) || str.Length < len)
            {
                return str;
            }
            return (str.Substring(0, len) + sub);
        }
        public static int SubStringLength(string demand)
        {
            if (string.IsNullOrEmpty(demand))
            {
                return 0;
            }
            ASCIIEncoding encoding = new ASCIIEncoding();
            int num = 0;
            byte[] bytes = encoding.GetBytes(demand);
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == 0x3f)
                {
                    num += 2;
                }
                else
                {
                    num++;
                }
            }
            return num;
        }
        /// <summary>
        /// 将字符串转换成大写拼音首字母组成的字符串
        /// </summary>
        /// <param name="str">要转换的汉字字符串</param>
        /// <returns></returns>
        public static string ChineseToPY(string str)
        {
            string tempStr = "";
            foreach (char c in str)
            {
                if ((int)c >= 48 && (int)c <= 122)
                {//字母和符号原样保留

                    tempStr += c.ToString().ToUpper();
                }
                else
                {//累加拼音声母
                    tempStr += GetPYChar(c.ToString()).ToUpper();
                }
            }
            return tempStr;
        }
        /// <summary>
        /// 取字符的拼音声母
        /// </summary>
        /// <param name="c">要转换的汉字</param>
        /// <returns>转换后的结果</returns>
        private static string GetPYChar(string c)
        {
            byte[] array = new byte[2];
            array = System.Text.Encoding.Default.GetBytes(c);
            int i = (short)(array[0] - '\0') * 256 + ((short)(array[1] - '\0'));

            if (i < 0xB0A1) return "*";
            if (i < 0xB0C5) return "a";
            if (i < 0xB2C1) return "b";
            if (i < 0xB4EE) return "c";
            if (i < 0xB6EA) return "d";
            if (i < 0xB7A2) return "e";
            if (i < 0xB8C1) return "f";
            if (i < 0xB9FE) return "g";
            if (i < 0xBBF7) return "h";
            if (i < 0xBFA6) return "j";
            if (i < 0xC0AC) return "k";
            if (i < 0xC2E8) return "l";
            if (i < 0xC4C3) return "m";
            if (i < 0xC5B6) return "n";
            if (i < 0xC5BE) return "o";
            if (i < 0xC6DA) return "p";
            if (i < 0xC8BB) return "q";
            if (i < 0xC8F6) return "r";
            if (i < 0xCBFA) return "s";
            if (i < 0xCDDA) return "t";
            if (i < 0xCEF4) return "w";
            if (i < 0xD1B9) return "x";
            if (i < 0xD4D1) return "y";
            if (i < 0xD7FA) return "z";
            return "*";
        }
        public static bool ValidateMD5(string password, string md5Value)
        {
            return ((string.Compare(password, md5Value) == 0) || (string.Compare(password, md5Value.Substring(8, 0x10)) == 0));
        }
        /// <summary>
        /// 字符串数组A是否包含B中的某一个成员(用于判断角色列表是否存在该角色)
        /// </summary>
        /// <returns>true包含,false不包含</returns>
        public static bool IsContain(string[] a, string[] b)
        {
            for (int i = 0; i < a.Length; i++)//b只一个个对比a的
            {
                for (int j = 0; j < b.Length; j++)
                {
                    if (a[i] == b[j]) return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 移除数组中重复的元素，返回筛选后的字符串
        /// </summary>
        /// <param name="a">需要移除元素的数组</param>
        /// <param name="b">用来做对比的数组</param>
        /// <returns>a中与b不重复的元素字符串</returns>
        public static string RemoveRepeat(string[] a, string[] b)
        {
            string result = "";
            for (int i = 0; i < b.Length; i++)
            {
                for (int j = 0; j < a.Length; j++)
                {
                    if (a[j] == b[i] && a[j] != "")
                        a[j] = "";
                }
            }
            foreach (string s in a)
            {
                if (!string.IsNullOrEmpty(s))
                    result += s + ",";
            }
            return result.TrimEnd(',');
        }
        /// <summary>
        /// 主用于显示模型图标
        /// </summary>
        public static string GetItemIcon(string url, string size = "")
        {
            if (string.IsNullOrEmpty(size)) { size = "width:14px;height:14px;"; }
            if (string.IsNullOrEmpty(url)||url.Contains("."))
            {
                return "<img src='" + url + "' style='"+size+ "' onerror=\"this.src='/UploadFiles/nopic.svg';\" />";
            }
            else { return "<i class='" + url + "'></i>"; }
        }
        /// <summary>
        /// 对关键词着色,用于搜索
        /// </summary>
        /// <returns></returns>
        public static string SkeyToRed(string str, string skey)
        {
            return str.Replace(skey, "<span style='color:red;'>" + skey + "</span>");
        }
        /// <summary>
        /// 字符转换为unicode,返回16进制或10进制
        /// </summary>
        public static string ToUnicode(string str, int code = 10)
        {
            //另有&#起始的编码为10进制(10进制最少需要三位)
            string r = "";
            byte[] bts = Encoding.Unicode.GetBytes(str);
            for (int i = 0; i < bts.Length; i += 2)
            {
                r += "\\u" + bts[i + 1].ToString("x").PadLeft(2, '0') + bts[i].ToString("x").PadLeft(2, '0');
            }
            switch (code)
            {
                case 10:
                    r = r.Replace(@"\u", "");
                    r = Convert.ToInt32(r, 16).ToString();
                    break;
                case 16:
                default:
                    break;
            }
            return r;
        }
        /// <summary>
        /// 将Unicode编码转换为汉字字符串(10|16进制)
        /// 10进制,自己加上前后缀
        /// </summary>
        public static string ToGB2312(string str, int code = 10)
        {
            string r = "";
            switch (code)
            {
                case 10://&#37261;  必须以;结尾 
                    if (!str.StartsWith("&#")) { str = "&#" + str; }
                    if (!str.EndsWith(";")) { str = str + ";"; }
                    r = HttpUtility.HtmlDecode(str);
                    break;
                case 16:
                default:
                    MatchCollection mc = Regex.Matches(str, @"\\u([\w]{2})([\w]{2})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    byte[] bts = new byte[2];
                    foreach (Match m in mc)
                    {
                        bts[0] = (byte)int.Parse(m.Groups[2].Value, NumberStyles.HexNumber);
                        bts[1] = (byte)int.Parse(m.Groups[1].Value, NumberStyles.HexNumber);
                        r += Encoding.Unicode.GetString(bts);
                    }
                    break;
            }
            return r;
        }
        /// <summary>
        /// 获取字符串长度(按字节)
        /// </summary>
        public static int GetStrLenByByte(string str)
        {
            int len = 0;
            byte[] b;
            for (int i = 0; i < str.Length; i++)
            {
                b = Encoding.Default.GetBytes(str.Substring(i, 1));
                if (b.Length > 1)
                    len += 2;
                else
                    len++;
            }
            return len;
        }
        /// <summary>
        /// 截取指定长度字符串(按字节)
        /// </summary>
        public static string CutStrByByte(string str, int length)
        {
            int len = 0;
            byte[] b;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                b = Encoding.Default.GetBytes(str.Substring(i, 1));
                if (b.Length > 1)
                    len += 2;
                else
                    len++;
                sb.Append(str[i]);
                if (len >= length) { break; } 
            }
            return sb.ToString();
        }
    }
}