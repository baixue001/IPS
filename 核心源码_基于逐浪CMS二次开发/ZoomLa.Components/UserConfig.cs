namespace ZoomLa.Components
{
    using System;
    [Serializable]
    public class UserConfig
    {
        #region 货币相关
        public int PresentPointAll
        {
            get;
            set;
        }
        public int PresentPointTg
        {
            get;
            set;
        }

        public double UserExpExchangePoint
        {
            get
            {
                return (this.UserExpExchangePointByExp / this.UserExpExchangePointByPoint);
            }
        }

        public double UserExpExchangePointByExp
        {
            get; set;
        }

        public double UserExpExchangePointByPoint
        {
            get; set;
        }

        public double UserExpExchangeValidDay
        {
            get
            {
                return (this.UserExpExchangeValidDayByExp / this.UserExpExchangeValidDayByValidDay);
            }
        }

        public double UserExpExchangeValidDayByExp
        {
            get; set;
        }

        public double UserExpExchangeValidDayByValidDay
        {
            get; set;
        }
        /// <summary>
        /// 兑换银币的积分
        /// </summary>
        public double ChangeSilverCoinByExp
        {
            get; set;
        }
        /// <summary>
        /// 积分兑换银币
        /// </summary>
        public double PointSilverCoin
        {
            get; set;
        }
        /// <summary>
        /// 兑换积分
        /// </summary>
        public double PointExp
        {
            get; set;
        }
        public double PresentExp
        {
            get; set;
        }
        public double PresentExpPerLogin
        {
            get; set;
        }
        public double SigninPurse { get; set; }
        public double PresentMoney
        {
            get; set;
        }
        public int PresentPoint
        {
            get; set;
        }
        public int PresentValidNum
        {
            get; set;
        }
        public double Integral
        {
            get; set;

        }
        public int PresentValidUnit
        {
            get; set;
        }
        public double MoneyExchangePoint
        {
            get
            {
                return (this.MoneyExchangePointByMoney / this.MoneyExchangePointByPoint);
            }
        }
        public double MoneyExchangePointByMoney
        {
            get; set;
        }
        public double MoneyExchangePointByPoint
        {
            get; set;
        }
        public double MoneyExchangeValidDay
        {
            get
            {
                return (this.MoneyExchangeValidDayByMoney / this.MoneyExchangeValidDayByValidDay);
            }
        }
        public double MoneyExchangeDummyPurseByMoney
        {
            get; set;
        }
        public double MoneyExchangeDummyPurseByDummyPurse
        {
            get; set;
        }
        public double MoneyExchangeValidDayByMoney
        {
            get; set;
        }
        public double MoneyExchangeValidDayByValidDay
        {
            get; set;
        }
        public string PointName
        {
            get; set;
        }
        public string PointUnit
        {
            get; set;
        }
        /// <summary>
        /// 积分兑换金钱
        /// </summary>
        public double PointMoney
        {
            get; set;
        }
        #endregion
        public double IntegralPercentage
        {
            get;
            set;
        }
        /// <summary>
        /// 用户邀请码最大数量
        /// </summary>
        public int InviteCodeCount { get; set; }
        private string _invite_format="";
        public string InviteFormat { get { return string.IsNullOrEmpty(_invite_format) ? "{0000000AAA}" : _invite_format; } set { _invite_format = value; } }
        public int InviteJoinGroup { get; set; }
        /// <summary>
        /// 可使用站内短信功能的用户组
        /// </summary>
        private string m_MessageGroup;
        public bool AdminCheckReg
        {
            get;
            set;
        }
        public bool EmailCheckReg
        {
            get;
            set;
        }
        /// <summary>
        /// 是否开启Email注册登录
        /// </summary>
        public bool EmailRegis
        {
            get; set;
        }
        //注册成功邮件提醒
        public bool EmailTell
        {
            get; set;
        }
        public string EmailPlatReg
        {
            get; set;
        }
        public string MobileRegInfo
        {
            get; set;
        }
        /// <summary>
        /// 0:三次,1:开启,2:不限制
        /// </summary>
        public string EnableCheckCodeOfLogin
        {
            get;set;
        }
        public bool EnableCheckCodeOfReg
        {
            get; set;
        }
        public bool EnableMultiLogin
        {
            get; set;
        }
        public bool EnableMultiRegPerEmail
        {
            get; set;
        }
        public bool EnableQAofReg
        {
            get; set;
        }
        public bool EnableRegCompany
        {
            get; set;
        }
        /// <summary>
        /// 是否开启会员注册
        /// </summary>
        public bool EnableUserReg
        {
            get; set;
        }
        public int GroupId
        {
            get; set;
        }
        public string RegFieldsMustFill
        {
            get; set;
        }
        /// <summary>
        /// 注册时选填项目
        /// </summary>
        public string RegFieldsSelectFill
        {
            get; set;
        }
        public int UserGetPasswordType
        {
            get; set;
        }
        /// <summary>
        /// 新会员注册时用户名最少字符数
        /// </summary>
        public int UserNameLimit
        {
            get; set;
        }
        public int UserNameMax
        {
            get; set;
        }
        /// <summary>
        /// 禁止注册的用户名
        /// </summary>
        public string UserNameRegDisabled
        {
            get; set;
        }
        /// <summary>
        /// 评论积分赠送
        /// </summary>
        public int CommentRule
        {
            get; set;
        }
        public int InfoRule
        {
            get; set;
        }
        public int RecommandRule
        {
            get; set;
        }
        public int LoginRule
        {
            get; set;
        }
        /// <summary>
        /// true不需要验证,false需要验证
        /// </summary>
        public bool UserValidateType
        {
            get;set;
        }
        /// <summary>
        /// 可使用站内短信用户组
        /// </summary>
        public string MessageGroup
        {
            get { return this.m_MessageGroup; }
            set { this.m_MessageGroup = value; }
        }
        /// <summary>
        /// 推广获得类型
        /// </summary>
        public int PromotionType
        {
            get; set;
        }
        /// <summary>
        /// 推广点数
        /// </summary>
        public int Promotion
        {
            get; set;
        }
        /// <summary>
        /// 注册用户名规则
        /// </summary>
        public string RegRule
        {
            get; set;
        }
        /// <summary>
        /// 注册协议显示方式
        /// </summary>
        public string Agreement
        {
            get; set;
        }
        /// <summary>
        /// 是否启用支付宝登录
        /// </summary>
        public bool EnableAlipayCheckReg
        {
            get;
            set;
        }
        /// <summary>
        /// 打卡奖励领取的类型:0为不奖励,1为金额,2为虚拟币,3为积分,4为点劵
        /// </summary>
        public int PunchType
        {
            get; set;
        }

        /// <summary>
        /// 打卡奖励领取的值
        /// </summary>
        public int PunchVal
        {
            get; set;
        }
       
        //是否开启UserID登录
        public bool UserIDlgn
        {
            get; set;
        }
        //是否开启手机注册
        public bool MobileReg
        {
            get; set;
        }
        private int _mobileCodeNum = 4;
        //手机验证码位数
        public int MobileCodeNum { get { if (_mobileCodeNum < 4) { _mobileCodeNum = 4; } return _mobileCodeNum; } set { _mobileCodeNum = value; } }
        /// <summary>
        /// 验证码生成规则 0:数字,1:字母,2:数字+字母
        /// </summary>
        public int MobileCodeType { get; set; }
        //0:用户必须手机验证才能修改,1:修改用户自由修改手机号
        public string UserMobilAuth { get; set; }
        /// <summary>
        /// 注册时邮箱是否必填
        /// </summary>
        public bool Reg_EmailMust { get; set; }
        /// <summary>
        /// 注册问答是否必填
        /// </summary>
        public bool Reg_AnswerMust { get; set; }
        /// <summary>
        /// 注册可选会员组
        /// </summary>
        public bool Reg_SelGroup { get; set; }
        /// <summary>
        /// 禁止哪些菜单显示(新加菜单或修改后不被禁止)
        /// </summary>
        public string UserNavBan { get; set; }
        //--------------------
        /// <summary>
        /// 单笔最小提现金额
        /// </summary>
        public int WD_Min { get; set; }
        /// <summary>
        /// 单笔提取最大提现金额
        /// </summary>
        public int WD_Max { get; set; }
        /// <summary>
        /// 提现金额倍数
        /// </summary>
        public int WD_Multi { get; set; }
        /// <summary>
        /// 提现手续费率
        /// </summary>
        public double WD_FeePrecent { get; set; } 
        /// <summary>
        /// 最大用户数
        /// </summary>
        public int MaximumUser { get; set; }
    }
}