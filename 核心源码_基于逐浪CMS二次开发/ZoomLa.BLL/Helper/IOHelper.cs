using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ZoomLa.Safe;

namespace ZoomLa.BLL.Helper
{
    /// <summary>
    /// 用于IO对象的相互转换,写入由SafeSC完成
    /// </summary>
    public class IOHelper
    {
        /// <summary>
        /// 转为二进制,用于存储与提交并入
        /// </summary>
        public static byte[] StreamToBytes(Stream stream)
        {
            //Response返回的流无法获取Length属性(此流不支持查找),所以必须先转一次
            MemoryStream ms = StreamToMStream(stream);
            byte[] bytes = new byte[ms.Length];
            ms.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            ms.Seek(0, SeekOrigin.Begin);
            return bytes;
        }
        /// <summary>
        /// 普通流转内存
        /// 用于:HttpRepsonse等必须先读取才能操作的流
        /// </summary>
        public static MemoryStream StreamToMStream(Stream stream)
        {
            MemoryStream ms = new MemoryStream();
            //byte[] buffer = new byte[1024];
            //while (true)
            //{
            //    int sz = stream.Read(buffer, 0, 1024);
            //    if (sz == 0) break;
            //    ms.Write(buffer, 0, sz);
            //}
            //ms.Position = 0;
            //----------------------------------------
            //byte[] b = new byte[stream.Length];
            //stream.Read(b, 0, b.Length);
            //MemoryStream ms = new MemoryStream();
            //ms.Write(b, 0, b.Length);
            //ms.Position = 0;
            //-----------------------------------
            int bufferLen = 4096;
            int count = 0;
            byte[] buffer = new byte[bufferLen];
            while ((count = stream.Read(buffer, 0, bufferLen)) > 0)
            {
                ms.Write(buffer, 0, count);
            }
            ms.Position = 0;
            return ms;
        }
        /// <summary>
        /// 二进制转换为内存流
        /// </summary>
        public static Stream BytesToStream(byte[] bytes)
        {
            Stream stream = new MemoryStream(bytes);
            return stream;
        }
        #region 文件操作
        /// <summary>
        /// 目录递归拷贝(物理路径)
        /// </summary>
        /// <param name="srcdir">来源目录物理路径</param>
        /// <param name="tardir">目标目录物理路径</param>
        public static CommonReturn Dir_Copy(string srcdir, string tardir)
        {
            if (!Directory.Exists(srcdir)) { return CommonReturn.Failed("[" + srcdir + "]目录不存在"); }
            if (!Directory.Exists(tardir)) { Directory.CreateDirectory(tardir); }
            srcdir = srcdir.Trim('\\') + "\\";
            tardir = tardir.Trim('\\') + "\\";
            //----------------------
            string[] fnames = Directory.GetFileSystemEntries(srcdir);
            foreach (string name in fnames)
            {
                try
                {
                    if (Directory.Exists(name))//如果是目录则拷贝
                    {
                        string currentdir = tardir + Dir_GetName(name);
                        if (!Directory.Exists(currentdir))
                        {
                            Directory.CreateDirectory(currentdir);
                        }
                        Dir_Copy(name, currentdir);//递归拷贝
                    }
                    else
                    {
                        CommonReturn retMod = File_Copy(name, tardir + Path.GetFileName(name));
                        if (!retMod.isok) { throw new Exception(retMod.err); }
                    }
                }
                catch (Exception ex) { return CommonReturn.Failed("[" + name + "]" + ex.Message); }
            }
            return CommonReturn.Success();
        }
        /// <summary>
        /// 返回目录名称
        /// </summary>
        public static string Dir_GetName(string dirPath)
        {
            if (string.IsNullOrEmpty(dirPath)) { return ""; }
            dirPath = dirPath.Trim('\\');
            int start = dirPath.LastIndexOf(@"\") + 1;
            int len = dirPath.Length - start;
            return dirPath.Substring(start, len);
        }
        /// <summary>
        /// 文件拷贝
        /// </summary>
        /// <param name="src">来源文件物理路径</param>
        /// <param name="tar">目标文件地址物理路径</param>
        /// <returns></returns>
        public static CommonReturn File_Copy(string src, string tar)
        {
            try
            {
                if (!File.Exists(src)) { return CommonReturn.Failed("[" + src + "]文件不存在"); }
                //---safe check
                if (SafeSC.FileNameCheck(src)) { return CommonReturn.Failed("[" + src + "]文件名异常"); }
                if (SafeSC.FileNameCheck(tar)) { return CommonReturn.Failed("[" + tar + "]文件名异常"); }

                if (!Directory.Exists(Path.GetDirectoryName(tar))) { Directory.CreateDirectory(Path.GetDirectoryName(tar)); }
                if (File.Exists(tar)) { File.Delete(tar); }
                File.Copy(src, tar);
            }
            catch (Exception ex) { return CommonReturn.Failed(ex.Message); }
            return CommonReturn.Success();
        }
        /// <summary>
        /// 对比文件内容是否一致
        /// </summary>
        public static bool File_CompareFile(string file1, string file2)
        {
            ////计算第一个文件的哈希值
            //var hash = System.Security.Cryptography.HashAlgorithm.Create();
            //var stream_1 = new System.IO.FileStream(src, System.IO.FileMode.Open);
            //byte[] hashByte_1 = hash.ComputeHash(stream_1);
            //stream_1.Close();
            ////计算第二个文件的哈希值
            //var stream_2 = new System.IO.FileStream(tar, System.IO.FileMode.Open);
            //byte[] hashByte_2 = hash.ComputeHash(stream_2);
            //stream_2.Close();
            ////比较两个哈希值
            //return (BitConverter.ToString(hashByte_1) == BitConverter.ToString(hashByte_2));

            if (file1 == file2) { return true; }
            int file1byte = 0;
            int file2byte = 0;
            FileStream fs2 = new FileStream(file2, FileMode.Open);
            using (FileStream fs1 = new FileStream(file1, FileMode.Open))
            {
                // 检查文件大小。如果两个文件的大小并不相同，则视为不相同。
                if (fs1.Length != fs2.Length)
                {
                    // 关闭文件。
                    fs1.Close();
                    fs2.Close();
                    return false;
                }
                // 逐一比较两个文件的每一个字节，直到发现不相符或已到达文件尾端为止。
                do
                {
                    // 从每一个文件读取一个字节。
                    file1byte = fs1.ReadByte();
                    file2byte = fs2.ReadByte();
                } while ((file1byte == file2byte) && (file1byte != -1));
                // 关闭文件。
                fs1.Close();
                fs2.Close();
            }
            return ((file1byte - file2byte) == 0);
        }
        #endregion

        public static string GetMD5HashFromFile(string ppath)
        {
            FileStream fs = new FileStream(ppath,FileMode.Open);
            string md5= GetMD5HashFromFile(fs);
            fs.Close();fs.Dispose();
            return md5;
        }
        public static string GetMD5HashFromFile(Stream sr)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(sr);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
