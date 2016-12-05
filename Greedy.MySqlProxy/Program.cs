using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Greedy.MySqlProxy
{
    class Program
    {
        static CancellationTokenSource tokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            var ip = IPAddress.Parse(MySqlProxyConfig.Server.IP);
            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(ip, MySqlProxyConfig.Server.Port));  //绑定IP地址：端口  
            serverSocket.Listen(MySqlProxyConfig.Server.Backlog);    //设定最多10个排队连接请求  
            Console.WriteLine("启动监听{0}成功", serverSocket.LocalEndPoint.ToString());
            //通过Clientsoket发送数据  
            var mainTask = new Task(ListenClientConnect, serverSocket, tokenSource.Token);
            mainTask.Start();
            Console.ReadLine();
        }

        /// <summary>  
        /// 监听客户端连接  
        /// </summary>  
        private static void ListenClientConnect(object serverSocket)
        {
            Socket myServerSocket = (Socket)serverSocket;
            while (!tokenSource.IsCancellationRequested)
            {
                Socket clientSocket = myServerSocket.Accept();

                var schedulerTask = new Task((state) =>
                {
                    var socket = state as Socket;
                    var scheduler = new Scheduler();
                    scheduler.Invoke(socket);
                }, clientSocket, tokenSource.Token);
                schedulerTask.Start();
                //clientSocket.Send(Encoding.ASCII.GetBytes("Server Say Hello"));
                //Thread receiveThread = new Thread(ReceiveMessage);
                //receiveThread.Start(clientSocket);
            }

            myServerSocket.Shutdown(SocketShutdown.Both);
            myServerSocket.Close();
            Console.WriteLine("关闭监听成功");
        }

        /// <summary>  
        /// 接收消息
        /// </summary>
        /// <param name="clientSocket"></param>  
        //private static void ReceiveMessage(object clientSocket)
        //{
        //    Socket myClientSocket = (Socket)clientSocket;
        //    var buffer = new byte[1024];

        //    while (!isClosed)
        //    {
        //        try
        //        {
        //            //通过clientSocket接收数据  
        //            int receiveNumber = myClientSocket.Receive(buffer);
        //            Console.WriteLine("接收客户端{0}消息{1}", myClientSocket.RemoteEndPoint.ToString(), Encoding.Default.GetString(buffer, 0, receiveNumber));
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //            myClientSocket.Shutdown(SocketShutdown.Both);
        //            myClientSocket.Close();
        //            break;
        //        }
        //    }
        //}
    }
}
