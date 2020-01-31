using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ZoomLa.Extend
{
    public class PlugConfig
    {
        private static string filePath = AppDomain.CurrentDomain.BaseDirectory + @"\Config\Plug.config";
        private static PlugConfigInfo configMod = null;
        private static object Lockobj = new object();
        private PlugConfig() { }
        private static PlugConfigInfo GetInstance()
        {
            if (configMod == null)
            {
                configMod = ConfigReadFromFile();
            }
            return configMod;
        }
        //从配置文件获取配置信息
        private static PlugConfigInfo ConfigReadFromFile()
        {
            using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PlugConfigInfo));
                return (PlugConfigInfo)serializer.Deserialize(stream);
            }
        }
        public static void Update()
        {
            lock (Lockobj)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PlugConfigInfo));
                using (Stream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add("", "");//加上这句，否则会自动在根节点上加两个属性
                    serializer.Serialize(stream, GetInstance(), namespaces);
                    stream.Close(); stream.Dispose();
                }
            }
        }
        public static PlugConfigInfo Info { get { return GetInstance(); } }
    }
    public class PlugConfigInfo
    {
        /// <summary>
        /// VIP,会员,注册之日计时,必须在指定天内累积到指定的积分,否则重计
        /// </summary>
        public int NeedDay { get; set; }
        public int NeedPoint { get; set; }
        /// <summary>
        /// 使用积分购物的订单,是否允许赠送积分 1:是
        /// </summary>
        public int AllowGivePoint {get;set; }
    }
}
