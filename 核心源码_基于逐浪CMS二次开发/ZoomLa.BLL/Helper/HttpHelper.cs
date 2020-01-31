using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using ZoomLa.Common;
/*
 *Http文传上传,下载
 *扩展:上传文件能接收返回信息
 *备注:手机上也支持上传下载,详见WP8版
 */
namespace ZoomLa.BLL.Helper
{
    //public class HttpConfig
    //{
    //    public bool IsAsync = false;
    //    public string fname = "media";//很多API会有指定文件名规范,用于上传文件时使用
    //}
    ///// <summary>
    ///// 文件上传类(数据与文件http请求)
    ///// </summary>
    //public class HttpMultipartFormRequest
    //{
    //    private readonly Encoding DefaultEncoding = Encoding.UTF8;
    //    private ResponseCallback m_Callback;
    //    private byte[] m_FormData;
    //    public HttpConfig config = new HttpConfig();
    //    public HttpMultipartFormRequest()
    //    {
    //    }
    //    public delegate void ResponseCallback(string msg);
    //    /// <summary>
    //    /// 传多个文件
    //    /// </summary>
    //    /// <param name="postUri">请求的URL</param>
    //    /// <param name="postParameters">[filename,FileParameter]</param>
    //    /// <param name="callback">回掉函数</param>
    //    public HttpResult AsyncHttpRequest(string postUri, Dictionary<string, object> postParameters, ResponseCallback callback)
    //    {
    //        HttpResult result = new HttpResult();
    //        // 随机序列，用作防止服务器无法识别数据的起始位置
    //        string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
    //        // 设置contentType
    //        string contentType = "multipart/form-data; boundary=" + formDataBoundary;
    //        // 将数据转换为byte[]格式
    //        m_FormData = GetMultipartFormData(postParameters, formDataBoundary);
    //        // 回调函数
    //        m_Callback = callback;
    //        // 创建http对象
    //        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(postUri));
    //        // 设为post请求
    //        request.Method = "POST";
    //        request.ContentType = contentType;
    //        // 请求写入数据流
    //        if (config.IsAsync)
    //        {
    //            request.BeginGetRequestStream(GetRequestStreamCallback, request);
    //            result.Html = "Async";
    //            return result;
    //        }
    //        else//不开启异步
    //        {
    //            var postStream = request.GetRequestStream();
    //            postStream.Write(m_FormData, 0, m_FormData.Length);
    //            postStream.Close();
    //            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
    //            StreamReader sr = new StreamReader(response.GetResponseStream());
    //            result.Html = sr.ReadToEnd();
    //            result.StatusCode = response.StatusCode;
    //            result.StatusDescription = response.StatusDescription;
    //            result.ResponseUri = response.ResponseUri.AbsoluteUri;
    //            response.Close();
    //            return result;
    //        }
    //    }
    //    private byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
    //    {
    //        Stream formDataStream = new MemoryStream();
    //        bool needsCLRF = false;

    //        foreach (var param in postParameters)
    //        {
    //            if (needsCLRF)
    //            {
    //                formDataStream.Write(DefaultEncoding.GetBytes("\r\n"), 0, DefaultEncoding.GetByteCount("\r\n"));
    //            }
    //            needsCLRF = true;
    //            if (param.Value is M_Http_File)
    //            {
    //                M_Http_File fileToUpload = (M_Http_File)param.Value;

    //                string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
    //                    boundary,
    //                    param.Key, // param.Key, //此处如果是请求的php，则需要约定好 存取一致 php:$_FILES['img']['name']
    //                    fileToUpload.FileName ?? param.Key,
    //                    fileToUpload.ContentType ?? "application/octet-stream");

    //                // 将与文件相关的header数据写到stream中
    //                formDataStream.Write(DefaultEncoding.GetBytes(header), 0, DefaultEncoding.GetByteCount(header));
    //                // 将文件数据直接写到stream中
    //                formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
    //            }
    //            else
    //            {
    //                string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
    //                    boundary,
    //                    param.Key,
    //                    param.Value);
    //                formDataStream.Write(DefaultEncoding.GetBytes(postData), 0, DefaultEncoding.GetByteCount(postData));
    //            }
    //        }

