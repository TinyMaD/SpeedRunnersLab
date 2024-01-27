using System;
using System.Text;
using System.Threading;

namespace SpeedRunners.Scheduler
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            new Task().Execute();

            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
        }
    }
}
