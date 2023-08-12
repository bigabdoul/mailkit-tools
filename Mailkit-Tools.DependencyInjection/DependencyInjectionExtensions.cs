using MailkitTools.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MailkitTools.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding services to a <see cref="IServiceCollection"/>.
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Adds MailkitTools services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <typeparam name="TFactory">The concrete type that implements the <see cref="IEmailConfigurationProvider"/> interface.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>A reference to the specified <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMailkitTools<TFactory>(this IServiceCollection services)
            where TFactory : class, IEmailConfigurationProvider
        {
            // email client configuration support
            return services
                .AddTransient<IEmailSender, EmailSender>()
                .AddTransient<IEmailConfigurationProvider, TFactory>()
                .AddTransient<IEmailClientService, EmailClientService>()
                .AddTransient<IPop3ClientService, Pop3ClientService>();
        }

        /// <summary>
        /// Adds MailkitTools services to the specified <see cref="IServiceCollection"/> using the
        /// <see cref="DefaultEmailConfigurationProvider"/> implementation factory.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <returns>A reference to the specified <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMailkitTools(this IServiceCollection services)
            => services.AddMailkitTools<DefaultEmailConfigurationProvider>();

        /// <summary>
        /// Adds MailkitTools services to the specified <see cref="IServiceCollection"/> using the
        /// <see cref="DefaultEmailConfigurationProvider"/> implementation factory and the specified
        /// <see cref="IEmailClientConfiguration"/> configuration. This method also adds a transient
        /// service for the <see cref="IConfiguredEmailService"/> provider.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
        /// <param name="config">The default email client configuration to add as a singleton service.</param>
        /// <returns>A reference to the specified <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddMailkitTools(this IServiceCollection services, IEmailClientConfiguration config)
        {
            return services.AddSingleton(config)
                .AddTransient<IConfiguredEmailService, ConfiguredEmailService>()
                .AddMailkitTools<DefaultEmailConfigurationProvider>();
        }
    }
}
