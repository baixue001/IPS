using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;
using System.Web;
using System.Xml.Serialization;

namespace ZoomLa.Components
{
    public class WaterModuleConfig
    {
        private readonly static string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config/WaterConfig.config");
        private static WaterModuleConfigInfo configMod = null;
        private static WaterModuleConfigInfo GetInstance()
        {
            if (configMod == null)
            {
                configMod = ConfigReadFromFile();
            }
            return configMod;
        }
        /// <summary>
        /// 从配置文件获取配置信息
        /// </summary>
        private static WaterModuleConfigInfo ConfigReadFromFile()
        {
            using (Stream stream = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(WaterModuleConfigInfo));
                return (WaterModuleConfigInfo)serializer.Deserialize(stream);
            }
        }
        /// <summary>
        /// 更新配置文件
        /// </summary>
        public static void Update()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(WaterModuleConfigInfo));
                using (Stream stream = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add("", "");
                    serializer.Serialize(stream, configMod, namespaces);
                }
            }
            catch (SecurityException)
            {
              
            }
        }
        public static WaterConfig WaterConfig
        {
            get
            {
                return GetInstance().WaterConfig;
            }
        }
    }
    [Serializable, XmlRoot("WaterModuleConfig")]
    public class WaterModuleConfigInfo
    {
        private WaterConfig m_WaterConfig;

        /// <summary>
        /// 构造函数
        /// </summary>
        public WaterModuleConfigInfo()
        {
            if (this.m_WaterConfig == null)
            {
                this.m_WaterConfig = new WaterConfig();
            }
        }
        /// <summary>
        /// 人才招聘模块配置
        /// </summary>
        public WaterConfig WaterConfig
        {
            get
            {
                return this.m_WaterConfig;
            }
            set
            {
                this.m_WaterConfig = value;
            }
        }
    }
}
