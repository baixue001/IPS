using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZoomLa.Components
{
    public class CompHelper
    {

        public static void Startup()
        {
            //加载log4net日志配置文件
            CMSLog.repository = LogManager.CreateRepository("NETCoreRepository");
            FileInfo info = new FileInfo("wwwroot/config/log.config");
            XmlConfigurator.Configure(CMSLog.repository, info);//Safe.IOPath.VToP("/config/log.config")
        }
    }
}