    //        string tailEnd = "\r\n--" + boundary + "--\r\n";
    //        formDataStream.Write(DefaultEncoding.GetBytes(tailEnd), 0, DefaultEncoding.GetByteCount(tailEnd));

    //        // 将Stream数据转换为byte[]格式
    //        formDataStream.Position = 0;
    //        byte[] formData = new byte[formDataStream.Length];
    //        formDataStream.Read(formData, 0, formData.Length);
    //        formDataStream.Close();

    //        return formData;
    //    }
    //    private void GetRequestStreamCallback(IAsyncResult ar)
    //    {
    //        HttpWebRequest request = ar.AsyncState as HttpWebRequest;
    //        using (var postStream = request.EndGetRequestStream(ar))
    //        {
    //            postStream.Write(m_FormData, 0, m_FormData.Length);
    //            postStream.Close();
    //        }
    //        request.BeginGetResponse(GetResponseCallback, request);
    //    }
    //    private void GetResponseCallback(IAsyncResult ar)
    //    {
    //        // 处理Post请求返回的消息
    //        //try
    //        //{
    //        HttpWebRequest request = ar.AsyncState as HttpWebRequest;
    //        HttpWebResponse response = request.EndGetResponse(ar) as HttpWebResponse;

    //        using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
    //        {
    //            string msg = reader.ReadToEnd();

    //            if (m_Callback != null)
    //            {
    //                m_Callback(msg);
    //            }
    //        }
    //        //}
    //        //catch (Exception e)
    //        //{
    //        //    string a = e.ToString();
    //        //    if (m_Callback != null)
    //        //    {
    //        //        m_Callback(string.Empty);
    //        //    }
    //        //}
    //    }
    //}
    ///// <summary>
    ///// Http返回参数类
    ///// </summary>
    //public class HttpResult
    //{
    //    /// <summary>
    //    /// Http请求返回的Cookie
    //    /// </summary>
    //    public string Cookie { get; set; }
    //    /// <summary>
    //    /// Cookie对象集合
    //    /// </summary>
    //    public CookieCollection CookieCollection { get; set; }
    //    private string _html = string.Empty;
    //    /// <summary>
    //    /// 返回的String类型数据 只有ResultType.String时才返回数据，其它情况为空
    //    /// </summary>
    //    public string Html
    //    {
    //        get { return _html; }
    //        set { _html = value; }
    //    }
    //    /// <summary>
    //    /// 返回的Byte数组 只有ResultType.Byte时才返回数据，其它情况为空
    //    /// </summary>
    //    public byte[] ResultByte { get; set; }
    //    /// <summary>
    //    /// header对象
    //    /// </summary>
    //    public WebHeaderCollection Header { get; set; }
    //    /// <summary>
    //    /// 返回状态说明
    //    /// </summary>
    //    public string StatusDescription { get; set; }
    //    /// <summary>
    //    /// 返回状态码,默认为OK
    //    /// </summary>
    //    public HttpStatusCode StatusCode { get; set; }
    //    /// <summary>
    //    /// 最后访问的URl
    //    /// </summary>
    //    public string ResponseUri { get; set; }
    //}
    //---------------------------
    public class HttpHelper
    {
        //public HttpConfig config = new HttpConfig();
        #region 文件上传 同步/异步 OLD
        /// <summary>
        /// 单文件上传,用于微信等异步上传文件后返回资源ID
        /// </summary>
        /// <param name="url">服务器URL</param>
        /// <param name="name">元素名称=Request.Files[name]</param>
        /// <param name="fileMod">文件信息(文件名称,字节,ContentType)</param>
        /// <param name="callback">文件上传统一异步,必须实现回调</param>
        public static M_Http_Request UploadSignleFile(string url, string name, M_Http_File fileMod, M_Http_Request.ResponseCallback callback = null)
        {
            M_Http_Request req = new M_Http_Request() { PostUrl = url };
            req.formDatas.Add(name, fileMod);
            req.callback = callback;
            if (req.callback != null) { req.IsAsync = true; }
            HttpHelper.PostWithFile(req);
            return req;
        }
        //Html必须编码后再传输,html默认的是将其UrlEncode后传递,服务端再编辑,直接传是不能的
        //public M_Http_Result UploadParam(string url, Dictionary<string, object> postParameters)
        //{
        //    HttpMultipartFormRequest req = new HttpMultipartFormRequest();
        //    //req.config = config;
        //    M_Http_Result result = new M_Http_Result();
        //    result = req.AsyncHttpRequest(url, postParameters, null);
        //    return result;
        //}
        #endregion


