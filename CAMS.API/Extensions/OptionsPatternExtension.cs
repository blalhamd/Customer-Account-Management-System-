using CAMS.Core.Constants;

namespace CAMS.API.Extensions
{
    public static class OptionsPatternExtension
    {
        public static IServiceCollection OptionsPatternConfig(this IServiceCollection services, IConfiguration configuration)
        {

            // Configure the JwtSetting options from the configuration

            services.Configure<JwtSetting>(configuration.GetSection(JwtSetting.SectionName));

            // Validate the configuration settings on start

            services.AddOptions<JwtSetting>(configuration[JwtSetting.SectionName])
                            .ValidateDataAnnotations()
                            .ValidateOnStart();


            // Configure the MailSettings options from the configuration

            services.Configure<MailSettings>(configuration.GetSection(MailSettings.SectionName));

            // Validate the configuration settings on start

            services.AddOptions<MailSettings>(configuration[MailSettings.SectionName])
                            .ValidateDataAnnotations()
                            .ValidateOnStart();

            return services;
        }
    }
}
