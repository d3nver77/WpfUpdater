using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(UpdaterAsp.Startup))]
namespace UpdaterAsp
{
    public partial class Startup {
        public void Configuration(IAppBuilder app) {
            ConfigureAuth(app);
        }
    }
}
