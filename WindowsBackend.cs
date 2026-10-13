using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace TailscaleRouting
{
    // Deliberately read-only: the official Tailscale client owns connections and exit nodes.
    public interface IBackend { }
    public sealed class WindowsBackend : INativeRoutingBackend
    {
        public NativeRoutingPlan ReadRoutingPlan()
        {
            foreach (var adapter in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                if (adapter.Name == "Switcher-Routing" && adapter.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                    throw new InvalidOperationException("Останови маршрутизацию в Switcher перед запуском Tailscale Routing.");
            return NativeRoutingPlan.Detect(Run(FindTailscale(), "status --json", 8000));
        }
        public static string FindTailscale()
        {
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Tailscale", "tailscale.exe");
            if (File.Exists(path)) return path;
            using (var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tailscale"))
            {
                string command = key == null ? "" : Convert.ToString(key.GetValue("ImagePath"));
                var match = System.Text.RegularExpressions.Regex.Match(command, "^\\s*(?:\"([^\"]+\\.exe)\"|(.+?\\.exe)(?:\\s|$))", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string service = Environment.ExpandEnvironmentVariables(match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value);
                    path = Path.Combine(Path.GetDirectoryName(service), "tailscale.exe");
                    if (File.Exists(path)) return path;
                }
            }
            throw new FileNotFoundException("Tailscale не найден. Установи официальный клиент Tailscale, подключись и выбери exit node.");
        }
        public static string Run(string file, string args, int timeout)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo(file, args) { UseShellExecute = false, CreateNoWindow = true,
                    RedirectStandardOutput = true, RedirectStandardError = true, WorkingDirectory = Path.GetDirectoryName(file) };
                process.Start();
                var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
                if (!process.WaitForExit(timeout))
                {
                    try { process.Kill(); process.WaitForExit(3000); } catch (InvalidOperationException) { }
                    throw new TimeoutException("Истекло время ожидания: " + Path.GetFileName(file));
                }
                if (!Task.WaitAll(new Task[] { stdout, stderr }, 3000)) throw new TimeoutException("Не удалось прочитать ответ программы.");
                if (process.ExitCode != 0) throw new InvalidOperationException(Path.GetFileName(file) + ": " + stderr.Result.Trim() + " " + stdout.Result.Trim());
                return stdout.Result;
            }
        }
    }
    public sealed class RoutingHealthPolicy
    {
        DateTime firstFailure; int failures;
        public bool Observe(bool reachable, DateTime now)
        {
            if (reachable) { failures = 0; firstFailure = DateTime.MinValue; return true; }
            if (failures++ == 0) firstFailure = now;
            return failures < 3 || now - firstFailure < TimeSpan.FromSeconds(30);
        }
    }
}
