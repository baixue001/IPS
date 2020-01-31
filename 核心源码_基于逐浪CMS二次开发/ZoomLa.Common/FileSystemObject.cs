﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace ZoomLa.Common
{
    public enum FsoMethod
    {
        Folder,
        File,
        All
    }
    public class FileSystemObject
    {
        //--------------------------------
        private static string[] FileName = new string[] { ".cer", ".exe", ".aspx", ".asp", ".php", ".jsp", ".ashx", ".ascx", ".asmx", "../" };//.cs,.config视情况禁
        private static string[] BanRead = new string[] { "web.config", "connectionstrings.config" };//禁止读取的文件
        private static void ReadCheck(string path)
        {
            path = path.ToLower().Replace(" ", "");
            foreach (string s in BanRead)
            {
                if (path.IndexOf(s) > -1) throw new Exception(path + "文件不允许读取!");
            }
        }
        private static void PathDeal(ref string path)
        {
            path = path.Replace(" ", "");
            while (path.Contains("../"))
            {
                path = path.Replace("../", "");
            }
        }
        private static void FNameCheck(string path)
        {
            path = path.ToLower();
            foreach (string s in FileName)
            {
                if (path.Contains(s)) { throw new Exception(path + "不允许创建该后缀名的文件!"); }
            }
        }
        private static DataTable CopyDT(DataTable parent, DataTable child)
        {
            for (int i = 0; i < child.Rows.Count; i++)
            {
                DataRow row = parent.NewRow();
                for (int j = 0; j < parent.Columns.Count; j++)
                {
                    row[j] = child.Rows[i][j];
                }
                parent.Rows.Add(row);
            }
            return parent;
        }
        //过滤掉没有文件的文件夹
        public  static DataTable FiterHasFile(DataTable filedt)
        {
            DataTable newfiledt = filedt.Clone();
            newfiledt.Columns.Add("id");
            newfiledt.Columns.Add("pid");//父级id
            for (int i = 0; i < filedt.Rows.Count; i++)
            {
                DataRow dr = newfiledt.NewRow();
                dr["type"] = filedt.Rows[i]["type"];
                dr["path"] = filedt.Rows[i]["path"];
                dr["rname"] = filedt.Rows[i]["rname"].ToString();
                dr["name"] = filedt.Rows[i]["name"];
                dr["id"] = i;
                if (filedt.Rows[i]["type"].Equals("1"))
                {
                    int childcout = 0;//子文件数量
                    for (int j = 0; j < filedt.Rows.Count; j++)
                    {
                        DataRow curRow = filedt.Rows[j];
                        if (curRow["type"].Equals("2") && curRow["path"].ToString().Equals(dr["rname"].ToString() + "\\"))//判断该文件夹是否包含文件
                        {
                            if (childcout == 0)//第一行加上文件目录信息
                                newfiledt.Rows.Add(dr);
                            DataRow child = newfiledt.NewRow();
                            child["type"] = curRow["type"];
                            child["path"] = curRow["path"];
                            child["rname"] = curRow["rname"].ToString();
                            child["name"] = curRow["name"];
                            child["pid"] = i;
                            newfiledt.Rows.Add(child);
                            childcout++;
                        }
                    }
                }
            }
            return newfiledt;
        }
        // 获取文件夹中文件数量信息
        private static long[] DirInfo(DirectoryInfo d)
        {
            long[] numArray = new long[3];
            long num = 0L;
            long num2 = 0L;
            long num3 = 0L;
            FileInfo[] files = d.GetFiles();
            num3 += files.Length;
            foreach (FileInfo info in files)
            {
                num += info.Length;
            }
            DirectoryInfo[] directories = d.GetDirectories();
            num2 += directories.Length;
            foreach (DirectoryInfo info2 in directories)
            {
                num += DirInfo(info2)[0];
                num2 += DirInfo(info2)[1];
                num3 += DirInfo(info2)[2];
            }
            numArray[0] = num;
            numArray[1] = num2;
            numArray[2] = num3;
            return numArray;
        }
        // 获取文件夹信息
        private static DataTable GetDirectoryAllInfo(DirectoryInfo d, FsoMethod method)
        {
            DataRow row;
            DataTable parent = new DataTable();
            parent.Columns.Add("name");
            parent.Columns.Add("rname");
            parent.Columns.Add("content_type");
            parent.Columns.Add("type");
            parent.Columns.Add("path");
            parent.Columns.Add("creatime", typeof(DateTime));
            parent.Columns.Add("size", typeof(int));
            DirectoryInfo[] directories = d.GetDirectories();
            foreach (DirectoryInfo info in directories)
            {
                if (method == FsoMethod.File)
                {
                    parent = CopyDT(parent, GetDirectoryAllInfo(info, method));
                }
                else
                {
                    row = parent.NewRow();
                    row[0] = info.Name;
                    row[1] = info.FullName;
                    row[2] = "";
                    row[3] = 1;
                    row[4] = info.FullName.Replace(info.Name, "");
                    row[5] = info.CreationTime;
                    row[6] = 0;
                    parent.Rows.Add(row);
                    parent = CopyDT(parent, GetDirectoryAllInfo(info, method));
                }
            }
            if (method != FsoMethod.Folder)
            {
                FileInfo[] files = d.GetFiles();
                foreach (FileInfo info2 in files)
                {
                    row = parent.NewRow();
                    row[0] = info2.Name;
                    row[1] = info2.FullName;
                    row[2] = info2.Extension.Replace(".", "");
                    row[3] = 2;
                    row[4] = info2.DirectoryName + @"\";
                    row[5] = info2.CreationTime;
                    row[6] = info2.Length;

                    parent.Rows.Add(row);
                }
            }
            return parent;
        }
        // 获取文件夹下的文件名数组
        private static string[] getFiles(string dir)
        {
            PathDeal(ref dir);
            return Directory.GetFiles(dir);
        }
        // 获取文件夹信息
        private static long[] GetDirInfos(string dir)
        {
            long[] numArray = new long[3];
            DirectoryInfo d = new DirectoryInfo(dir);
            return DirInfo(d);
        }
        // 获取文件夹信息
        private static string[] getDirs(string dir)
        {
            PathDeal(ref dir);
            return Directory.GetDirectories(dir);
        }
        /// <summary>
        /// 检查文件名是否合法.文字名中不能包含字符\/:*?"<>|
        /// </summary>
        /// <param name="fileName">文件名,不包含路径</param>
        /// <returns></returns>
        private static bool IsValidFileName(string fileName)
        {
            bool isValid = true;
            string errChar = "\\/:*?\"<>|";  //
            if (string.IsNullOrEmpty(fileName))
            {
                isValid = false;
            }
            else
            {
                for (int i = 0; i < errChar.Length; i++)
                {
                    if (fileName.Contains(errChar[i].ToString()))
                    {
                        isValid = false;
                        break;
                    }
                }
            }
            return isValid;
        }
        //--------------------------------
        /// <summary>
        /// 将文件大小输出为GB MB KB B格式显示
        /// </summary>
        public static string ConvertSizeToShow(int fileSize)
        {
            int num = fileSize / 0x400;
            if (num >= 1)
            {
                if (num >= 0x400)
                {
                    int num2 = num / 0x400;
                    if (num2 >= 1)
                    {
                        if (num2 >= 0x400)
                        {
                            num2 /= 0x400;
                            return (num2.ToString() + "<span style='color:red'>&nbsp;GB</span>");
                        }
                        return (num2.ToString() + "<span style='color:red'>&nbsp;MB</span>");
                    }
                }
                return (num.ToString() + "<span style='color:red'>&nbsp;KB</span>");
            }
            return (fileSize.ToString() + "<span style='color:red'>&nbsp;&nbsp;B</span>");
        }
        public static string ConvertSizeToShow(long fileSize)
        {
            long num = fileSize / 0x400;
            if (num >= 1)
            {
                if (num >= 0x400)
                {
                    long num2 = num / 0x400;
                    if (num2 >= 1)
                    {
                        if (num2 >= 0x400)
                        {
                            num2 /= 0x400;
                            return (num2.ToString() + "GB");
                        }
                        return (num2.ToString() + "MB");
                    }
                }
                return (num.ToString() + "KB");
            }
            return (fileSize.ToString() + "B");
        }
        public static string GetFileSizeByPath(string vpath)
        {
            string ppath = function.VToP(vpath);
            if (File.Exists(ppath))
            {
                return GetFileSize(new FileInfo(ppath).Length.ToString());
            }
            else
            {
                return GetFileSize("0");
            }
        }
        public static string GetFileSize(string size)
        {
            if (string.IsNullOrEmpty(size))
            {
                return string.Empty;
            }
            int num = DataConverter.CLng(size);
            int num2 = num / 0x400;
            if (num2 < 1)
            {
                return (num.ToString() + "B");
            }
            if (num2 < 0x400)
            {
                return (num2.ToString() + "KB");
            }
            int num3 = num2 / 0x400;
            if (num3 < 1)
            {
                return (num2.ToString() + "KB");
            }
            if (num3 >= 0x400)
            {
                num3 /= 0x400;
                return (num3.ToString() + "GB");
            }
            return (num3.ToString() + "MB");
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        public static DataSet ReadXML(string file, string rootname)
        {
            ReadCheck(file);
            string str = "";
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    try
                    {
                        str = reader.ReadToEnd();
                        reader.Dispose();
                        stream.Dispose();
                    }
                    catch
                    {
                    }
                }
            }
            StringReader readerxml = new StringReader(str);
            DataSet ds = new DataSet(rootname);
            ds.ReadXml(readerxml as System.IO.TextReader);
            readerxml.Close();
            return ds;
        }
        /// <summary>
        /// 操作对象是否存在
        /// </summary>
        public static bool IsExist(string file, FsoMethod method)
        {
            if (method == FsoMethod.File)
            {
                return File.Exists(file);
            }
            return ((method == FsoMethod.Folder) && Directory.Exists(file));
        }
        /// <summary>
        /// 读取文件
        /// </summary>
        /// <param name="file">物理路径</param>
        public static string ReadFile(string file)
        {
            ReadCheck(file);
            string str = "";
            if (!File.Exists(file))
            {
                throw new Exception("文件:[" + function.PToV(file) + "]不存在！");
            }
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    try
                    {
                        str = reader.ReadToEnd();
                        reader.Dispose();
                        stream.Dispose();
                    }
                    catch
                    {
                    }
                    return str;
                }
            }
        }
        //--------------------------------
        /// <summary>
        /// 递归获取文件夹下，文件夹与文件的总数量，用于压缩时进度显示
        /// </summary>
        public static void GetTotalDF(string dir, ref int dirsCount, ref int filesCount)
        {
            foreach (string s in Directory.EnumerateDirectories(dir))
            {
                GetTotalDF(s, ref dirsCount, ref filesCount);
            }
            dirsCount += Directory.GetFiles(dir).Length;
            filesCount += Directory.GetDirectories(dir).Length;
        }
        /// <summary>
        /// 获取格式(推荐)
        /// </summary>
        public static DataTable GetDTFormat()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("Size", typeof(long));
            table.Columns.Add("Content_Type", typeof(string));
            table.Columns.Add("CreateTime", typeof(DateTime));
            table.Columns.Add("LastWriteTime", typeof(DateTime));
            table.Columns.Add("ExSize", typeof(string));//将其转为KB容量;
            table.Columns.Add("ExName", typeof(string));//后缀名
            table.Columns.Add("ExType", typeof(string));//判断后的文件类型
            table.Columns.Add("FullPath", typeof(string));//全路径,主要用于文件夹，可以打开下一文件
            table.Columns.Add("ID", typeof(int));//索引
            table.Columns.Add("VPath", typeof(string));
            return table;
        }
        public static DataTable GetDTFormat2()
        {
            DataTable table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("Path", typeof(string));
            table.Columns.Add("UpdateTime", typeof(string));
            table.Columns.Add("CreationTime", typeof(string));
            table.Columns.Add("Type", typeof(string));
            table.Columns.Add("Size", typeof(long));
            return table;
        }
        public static DataTable GetDTFormat3()
        {
            DataTable table = new DataTable();
            table.Columns.Add("name", typeof(string));
            table.Columns.Add("type", typeof(string));
            table.Columns.Add("size", typeof(string));
            table.Columns.Add("content_type", typeof(string));
            table.Columns.Add("createTime", typeof(DateTime));
            table.Columns.Add("lastWriteTime", typeof(DateTime));
            table.Columns.Add("Path", typeof(string));
            return table;
        }
        /// <summary>
        /// 获取当前可用的模板列表
        /// </summary>
        //public static DataTable GetDTForTemplate()
        //{
        //    string pathdir = (AppDomain.CurrentDomain.BaseDirectory + SiteConfig.SiteOption.TemplateDir.Replace("/", @"\")).Replace(@"\\", @"\");
        //    DataTable tables = FileSystemObject.GetDirectoryAllInfos(pathdir, FsoMethod.All);
        //    tables.DefaultView.RowFilter = "type=1 OR name LIKE '%.html'";
        //    DataTable newtables = FiterHasFile(tables.DefaultView.ToTable());

        //    //追加根目录文件
        //    foreach (DataRow dr in tables.Rows)
        //    {
        //        //如果是html文件，如放置在根目录
        //        if (dr["type"].Equals("2") && Path.GetExtension(dr["name"].ToString()).Equals(".html"))
        //        {
        //            DataRow[] temprows = newtables.Select("name='" + dr["name"] + "'");
        //            if (temprows.Length == 0)
        //            {
        //                DataRow child = newtables.NewRow();
        //                child["type"] = 3;
        //                child["path"] = dr["path"];
        //                child["rname"] = dr["rname"].ToString();
        //                child["name"] = dr["name"];
        //                child["pid"] = 0;
        //                newtables.Rows.Add(child);
        //            }
        //        }
        //    }
        //    //忽略部分目录，并替换路径
        //    for (int i = 0; i < newtables.Rows.Count; i++)
        //    {
        //        DataRow dr = newtables.Rows[i];
        //        if (dr["rname"].ToString().Contains(@"\views")) { dr["rname"] = "ignore"; }
        //        else { dr["rname"] = dr["rname"].ToString().Replace(pathdir, "").Replace(@"\", "/").Substring(1); }
        //    }
        //    newtables.DefaultView.RowFilter = "rname not in ('ignore')";
        //    newtables = newtables.DefaultView.ToTable();
        //    //加一条空模板数据
        //    DataRow empdr = newtables.NewRow();
        //    empdr["type"] = 4;
        //    empdr["path"] = "";
        //    empdr["rname"] = "";
        //    empdr["name"] = "不指定模板!";
        //    empdr["pid"] = 0;
        //    newtables.Rows.InsertAt(empdr, 0);
        //    return newtables;
        //}
        //--------------------------------
        /// <summary>
        /// 向文件写入内容
        /// </summary>
        public static string WriteFile(string file, string fileContent)
        {
            FNameCheck(file);
            string str;
            FileStream stream = null;
            StreamWriter writer = null;
            try
            {
                FileInfo info = new FileInfo(file);
                if (!Directory.Exists(info.DirectoryName))
                {
                    Directory.CreateDirectory(info.DirectoryName);
                }
                stream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                writer = new StreamWriter(stream, Encoding.UTF8);
                writer.Write(fileContent);
                str = fileContent;
            }
            catch (Exception ex)
            {
                throw new Exception(file + ":" + ex.Message);
            }
            finally
            {
                if (writer != null) { writer.Close(); }
                if (stream != null) { stream.Close(); }
            }
            return str;
        }
        /// <summary>
        /// 向文件写入内容并根据bool append参数决定是改写还是追加 append=false则改写
        /// </summary>
        public static void WriteFile(string file, string fileContent, bool append)
        {
            FNameCheck(file);
            FileInfo info = new FileInfo(file);
            if (!Directory.Exists(info.DirectoryName))
            {
                Directory.CreateDirectory(info.DirectoryName);
            }
            StreamWriter writer = new StreamWriter(file, append, Encoding.GetEncoding("utf-8"));
            try
            {
                try
                {
                    writer.Write(fileContent);
                }
                catch (Exception exception)
                {
                    throw new FileNotFoundException(exception.ToString());
                }
            }
            finally
            {
                writer.Flush();
                writer.Close();
            }
        }
        /// <summary>
        /// 复制文件
        /// </summary>
        public static void CopyFile(string oldFile, string newFile)
        {
            File.Copy(oldFile, newFile, true);
        }
        /// <summary>
        /// 删除操作
        /// <param name="file">file待删除的文件</param>
        /// <param name="method">method 操作对象FsoMethod的枚举类型Folder文件夹 File文件</param>
        /// <param name="delSelf">删除文件夹时,是否删除本身</param>
        /// </summary>
        public static void Delete(string file, FsoMethod method, bool delSelf = true)
        {
            if ((method == FsoMethod.File) && File.Exists(file))
            {
                File.Delete(file);
            }
            if ((method == FsoMethod.Folder) && Directory.Exists(file))
            {
                string curPath = file;
                try
                {
                    DirectoryInfo dir = new DirectoryInfo(file);
                    DirectoryInfo[] childs = dir.GetDirectories();
                    foreach (DirectoryInfo child in childs)
                    {
                        curPath = child.FullName;
                        child.Delete(true);
                    }
                    if (delSelf) { dir.Delete(true); }
                }
                catch (Exception ex) { throw new Exception("[" + curPath + "]" + ex.Message); }
            }
        }
        /// <summary>
        /// 获取目录下文件信息
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static DataTable GetFileinfos(string dirPath)
        {
            PathDeal(ref dirPath);
            DataTable table = GetDTFormat2();
            string[] files = Directory.GetFiles(dirPath);
            foreach (string str2 in files)
            {
                new DirectoryInfo(str2);
                FileInfo info2 = new FileInfo(str2);
                DataRow row2 = table.NewRow();
                row2["Name"] = info2.Name;
                row2["Path"] = str2;
                row2["UpdateTime"] = info2.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                row2["CreationTime"] = info2.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                row2["Type"] = Path.GetExtension(str2).ToLower();
                row2["Size"] = info2.Length;
                table.Rows.Add(row2);
                info2 = null;
            }
            return table;
        }
        /// <summary>
        /// 获取目录下的子目录及文件信息
        /// </summary>
        /// <param name="dirPath"></param>
        /// <returns></returns>
        public static DataTable GetFileList(string dirPath)
        {
            PathDeal(ref dirPath);
            DataTable table = GetDTFormat2();
            string[] directories = Directory.GetDirectories(dirPath);
            string[] files = Directory.GetFiles(dirPath);
            foreach (string str in directories)
            {
                DirectoryInfo d = new DirectoryInfo(str);
                DataRow row = table.NewRow();
                row["Name"] = d.Name;
                row["Path"] = str;
                row["UpdateTime"] = d.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                row["CreationTime"] = d.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                row["Type"] = "";
                row["Size"] = DirInfo(d)[0];
                table.Rows.Add(row);
                d = null;
            }
            foreach (string str2 in files)
            {
                new DirectoryInfo(str2);
                FileInfo info2 = new FileInfo(str2);
                DataRow row2 = table.NewRow();
                row2["Name"] = info2.Name;
                row2["Path"] = str2;
                row2["UpdateTime"] = info2.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss");
                row2["CreationTime"] = info2.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                row2["Type"] = Path.GetExtension(str2).ToLower();
                row2["Size"] = info2.Length;
                table.Rows.Add(row2);
                info2 = null;
            }
            return table;
        }
        /// <summary>
        /// 获取文件夹信息
        /// </summary>
        public static DataTable GetDirectoryAllInfos(string dir, FsoMethod method)
        {
            PathDeal(ref dir);
            DataTable directoryAllInfo;
            try
            {
                DirectoryInfo d = new DirectoryInfo(dir);
                directoryAllInfo = GetDirectoryAllInfo(d, method);
            }
            catch (Exception exception)
            {
                throw new FileNotFoundException(exception.ToString());
            }
            return directoryAllInfo;
        }
        /// <summary>
        /// 获取文件夹信息[推荐]
        /// </summary>
        /// <dir>目录物理路径</dir>
        public static DataTable GetDirectoryInfos(string dir, FsoMethod method)
        {
            string vdir = function.PToV(dir);
            PathDeal(ref dir);
            //rar,aspx,asp,php,html,htm,txt,xml,config
            DataRow row;
            int num;
            DataTable table = GetDTFormat();
            FileSystemObject fs = new FileSystemObject();
            if (method != FsoMethod.File)
            {
                for (num = 0; num < getDirs(dir).Length; num++)
                {
                    row = table.NewRow();
                    DirectoryInfo info = new DirectoryInfo(getDirs(dir)[num]);
                    //long[] numArray = DirInfo(d);
                    row[0] = info.Name;
                    row[1] = 1;
                    //long size = getDirectorySize(@"" + d.FullName + "");//文件夹不统计大小，节省资源
                    //row["ExSize"] = ((size / 1024) + (size % 1024 > 1 ? 1 : 0)).ToString() + "KB";
                    row[3] = "";
                    row[4] = info.CreationTime;
                    row[5] = info.LastWriteTime;
                    row["ExName"] = "FileFolder";
                    row["ExType"] = "文件夹";
                    row["FullPath"] = info.FullName;
                    row["VPath"] = vdir + info.Name;
                    table.Rows.Add(row);
                }
            }
            if (method != FsoMethod.Folder)
            {
                for (num = 0; num < getFiles(dir).Length; num++)
                {
                    row = table.NewRow();
                    FileInfo info = new FileInfo(getFiles(dir)[num]);
                    row[0] = info.Name;
                    row[1] = 2;
                    row[2] = info.Length;
                    row[3] = info.Extension.Replace(".", "");
                    row[4] = info.CreationTime;
                    row[5] = info.LastWriteTime;
                    row["ExSize"] = ((info.Length / 1024) + (info.Length % 1024 > 1 ? 1 : 0)).ToString() + "KB";
                    row["ExName"] = info.Extension;
                    row["ExType"] = "";// fs.GetTypeName(getFiles(dir)[num]);
                    row["FullPath"] = info.FullName;
                    row["VPath"] = vdir + info.Name;
                    table.Rows.Add(row);
                }
            }
            for (int i = 0; i < table.Rows.Count; i++)
            {
                table.Rows[i]["ID"] = (i + 1);
            }
            return table;
        }
        /// <summary>
        /// 获取文件夹信息(仅用于B_Label)
        /// </summary>
        public static DataTable GetDirectoryInfoflo(string dir, FsoMethod method)
        {
            PathDeal(ref dir);
            DataRow row;
            int num;
            DataTable table = GetDTFormat3();
            if (method != FsoMethod.File)
            {
                for (num = 0; num < getDirs(dir).Length; num++)
                {
                    row = table.NewRow();
                    DirectoryInfo d = new DirectoryInfo(getDirs(dir)[num]);
                    //long[] numArray = DirInfo(d);
                    row[0] = d.Name;
                    row[1] = 1;
                    row[2] = 0;
                    row[3] = "";
                    row[4] = d.CreationTime;
                    row[5] = d.LastWriteTime;
                    table.Rows.Add(row);
                }
            }
            return table;
        }
        /// <summary>
        /// 获取文件夹信息
        /// </summary>
        public static DataTable GetDirectorySmall(string dir)
        {
            PathDeal(ref dir);
            int num;
            DataTable table = GetDTFormat3();
            for (num = 0; num < getDirs(dir).Length; num++)
            {
                DataRow row = table.NewRow();
                DirectoryInfo d = new DirectoryInfo(getDirs(dir)[num]);
                row[0] = d.Name;
                row[1] = 1;
                row[2] = "";
                row[3] = "FileFolder";
                row[4] = d.CreationTime;
                row[5] = d.LastWriteTime;
                table.Rows.Add(row);
            }
            return table;
        }
        /// <summary>
        /// 查找文件夹下某类后缀名的文件
        /// </summary>
        public static DataTable SearchFiles(string dir, string searchPattern)
        {
            DataTable table = GetDTFormat3();
            DirectoryInfo info = new DirectoryInfo(dir);
            FileInfo[] files = info.GetFiles(searchPattern, SearchOption.AllDirectories);
            foreach (FileInfo info2 in files)
            {
                DataRow row = table.NewRow();
                row[0] = info2.FullName.Remove(0, info.FullName.Length);
                row[1] = 2;
                row[2] = info2.Length;
                row[3] = info2.Extension.Replace(".", "");
                row[4] = info2.CreationTime;
                row[5] = info2.LastWriteTime;
                row[6] = info2;
                table.Rows.Add(row);
            }
            return table;
        }
        /// <summary>
        /// 搜索指定目录下的图片文件
        /// </summary>
        /// <param name="dir">全物理路径</param>
        /// <returns></returns>
        public static DataTable SearchImg(string dir)
        {
            DataTable dt = SearchFiles2(dir, "*.jpg|*.png|*.gif|*.bmp");
            dt.DefaultView.Sort = "LastWriteTime DESC";
            return dt.DefaultView.ToTable();
        }
        public static DataTable SearchWord(string dir)
        {
            return SearchFiles2(dir, "*.docx");
        }
        public static DataTable SearchFiles2(string dir, string searchPattern)
        {
            //*.jpg|*.png|*.gif|*.bmp
            DataTable table = GetDTFormat3();
            DirectoryInfo info = new DirectoryInfo(dir);
            string[] searchPatterns = searchPattern.Split('|');
            foreach (string pattern in searchPatterns)
            {
                FileInfo[] files = info.GetFiles(pattern, SearchOption.AllDirectories);
                foreach (FileInfo info2 in files)
                {
                    DataRow row = table.NewRow();
                    row["Name"] = Path.GetFileName(info2.FullName);
                    row["Type"] = 2;
                    row["Size"] = info2.Length;
                    row["Content_Type"] = info2.Extension.Replace(".", "");
                    row["CreateTime"] = info2.CreationTime;
                    row["LastWriteTime"] = info2.LastWriteTime;
                    row["Path"] = function.PToV(info2.FullName);
                    table.Rows.Add(row);
                }
            }
            return table;
        }
        /// <summary>
        /// 创建文件夹
        /// <param>folderName文件夹名</param>
        /// <param>context当前Http请求对象</param>
        /// <returns>返回文件夹Url路径</returns>
        /// </summary>
        public static string CreateFileFolder(string folderName)
        {
            PathDeal(ref folderName);
            FNameCheck(folderName);
            if (string.IsNullOrEmpty(folderName))
            {
                throw new ArgumentNullException("folderName", "文件夹参数不能为空");
            }
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }





    }
}
