using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Tree.Startup))]
namespace Tree
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

    }
}
