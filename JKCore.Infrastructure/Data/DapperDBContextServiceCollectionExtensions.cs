using JKCore.Domain.IData;
using JKCore.Infrastructure.DBContext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JKCore.Infrastructure.Data
{
    public static class DapperDBContextServiceCollectionExtensions
    {
        public static IServiceCollection AddDapperDBContext(this IServiceCollection services, IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddScoped(s => new JiakeDapperDBContext(configuration["DBConfig:JiakeConfiguration"]));
            services.AddScoped<IJiakeUnitOfWorkFactory, JiakeDapperUnitOfWorkFactory>();
            return services;
        }
    }
}
