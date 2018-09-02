using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(cis237_assignment6.Startup))]
namespace cis237_assignment6
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
