using System;
using System.Text;
using System.Threading;

namespace SpeedRunners.Scheduler
{
    internal class Program
    {
        private static readonly LogHelper Log = LogHelper.GetCurrentClassLogHelper();
        private static readonly TimeSpan UpdateSteamStateRetryDelay = TimeSpan.FromSeconds(30);

        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Task task = new Task();
            task.Execute();

            while (true)
            {
                try
                {
                    task.UpdateSteamState().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    Log.Error("UpdateSteamState failed; retrying after delay.", ex);
                    Console.Error.WriteLine($"UpdateSteamState failed at {DateTime.Now}: {ex}");
                    Thread.Sleep(UpdateSteamStateRetryDelay);
                }
            }
        }
    }
}
