using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(AKV.ExamEnq.Startup))]
namespace AKV.ExamEnq
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
