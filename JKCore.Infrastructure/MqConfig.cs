using System;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Infrastructure
{
    /// <summary>
    /// rabbitmq 配置信息
    /// </summary>
    public class MqConfig
    {
        public string MqPwd { get; set; }
        public string MqHost { get; set; }
        public string MqUserName { get; set; }
        public string MqExchangeName { get; set; }
    }
}
