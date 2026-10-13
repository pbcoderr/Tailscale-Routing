using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
[assembly: AssemblyTitle("Tailscale Routing")]
[assembly: AssemblyProduct("Tailscale Routing")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
namespace TailscaleRouting
{
    static class Program
    {
        [STAThread] static void Main()
        {
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            bool first;
            using(var mutex=new Mutex(true,@"Global\TailscaleRouting-Desktop",out first))
            {
                if(!first) { MessageBox.Show("Tailscale Routing уже работает. Открой окно через значок в трее.","Tailscale Routing"); return; }
                try
                {
                    string root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"TailscaleRouting");
                    Directory.CreateDirectory(root);
                    using(var controller=new RoutingController(root))
                    using(var form=new RoutingForm(controller,new WindowsBackend())) Application.Run(form);
                }
                catch(Exception ex) { MessageBox.Show(ex.Message,"Tailscale Routing",MessageBoxButtons.OK,MessageBoxIcon.Error); }
                finally { mutex.ReleaseMutex(); }
            }
        }
    }
}
