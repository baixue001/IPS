using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ZoomLa.Model;

namespace ZoomLa.BLL.Helper.Addon
{
    public class CMDHelper
    {
        /// <summary>
        /// 执行命令行
        /// 1,如果调用的是NodeJS,且目标是异步,则该方法必须放在异步方法中执行(delegate即可)
        /// 2,部分命令需要以Exe或管理员方式执行,否则权限不够
        /// 示例1:CMDHelper.ExecBatCommand(p =>{p("dir");p("@echo finished");}, DataReceived, 0);
        /// 示例2:长路径string cmd = "C:\\Progra~1\\ImageMagick-7.0.7-Q16\\magick.exe \"" + source + "\" \"" + target + "\"";
        /// </summary>
        ///// <param name="workdir">工作目录,很多场景必须指定</param>
        /// <param name="inputAction">cmd操作</param>
        /// <param name="DataReceived">回调方法</param>
        /// <param name="exitTime">以秒为单位</param>
        /// <summary>
        public static void ExecBatCommand(Action<Action<string>> inputAction, DataReceivedEventHandler receive, int timeOut = 10)
        {
            StreamWriter sIn = null;
            try
            {
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "cmd.exe";
                    //process.StartInfo.Arguments = arguments;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;

                    StringBuilder output = new StringBuilder();
                    StringBuilder error = new StringBuilder();

                    using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
                    using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
                    {
                        if (receive != null)
                        {
                            process.OutputDataReceived += receive;
                            process.ErrorDataReceived += receive;
                        }
                        process.Start();

                        //执行命令
                        sIn = process.StandardInput;
                        sIn.AutoFlush = true;
                        inputAction(value => sIn.WriteLine(value));



                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        if (timeOut > 0) { process.WaitForExit(timeOut * 1000); }
                        else { process.WaitForExit(); }
                    }
                }
            }
            catch { }
            finally { if (sIn != null) { sIn.Dispose(); } }
        }
        /*
        /// <summary>
        /// 对输出字段进行处理,根据是否输出特定字符串,判断是否完成逻辑
        /// </summary>
        private static void DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e == null || string.IsNullOrEmpty(e.Data)) { return; }
            Console.WriteLine(e.Data);
            if (e.Data.Equals("finished"))
            {
                ((Process)sender).Kill();
            }
        }
        */
    //    CMDHelper.ExecBatCommand(p => {
    //            p("iisreset /stop");
    //            p(winrar + " x -y \"" + srcZip + "\" \"" + siteDir + "\"");
    //            p("iisreset /start");
    //            p("@echo finished");
    //}, ShowReceived, 0);
    }
}
