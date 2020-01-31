using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace ZoomLa.BLL.Helper.Encry
{
    /*
     * 1,需要使用到BouncyCastle.Crypto.dll
     * 2,java与net提供的类库所用的密钥格式不同,net为xml,需要转换
     * 3,可以直接调用net类库生成公钥与私钥格式
     * 4,私钥格式java一般为base64,便于记录不可显示的字符
     * 
     * 公钥加密,私钥解密
     */
    public class RSAHelper
    {
        /// <summary>
        /// RSA加密+SHA1,并转为base64
        /// </summary>
        /// <param name="xmlPublicKey">公钥|私钥</param>
        /// <param name="m_strEncryptString">MD5加密后的数据</param>
        /// <returns>RSA公钥加密后的数据</returns>
        public static string RSAEncryptWithSHA1(string key, string signStr)
        {
            string result = "";
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(key);
            //byte[] bytes = new UnicodeEncoding().GetBytes(m_strEncryptString);
            byte[] bytes = rsa.SignData(Encoding.UTF8.GetBytes(signStr), new SHA1CryptoServiceProvider());
            result = Convert.ToBase64String(bytes);//显示加密后的，为了显示不可见字符，使用的是 Base64 编码。 
            return result;
        }
        /// <summary>
        /// 返回RSA公钥与私钥,建议存为Xml
        /// </summary>
        /// <returns></returns>
        public static void GetRsaKey(ref string publicKey, ref string privateKey)
        {
            RSACryptoServiceProvider rsaProvider;
            rsaProvider = new RSACryptoServiceProvider(1024);
            publicKey = rsaProvider.ToXmlString(false); //将RSA算法的公钥导出到字符串PublicKey中，参数为false表示不导出私钥
            privateKey = rsaProvider.ToXmlString(true);//将RSA算法的私钥导出到字符串PrivateKey中，参数为true表示导出私钥
        }
        public static string RsaEncrypt(string encryptString, string key)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(key);
            cipherbytes = rsa.Encrypt(Encoding.UTF8.GetBytes(encryptString), false);
            return Convert.ToBase64String(cipherbytes);
        }
        public static string RsaDecrypt(string decryptString, string key)
        {
            if (string.IsNullOrEmpty(decryptString)) { return ""; }
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(key);
            cipherbytes = rsa.Decrypt(Convert.FromBase64String(decryptString), false);
            return Encoding.UTF8.GetString(cipherbytes);
        }
    }
    /// <summary>
    /// RSA密钥格式转换
    /// </summary>
    public class RSAKeyConvert
    {
        ///// <summary>
        ///// RSA私钥格式转换，java->.net
        ///// </summary>
        ///// <param name="privateKey">java生成的RSA私钥</param>
        /// <returns></returns>
        //public static string RSAPrivateKeyJava2DotNet(string privateKey)
        //{
        //    RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKey));

        //    return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
        //        Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
        //        Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
        //        Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()),
        //        Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
        //        Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
        //        Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
        //        Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
        //        Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned()));
        //}

        ///// <summary>
        ///// RSA私钥格式转换，.net->java
        ///// </summary>
        ///// <param name="privateKey">.net生成的私钥</param>
        ///// <returns></returns>
        //public static string RSAPrivateKeyDotNet2Java(string privateKey)
        //{
        //    XmlDocument doc = new XmlDocument();
        //    doc.LoadXml(privateKey);
        //    BigInteger m = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
        //    BigInteger exp = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
        //    BigInteger d = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("D")[0].InnerText));
        //    BigInteger p = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("P")[0].InnerText));
        //    BigInteger q = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Q")[0].InnerText));
        //    BigInteger dp = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("DP")[0].InnerText));
        //    BigInteger dq = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("DQ")[0].InnerText));
        //    BigInteger qinv = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("InverseQ")[0].InnerText));

        //    RsaPrivateCrtKeyParameters privateKeyParam = new RsaPrivateCrtKeyParameters(m, exp, d, p, q, dp, dq, qinv);

        //    PrivateKeyInfo privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKeyParam);
        //    byte[] serializedPrivateBytes = privateKeyInfo.ToAsn1Object().GetEncoded();
        //    return Convert.ToBase64String(serializedPrivateBytes);
        //}

        ///// <summary>
        ///// RSA公钥格式转换，java->.net
        ///// </summary>
        ///// <param name="publicKey">java生成的公钥</param>
        ///// <returns></returns>
        //public static string RSAPublicKeyJava2DotNet(string publicKey)
        //{
        //    RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
        //    return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
        //        Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
        //        Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        //}

        ///// <summary>
        ///// RSA公钥格式转换，.net->java
        ///// </summary>
        ///// <param name="publicKey">.net生成的公钥</param>
        ///// <returns></returns>
        //public static string RSAPublicKeyDotNet2Java(string publicKey)
        //{
        //    XmlDocument doc = new XmlDocument();
        //    doc.LoadXml(publicKey);
        //    BigInteger m = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Modulus")[0].InnerText));
        //    BigInteger p = new BigInteger(1, Convert.FromBase64String(doc.DocumentElement.GetElementsByTagName("Exponent")[0].InnerText));
        //    RsaKeyParameters pub = new RsaKeyParameters(false, m, p);

        //    SubjectPublicKeyInfo publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(pub);
        //    byte[] serializedPublicBytes = publicKeyInfo.ToAsn1Object().GetDerEncoded();
        //    return Convert.ToBase64String(serializedPublicBytes);
        //}
    }
}
