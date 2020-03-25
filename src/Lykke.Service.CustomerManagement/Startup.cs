using System;
using System.Text.RegularExpressions;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Logs.Loggers.LykkeSanitizing;
using Lykke.Sdk;
using Lykke.Service.CustomerManagement.AutoMapperProfiles;
using Lykke.Service.CustomerManagement.DomainServices.AutoMapperProfiles;
using Lykke.Service.CustomerManagement.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.CustomerManagement
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "CustomerManagement API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Extend = (collection, manager) =>
                {
                    collection.Configure<ApiBehaviorOptions>(apiBehaviorOptions =>
                    {
                        apiBehaviorOptions.SuppressModelStateInvalidFilter = true;
                    });
                    collection.AddAutoMapper(typeof(AutoMapperProfile), typeof(VerificationEmailAutoMapperProfile));
                };

                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "CustomerManagementLog";
                    logs.AzureTableConnectionStringResolver = settings => settings.CustomerManagementService.Db.LogsConnString;
                    logs.Extended = x => x.AddSanitizingFilter(new Regex(@"(\\?""?[Pp]assword\\?""?:\s*\\?"")(.*?)(\\?"")"), "$1*$3")
                        .AddSanitizingFilter(new Regex(@"(\\?""?[Ll]ogin\\?""?:\s*\\?"")(.*?)(\\?"")"), "$1*$3")
                        .AddSanitizingFilter(new Regex(@"(\\?""?[Ee]mail\\?""?:\s*\\?"")(.*?)(\\?"")"), "$1*$3");
                };
            });
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IMapper mapper)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;
            });

            mapper.ConfigurationProvider.AssertConfigurationIsValid();
        }
    }
}
