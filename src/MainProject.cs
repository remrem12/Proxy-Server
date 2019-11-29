using System;
using System.Net;
using System.Threading;

namespace ProjectSocket
{
    public class MainProject
    {
        private static void Main(string[] args)
        {
            HttpListener Listener = new HttpListener(); //Tạo biến listener để lắng nghe request

            Listener.Prefixes.Add("http://127.0.0.1:8888/"); //Khởi tạo cấu hình IP và Port
            Listener.Start();

            Console.WriteLine("Proxy is listening for request...");

            while (true)
            {
                HttpListenerContext context = Listener.GetContext();
                Process newPrs = new Process(context);
                new Thread(newPrs.ProcessRequest).Start();
            }
        }
    }
}

