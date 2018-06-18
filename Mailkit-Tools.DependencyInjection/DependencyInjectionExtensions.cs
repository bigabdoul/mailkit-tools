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
                .AddTransient<IEmailConfigurationProvider, TFactory>()
                .AddTransient<IEmailClientService, EmailClientService>()
                .AddTransient<IPop3ClientService, Pop3ClientService>();
        }
        
    }
}
