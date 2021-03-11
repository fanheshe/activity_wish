using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JKCore.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Senparc.CO2NET;
using Senparc.CO2NET.AspNet;
using Senparc.Weixin;
using Senparc.Weixin.Entities;
using Senparc.Weixin.RegisterServices;
using Senparc.Weixin.TenPay;
using Coravel;
using JKCore.Infrastructure;
using JKCore.Domain.IRepository;
using JKCore.Infrastructure.Repository;
using Senparc.Weixin.MP;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Senparc.Weixin.WxOpen;
using JKCore.Activity.Api.Invocable;

namespace JKCore.Activity.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<DBConfig>(Configuration.GetSection("DBConfig"));
            services.AddControllers();

            //.net core 3.1 senparc.weixin.pay配置可以同步请求读取流数据
            services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true);

            services.AddDapperDBContext(Configuration);
            services.AddTransient(typeof(IJiakeRepository<>), typeof(BaseJiakeRepository<>));
            //序列化
            services.AddControllersWithViews().AddNewtonsoftJson(options =>
            {
                //忽略循环引用
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                //使用小驼峰样式的key
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                //设置时间格式
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
            });
            //批量注册Application
            var assembly = Assembly.Load("Activity.Application")
                        .DefinedTypes
                        .Where(a => a.Name.EndsWith("Application") && !a.Name.StartsWith("I"));

            foreach (var item in assembly)
            {
                services.AddTransient(item.GetInterfaces().FirstOrDefault(), item);
            }

            //Senparc.Weixin 注册（必须）
            services.AddSenparcWeixinServices(Configuration);
            // coravel
            services.AddCache();
            services.AddTransient<ActivityCloseInvocable>();
            services.AddTransient<ActivityMchWishInvocable>();
            services.AddTransient<ActivityFakeInvocable>();
            services.AddTransient<ActivityNoExchangeInvocable>();
            services.AddTransient<CronJobParam>();
            services.AddScheduler();
            // coravel
            services.AddHttpClient("default");
            services.AddMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<SenparcSetting> senparcSetting, IOptions<SenparcWeixinSetting> senparcWeixinSetting)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseStaticFiles();

            // 启动 CO2NET 全局注册，必须！
            // 关于 UseSenparcGlobal() 的更多用法见 CO2NET Demo：https://github.com/Senparc/Senparc.CO2NET/blob/master/Sample/Senparc.CO2NET.Sample.netcore3/Startup.cs
            app.UseSenparcGlobal(env, senparcSetting.Value, globalRegister => { })
            //使用 Senparc.Weixin SDK
            .UseSenparcWeixin(senparcWeixinSetting.Value, weixinRegister =>
            {
                weixinRegister

                #region 注册公众号或小程序（按需）

                            .RegisterMpAccount(senparcWeixinSetting.Value, "【月野兔】公众号")// DPBMARK_END

                            //注册多个公众号或小程序（可注册多个）                                        -- DPBMARK MiniProgram
                            .RegisterWxOpenAccount(senparcWeixinSetting.Value, "【月野兔】小程序");// DPBMARK_END

                #endregion 注册公众号或小程序（按需）
            });

            app.UseAuthorization();

            // 定时任务
            var provider = app.ApplicationServices;
            provider.UseScheduler(scheduler =>
            {
                Console.WriteLine("---- Start CronJob ----");
                //活动结束，许愿中状态变更为：未达成-活动结束取消（每天执行，内部判断日期）----- 活动开始，发活动开始提醒模板消息
                scheduler.Schedule<ActivityCloseInvocable>().Cron("0 2 * * *").PreventOverlapping("ActivityCloseInvocable");

                //活动第三天上午十点，所有商家统一增加152值（每天执行，内部判断日期）
                scheduler.Schedule<ActivityMchWishInvocable>().Cron("0 2 * * *").PreventOverlapping("ActivityMchWishInvocable");

                //每2分钟更新，增加随机数1-2，第2-4天每2分钟更新，增加随机数3-4，第5-7天，每2分钟更新，增加随机数1-2 （每2分钟执行一次）
                scheduler.Schedule<ActivityFakeInvocable>().Cron("*/2 * * * *").PreventOverlapping("ActivityFakeInvocable");

                //中午12:30发送未兑现愿望提醒，次日中午12:30发送提醒，第四日仍未领取12:30发送，领取后不再发送。
                scheduler.Schedule<ActivityNoExchangeInvocable>().Cron("30 4 * * *").PreventOverlapping("ActivityNoExchangeInvocable");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
