using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZoomLa.BLL;
using ZoomLa.BLL.Helper;
using ZoomLa.Common;
using ZoomLa.Components;
using ZoomLa.Model;
using ZoomLa.SQLDAL;
using ZoomLa.SQLDAL.SQL;
using ZoomLaCMS.Ctrl;

namespace ZoomLaCMS.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "Admin")]
    public class ConfigController : Ctrl_Admin
    {
        B_Group gpBll = new B_Group();
        B_Search shBll = new B_Search();
        public IActionResult Index()
        {
            return RedirectToAction("SiteInfo");
        }
        public IActionResult SiteInfo()
        {
            return View(SiteConfig.SiteInfo);
        }
        public IActionResult SiteInfo_Submit()
        {
            SiteConfig.SiteInfo.SiteName = RequestEx["SiteName_T"];
            SiteConfig.SiteInfo.SiteTitle = RequestEx["SiteTitle_T"];
            SiteConfig.SiteInfo.SiteUrl = RequestEx["SiteUrl_T"];
            SiteConfig.SiteInfo.LogoUrl = RequestEx["LogoUrl_T"];
            SiteConfig.SiteInfo.BannerUrl = RequestEx["QRCode_T"];
            SiteConfig.SiteInfo.Webmaster = RequestEx["Webmaster_T"];
            SiteConfig.SiteInfo.MasterPhone = RequestEx["MasterPhone_T"];
            SiteConfig.SiteInfo.WebmasterEmail = RequestEx["WebmasterEmail_T"];
            SiteConfig.SiteInfo.Copyright = RequestEx["Copyright_T"];
            SiteConfig.SiteInfo.MetaKeywords = RequestEx["MetaKeywords_T"];
            SiteConfig.SiteInfo.MetaDescription = RequestEx["MetaDescription_T"];
            SiteConfig.SiteInfo.LogoAdmin = RequestEx["LogoAdmin_T"];
            SiteConfig.SiteInfo.LogoPlatName = RequestEx["LogoPlatName_T"];
            SiteConfig.SiteInfo.AllSiteJS = RequestEx["allSiteJS"];
            SiteConfig.SiteInfo.CompanyName = RequestEx["ComName_T"];

            SiteConfig.Update();
            return WriteOK("保存成功", "SiteInfo");

        }
        public IActionResult PWA_Submit()
        {
            string dir = ZLHelper.GetUploadDir_System("PWA");
            string dir_icon = (dir + function.GetRandomString(8) + "_icon-");//文件名不重复,避免更新不生效
            string ext = ".png";
            var file = Request.Form.Files["pwa_icon_up"];
            if (file!=null&&file.Length>100)
            {
                //144 96
                //192 180 152 120 76
                
                int[] sizes = new int[] { 180, 152, 120, 76, 144, 96 };
                //---------------------
                System.Drawing.Image img = System.Drawing.Image.FromStream(file.OpenReadStream());
                if (img.Width != 192 || img.Height != 192) { return WriteErr("上传的图片宽高必须为192*192"); }
                ImgHelper imgHelp = new ImgHelper();
                foreach (int size in sizes)
                {
                    System.Drawing.Bitmap bmp = imgHelp.ZoomImg(img, size, size);
                    string path = dir_icon + size + ext;
                    imgHelp.SaveImg(path, bmp);
                    img = System.Drawing.Image.FromStream(file.OpenReadStream());
                }
                ImgHelper.SaveImage(dir_icon + "192" + ext, img);
                SiteConfig.SiteOption.PWA_Icon = dir_icon + "192" + ext;
            }

            //-------------------写入文件生成mainfest
            string json = SafeSC.ReadFileStr("/lib/pwa/pwa.json");
            json = json.Replace("@short_name", GetParam("ShortName_T"))
                .Replace("@name", GetParam("Name_T"))
                .Replace("@background_color", string.IsNullOrEmpty(GetParam("BKColor_T")) ? "#fff" : GetParam("BKColor_T"))
                .Replace("@icon96", dir_icon + "96" + ext)
                .Replace("@icon144", dir_icon + "144" + ext)
                .Replace("@start_url", GetParam("StartUrl_T"));
            SafeSC.WriteFile(dir + "manifest.json", json);
            StringBuilder sb = new StringBuilder();
            sb.Append("<link rel=\"manifest\" href=\"" + dir + "manifest.json\">\n");
            sb.Append("<meta name=\"apple-mobile-web-app-capable\" content=\"yes\"/>\n");
            sb.Append("<meta name=\"apple-mobile-web-app-status-bar-style\" content=\"black-translucent\" />\n");
            sb.Append("<meta name=\"apple-mobile-web-app-title\" content=\"" + GetParam("ShortName_T") + "\">\n");
            foreach (int size in (new int[] { 180, 152, 120, 76 }))
            {
                string path = dir_icon + size + ext;
                sb.Append("<link rel=\"apple-touch-icon\" sizes=\"" + size + "x" + size + "\" href=\"" + path + "\" />\n");
            }
            sb.Append("<link rel=\"icon\" sizes=\"192x192\" href=\"" + (dir_icon + "192" + ext) + "\" />");
            //QuoteContent_T.Text = sb.ToString();

            SiteConfig.SiteOption.PWA_ShortName = GetParam("ShortName_T");
            SiteConfig.SiteOption.PWA_Name = GetParam("Name_T");
            SiteConfig.SiteOption.PWA_BKColor = GetParam("BKColor_T");

            SiteConfig.SiteOption.PWA_StartUrl = GetParam("StartUrl_T");
            SiteConfig.SiteOption.PWA_Content = sb.ToString();
            SiteConfig.Update();
            return WriteOK("PWA配置保存成功");
        }
        public IActionResult SiteOption()
        {
            return View();
        }
        public IActionResult SiteOption_Submit()
        {
            SiteConfig.SiteOption.AdvertisementDir = RequestEx["txtAdvertisementDir"];
            SiteConfig.SiteOption.CssDir = RequestEx["txtCssDir"];
            SiteConfig.SiteOption.StylePath = RequestEx["txtStylePath"];
            SiteConfig.SiteOption.SiteManageMode = DataConvert.CLng(RequestEx["SiteManageMode_Chk"]);

            SiteConfig.SiteOption.DesignIsClose = DataConvert.CLng(RequestEx["DesignIsClose"])==1;
            SiteConfig.SiteOption.EnableSiteManageCode = DataConvert.CLng(RequestEx["EnableSiteManageCode"]) == 1;
            SiteConfig.SiteOption.EnableSoftKey = DataConvert.CLng(RequestEx["EnableSoftKey"]) == 1; 
            SiteConfig.SiteOption.EnableUploadFiles = DataConvert.CLng(RequestEx["EnableUploadFiles"]) == 1;
            SiteConfig.SiteOption.IsAbsoluatePath = DataConvert.CLng(RequestEx["IsAbsoluatePath"]) == 1; 
            SiteConfig.SiteOption.OpenSendMessage = DataConvert.CLng(RequestEx["OpenSendMessage"]) == 1; 
            SiteConfig.SiteOption.VerifyType = DataConvert.CLng(RequestEx["VerifyType_rad"]);
            SiteConfig.SiteOption.VerifyForm = DataConvert.CLng(RequestEx["VerifyForm_rad"]);
            SiteConfig.SiteOption.VerifyLen = DataConvert.CLng(RequestEx["VerifyLen_T"]);
            SiteConfig.SiteOption.DomainMerge = DataConvert.CLng(RequestEx["DomainMerge_Chk"])==1;
            SiteConfig.SiteOption.Domain_PC = RequestEx["Domain_PC_T"];
            SiteConfig.SiteOption.Domain_Mobile =RequestEx["Domain_Mobile_T"];
            SiteConfig.SiteOption.Domain_Wechat = RequestEx["Domain_Wechat_T"];
            SiteConfig.SiteOption.SSOCookies_Domain = RequestEx["SSOCookies_Domain_T"];
            //SiteConfig.SiteOption.IsMall = IsMall.Checked;
            SiteConfig.SiteOption.CloudLeadTips = DataConvert.CLng(RequestEx["cloudLeadTips"]).ToString();
            SiteConfig.SiteOption.UAgent = DataConvert.CLng(RequestEx["UAgent"]) == 1;
            SiteConfig.SiteOption.KDKey = RequestEx["KDKey_T"];
            //商城模块配置
            //SiteConfig.SiteOption.OrderMsg_Chk = GetCheckVal(OrderMsg_Chk);
            SiteConfig.SiteOption.THDate = Convert.ToInt32(RequestEx["ReturnDate_T"]);
            //SiteConfig.SiteOption.OrderMsg_Tlp = GetJson(orderparam, OrderMsg_ordered_T.Text, OrderMsg_payed_T.Text);
            //SiteConfig.SiteOption.OrderMasterMsg_Chk = GetCheckVal(OrderMasterMsg_Chk);
            //SiteConfig.SiteOption.OrderMasterMsg_Tlp = GetJson(orderparam, OrderMasterMsg_ordered_Tlp.Text, OrderMasterMsg_payed_Tlp.Text);
            //SiteConfig.SiteOption.OrderMasterEmail_Chk = GetCheckVal(OrderMasterEmail_Chk);
            //SiteConfig.SiteOption.OrderMasterEmail_Tlp = GetJson(orderparam, OrderMasterEmail_ordered_Tlp.Text, OrderMasterEmail_payed_Tlp.Text);
            //----
            SiteConfig.SiteOption.DomainRoute = DataConvert.CLng(RequestEx["DomainRoute_chk"]).ToString();

            SiteConfig.SiteOption.SiteCollKey = RequestEx["SiteCollKey_T"];
            SiteConfig.SiteOption.SafeDomain = DataConvert.CLng(RequestEx["safeDomain_Chk"]).ToString();
            SiteConfig.SiteOption.SiteManageCode = RequestEx["txtSiteManageCode"];
            SiteConfig.SiteOption.TemplateDir = RequestEx["DropTemplateDir"];
            SiteConfig.SiteOption.ProjectServer = RequestEx["txtProjectServer"];
            SiteConfig.SiteOption.GeneratedDirectory = RequestEx["GeneratedDirectory"];
            SiteConfig.SiteOption.PdfDirectory = RequestEx["PdfDirectory"];
            SiteConfig.SiteOption.IndexEx = RequestEx["IndexEx"];
            SiteConfig.SiteOption.IndexTemplate = RequestEx["IndexTemplate_DP_hid"];
            SiteConfig.SiteOption.ShopTemplate = RequestEx["ShopTemplate_DP_hid"];
            SiteConfig.SiteOption.UploadDir = RequestEx["txtUploadDir"];
            SiteConfig.SiteOption.UploadFileExts = RequestEx["txtUploadFileExts"];
            SiteConfig.SiteOption.UploadPicExts = RequestEx["TxtUpPicExt"];
            SiteConfig.SiteOption.UploadPicMaxSize = DataConvert.CLng(RequestEx["TxtUpPicSize"]);
            //SiteConfig.SiteOption.EditVer = EditVer.SelectedValue;
            //SiteConfig.SiteOption.IsSaveRemoteImage = EditVer.SelectedValue == "1" ? true : false;
            SiteConfig.SiteOption.UploadMdaExts = RequestEx["TxtUpMediaExt"];
            SiteConfig.SiteOption.UploadMdaMaxSize = DataConvert.CLng(RequestEx["TxtUpMediaSize"]);

            SiteConfig.SiteOption.UploadFlhMaxSize = DataConvert.CLng(RequestEx["TxtUpFlashSize"]);
            //SiteConfig.ShopConfig.OrderNum = decimal.Parse(txtSetPrice.Text);
            SiteConfig.ShopConfig.ItemRegular = RequestEx["ItemRegular_T"];
            SiteConfig.ShopConfig.IsCheckPay = DataConvert.CLng(RequestEx["IsCheckPay"]);
            SiteConfig.ShopConfig.OrderExpired = DataConvert.CLng(RequestEx["OrderExpired_T"]);
            //SiteConfig.ShopConfig.EnablePointBuy = EnablePointBuy_Chk.Checked;
            SiteConfig.ShopConfig.PointRatiot = DataConvert.CDouble(RequestEx["PointRatio_T"]);
            SiteConfig.ShopConfig.PointRate = DataConvert.CDouble(RequestEx["PointRate_T"]);
            SiteConfig.ShopConfig.ExpNames = DataConvert.CStr(RequestEx["ExpNames_T"]);
            //SiteConfig.SiteOption.OpenMoneySel = OpenMoneySel_Chk.Checked;
            SiteConfig.SiteOption.OpenMessage = DataConvert.CLng(RequestEx["OpenMessage"]);
            SiteConfig.SiteOption.DupTitleNum = DataConvert.CLng(RequestEx["DupTitleNum_T"]);
            SiteConfig.SiteOption.FileRj = SiteConfig.SiteOption.DupTitleNum > 0 ? 1 : 0;
            SiteConfig.SiteOption.OpenAudit = DataConvert.CLng(RequestEx["OpenAudit"]);
            SiteConfig.SiteOption.IsSensitivity = DataConvert.CLng(RequestEx["rdoIsSensitivity"]);
            SiteConfig.SiteOption.Sensitivity = RequestEx["TxtSensitivity"];
            SiteConfig.SiteOption.Videourl = RequestEx["Videourl"];


            SiteConfig.SiteOption.RegPageStart = DataConvert.CLng(RequestEx["RegPageStart"]) == 1;
            SiteConfig.SiteOption.MailPermission = DataConvert.CStr(RequestEx["MailPermission"]);
            SiteConfig.SiteOption.FileN = DataConvert.CLng(RequestEx["FileN"]);
            SiteConfig.SiteOption.IsOpenHelp = DataConvert.CLng(RequestEx["IsOpenHelp"]).ToString();
            SiteConfig.SiteOption.SiteID = RequestEx["PayType"];
            SiteConfig.SiteOption.LoggedUrl = RequestEx["LoggedUrl_T"];

            SiteConfig.UserConfig.CommentRule = DataConvert.CLng(RequestEx["CommentRule"]);
            SiteConfig.Update();
            //XmlDocument appDoc = new XmlDocument();
            //appDoc.Load(Server.MapPath("/Config/AppSettings.config"));
            //XmlNodeList amde = appDoc.SelectSingleNode("appSettings").ChildNodes;
            //try
            //{
            //    appDoc.Save(Server.MapPath("/Config/AppSettings.config"));
            //}
            //catch (System.IO.IOException) { }

            //if (Convert.ToInt64(txtUploadFileMaxSize.Text) > 4096) { return WriteErr("IIS可支持最大文件上传的容量为4G!"); }
            //webhelper.UpdateMaxFile((Convert.ToInt64(txtUploadFileMaxSize.Text) * 1024 * 1024).ToString());

         
            return WriteOK("网站参数配置保存成功", "SiteOption");
        }
        public IActionResult MailConfig()
        {
            return View();
        }
        public IActionResult MailConfig_Submit()
        {
            SiteConfig.MailConfig.MailFrom = GetParam("MailName_T");
            SiteConfig.MailConfig.MailServer = GetParam("SMTP_T");
            SiteConfig.MailConfig.Port = Convert.ToInt32(GetParam("MailPort_T"));
            //if (RadioButton1.Checked) { SiteConfig.MailConfig.AuthenticationType = SendMail.AuthenticationType.None; }
            //if (RadioButton2.Checked) { SiteConfig.MailConfig.AuthenticationType = SendMail.AuthenticationType.Basic; }
            //if (RadioButton3.Checked) { SiteConfig.MailConfig.AuthenticationType = SendMail.AuthenticationType.Ntlm; }

            SiteConfig.MailConfig.MailServerUserName = GetParam("MailName_T");
            SiteConfig.MailConfig.MailServerPassWord = GetParam("MailPwd_T");
            SiteConfig.MailConfig.MailServerList = GetParam("TextBox4");
            SiteConfig.Update();
            return WriteOK("邮件参数配置保存成功！", "MailConfig");
        }
        public IActionResult UserConfig()
        {
            return View();
        }
        public IActionResult UserConfig_Submit()
        {
            SiteConfig.UserConfig.EnableUserReg = DataConvert.CBool(GetParam("EnableUserReg"));
            SiteConfig.UserConfig.UserValidateType = DataConvert.CBool(GetParam("UserValidateType"));
            SiteConfig.UserConfig.EmailCheckReg = DataConvert.CBool(RequestEx["EmailCheckReg"]);
            //obj2.UserConfig.EnableAlipayCheckReg = rdoAlipayCheck.Items[0].Selected;//支付宝
            SiteConfig.UserConfig.EmailRegis = DataConvert.CBool(RequestEx["EmailRegis"]);
            SiteConfig.UserConfig.UserIDlgn = DataConvert.CBool(RequestEx["UserIDlgn"]) ;//会员ID登录
            SiteConfig.UserConfig.MobileReg = DataConvert.CBool(RequestEx["MobileReg"]);//会员手机注册
            SiteConfig.UserConfig.MobileCodeNum = DataConverter.CLng(RequestEx["MobileCodeNum"]);
            //SiteConfig.UserConfig.MobileCodeType = DataConverter.CLng(Request.Form["MobileCodeType"]);
            SiteConfig.UserConfig.EmailTell = DataConvert.CBool(RequestEx["EmailTell"]);
            SiteConfig.UserConfig.AdminCheckReg = DataConvert.CBool(RequestEx["AdminCheckReg"]);
            SiteConfig.UserConfig.EnableCheckCodeOfReg = DataConvert.CBool(RequestEx["EnableCheckCodeOfReg"]);
            SiteConfig.UserConfig.UserNameLimit = DataConvert.CLng(RequestEx["UserNameLimit"],3);
            SiteConfig.UserConfig.UserNameMax = DataConvert.CLng(RequestEx["UserNameMax"],20);
            SiteConfig.UserConfig.UserNameRegDisabled = RequestEx["UserNameRegDisabled"];

            SiteConfig.UserConfig.EnableCheckCodeOfLogin = Request.Form["EnableCheckCodeOfLogin_rad"];
            //SiteConfig.UserConfig.EnableMultiLogin = RadioButtonList7.Checked;
            SiteConfig.UserConfig.CommentRule = DataConvert.CLng(RequestEx["CommentRule"]);
            SiteConfig.UserConfig.InfoRule = DataConvert.CLng(RequestEx["InfoRule"]);
            SiteConfig.UserConfig.RecommandRule = DataConvert.CLng(RequestEx["RecommandRule"]);
            //obj2.UserConfig.LoginRule = int.Parse(tb_LoginRule.Text.Trim());            
            //SiteConfig.UserConfig.UserGetPasswordEmail = txtGetPassword.Text;
            SiteConfig.UserConfig.RegFieldsMustFill = RequestEx["HdnRegFields_MustFill"];
            SiteConfig.UserConfig.RegFieldsSelectFill = RequestEx["HdnRegFields_SelectFill"];
            //SiteConfig.UserConfig.EmailTellContent = txtEmailTell.Text;
            SiteConfig.UserConfig.MobileRegInfo = RequestEx["MobileRegInfo"];
            //SiteConfig.UserConfig.PointExp = DataConverter.CDouble(TxtCUserExpExchangePoints.Text);
            //SiteConfig.UserConfig.PointMoney = DataConverter.CDouble(TxtCUserExpExchangeMoney.Text);

            //SiteConfig.UserConfig.ChangeSilverCoinByExp = DataConverter.CDouble(TxtCUserExpExchangeExp.Text);
            //SiteConfig.UserConfig.PointSilverCoin = DataConverter.CDouble(TxtCUserExpExchangeSilverCoin.Text);

            SiteConfig.UserConfig.PresentExp = DataConverter.CDouble(RequestEx["PresentExp"]);
            SiteConfig.UserConfig.PresentMoney = DataConverter.CDouble(RequestEx["PresentMoney"]);
            SiteConfig.UserConfig.PresentPoint = DataConverter.CLng(RequestEx["PresentPoint"]);
            SiteConfig.UserConfig.PresentPointAll = DataConverter.CLng(RequestEx["PresentPointAll"]);
            SiteConfig.UserConfig.PresentValidNum = DataConverter.CLng(RequestEx["PresentValidNum"]);
            SiteConfig.UserConfig.PresentValidUnit = DataConverter.CLng(RequestEx["PresentValidUnit"]);
            SiteConfig.UserConfig.PresentExpPerLogin = DataConverter.CDouble(RequestEx["PresentExpPerLogin"]);
            SiteConfig.UserConfig.SigninPurse = DataConverter.CDouble(RequestEx["SigninPurse"]);
            SiteConfig.UserConfig.Integral = DataConverter.CLng(RequestEx["Integral"]);
            SiteConfig.UserConfig.IntegralPercentage = DataConverter.CDouble(RequestEx["IntegralPercentage"]);

            //SiteConfig.UserConfig.MoneyExchangePointByMoney = DataConverter.CDouble(TxtMoneyExchangePoint.Text);
            //SiteConfig.UserConfig.MoneyExchangeValidDayByMoney = DataConverter.CDouble(TxtMoneyExchangeValidDay.Text);
            //SiteConfig.UserConfig.UserExpExchangePointByExp = DataConverter.CDouble(TxtUserExpExchangePoint.Text);
            //SiteConfig.UserConfig.UserExpExchangeValidDayByExp = DataConverter.CDouble(TxtUserExpExchangeValidDay.Text);
            //SiteConfig.UserConfig.MoneyExchangePointByPoint = DataConverter.CDouble(TxtCMoneyExchangePoint.Text);
            //SiteConfig.UserConfig.MoneyExchangeValidDayByValidDay = DataConverter.CDouble(TxtCMoneyExchangeValidDay.Text);
            //SiteConfig.UserConfig.UserExpExchangePointByPoint = DataConverter.CDouble(TxtCUserExpExchangePoint.Text);
            //SiteConfig.UserConfig.UserExpExchangeValidDayByValidDay = DataConverter.CDouble(TxtCUserExpExchangeValidDay.Text);
            //SiteConfig.UserConfig.MoneyExchangeDummyPurseByDummyPurse = DataConverter.CDouble(txtCMoneyExchangeDummyPurse.Text);
            //SiteConfig.UserConfig.MoneyExchangeDummyPurseByMoney = DataConverter.CDouble(txtMoneyExchangeDummyPurse.Text);
            //SiteConfig.UserConfig.PunchType = DataConverter.CLng(selPunch.Value);
            //SiteConfig.UserConfig.PunchVal = DataConverter.CLng(txtPunch.Text);
            //SiteConfig.UserConfig.PointName = TxtPointName.Text.Trim();
            //SiteConfig.UserConfig.PointUnit = TxtPointUnit.Text.Trim();
            //SiteConfig.UserConfig.PromotionType = DataConverter.CLng(RadioButtonList10.SelectedValue);
            //SiteConfig.UserConfig.Promotion = DataConverter.CLng(txtPromotion.Text);
            SiteConfig.UserConfig.Reg_EmailMust = DataConvert.CBool(RequestEx["Reg_EmailMust"]);
            SiteConfig.UserConfig.Reg_AnswerMust = DataConvert.CBool(RequestEx["Reg_AnswerMust"]);
            SiteConfig.UserConfig.Reg_SelGroup = DataConvert.CBool(RequestEx["Reg_SelGroup"]);
            //SiteConfig.UserConfig.EmailRegInfo = txtEmailRegInfo.Text.Trim();
            //SiteConfig.UserConfig.DisCuzNT = DisCuzNT.Checked;
            //string regrulelist = "";
            //for (int i = 0; i < RegRule.Items.Count; i++)
            //{
            //    if (RegRule.Items[i].Selected)
            //    {
            //        regrulelist = regrulelist + RegRule.Items[i].Value;
            //    }

            //    if (i < RegRule.Items.Count - 1)
            //    {
            //        regrulelist = regrulelist + ",";
            //    }
            //}

            SiteConfig.UserConfig.RegRule = RequestEx["RegRule"];
            SiteConfig.UserConfig.Agreement = RequestEx["Agreement"];
            SiteConfig.UserConfig.UserNavBan = Request.Form["UserNavBan"];
            //SiteConfig.UserConfig.InviteCodeCount = DataConverter.CLng(InviteCode_T.Text);
            //SiteConfig.UserConfig.InviteFormat = InviteFormat_T.Text;
            //SiteConfig.UserConfig.InviteJoinGroup = DataConverter.CLng(InviteJoinGroup_DP.SelectedValue);
            SiteConfig.UserConfig.WD_Min = DataConverter.CLng(RequestEx["WD_Min"], 1);
            SiteConfig.UserConfig.WD_Max = DataConverter.CLng(RequestEx["WD_Max"]);
            SiteConfig.UserConfig.WD_Multi = DataConverter.CLng(RequestEx["WD_Multi"]);
            SiteConfig.UserConfig.WD_FeePrecent = DataConverter.CLng(RequestEx["WD_FeePrecent"]);
            SiteConfig.UserConfig.MaximumUser = DataConverter.CLng(RequestEx["MaximumUser"]);
            SiteConfig.Update();
            return WriteOK("操作成功", "UserConfig");
        }
        public IActionResult SetOrderStatus()
        {
            return View();
        }
        public IActionResult SetOrderStatus_Submit()
        {
            //订单
            OrderConfig.SetOrderStatus((int)M_OrderList.StatusEnum.Normal, GetParam("OrderNormal_T"));
            //OrderConfig.SetOrderStatus((int)M_OrderList.StatusEnum.Sured, OrderSured_T.Text);
            OrderConfig.SetOrderStatus((int)M_OrderList.StatusEnum.OrderFinish, GetParam("OrderFinish_T"));
            //OrderConfig.SetOrderStatus((int)M_OrderList.StatusEnum.UnitFinish, UnitFinish_T.Text);
            OrderConfig.SetOrderStatus(2, GetParam("OrderDealIng_T"));
            OrderConfig.SetOrderStatus((int)M_OrderList.StatusEnum.DrawBack,GetParam("DrawBack_T"));
            OrderConfig.SetOrderStatus((int)M_OrderList.StatusEnum.UnDrawBack, GetParam("UnDrawBack_T"));
            OrderConfig.SetOrderStatus((int)M_OrderList.StatusEnum.CheckDrawBack, GetParam("CheckDrawBack_T"));
            OrderConfig.SetOrderStatus((int)M_OrderList.StatusEnum.CancelOrder,GetParam("CancelOrder_T"));
            OrderConfig.SetOrderStatus((int)M_OrderList.StatusEnum.Recycle, GetParam("Recycle_T"));
            //物流
            OrderConfig.SetExpStatus(0, GetParam("UnDelivery_T"));
            OrderConfig.SetExpStatus(1, GetParam("Delivery_T"));
            OrderConfig.SetExpStatus(2, GetParam("Signed_T"));
            OrderConfig.SetExpStatus(-1, GetParam("UnSiged_T"));
            //支付状态
            OrderConfig.SetPayStatus((int)M_OrderList.PayEnum.NoPay, GetParam("NoPay_T"));
            OrderConfig.SetPayStatus((int)M_OrderList.PayEnum.HasPayed, GetParam("HasPayed_T"));
            OrderConfig.SetPayStatus((int)M_OrderList.PayEnum.RequestRefund, GetParam("RequestRefund_T"));
            OrderConfig.SetPayStatus((int)M_OrderList.PayEnum.Refunded, GetParam("Refunded_T"));
            OrderConfig.SetPayStatus((int)M_OrderList.PayEnum.SurePayed,GetParam("SurePayed_T"));
            //支付方式
            OrderConfig.SetPayType((int)M_OrderList.PayTypeEnum.Normal, GetParam("PayNormal_T"));
            OrderConfig.SetPayType((int)M_OrderList.PayTypeEnum.PrePay, GetParam("PrePay_T"));
            OrderConfig.SetPayType((int)M_OrderList.PayTypeEnum.HelpReceive,GetParam("HelpReceive_T"));

            OrderConfig.Update();
            return WriteOK("保存成功!");
        }
        public IActionResult SMSCfg()
        {
            return View();
        }
        public IActionResult SMSConfig_Submit()
        {
            //北京网通
            SMSConfig.Instance.G_eid = GetParam("txtg_eid");
            SMSConfig.Instance.G_uid = GetParam("txtg_uid");
            SMSConfig.Instance.G_pwd = GetParam("txtg_pwd");
            SMSConfig.Instance.G_gate_id = GetParam("txt_h_gate_id");
            //深圳电信
            SMSConfig.Instance.MssUser = GetParam("TxtMssUser");
            SMSConfig.Instance.MssPsw = GetParam("TxtMssPsw");

            //北京亿美
            SMSConfig.Instance.sms_key = GetParam("smskeyT");
            SMSConfig.Instance.sms_pwd = GetParam("smspwdT");
            SMSConfig.Instance.DefaultSMS = GetParam("ddlMessageCheck_DP");
            SMSConfig.Instance.Tlp_Reg = GetParam("Tlp_Reg");
            SMSConfig.Instance.Tlp_GetBack = GetParam("Tlp_GetBack");
            SMSConfig.Instance.Tlp_ChangeMobile = GetParam("Tlp_ChangeMobile");

            SMSConfig.Instance.QCloud_APPID = GetParam("QC_APPID");
            SMSConfig.Instance.QCloud_APPKey = GetParam("QC_APPKey");
            SMSConfig.Instance.QCloud_Sign = GetParam("QC_Sign");


            SiteConfig.SiteOption.MaxMobileMsg = DataConverter.CLng(GetParam("MaxPhoneMsg"));
            SiteConfig.SiteOption.MaxIpMsg = DataConverter.CLng(GetParam("MaxIpMsg"));
            SiteConfig.UserConfig.UserMobilAuth = GetParam("userMobilAuth");
            SMSConfig.Update();
            SiteConfig.Update();
            return WriteOK("操作成功", "SMSCfg");
        }
        //--------------
        private string[] colorArr = new string[] { "#852b99", "#4B7F8C", "#1E86EA", "#ffb848", "#00CCFF", "#FE7906", "#004B9B", "#74B512", "#A43AE3", "#22AFC2", "#808081", "#F874A4" };
        public IActionResult SearchFunc()
        {
            PageSetting setting = shBll.SelPage(CPage, PSize, new F_Search()
            {
                path = SiteConfig.SiteOption.ManageDir,
                type = DataConvert.CLng(GetParam("type"),1),
                state = DataConvert.CLng(GetParam("state"))
            });
            if (Request.IsAjax())
            {
                return PartialView("SearchFunc_List", setting);
            }
            else
            {
                return View(setting);
            }
        }
        public ContentResult SearchFunc_API()
        {
            switch (action)
            {
                case "del":
                    shBll.DelByIDS(ids);
                    break;
                case "order":
                    {
                        //mid,oid,nid,wid
                        string[] wid = GetParam("order_T").Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);//需要更换成的ID
                        string[] ids = GetParam("order_Hid").Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);//信息描述
                        for (int i = 0; i < wid.Length; i++)
                        {
                            //if (wid[i] == "")
                            //{
                            //    function.Script(this, "alert('排序不能为空!');");
                            //    return;
                            //}
                            int wantPid = DataConverter.CLng(wid[i]);
                            int mid = DataConverter.CLng(ids[i].Split(':')[0]);
                            int oid = DataConverter.CLng(ids[i].Split(':')[1]);
                            int nowPid = DataConverter.CLng(ids[i].Split(':')[2]);
                            if (wantPid == nowPid) continue;//没有修改排序值
                            else
                            {
                                shBll.UpdateOrder(mid, wantPid);
                            }// if end;
                        }//for end;
                    }
                    break;
            }
            return Content(Success.ToString());
        }
        public IActionResult SearchFuncAdd() {
            M_Search shMod = new M_Search();
            if (Mid > 0)
            {
                shMod = shBll.GetSearchById(Mid);
            
            }
            else
            {
                shMod.BKColor = colorArr[new Random().Next(colorArr.Length - 1)];
            }
            return View(shMod);
        }
        public IActionResult SearchFuncAdd_Submit(M_Search model)
        {
            M_Search shMod = new M_Search();
            if (Mid > 0) { shMod = shBll.GetSearchById(Mid); }
            shMod.Name = model.Name;
            shMod.FlieUrl = model.FlieUrl;
            shMod.Ico = GetParam("ItemIcon_T");
            shMod.Mobile = model.Mobile;
            shMod.Size = model.Size;
            shMod.OpenType = model.OpenType;
            shMod.BKColor = model.BKColor;
            if (Mid > 0)
            {
                shBll.UpdateByID(shMod);
            }
            else
            {
                shMod.AdminID = adminMod.AdminId;
                shMod.LinkState = 2;
                shMod.OrderID = shBll.SelMaxOrder() + 1;
                shBll.insert(shMod);
            }
            string pageUrl = shMod.Type == 1 ? "SearchFunc?EliteLevel=" + shMod.EliteLevel : "SearchFunc";
            return WriteOK("操作成功", pageUrl);
        }
        public IActionResult FontIcon() { return View(); }
    }
}