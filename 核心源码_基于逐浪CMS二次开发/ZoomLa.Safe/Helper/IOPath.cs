using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ZoomLa.Safe
{
    public class IOPath
    {
        /// <summary>
        /// 物理路径转虚拟路径
        /// </summary>
        public static string PToV(string ppath)
        {
            ppath = ppath.Replace(@"\\", "\\").Replace(AppDomain.CurrentDomain.BaseDirectory, @"\").Replace("\\wwwroot\\","").Replace(@"\", "/");
            return ("/" + ppath).Replace("//", "/");//避免有些带/有些不带
        }
        /// <summary>
        /// 虚拟路径转物理路径,用于异步等地方
        /// </summary>
        /// <param name="vpath">虚拟路径(全路径)</param>
        public static string VToP(string vpath)
        {
            if (vpath.Contains(":"))
            {
                return vpath;
            }
            else
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    vpath = "\\wwwroot\\" + vpath.Replace("/", "\\");
                }
                else
                {
                    vpath = (AppDomain.CurrentDomain.BaseDirectory + "\\wwwroot\\" + vpath.Replace("/", "\\"));
                }
                while (vpath.Contains(@"\\")) { vpath = vpath.Replace(@"\\", "\\"); }
                return vpath;
            }


        }
        public static string GetRandomString(int length, int type = 1)
        {
            Random rnd = new Random(Guid.NewGuid().GetHashCode());
            string charPool = "";
            switch (type)
            {
                case 2:
                    charPool = "1234567890";
                    break;
                case 3:
                    charPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                    break;
                case 4://验证码使用
                    //charPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                    charPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefhjklmnrtuvwxyz234578";//soi1069gpq
                    break;
                default:
                    charPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
                    break;
            }
            System.Text.StringBuilder rs = new System.Text.StringBuilder();
            while (length-- > 0)
                rs.Append(charPool[(int)(rnd.NextDouble() * charPool.Length)]);
            return rs.ToString();
        }
        /// <summary>
        /// 获取目录的根路径,带前后//,小写
        /// </summary>
        public static string GetPathRoot(string vpath)
        {
            try
            {
                if (vpath.Contains(":\\")) { throw new Exception("仅允许虚拟路径"); }
                vpath = vpath.Trim('/').ToLower() + "/";
                return "/" + vpath.Substring(0, vpath.IndexOf('/')) + "/";
            }
            catch (Exception ex) { throw new Exception("路径[" + vpath + "]不正确," + ex.Message); }
        }
        /// <summary>
        /// 获取目录路径和文件名称
        /// </summary>
        /// <param name="vpath">带文件名称的虚拟路径</param>
        /// <param name="name">out 文件名称</param>
        /// <param name="path">out 物理路径</param>
        public static void GetNameAndPath(string vpath, out string name, out string path)
        {
            if (string.IsNullOrEmpty(vpath) || vpath.Length == vpath.LastIndexOf("/") || vpath.LastIndexOf("/") < 0)
            {
                throw new Exception(vpath + "路径不正确");
            }
            try
            {
                vpath = SafeC.PathDeal(vpath);
                name = Path.GetFileName(vpath);
                path = vpath.Substring(0, vpath.LastIndexOf("/")) + "/";
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + ":" + vpath);
            }
        }
        /// <summary>
        /// 是否为允许保存的web图片文件
        /// </summary>
        public static bool IsImage(string fname)
        {
            if (string.IsNullOrEmpty(fname) || !fname.Contains(".")) return false;
            string str = Path.GetExtension(fname).ToLower().Replace(".", "");
            return Enumerable.Contains<string>((IEnumerable<string>)SafeC.ImageArr, str);
        }
        /// <summary>
        /// 是否为图片文件
        /// </summary>
        public static bool IsImageFile(string fname)
        {
            if (string.IsNullOrEmpty(fname) || !fname.Contains(".")) return false;
            string str = Path.GetExtension(fname).ToLower().Replace(".", "");
            return Enumerable.Contains<string>((IEnumerable<string>)SafeC.ImageFileArr, str);
        }
    }
}
