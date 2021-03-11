using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JKCore.Infrastructure
{
    /// <summary>
    /// 反序列decimal
    /// </summary>
    public class DecimalConvert : CustomCreationConverter<decimal>
    {/// <summary>
        /// 序列化后保留小数位数
        /// </summary>
        public virtual int Digits { get; private set; }

        /// <summary>
        /// .ctor
        /// </summary>
        public DecimalConvert()
        {
            this.Digits = 2;
        }

        /// <summary>
        /// .ctor
        /// </summary>
        /// <param name="digits">序列化后保留小数位数</param>
        public DecimalConvert(int digits)
        {
            this.Digits = digits;
        }

        /// <summary>
        /// 重载是否可写
        /// </summary>
        public override bool CanWrite { get { return true; } }

        /// <summary>
        /// 重载创建方法
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public override decimal Create(Type objectType)
        {
            return 0;
        }

        /// <summary>
        /// 重载序列化方法
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="serializer"></param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                var formatter = (Convert.ToDecimal(value)).ToString("f" + Digits.ToString());
                writer.WriteValue(formatter);
            }

        }
    }
}
