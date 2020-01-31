using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Web;
using System.Xml;
namespace ZoomLa.Common
{
    public class ZipClass
    {
     
        /*
         * 需改进:静态变量的方法只适合单个使用
         */ 
        /// <summary>
        ///  需要压缩的文件数
        /// </summary>
        public static int zipTotal = 0;
        /// <summary>
        /// 已处理的文件数,使用前清零操作
        /// </summary>
        public static int zipProgress = 0;
        /// <summary>
        /// 整个文件流的大小
        /// </summary>
        public static long unZipTotal = 0;
        /// <summary>
        /// 已处理到的字节,使用前清零操作
        /// </summary>
        public static long unZipProgress = 0;
        //标识已完成
        public readonly static int completeFlag=-2;
        public static bool ContainTemp = false;
        public string IgnoreFile = "";//忽略压缩文件本身
        #region ZipFileDictory

        #endregion
 
        #region Zip
        /// <summary>
        /// 压缩文件和文件夹
        /// </summary>
        /// <param name="FileToZip">待压缩的文件或文件夹，物理路径</param>
        /// <param name="ZipedFile">压缩后生成的压缩文件名，物理路径</param>
        /// <param name="Password">压缩密码</param>
        /// <returns></returns>
        public bool Zip(string FileToZip, string ZipedFile, string Password="")
        {
            return false;
        }
        #endregion
        /// <summary>
        /// 来源压缩文件,目标目录
        /// </summary>
        public  bool UnZipFiles(string file, string dir,string flag="")
        {
            return false;
        }
        /// <summary>
        /// 输入文件大小或文件数,如果nowProgress=-2,则也表示完成
        /// </summary>
        public static int GetPercent(long total,long nowProgress) 
        {
            if (total <1) return -1;
            long percent = total / 100;
            if (nowProgress == completeFlag) return 100;
            return Convert.ToInt32(nowProgress / percent);
        }
    }
}