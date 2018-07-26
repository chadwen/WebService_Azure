using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebService_Azure.Startup))]
namespace WebService_Azure
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
