using System.Reflection;
using System.ServiceProcess;

namespace printServer
{
    class WinService : ServiceBase
    {

        public static Server HttpServer = new Server(int.Parse(Program.Config("port")));

        public WinService()
        {
            this.ServiceName = Assembly.GetExecutingAssembly().GetName().Name;
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            CoreStart();
        }

        public static void CoreStart()
        {
            HttpServer.Start();
        }

        protected override void OnStop()
        {
            try
            {
                HttpServer.Stop();
            }
            catch { }
            base.OnStop();
        }
    }
}
