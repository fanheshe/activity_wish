using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace JKCore.Infrastructure
{
    public class CustomTimeSpanConvert : CustomCreationConverter<TimeSpan>
    {
        public CustomTimeSpanConvert()
        {

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
        public override TimeSpan Create(Type objectType)
        {
            return TimeSpan.Zero;
        }


        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
            }
            else
            {
                if (value.ToString() == "23:59:59")
                {
                    writer.WriteValue("24:00");
                }
                else
                {
                    var formatter = value.ToString().Substring(0, 5);
                    writer.WriteValue(formatter);
                }
                
            }
        }


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value.ToString() == "24:00")
            {
                return TimeSpan.Parse("23:59:59");
            }
            else
            {
                return TimeSpan.Parse(reader.Value.ToString());
            }
            
        }
    }

    /// <summary>
    /// 将时间序列化为yyyy-MM-dd格式
    /// </summary>
    public class ConvertToDay : IsoDateTimeConverter
    {
        public ConvertToDay() : base()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }
    }

    /// <summary>
    /// 将时间序列化为yyyy-MM-dd HH:mm格式
    /// </summary>
    public class ConvertToDayHourMinute : IsoDateTimeConverter
    {
        public ConvertToDayHourMinute() : base()
        {
            DateTimeFormat = "yyyy-MM-dd HH:mm";
        }
    }

    /// <summary>
    /// 将时间序列化为yyyy-MM-dd HH:mm:ss格式
    /// </summary>
    public class ConvertToDayHourMinuteSecond : IsoDateTimeConverter
    {
        public ConvertToDayHourMinuteSecond() : base()
        {
            DateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        }
    }

    /// <summary>
    /// 将时间序列化为yyyy年MM月dd日 HH:mm格式
    /// </summary>
    public class ConvertToDayHourMinuteChinese : IsoDateTimeConverter
    {
        public ConvertToDayHourMinuteChinese() : base()
        {
            DateTimeFormat = "yyyy年MM月dd日 HH:mm";
        }
    }

    /// <summary>
    /// 将时间序列化为HH:mm:ss格式
    /// </summary>
    public class ConvertToTime : IsoDateTimeConverter
    {
        public ConvertToTime() : base()
        {
            DateTimeFormat = "HH:mm:ss";
        }
    }
    /// <summary>
    /// 将时间序列化为HH:mm:ss格式
    /// </summary>
    public class ConvertToHour : IsoDateTimeConverter
    {
        public ConvertToHour() : base()
        {
            DateTimeFormat = "HH:mm";
        }
    }
}