        #region 文件下载
        /// <summary>
        /// 从指定服务器上下载文件,支持断点续传
        /// </summary>
        /// <param name="url">目标Url</param>
        /// <param name="vpath">本地虚拟路径</param>
        /// <param name="begin">开始位置,默认为0</param>
        public static void DownFile(string url, string vpath, int begin = 0)
        {
            //尝试除以0,原因:下载的文字过小,占用字节为0
            vpath = SafeSC.PathDeal(vpath);
            if (SafeSC.FileNameCheck(vpath)) { throw new Exception("不支持下载[" + Path.GetFileName(vpath) + "]文件"); }
            string ppath = function.VToP(vpath);

            //long percent = 0; 
            long sPosstion = 0;//磁盘现盘文件的长度
            //long count = 0;// count += sPosstion,从指定位置开始写入字节
            FileStream FStream;

            if (File.Exists(ppath))
            {
                FStream = File.OpenWrite(ppath);//打开继续写入,并从尾部开始,用于断点续传(如果不需要,则应该删除其)
                sPosstion = FStream.Length;
                FStream.Seek(sPosstion, SeekOrigin.Current);//移动文件流中的当前指针
            }
            else
            {
                string dir = Path.GetDirectoryName(ppath);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                try { FStream = new FileStream(ppath, FileMode.Create); }
                catch (Exception ex) { throw new Exception(ex.Message + "||" + ppath); }
                sPosstion = 0;
            }
            //打开网络连接
            Stream myStream = null;
            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);
                //if (CompletedLength > 0)
                //    myRequest.AddRange((int)CompletedLength);//设置Range值,即头，从指定位置开始接收文件..
                //向服务器请求，获得服务器的回应数据流
                HttpWebResponse webResponse = (HttpWebResponse)myRequest.GetResponse();
                //long FileLength = webResponse.ContentLength;//文件大小
                //percent = FileLength / 100;
                myStream = webResponse.GetResponseStream();
                byte[] btContent = new byte[1024];
                //开始写入
                int count = 0;
                while ((count = myStream.Read(btContent, 0, 1024)) > 0)//返回读了多少字节,为0表示全部读完
                {
                    FStream.Write(btContent, 0, count);//知道有多少个数字节后再写入
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("基础连接已经关闭"))
                { throw new Exception("DownFile:" + ex.Message + "|" + url); }
            }
            finally
            {
                if (myStream != null) { myStream.Close(); }
                if (FStream != null) { FStream.Close(); }
            }
        }
        /// <summary>
        /// 主用于采集,处理成正确的路径方便采集
        /// </summary>
        /// <param name="baseurl">当前网址路径</param>
        /// <param name="url">路径名,如为带http的则不处理</param>
        /// <param name="ppath">本地保存的路径</param>
        /// <return>虚拟路径</return>
        public static string DownFile(string cururl, string url, string ppath)
        {
            string baseurl = StrHelper.GetUrlRoot(cururl).TrimEnd('/') + "/";
            string urlpath = cururl.Contains("/") ? (cururl.Substring(0, cururl.LastIndexOf("/")).TrimEnd('/') + "/") : cururl;
            string strurl = url.ToLower();//不更改原大小写,因为站点资源可能大小写敏感
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(ppath)) return url;
            if (strurl.Contains("file:")) return url;//如果对方网站弄错路径,则不下载
            if (strurl.StartsWith("http:") || strurl.StartsWith("https:"))//不需做任何处理
            {

            }
            else if (url.StartsWith("/"))//   /image/123.jpg,根目录
            {
                url = (baseurl + url.TrimStart('/'));
            }
            else if (url.StartsWith("../"))//上一级目录,暂只处理一级,多级使用for循环
            {
                string parent = (urlpath.TrimEnd('/'));
                parent = parent.Substring(0, parent.LastIndexOf("/")) + "/";
                url = parent + url.Replace("../", "");
            }
            else if (url.StartsWith("./")) //当前目录
            {
                url = urlpath + url.Replace("./", "");
            }
            else //同于./   image/123.jpg
            {
                url = url.Replace("../", "").Replace("./", "");
                url = (baseurl + url.TrimStart('/'));
            }
            DownFile(url, ppath);
            return function.PToV(ppath);
        }
        #endregion
        #region 文件异步上传
        public static void PostWithFile(M_Http_Request req)
        {
            // 随机序列，用作防止服务器无法识别数据的起始位置
            string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
            // 将数据转换为byte[]格式
            req.formDataBytes = GetMultipartFormData(req.formDatas, formDataBoundary);
            // 创建http对象
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(req.PostUrl));
            // 设为post请求
            request.Method = "POST";
            request.ContentType = "multipart/form-data; boundary=" + formDataBoundary;
            if (req.IsAsync)
            {
                request.BeginGetRequestStream(req.GetRequestStreamCallback, request);
            }
            else
            {
                var postStream = request.GetRequestStream();
                postStream.Write(req.formDataBytes, 0, req.formDataBytes.Length);
                postStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                req.result.isSuccess = true;
                req.result.resultStr = sr.ReadToEnd();
                response.Close();
            }
        }
        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new MemoryStream();
            bool needsCLRF = false;

            var DefaultEncoding = Encoding.UTF8;
            foreach (var param in postParameters)
            {
                if (needsCLRF)
                {
                    formDataStream.Write(DefaultEncoding.GetBytes("\r\n"), 0, DefaultEncoding.GetByteCount("\r\n"));
                }
                needsCLRF = true;
                if (param.Value is M_Http_File)
                {
                    M_Http_File fileToUpload = (M_Http_File)param.Value;

                    string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\";\r\nContent-Type: {3}\r\n\r\n",
                        boundary,
                        param.Key, // param.Key, //此处如果是请求的php，则需要约定好 存取一致 php:$_FILES['img']['name']
                        fileToUpload.FileName ?? param.Key,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    // 将与文件相关的header数据写到stream中
                    formDataStream.Write(DefaultEncoding.GetBytes(header), 0, DefaultEncoding.GetByteCount(header));
                    // 将文件数据直接写到stream中
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(DefaultEncoding.GetBytes(postData), 0, DefaultEncoding.GetByteCount(postData));
                }
            }

            string tailEnd = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(DefaultEncoding.GetBytes(tailEnd), 0, DefaultEncoding.GetByteCount(tailEnd));

            // 将Stream数据转换为byte[]格式
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Close();

            return formData;
        }
        #endregion
        #region Get/Post/转发
        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //直接确认，否则打不开    
            return true;
        }
        /// <summary>
        /// 带Cookies|证书等的提交
        /// 证书未设置好会有较长时间的卡顿
        /// </summary>
        public static M_Http_Result Post(M_Http_Request reqMod)
        {
            //System.GC.Collect();//垃圾回收，回收没有正常关闭的http连接
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(reqMod.PostUrl);
                //设置https验证方式并带证书
                if (!string.IsNullOrEmpty(reqMod.CertPath))
                {
                    //有些API请求只是需要验证自己发布的证书,并不需要https
                    if (reqMod.PostUrl.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                    {
                        ServicePointManager.DefaultConnectionLimit = 200;
                        ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                    }
                    //X509Certificate2 cert = new X509Certificate2(function.VToP(appMod.Pay_SSLPath), appMod.Pay_SSLPassword);
                    ////@"D:\Code\ssw.cer"  ||证书名称
                    //Cryptography.X509Certificates.X509Certificate2 cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(reqMod.CertPath, reqMod.CertPwd);
                    //request.ClientCertificates.Add(cert);
                }
                if (reqMod.cookie.Count == 0)
                {
                    request.CookieContainer = new CookieContainer();
                    reqMod.cookie = request.CookieContainer;
                }
                else
                {
                    request.CookieContainer = reqMod.cookie;
                }

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                byte[] data = Encoding.UTF8.GetBytes(reqMod.PostDataStr);
                request.ContentLength = data.Length;
                //向服务端写入数据
                Stream reqStream = request.GetRequestStream();
                reqStream.Write(data, 0, data.Length);
                reqStream.Close();
                //request.Timeout = 10 * 1000;

                //设置代理服务器
                //WebProxy proxy = new WebProxy();                          //定义一个网关对象
                //proxy.Address = new Uri(WxPayConfig.PROXY_URL);              //网关服务器端口:端口
                //request.Proxy = proxy;

                response = (HttpWebResponse)request.GetResponse();
                Stream myResponseStream = response.GetResponseStream();
                StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.UTF8);
                reqMod.result.resultStr = myStreamReader.ReadToEnd();
                myStreamReader.Close();
                myResponseStream.Close();
                reqMod.result.isSuccess = true;
            }
            catch (Exception ex)
            {
                reqMod.result.isSuccess = false;
                reqMod.result.exmsg = ex.Message;
                ZLLog.L("Post,[" + reqMod.PostUrl + "][" + reqMod.PostDataStr + "],err:" + ex.Message);
            }
            finally
            {
                //关闭连接和流
                if (response != null) { response.Close(); }
                if (request != null) { request.Abort(); }
            }
            return reqMod.result;
        }
        /// <summary>
        /// Get("https://www.z01.com/Tools/test","",ref cc);
        /// </summary>
        public static string Get(string Url, string postDataStr, ref CookieContainer cookie)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url + (postDataStr == "" ? "" : "?") + postDataStr);
            if (cookie.Count == 0)
            {
                request.CookieContainer = new CookieContainer();
                cookie = request.CookieContainer;
            }
            else
            {
                request.CookieContainer = cookie;
            }

            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }
        #endregion
        #region Tools
        /// <summary>
        /// 根据域名获取IP地址
        /// </summary>
        /// <param name="domain">www.zoomla.cn</param>
        /// <returns></returns>
        public static string GetIPByDomain(string domain)
        {
            string result = "";
            if (string.IsNullOrEmpty(domain)) { return result; }
            if (RegexHelper.IsIP(domain)) { return domain; }
            domain = domain.ToLower().Replace("http://", "").Replace("https://", "").Replace("/", "");
            IPAddress[] IPs = Dns.GetHostAddresses(domain);
            if (IPs.Length > 0) { result = IPs[0].ToString(); }
            return result;
        }
        #endregion
    }
    public class M_Http_Request
    {
        /// <summary>
        /// 是否开启异步
        /// </summary>
        public bool IsAsync = false;
        public string PostUrl = "";
        /// <summary>
        /// 格式:"type=2&openId=oo9oqwCcIbdWgfXMyuBWd-UTX9wo";
        /// </summary>
        public string PostDataStr = "";
        /// <summary>
        /// 证书的物理路径,如果是已布署至本机的证书则为证书名称(微信)
        /// </summary>
        public string CertPath = "";
        /// <summary>
        /// 证书密码
        /// </summary>
        public string CertPwd = "";
        /// <summary>
        /// 需要request的cookie值,并会根据返回更新
        /// </summary>
        public CookieContainer cookie = new CookieContainer();
        public M_Http_Result result = new M_Http_Result();
        //-------------------------单独分离出来
        /// <summary>
        /// 仿Form表单文件上传
        /// </summary>
        public Dictionary<string, object> formDatas = new Dictionary<string, object>();
        public byte[] formDataBytes = null;
        /// <summary>
        /// 异步处理后的回调方法(用于上传与下载文件)
        /// </summary>
        public delegate void ResponseCallback(string result);
        public ResponseCallback callback = null;
        public void GetRequestStreamCallback(IAsyncResult ar)
        {
            HttpWebRequest request = ar.AsyncState as HttpWebRequest;
            using (var postStream = request.EndGetRequestStream(ar))
            {
                postStream.Write(this.formDataBytes, 0, this.formDataBytes.Length);
                postStream.Close();
            }
            request.BeginGetResponse(GetResponseCallback, request);
        }
        public void GetResponseCallback(IAsyncResult ar)
        {
            // 处理Post请求返回的消息
            //try
            //{
            HttpWebRequest request = ar.AsyncState as HttpWebRequest;
            HttpWebResponse response = request.EndGetResponse(ar) as HttpWebResponse;

            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                string msg = reader.ReadToEnd();
                if (this.callback != null)
                {
                    this.callback(msg);
                }
            }
        }
    }
    public class M_Http_Result
    {
        /// <summary>
        /// 该次请求是否成功
        /// </summary>
        public bool isSuccess = false;
        /// <summary>
        /// 返回的值
        /// </summary>
        public string resultStr = "";
        /// <summary>
        /// 异常信息存储
        /// </summary>
        public string exmsg = "";
    }
    /// <summary>
    /// 需要通过http上传的文件参数
    /// </summary>
    public class M_Http_File
    {
        // 文件内容
        public byte[] File { get; set; }
        // 文件名
        public string FileName { get; set; }
        // 文件内容类型
        public string ContentType { get; set; }
        public M_Http_File(byte[] file, string filename, string contentType = "multipart/form-data")
        {
            File = file;
            FileName = filename;
            ContentType = contentType;
        }
        /// <summary>
        /// 返回图片的提交contentType
        /// </summary>
        public static string GetImgContentType(string imgurl)
        {
            string ExName = Path.GetExtension(imgurl);
            switch (ExName.ToLower())
            {
                case "png":
                    return "image/png";
                case "gif":
                    return "image/gif";
                case "bmp":
                    return "image/bmp";
                case "ico":
                    return "image/ico";
                default:
                    return "image/jpeg";
            }
        }
    }
    /*
    *  --PostWithFile
    *  M_Http_Request req = new M_Http_Request();
    req.formDatas.Add("file",new M_Http_File(SafeSC.ReadFileByte("/Tools/getOrderInfo.txt"), "123.txt"));
    req.formDatas.Add("uname", "i am jack");
    req.PostUrl = "http://win100:86/Tools/Example";
    req.callback = ResponseCallBack;
    HttpHelper.PostWithFile(req);
    public void ResponseCallBack(string msg)
    {
        ZLLog.L("server return:"+msg);
    }
    * 
    * --Transfer
    *  ///var formdata = new FormData();formdata.append("file", document.getElementById("test_f").files[0]);
    ///$.ajax({
    ///  type: 'POST',url: '/Tools/Example',
    ///  data: formdata,processData: false,contentType: false,
    ///  success: function(data) { console.log(data); }});
    * 
    * --Post
    * /// M_Http_Result result = HttpHelper.Post(new M_Http_Request() { PostUrl = url, PostDatStr = param, CertPath = @"D:\Code\ssw.cer" });
    /// CookieContainer cc = new CookieContainer();
    /// cc.Add(new System.Uri("http://win01:86/"), new Cookie("ManageState", "ManageId=1&LoginName=YWRtaW4=&TrueName=YWRtaW4=&Password=7fef6171469e80d32c0559f88b377245")); 
    * 
    * 
    */
}
