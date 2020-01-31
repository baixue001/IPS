using log4net;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ZoomLa.Components
{
    public class CMSLog
    {
        public static ILoggerRepository repository { get; set; }
        public static void L(string type,string msg)
        {
            var log = LogManager.GetLogger(repository.Name, type);
            log.Info(msg);
        }
    }
}
