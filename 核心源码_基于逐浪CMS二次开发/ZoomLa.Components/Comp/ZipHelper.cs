using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZoomLa.Common;
using ZoomLa.Safe;

namespace ZoomLa.Components.Comp
{
    public class ZipHelper
    {
        public static ZipResult Zip(ZipConfig cfg)
        {
            if (string.IsNullOrEmpty(cfg.ZipSrc)) { return cfg.Result.Failed("来源路径不能为空"); }
            if (string.IsNullOrEmpty(cfg.ZipSave)) { return cfg.Result.Failed("Zip文件保存路径不能为空"); }
            if (File.Exists(cfg.ZipSrcPath)) { cfg.ZipType = "file"; }
            else if (Directory.Exists(cfg.ZipSrcPath)) { cfg.ZipType = "dir"; }
            else { return cfg.Result.Failed("需要压缩的文件或目录[" + cfg.ZipSrc + "]不存在"); }
            if (!Directory.Exists(Path.GetDirectoryName(cfg.ZipSavePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(cfg.ZipSavePath));
            }
            //ZipTaskList.Add(cfg);
            //----------开始压缩
            switch (cfg.ZipType)
            {
                case "file":
                    if (cfg.IsAsync) { new System.Threading.Thread(AsyncZipFile).Start(cfg); }
                    else { ZipFile(cfg); }
                    break;
                case "dir":
                default:
                    int dirsCount = 0, filesCount = 0;
                    FileSystemObject.GetTotalDF(cfg.ZipSrcPath, ref dirsCount, ref filesCount);
                    cfg.Result.P_Total = dirsCount;//只计目录数
                    if (cfg.IsAsync) { new System.Threading.Thread(AsyncZipFileDictory).Start(cfg); }
                    else { ZipFileDictory(cfg); }
                    break;
            }
            return cfg.Result;
        }
        /// <summary>
        /// 递归压缩文件夹方法,实际的处理方法
        /// </summary>
        /// <param name="cfg">压缩配置</param>
        /// <param name="FolderToZip">待压缩目录</param>
        /// <param name="s">zip压缩文件流</param>
        /// <param name="ParentFolderName">待压缩目录所处的父目录</param>
        private static bool ZipFileDictory(ZipConfig cfg, string FolderToZip, ZipOutputStream s, string ParentFolderName)
        {
            //勾选全部备份,则包含temp目录
            //if (!ContainTemp && FolderToZip.Substring(FolderToZip.Length - 5, 5).ToLower().Equals(@"\temp")) return true;
            string dirPath = FolderToZip.ToLower().TrimEnd('\\') + "\\";
            if (cfg.IgnoreDirs.FirstOrDefault(p => function.VToP(p).ToLower().Equals(dirPath)) != null) { return true; }
            bool res = true;
            string[] folders, filenames;
            ZipEntry entry = null;
            FileStream fs = null;
            Crc32 crc = new Crc32();
            try
            {
                //创建当前文件夹
                entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip) + "/"));  //加上 “/” 才会当成是文件夹创建
                //entry.CompressionMethod = CompressionMethod.Stored;//如果有报错match则加上此句
                s.PutNextEntry(entry);
                s.Flush();
                //先压缩文件，再递归压缩文件夹 
                filenames = Directory.GetFiles(FolderToZip);
                foreach (string file in filenames)
                {
                    //如已在忽略列表,或是正在压缩的文件,则忽略
                    if (cfg.IgnoreFiles.FirstOrDefault(p => function.VToP(p).ToLower().Equals(file.ToLower())) != null ||
                       cfg.ZipSavePath.ToLower().Equals(file.ToLower()))
                    { cfg.Result.P_Position++; continue; }
                    //打开压缩文件
                    fs = File.OpenRead(file);
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    entry = new ZipEntry(Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip) + "/" + Path.GetFileName(file)));
                    entry.DateTime = DateTime.Now;
                    entry.Size = fs.Length;
                    fs.Close();
                    crc.Reset();
                    crc.Update(buffer);
                    entry.Crc = crc.Value;
                    s.PutNextEntry(entry);
                    s.Write(buffer, 0, buffer.Length);
                    cfg.Result.P_Position++;
                }
            }
            catch
            {
                res = false;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                    fs = null;
                }
                if (entry != null)
                    entry = null;
                GC.Collect();
                GC.Collect(1);
            }
            folders = Directory.GetDirectories(FolderToZip);
            foreach (string folder in folders)
            {
                if (!ZipFileDictory(cfg, folder, s, Path.Combine(ParentFolderName, Path.GetFileName(FolderToZip))))
                    return false;
            }
            return res;
        }
        //压缩目录
        private static bool ZipFileDictory(ZipConfig cfg)
        {
            bool res = false;
            try
            {
                ZipOutputStream s = new ZipOutputStream(File.Create(cfg.ZipSavePath));
                s.SetLevel(6);
                res = ZipFileDictory(cfg, cfg.ZipSrcPath, s, "");
                if (res == false) { return res; }
                s.Finish();
                s.Close();
                cfg.Result.P_Position = cfg.Result.P_Total;
                cfg.Result.Success();
            }
            catch (Exception ex)
            {
                cfg.Result.Failed(ex.Message); res = false;
            }
            return res;
        }
        // 压缩文件
        private static void ZipFile(ZipConfig cfg)
        {
            //string FileToZip, string ZipedFile, string Password

            //FileStream fs = null;
            FileStream ZipFile = null;
            ZipOutputStream ZipStream = null;
            ZipEntry ZipEntry = null;
            try
            {
                ZipFile = File.OpenRead(cfg.ZipSrcPath);
                byte[] buffer = new byte[ZipFile.Length];
                ZipFile.Read(buffer, 0, buffer.Length);
                ZipFile.Close();
                ZipFile = File.Create(cfg.ZipSavePath);
                ZipStream = new ZipOutputStream(ZipFile);
                //if (!string.IsNullOrEmpty(Password.Trim()))
                //    ZipStream.Password = Password.Trim();
                ZipEntry = new ZipEntry(Path.GetFileName(cfg.ZipSavePath));
                ZipStream.PutNextEntry(ZipEntry);
                ZipStream.SetLevel(6);
                ZipStream.Write(buffer, 0, buffer.Length);
                cfg.Result.Success();
            }
            catch (Exception ex)
            {
                cfg.Result.Failed(ex.Message);
            }
            finally
            {
                if (ZipEntry != null)
                {
                    ZipEntry = null;
                }
                if (ZipStream != null)
                {
                    ZipStream.Finish();
                    ZipStream.Close();
                }
                if (ZipFile != null)
                {
                    ZipFile.Close();
                    ZipFile = null;
                }
                GC.Collect();
                GC.Collect(1);
            }
        }
        /// <summary>
        /// 来源压缩文件,目标目录
        /// </summary>
        public ZipResult UnZipFiles(ZipConfig cfg)
        {
            try
            {
                if (!Directory.Exists(cfg.UnZipDirPath)) { Directory.CreateDirectory(cfg.UnZipDirPath); }
                ZipInputStream s = new ZipInputStream(File.OpenRead(cfg.ZipSrcPath));
                cfg.Result.P_Total = s.Length;
                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {
                    string directoryName = Path.GetDirectoryName(theEntry.Name);
                    string fileName = Path.GetFileName(theEntry.Name);

                    if (directoryName != String.Empty) { Directory.CreateDirectory(cfg.UnZipDirPath + directoryName); }
                    if (fileName != String.Empty)
                    {
                        FileStream streamWriter = File.Create(cfg.UnZipDirPath + theEntry.Name);
                        int size = 2048;
                        byte[] data = new byte[2048];
                        while (true)
                        {
                            size = s.Read(data, 0, data.Length);
                            if (size > 0)
                            {
                                streamWriter.Write(data, 0, size);
                            }
                            else
                            {
                                break;
                            }
                            cfg.Result.P_Position = s.Position;
                        }
                        streamWriter.Close();
                    }
                }

                s.Close();
                return cfg.Result.Success();
            }
            catch (Exception ex)
            {
                return cfg.Result.Failed(ex.Message);
            }
            finally
            {
                cfg.Result.P_Position = cfg.Result.P_Total;
            }
        }
        protected static void AsyncZipFile(object o) { ZipHelper.ZipFile((ZipConfig)o); }
        protected static void AsyncZipFileDictory(object o) { ZipHelper.ZipFileDictory((ZipConfig)o); }
    }
    public class ZipConfig
    {
        /// <summary>
        /// 需要忽略的文件(如压缩文件本身或正在被使用的文件),示例: /temp/
        /// </summary>
        public List<string> IgnoreDirs = new List<string> { "/temp/", "/log/", "/CMSPlugins/" };
        /// <summary>
        /// 需要忽略的目录,如全站备份下的UploadFiles与Log目录,Temp目录
        /// </summary>
        public List<string> IgnoreFiles = new List<string>() { "/log.txt" };
        /// <summary>
        /// 是否启用异步
        /// </summary>
        public bool IsAsync = false;
        /// <summary>
        /// 压缩文件的来源或目录(虚拟路径)
        /// </summary>
        public string ZipSrc = "";
        public string ZipSrcPath { get { return IOPath.VToP(ZipSrc); } }
        /// <summary>
        /// 压缩为的zip文件保存目录(虚拟路径)
        /// </summary>
        public string ZipSave = "";
        public string ZipSavePath { get { return IOPath.VToP(ZipSave); } }
        public string ZipType = "file";// file|dir
        //---------------------------
        /// <summary>
        /// 解压缩目录,示例:/test/zip/ 不含文件名
        /// </summary>
        public string UnZipDir = "";
        public string UnZipDirPath { get { return IOPath.VToP(UnZipDir); } }
        /// <summary>
        /// 处理任务代号
        /// </summary>
        public string Flow = "";
        /// <summary>
        /// 压缩处理结果
        /// </summary>
        public ZipResult Result = new ZipResult();
    }
    public class ZipResult
    {
        public bool IsOK = false;
        public double Percent = 0;
        public string Error = "";
        //需要压缩的文件数|需要解压的总字节数
        public double P_Total = 0;
        //已完成的进度 文件数|字节数
        public double P_Position = 0;
        //压缩的进度%
        public string P_Percent
        {
            get
            {
                if (P_Total <= P_Position) { return "100.00"; }
                return ((P_Position / P_Total) * 100).ToString("F2");
            }
        }
        public ZipResult Failed(string err) { IsOK = false; Error = err; return this; }
        public ZipResult Success() { IsOK = true; return this; }
    }
}
