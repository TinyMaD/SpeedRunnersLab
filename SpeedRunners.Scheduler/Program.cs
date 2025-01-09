using System.Text;

namespace SpeedRunners.Scheduler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Task task = new Task();
            task.Execute();

            while (true)
            {
                task.UpdateSteamState().Wait();
            }
        }
    }
}
