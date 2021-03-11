using System;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Infrastructure
{
    public class ExternalUrl
    {
        /// <summary>
        /// JKCore.WeiXin.Api
        /// </summary>
        public string HttpClientBaseAddress { get; set; }
        public string WxTemplateAddress { get; set; }
        public string IdentityAddress { get; set; }
        public string FakeLoginUrl { get; set; }

        public string OAuthSucessAddress { get; set; }

        public string GetOpenIdAddress { get; set; }

        public string WxPayNotifyAddress { get; set; }
        public string SignTemplateUrl { get; set; }
        public string MemberClientPage { get; set; }
        /// <summary>
        /// 微信退款证书路径-非具体路径
        /// </summary>
        public string WxRefundCertPath { get; set; }

        public string GetMchInfoAddress { get; set; }
       
        /// <summary>
        /// 微信API项目路径
        /// </summary>
        public string WxApiAddress { get; set; }
        /// <summary>
        /// 微信服务商分账地址
        /// </summary>
        /// <value></value>
        public string WxProfitsharingAddress{get;set;}
    }
}
