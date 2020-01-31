using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ZoomLa.Safe.Helper;
namespace ZoomLa.Safe
{
     public class SafeC
    {
        private static string StrRegex = "";//用于检测Cookies,地址栏传参,Post(性能消耗重)传参
        private static string[] StrRegex2 = new string[] { "/*", "*/", "@@", "'", ">", "<", "(", ")" };//用于补充上方忽略的传参
        private static string[] BanExt = new string[] { ".cer", ".exe", ".dll", ".asp", ".php", ".jsp", ".jspx", ".aspx", ".ashx", ".cgi", ".ascx", ".asmx", ".master", ".cshtml" };
        private static string[] BanChar = new string[] { "../", "<", ">", "=", ";" };//文件名中不能包含的特殊符号
        private static string[] DirBanChars = new string[] { "\\", "/", "?", ":", "：", "*", "<", ">", "|", "\"", "'", "&" };//目录中禁止的字符
        private static string[] BanDownName = "config|aspx|ashx|asmx|ascx|master|cs|cshtml".Split('|');//禁止下载的文件后缀名
        private static string[] BanPathName = "/manage/|/config/|/bin/|/user/|/js/|/areas/|/cart/|/bu/|/common/|/app_data/|/api/|/dist/|/install/|/views/".Split('|');//禁止写入文件的根目录
        private static string[] BanDelDir = BanPathName;// "/manage/|/bin/|/plat/|/areas/".Split('|');//禁止删除的目录
        private static string[] BanUserName = ".aspx|.ashx|.asmx|.ascx|.asp|.cer|.master|.htm|.shtml|.cshtml|.config|.exe|/|\\|--|*|:|;|(|)".Split('|');//禁止用户名
        private static string[] BanRead = "web.config|connectionstrings.config".Split('|');//禁止在程序中读取的config文件
        public static string[] ImageArr = new string[] { "gif", "jpg", "jpeg", "png" };//允许保存的Web图片文件
        public static string[] ImageFileArr = new string[] { "gif", "jpg", "jpeg", "png", "bmp", "ico" };//允许保存的图片文件
        static SafeC()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(HttpUtility.UrlDecode("%00") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%01") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%02") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%03") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%04") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%05") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%06") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%07") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%08") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%11") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%12") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%13") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%14") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%15") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%16") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%17") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%18") + "|");
            stringBuilder.Append(HttpUtility.UrlDecode("%19"));
            StrRegex = ".*(use|exec|create|drop|insert|select|delete|update|count|alter|truncate|declare|or|and|from|set|backup|where)(\\s|\\+|" + (object)stringBuilder + ").*";
            Cert_Update();
        }
        /// <summary>
        /// 如不存在,则创建目录
        /// </summary>
        /// <param name="vpath">虚拟目录路径,不包含文件名</param>
        public static void CreateDir(string vpath)
        {
            string ppath = IOPath.VToP(vpath);
            while (ppath.EndsWith("\\")) { ppath = ppath.TrimEnd('\\'); }
            int num = ppath.LastIndexOf("\\") + 1;
            CreateDir(ppath.Substring(0, num), ppath.Substring(num, ppath.Length - num));
        }
        public static void CreateDir(string dirPath, string dirName)
        {
            dirPath = PathDeal(dirPath.TrimEnd('\\') + "\\");
            dirName = PathDeal(dirName.Trim('\\'));
            if (CheckDirName(dirName))
            {
                throw new Exception("文件夹名不能包含. \\ / ? : * <  > | \" ' &等特殊字符");
            }
            else
            {
                try
                {
                    if (!Directory.Exists(dirPath + dirName)) { Directory.CreateDirectory(dirPath + dirName); }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message + ":" + dirPath + dirName);
                }
            }
        }
        /// <summary>
        /// 目录名是否包含禁止字符
        /// </summary>
        /// <returns>true:不安全</returns>
        public static bool CheckDirName(string dirname)
        {
            foreach (string banChar in DirBanChars)
            {
                if (dirname.Contains(banChar)) { return true; }
            }
            return false;
        }
        /// <summary>
        /// 移除文件或目录
        /// </summary>
        /// <param name="vpath">文件或目录虚拟路径</param>
        /// <returns>true:删除成功</returns>
        public static bool DelFile(string vpath)
        {
            if (string.IsNullOrEmpty(vpath)) { return false; }
            vpath = PathDeal(vpath).ToLower();
            string path = IOPath.VToP(vpath);
            //是否禁止删除
            foreach (string ban in BanDelDir) { if (vpath.StartsWith(ban)) { return false; } }
            if (File.Exists(path)) { File.Delete(path); }
            else if (Directory.Exists(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                DirectoryInfo[] childs = dir.GetDirectories();
                foreach (DirectoryInfo child in childs)
                {
                    child.Delete(true);
                }
                dir.Delete(true);
            }
            return true;
        }
        /// <summary>
        /// 检测文件名是否包含特殊符号,后缀名是否为允许
        /// </summary>
        /// <param name="fnames">带后缀的文件名</param>
        /// <returns>True危险后缀名或含了特殊符号</returns>
        public static bool FileNameCheck(params string[] fnames)
        {
            foreach (string fname in fnames)
            {
                foreach (string bchar in BanChar) { if (fname.Contains(bchar)) { return true; } }
                string ext = Path.GetExtension(fname).Replace(" ", "");
                if (BanExt.Contains(ext)) { return true; }
            }
            return false;
        }
        /// <summary>
        /// 该路径的根目录是否允许写入
        /// </summary>
        /// <param name="vpath">保存的虚拟路径</param>
        /// <returns>true:检测通过</returns>
        public static bool VPathCheck(string vpath)
        {
            string root = IOPath.GetPathRoot(vpath);
            for (int index = 0; index < BanPathName.Length; ++index)
            {
                if (BanPathName[index].Equals(root))
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// 处理路径,防止回溯
        /// </summary>
        public static string PathDeal(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";
            path = path.Replace(" ", "").Replace("//", "/");
            while (path.Contains("../") || path.Contains(@"\..")) { path = path.Replace("../", "").Replace(@"\..", ""); }
            return path;
        }
        /// <summary>
        /// 处理目录路径
        /// </summary>
        /// <param name="vpath"></param>
        public static void DirPathDel(ref string vpath)
        {
            vpath = PathDeal(vpath);
            if (!vpath.Contains("."))
                return;
            vpath = Path.GetDirectoryName(vpath).Replace("\\", "/") + "/";
        }
        /// <summary>
        /// 保存文件(限定保存目录)
        /// </summary>
        public static string SaveFile(string vpath, string fileName, byte[] file)
        {
            DirPathDel(ref vpath);
            if (FileNameCheck(fileName) || !VPathCheck(vpath)) { throw new Exception(vpath + fileName + "保存失败,不符合命名规范!"); }
            string dirPath = IOPath.VToP(vpath.TrimEnd('/') + "/");
            CreateDir(dirPath, "");
            File.WriteAllBytes(dirPath + fileName, file);
            return IOPath.PToV(dirPath + fileName);
        }
        //public static string SaveFile(string vpath, FileUpload file, string fileName = "")
        //{
        //    return SaveFile(vpath, file.PostedFile, fileName);
        //}
        //public static string SaveFile(string vpath, HttpPostedFile file, string fileName = "")
        //{
        //    SafeC.DirPathDel(ref vpath);
        //    string dirPath = IOPath.VToP(vpath);
        //    if (string.IsNullOrEmpty(fileName)) { fileName = Path.GetFileName(file.FileName); }
        //    if (SafeC.FileNameCheck(fileName) || !SafeC.VPathCheck(vpath)) { throw new Exception(vpath + "保存失败,不符合命名规范!"); }
        //    SafeC.CreateDir(dirPath, "");
        //    string savePath = dirPath + fileName.Replace(" ", "");
        //    try { file.SaveAs(savePath); }
        //    catch (Exception ex) { throw new Exception("保存出错,原因:" + ex.Message + ",路径:" + savePath); }
        //    return IOPath.PToV(savePath);
        //}
        public static string SaveFile(string vpath, string fileName, Stream sm, int len)
        {
            byte[] file = new byte[len];
            sm.Read(file, 0, len);
            return SaveFile(vpath, fileName, file);
        }
        /// <summary>
        /// 允许JS与Html
        /// </summary>
        public static string WriteFile(string vpath, string fname, string content)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            return SaveFile(vpath, fname, bytes);
        }
        public static string WriteFile(string vpath, string content)
        {
            string name = "";
            string path = "";
            IOPath.GetNameAndPath(vpath, out name, out path);
            return WriteFile(path, name, content);
        }
        /// <summary>
        /// 允许写入JS与html,config等文件
        /// </summary>
        /// <param name="vpath"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string WriteUnSafeFile(string vpath, string content)
        {
            string name = "";
            string path = "";
            IOPath.GetNameAndPath(vpath, out name, out path);
            DirPathDel(ref vpath);
            string dirPath = IOPath.VToP(vpath);
            //if (SafeC.FileNameCheck(new string[1] { fileName }) || !SafeC.VPathCheck(vpath)) { throw new Exception(vpath + fileName + "保存失败,不符合命名规范!"); }
            CreateDir(dirPath, "");
            string ppath = dirPath + name;
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            File.WriteAllBytes(ppath, bytes);
            return IOPath.PToV(ppath);
        }
        public static string ReadFileStr(string vpath, bool lines = false)
        {
            FileEncodeHelper fileEncodeHelper = new FileEncodeHelper();
            vpath = vpath.ToLower();
            if (Enumerable.Count<string>((IEnumerable<string>)BanRead, (Func<string, bool>)(p => vpath.IndexOf(p) > -1)) > 0)
                throw new Exception("该文件不允许读取!");
            string str1 = IOPath.VToP(vpath);
            if (!File.Exists(str1))
                throw new Exception(vpath + "文件不存在!");
            if (!lines)
                return File.ReadAllText(str1, fileEncodeHelper.GetType(str1));
            string str2 = "";
            foreach (string str3 in File.ReadLines(str1, fileEncodeHelper.GetType(str1)))
                str2 = str2 + str3 + "<br/>";
            return str2;
        }
        public static byte[] ReadFileByte(string vpath)
        {
            vpath = vpath.ToLower();
            if (Enumerable.Count<string>((IEnumerable<string>)BanRead, (Func<string, bool>)(p => vpath.IndexOf(p) > -1)) > 0)
                throw new Exception("该文件不允许读取!");
            string path = IOPath.VToP(vpath);
            if (!File.Exists(path))
                throw new Exception(vpath + "文件不存在!");
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);
                fileStream.Close();
                return buffer;
            }
        }
        /// <summary>
        /// 移除js,iframe等注入信息
        /// </summary>
        public static string RemoveXss(string input)
        {
            input = Regex.Replace(input, "(&#*\\w+)[\\x00-\\x20]+;", "$1;");
            input = Regex.Replace(input, "(&#x*[0-9A-F]+);*", "$1;", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "&(amp|lt|gt|nbsp|quot);", "&amp;$1;");
            input = HttpUtility.HtmlDecode(input);
            input = Regex.Replace(input, "[\\x00-\\x08\\x0b-\\x0c\\x0e-\\x19]", "");
            input = Regex.Replace(input, "(<[^>]+[\\x00-\\x20\"'/])(on|xmlns)[^>]*>", "$1>", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "([a-z]*)[\\x00-\\x20]*=[\\x00-\\x20]*([`'\"]*)[\\x00-\\x20]*j[\\x00-\\x20]*a[\\x00-\\x20]*v[\\x00-\\x20]*a[\\x00-\\x20]*s[\\x00-\\x20]*c[\\x00-\\x20]*r[\\x00-\\x20]*i[\\x00-\\x20]*p[\\x00-\\x20]*t[\\x00-\\x20]*:", "$1=$2nojavascript...", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "([a-z]*)[\\x00-\\x20]*=[\\x00-\\x20]*([`'\"]*)[\\x00-\\x20]*v[\\x00-\\x20]*b[\\x00-\\x20]*s[\\x00-\\x20]*c[\\x00-\\x20]*r[\\x00-\\x20]*i[\\x00-\\x20]*p[\\x00-\\x20]*t[\\x00-\\x20]*:", "$1=$2novbscript...", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "(<[^>]+)style[\\x00-\\x20]*=[\\x00-\\x20]*([`'\"]*).*expression[\\x00-\\x20]*\\([^>]*>", "$1>", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "(<[^>]+)style[\\x00-\\x20]*=[\\x00-\\x20]*([`'\"]*).*behaviour[\\x00-\\x20]*\\([^>]*>", "$1>", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "(<[^>]+)style[\\x00-\\x20]*=[\\x00-\\x20]*([`'\"]*).*s[\\x00-\\x20]*c[\\x00-\\x20]*r[\\x00-\\x20]*i[\\x00-\\x20]*p[\\x00-\\x20]*t[\\x00-\\x20]*:*[^>]*>", "$1>", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, "</*\\w+:\\w[^>]*>", "");
            string str;
            do
            {
                str = input;
                input = Regex.Replace(input, "</*(applet|meta|xml|blink|link|style|script|embed|object|iframe|frame|frameset|ilayer|layer|bgsound|title|base)[^>]*>", "", RegexOptions.IgnoreCase);
            }
            while (str != input);
            return input;
        }
        /// <summary>
        /// 移除路径中的特殊字符
        /// </summary>
        /// <param name="fname">文件名</param>
        /// <returns>优化过后的路戏</returns>
        public static string RemovePathChar(string fname)
        {
            foreach (string banChar in DirBanChars)
            {
                fname = fname.Replace(banChar, "");
            }
            return fname;
        }
        //==============================Check

        /// <summary>
        /// false,未通过
        /// </summary>
        public static bool CheckIDS(string ids)
        {
            if (string.IsNullOrEmpty(ids)) { return false; }
            string[] strArray = ids.Split(new char[1]
      {
        ','
      }, StringSplitOptions.RemoveEmptyEntries);
            bool flag = true;
            int result = 0;
            for (int index = 0; index < strArray.Length; ++index)
            {
                if (!int.TryParse(strArray[index], out result))
                {
                    flag = false;
                    break;
                }
            }
            return flag;
        }
        public static void CheckIDSEx(string ids)
        {
            if (!CheckIDS(ids))
                throw new Exception("传入的" + ids + "不合法,请参照格式1,2,3");
        }
        /// <summary>
        /// 检测字符串,是否包含异常字符
        /// (SQL防注入)
        /// </summary>
        public static bool CheckData(string inputData)
        {
            return Regex.IsMatch(inputData.ToLower(), StrRegex) || CheckData2(inputData);
        }
        public static void CheckDataEx(params string[] inputData)
        {
            for (int index = 0; index < inputData.Length; ++index)
            {
                if (CheckData(inputData[index]))
                    throw new Exception("错误,可能包含SQL注入语句!!");
            }
        }
        private static bool CheckData2(string inputData)
        {
            return Enumerable.Count<string>(Enumerable.Where<string>((IEnumerable<string>)StrRegex2, (Func<string, bool>)(p => inputData.Contains(p)))) > 0;
        }
        /// <summary>
        /// 用户名是否包含特殊字符
        /// </summary>
        /// <returns>true:不安全</returns>
        public static bool CheckUName(string uname)
        {
            if (string.IsNullOrEmpty(uname)) return false;
            uname = uname.ToLower();
            bool flag = true;
            for (int index = 0; index < BanUserName.Length && flag; ++index)
                flag = !uname.Contains(BanUserName[index]);
            return flag;
        }
        //----------------下载文件|字符串
        public static void DownFile(string vpath, string fname = "")
        {
            vpath = PathDeal(vpath);
            string str = IOPath.VToP(vpath);
            string exName = Path.GetExtension(vpath).ToLower();
            if (Enumerable.Count<string>((IEnumerable<string>)BanDownName, (Func<string, bool>)(p => p.Equals(exName))) != 0 || !File.Exists(str))
                return;
            DownLoadFile(str, fname, Encoding.UTF8);
        }
        public static void DownFile(byte[] bytes, string fname)
        {
            DownLoadFile(bytes, fname, Encoding.UTF8);
        }
        public static void DownStr(string str, string fname)
        {
            DownStr(str, fname, Encoding.UTF8);
        }
        private static void DownLoadFile(string ppath, string fname, Encoding encode)
        {
            FileInfo fileInfo = new FileInfo(ppath);
            if (string.IsNullOrEmpty(fname))
                fname = fileInfo.Name;
            fname = HttpUtility.UrlPathEncode(fname);
            //HttpContext.Current.Response.Clear();
            //if (encode.Equals(Encoding.UTF8)) { HttpContext.Current.Response.Charset = "UTF-8"; }
            //else { HttpContext.Current.Response.Charset = "GB2312"; }
            //HttpContext.Current.Response.ContentEncoding = encode;
            //HttpContext.Current.Response.Buffer = true;
            //HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + fname);
            //HttpContext.Current.Response.WriteFile(ppath);
            //HttpContext.Current.Response.Flush();
            //HttpContext.Current.Response.End();
        }
        private static void DownLoadFile(byte[] bytes, string fname, Encoding encode)
        {
            fname = HttpUtility.UrlPathEncode(fname);
            //HttpContext.Current.Response.Clear();
            //if (encode.Equals(Encoding.UTF8)) { HttpContext.Current.Response.Charset = "UTF-8"; }
            //else { HttpContext.Current.Response.Charset = "GB2312"; }
            //HttpContext.Current.Response.ContentEncoding = encode;
            //HttpContext.Current.Response.Buffer = true;
            //HttpContext.Current.Response.AppendHeader("Content-Disposition", "attachment;filename=" + fname);
            //HttpContext.Current.Response.BinaryWrite(bytes);
            //HttpContext.Current.Response.Flush();
            //HttpContext.Current.Response.End();
        }
        public static void DownStr(string str, string fname, Encoding encode)
        {
            fname = HttpUtility.UrlPathEncode(fname);
            //HttpContext.Current.Response.Clear();
            //HttpContext.Current.Response.Buffer = true;
            //HttpContext.Current.Response.ContentType = "application/vnd.ms-excel";
            //HttpContext.Current.Response.AddHeader("Content-Disposition", "attachment;filename=" + fname);
            //if (encode.Equals(Encoding.UTF8)) { HttpContext.Current.Response.Charset = "UTF-8"; }
            //else { HttpContext.Current.Response.Charset = "GB2312"; }
            //HttpContext.Current.Response.ContentEncoding = encode;
            //HttpContext.Current.Response.Write(str);
            //HttpContext.Current.Response.End();
        }
        public static string RemoveBadChar(string str)
        {
            if (string.IsNullOrEmpty(str)) { return ""; }
            //为了效率只处理单个字符,多个字符需要用while
            string[] ban_chars = new string[] { "'", "`", "“", ";", "=", "(", ")", "<", ">" };
            foreach (string c in ban_chars)
            {
                str = str.Replace(c, "");
            }
            return str;
        }
        #region Tools
        /// <summary>
        /// 是否为允许保存的web图片文件
        /// </summary>
        public static bool IsImage(string fname)
        {
            return IOPath.IsImage(fname);
        }
        /// <summary>
        /// 是否为图片文件
        /// </summary>
        public static bool IsImageFile(string fname)
        {
            return IOPath.IsImageFile(fname);
        }
        #endregion
        //----------------Cert 相关
        private static string certPath = "/config/zlcert.ct";
        public static string certKey = "";
        /// <summary>
        /// 创建Cert并更新Key
        /// </summary>
        public static string Cert_NoExistThenNew()
        {
            string ppath = IOPath.VToP(certPath);
            string key = "";
            if (!File.Exists(ppath))
            {
                key = "ae" + IOPath.GetRandomString(14, 3).ToLower();
                byte[] text = Encoding.UTF8.GetBytes(key);
                File.WriteAllBytes(ppath, text);
            }
            else
            {
                key = ReadFileStr(certPath);
                if (string.IsNullOrEmpty(key))
                {
                    key = "ae" + IOPath.GetRandomString(14, 3).ToLower();
                    byte[] text = Encoding.UTF8.GetBytes(key);
                    File.WriteAllBytes(ppath, text);
                }
            }
            certKey = key;
            return key;
        }
        /// <summary>
        /// 更新Cert,如果不存在则新建
        /// </summary>
        public static void Cert_Update()
        {
            if (!File.Exists(IOPath.VToP(certPath))) { certKey = Cert_NoExistThenNew(); }
            try
            {
                string key = ReadFileStr(certPath);
                if (!string.IsNullOrEmpty(key)) { key = EncryHelper.DesDecrypt(key); }
                else { key = ""; }
                certKey = key;
            }
            catch { certKey = ""; }
        }
        public static string Cert_Encry(string str) { return EncryHelper.AESEncrypt(str, certKey); }
        public static string Cert_Decry(string str) { try { return EncryHelper.AESDecrypt(str, certKey); } catch { return str; }; }
    }
}
