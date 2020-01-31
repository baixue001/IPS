namespace ZoomLa.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Xml.Serialization;
    public class StationGroup
    {
        private static string filePath = AppDomain.CurrentDomain.BaseDirectory + @"\Config\StationGroup.config";
       
        public static StationGroupInfo _stInfo = null;
        //获取实例
        public static StationGroupInfo GetInstance()
        {
            if (_stInfo == null)
            {
                _stInfo = ConfigReadFromFile();
            }
            return _stInfo;
        }
        /// <summary>
        /// 使用时实例= ConfigReadFromFile();
        /// </summary>
        /// <returns></returns>
        private static StationGroupInfo ConfigReadFromFile()
        {
            using (Stream stream = new FileStream(new StationGroup().FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(StationGroupInfo));
                return (StationGroupInfo)serializer.Deserialize(stream);
            }
        }
        public static void Update()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(StationGroupInfo));
                using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add("", "");//加上这句，否则会自动在根节点上加两个属性
                    serializer.Serialize(stream, GetInstance(), namespaces);
                    stream.Close();
                    //_stInfo = ConfigReadFromFile();//更新完成，再同步一次，可不需要，因为SiteConfig上本就是对该对象的实时操作。
                }
            }
            finally{}
        }
        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                filePath = value;
            }
        }
        public static bool EnableSA
        {
            get
            {
                return GetInstance().EnableSA;
            }
            set
            {
                GetInstance().EnableSA = value;
            }
        }
        public static string SAName
        {
            get
            {
                return GetInstance().SAName;
            }
            set
            {
                GetInstance().SAName = value;
            }
        }
        public static string SAPassWord
        {
            get
            {
                return GetInstance().SAPassWord;
            }
            set
            {
                GetInstance().SAPassWord = value;
            }
        }
        /// <summary>
        /// 源码下载体地址
        /// </summary>
        public static string CodeSourceUrl
        {
            get
            {
                return GetInstance().CodeSourceUrl;
            }
            set
            {
                GetInstance().CodeSourceUrl = value;
            }
        }
        /// <summary>
        /// 下载源码,在网站目录下的存放路径
        /// </summary>
        public static string ZipSavePath
        {
            get
            {
                return GetInstance().ZipSavePath;
            }
            set
            {
                GetInstance().ZipSavePath = value;
            }
        }
        /// <summary>
        /// 安装路径(网站下虚拟路径,非物理路径)
        /// </summary>
        public static string SetupPath
        {
            get
            {
                return GetInstance().SetupPath;
            }
            set
            {
                GetInstance().SetupPath = value;
            }
        }
        /// <summary>
        /// 备份Url,用于恢复默认设置
        /// </summary>
        public static string BackupUrl
        {
            get
            {
                return GetInstance().BackupUrl;
            }
            set
            {
                GetInstance().BackupUrl = value;
            }
        }
        /// <summary>
        /// 相对网站来说的路径
        /// </summary>
        public static string RootPath
        {
            get
            {
                return GetInstance().RootPath;
            }
            set
            {
                GetInstance().RootPath = value;
            }
        }
        /// <summary>
        /// 下载源码包的文件名
        /// </summary>
        public static string ZipName
        {
            get
            {
                return GetInstance().ZipName;
            }
            set
            {
                GetInstance().ZipName = value;
            }
        }
        /// <summary>
        /// 用户申请站点的物理目录
        /// </summary>
        public static string SitePath
        {
            get
            {
                return GetInstance().SitePath;
            }
            set
            {
                GetInstance().SitePath = value;
            }
        }
        public static string DefaultIP
        {
            get
            {
                return GetInstance().DefaultIP;
            }
            set
            {
                GetInstance().DefaultIP = value;
            }
        }
       /// <summary>
        ///  //新网用户名与API密码
       /// </summary>
        public static string newNetClientID
        {
            get
            {
                return GetInstance().newNetClientID;
            }
            set
            {
                GetInstance().newNetClientID = value;
            }
        }
        public static string newNetApiPasswd
        {
            get
            {
                return GetInstance().newNetApiPasswd;
            }
            set
            {
                GetInstance().newNetApiPasswd = value;
            }
        }
        /// <summary>
        /// 默认前台选中
        /// </summary>
        public static string DefaultCheck
        {
            get
            {
                return GetInstance().DefaultCheck;
            }
            set
            {
                GetInstance().DefaultCheck = value;
            }
        }
        /// <summary>
        ///默认前台显示
        /// </summary>
        public static string DefaultDisplay
        {
            get
            {
                return GetInstance().DefaultDisplay;
            }
            set
            {
                GetInstance().DefaultDisplay = value;
            }
        }
        /// <summary>
        /// 是否开启自定义DNS
        /// </summary>
        public static string DnsOption
        {
            get
            {
                return GetInstance().DnsOption;
            }
            set
            {
                GetInstance().DnsOption = value;
            }
        }
        /// <summary>
        ///商城模型ID
        /// </summary>
        public static string ModelID
        {
            get
            {
                return GetInstance().ModelID;
            }
            set
            {
                GetInstance().ModelID = value;
            }
        }
        /// <summary>
        /// 商城节点ID
        /// </summary>
        public static string NodeID
        {
            get
            {
                return GetInstance().NodeID;
            }
            set
            {
               GetInstance().NodeID=value;
            }
        }
        /// <summary>
        /// 二级域名,示例格式.Zoomla.cn
        /// </summary>
        public static string TDomName
        {
            get
            {
                return GetInstance().TDomName;
            }
            set
            {
                GetInstance().TDomName = value;
            }
        }
        /// <summary>
        /// DNS文本文件默认输出目录
        /// </summary>
        public static string DnsOutputPath
        {
            get
            {
                return GetInstance().DnsOutputPath;
            }
            set
            {
                GetInstance().DnsOutputPath = value;
            }
        }
        /// <summary>
        /// 智能建站时允许自动生成数据库
        /// </summary>
        public static bool AutoCreateDB
        {
            get
            {
                return GetInstance().AutoCreateDB;
            }
            set
            {
                GetInstance().AutoCreateDB = value;
            }
        }
        /// <summary>
        /// 数据库SA权限管理员帐号，密码
        /// </summary>
        public static string DBManagerName
        {
            get
            {
                return GetInstance().DBManagerName;
            }
            set
            {
                GetInstance().DBManagerName = value;
            }
        }
        public static string DBManagerPasswd
        {
            get
            {
                return GetInstance().DBManagerPasswd;
            }
            set
            {
                GetInstance().DBManagerPasswd = value;
            }
        }
        /// <summary>
        /// 开启远程用户校验(Disuse)
        /// </summary>
        public static bool RemoteUser 
        {
            get { return GetInstance().RemoteUser; }
            set { GetInstance().RemoteUser = value; }
        }
        //是否已开启远程
        public static bool RemoteEnable
        {
            get { return GetInstance().RemoteEnable; }
            set { GetInstance().RemoteEnable = value; }
        }
        public static string RemoteUrl
        {
            get { return GetInstance().RemoteUrl; }
            set { GetInstance().RemoteUrl = value; }
        }
        /// <summary>
        /// 主站数据库名
        /// </summary>
        public static string DBName
        {
            get { return GetInstance().DBName; }
            set { GetInstance().DBName = value; }
        }
        /// <summary>
        /// 主站数据用户名,密码不保存
        /// </summary>
        public static string DBUName
        {
            get { return GetInstance().DBUName; }
            set { GetInstance().DBUName = value; }
        }
        /// <summary>
        /// 子站与主站之间的凭证
        /// </summary>
        public static string Token
        {
            get { return GetInstance().Token; }
            set { GetInstance().Token = value; }
        }
    }
    [Serializable, XmlRoot("Site")]

    public class StationGroupInfo
    {
        private bool _enableSA;
        private string _saName;
        private string _saPassWord;
        private string _codeSourceUrl;
        private string _zipSavePath;
        private string _setupPath;
        private string _backupUrl;
        private string _rootPath;
        private string _zipName;
        private string sitePath;
        private string _tDomName;
        private string _dnsOutputPath;
        private string _newNetClientID;
        private string _newNetApiPasswd;
        private string _DefaultCheck;
        private string _DefaultDisplay;
        private string _DnsOption;
        private string _ModelID;
        private string _NodeID;
        private string _DefaultIP;

        private string _DNSTemplate;

        public StationGroupInfo()
        {

        }
        public bool EnableSA
        {
            get
            {
                return this._enableSA;
            }
            set
            {
                this._enableSA = value;
            }
        }
        public string SAName
        {
            get
            {
                return this._saName;
            }
            set
            {
                this._saName = value;
            }
        }
        public string SAPassWord
        {
            get
            {
                return this._saPassWord;
            }
            set
            {
                this._saPassWord = value;
            }
        }
        public string CodeSourceUrl
        {
            get
            {
                return this._codeSourceUrl;
            }
            set
            {
                this._codeSourceUrl = value;
            }
        }
        public string ZipSavePath
        {
            get
            {
                return this._zipSavePath;
            }
            set
            {
                this._zipSavePath = value;
            }
        }
        public string SetupPath
        {
            get
            {
                return this._setupPath;
            }
            set
            {
                this._setupPath = value;
            }
        }
        public string BackupUrl
        {
            get
            {
                return this._backupUrl;
            }
            set
            {
                this._backupUrl = value;
            }
        }
        public string RootPath
        {
            get
            {
                return this._rootPath;
            }
            set
            {
                this._rootPath = value;
            }
        }
        public string ZipName
        {
            get
            {
                return this._zipName;
            }
            set
            {
                this._zipName = value;
            }
        }
        public string SitePath
        {
            get
            {
                return this.sitePath.EndsWith(@"\") ? this.sitePath : this.sitePath + @"\";
            }
            set
            {
                this.sitePath = value.EndsWith(@"\") ? value : value + @"\";
            }
        }
        public string TDomName
        {
            get
            {
                return this._tDomName.IndexOf('.') != 0 ? "." + this._tDomName : this._tDomName;
            }
            set
            {
                this._tDomName = value;
            }
        }
        public string DnsOutputPath
        {
            get
            {
                return this._dnsOutputPath.EndsWith(@"\") ? this._dnsOutputPath : this._dnsOutputPath + @"\";
            }
            set
            {
                this._dnsOutputPath = value.EndsWith(@"\") ? value : value + @"\";
            }
        }
        public string newNetClientID
        {
            get
            {
                return this._newNetClientID;
            }
            set
            {
                this._newNetClientID = value;
            }
        }
        public string newNetApiPasswd
        {
            get
            {
                return this._newNetApiPasswd;
            }
            set
            {
                this._newNetApiPasswd = value;
            }
        }
        public string DefaultCheck
        {
            get
            {
                return this._DefaultCheck;
            }
            set
            {
                this._DefaultCheck = value;
            }
        }
        public string DefaultDisplay
        {
            get
            {
                return this._DefaultDisplay;
            }
            set
            {
                this._DefaultDisplay = value;
            }
        }
        public string DnsOption
        {
            get
            {
                return this._DnsOption;
            }
            set
            {
                this._DnsOption = value;
            }
        }
        public string ModelID
        {
            get
            {
                return this._ModelID;
            }
            set
            {
                this._ModelID = value;
            }
        }
        public string NodeID
        {
            get
            {
                return this._NodeID;
            }
            set
            {
                this._NodeID = value;
            }
        }
        public string DefaultIP
        {
            get
            {
                return this._DefaultIP;
            }
            set
            {
                this._DefaultIP = value;
            }
        }
        public string DNSTemplate
        {
            get
            {
                return this._DNSTemplate;
            }
            set
            {
                this._DNSTemplate = value;
            }
        }
        public bool AutoCreateDB { get; set; }
        public string DBManagerName { get; set; }
        public string DBManagerPasswd { get; set; }
        public bool RemoteUser { get; set; }
        public bool RemoteEnable { get; set; }
        public string DBName { get; set; }
        public string DBUName { get; set; }
        public string RemoteUrl { get; set; }
        public string Token { get; set; }
    }
}