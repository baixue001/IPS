using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;


using ZoomLa.Safe;
using ZoomLa.Common;

namespace ZoomLa.BLL
{
    /*
     * IO安全控制,上传,下载,写入,读出,写入数据库
     */
    public static class SafeSC
    {
        //------------IO相关(本地文件写入,读取)
        public static void CreateDir(string vpath)
        {
            ZoomLa.Safe.SafeC.CreateDir(vpath);
        }
        public static void CreateDir(string dirPath, string dirName)
        {
            ZoomLa.Safe.SafeC.CreateDir(dirPath, dirName);
        }
        public static bool CheckDirName(string dirname)
        {
            return ZoomLa.Safe.SafeC.CheckDirName(dirname);
        }
        public static bool DelFile(string vpath)
        {
            return ZoomLa.Safe.SafeC.DelFile(vpath);
        }
        public static bool FileNameCheck(params string[] fnames)
        {
            return ZoomLa.Safe.SafeC.FileNameCheck(fnames);
        }
        public static bool VPathCheck(string vpath)
        {
            return ZoomLa.Safe.SafeC.VPathCheck(vpath);
        }
        public static string PathDeal(string path)
        {
            return ZoomLa.Safe.SafeC.PathDeal(path);
        }
        public static void DirPathDel(ref string vpath)
        {
            ZoomLa.Safe.SafeC.DirPathDel(ref vpath);
        }
        /// <summary>
        /// SafeSC.SaveFile(Path.GetDirectoryName(strFileName) + "\\", Path.GetFileName(strFileName), bytes);
        /// </summary>
        public static string SaveFile(string vpath, string fileName, byte[] file)
        {
            return ZoomLa.Safe.SafeC.SaveFile(vpath, fileName, file);
        }
        public static string WriteFile(string vpath, string fname, string content)
        {
            return ZoomLa.Safe.SafeC.WriteFile(vpath, fname, content);
        }
        public static string WriteFile(string vpath, string content)
        {
            return ZoomLa.Safe.SafeC.WriteFile(vpath, content);
        }
        public static string ReadFileStr(string vpath, bool lines = false)
        {
            if (!File.Exists(function.VToP(vpath))) { return ""; }
            return ZoomLa.Safe.SafeC.ReadFileStr(vpath, lines);
        }
        public static byte[] ReadFileByte(string vpath)
        {
            return ZoomLa.Safe.SafeC.ReadFileByte(vpath);
        }
        //------------文件上传下载(上传置入ZoomlaSecurity)
        //public static void DownFile(string vpath, string fname = "")
        //{
        //    ZoomLa.Safe.SafeC.DownFile(vpath, fname);
        //}
        //public static void DownFile(byte[] bytes, string fname)
        //{
        //    ZoomLa.Safe.SafeC.DownFile(bytes, fname);
        //}
        //public static void DownStr(string str, string fname)
        //{
        //    ZoomLa.Safe.SafeC.DownStr(str, fname);
        //}
        //------------数据库相关
        public static bool CheckIDS(string ids)
        {
            return ZoomLa.Safe.SafeC.CheckIDS(ids);
        }
        public static void CheckIDSEx(string ids)
        {
            ZoomLa.Safe.SafeC.CheckIDSEx(ids);
        }
        public static bool CheckData(string inputData)
        {
            return ZoomLa.Safe.SafeC.CheckData(inputData);
        }
        public static void CheckDataEx(params string[] inputData)
        {
            ZoomLa.Safe.SafeC.CheckDataEx(inputData);
        }
        //------------XSS
        public static string RemoveXss(string input)
        {
            return ZoomLa.Safe.SafeC.RemoveXss(input);
        }
        //------------其它
        public static bool CheckUName(string uname)
        {
            return ZoomLa.Safe.SafeC.CheckUName(uname);
        }
        public static bool IsImage(string file)
        {
            return IOPath.IsImage(file);
        }
        //仅用于移除queryString中的值
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
      
    }
}
