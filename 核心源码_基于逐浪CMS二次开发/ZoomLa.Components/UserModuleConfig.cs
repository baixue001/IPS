using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Security;
using System.Web;
using System.Xml.Serialization;
using ZoomLa.Common;

namespace ZoomLa.Components
{
    public class UserModuleConfig
    {
        //用户模块配置文件路径
        private string filePath;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UserModuleConfig()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                filePath = function.VToP("/Config/ModuleConfig.config");
            }
        }
        /// <summary>
        /// 获取配置信息 先从缓存中读取配置信息，若缓存中没有配置信息则从配置文件中读取，并将配置信息设置到缓存
        /// </summary>
        /// <returns></returns>
        public static UserModuleConfigInfo ConfigInfo()
        {
            UserModuleConfigInfo info;
            info = ConfigReadFromFile();
            return info;
        }
        /// <summary>
        /// 从配置文件获取配置信息
        /// </summary>
        /// <returns>SiteConfigInfo</returns>
        public static UserModuleConfigInfo ConfigReadFromFile()
        {
            using (Stream stream = new FileStream(new UserModuleConfig().FilePath, FileMode.Open, FileAccess.Read))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(UserModuleConfigInfo));
                return (UserModuleConfigInfo)serializer.Deserialize(stream);
            }
        }
        /// <summary>
        /// 更新配置文件
        /// </summary>
        /// <param name="config"></param>
        public void Update(UserModuleConfigInfo config)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(UserModuleConfigInfo));
            using (Stream stream = new FileStream(this.filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", "");
                serializer.Serialize(stream, config, namespaces);
            }
        }
        /// <summary>
        /// 配置文件路径属性
        /// </summary>
        public string FilePath
        {
            get
            {
                return this.filePath;
            }
            set
            {
                this.filePath = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public static JobsConfig JobsConfig
        {
            get
            {
                return ConfigInfo().JobsConfig;
            }
        }
    }
    [Serializable, XmlRoot("UserModuleConfig")]
    public class UserModuleConfigInfo
    {
        private JobsConfig m_JobsConfig;

        /// <summary>
        /// 构造函数
        /// </summary>
        public UserModuleConfigInfo()
        {
            if (this.m_JobsConfig == null)
            {
                this.m_JobsConfig = new JobsConfig();
            }
        }
        /// <summary>
        /// 人才招聘模块配置
        /// </summary>
        public JobsConfig JobsConfig
        {
            get
            {
                return this.m_JobsConfig;
            }
            set
            {
                this.m_JobsConfig = value;
            }
        }
    }
}
