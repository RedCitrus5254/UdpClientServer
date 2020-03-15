using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        int port = 8005; // порт для приема входящих запросов
        Socket listenSocket;

        List<EndPoint> players = new List<EndPoint>();

        public Server()
        {
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }
        public void Start()
        {
            try
            {
                //listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Task listeningTask = new Task(Listening);
                listeningTask.Start();

                //отправка сообщений на разные порты
                while (true)
                {
                    string message = Console.ReadLine();

                    byte[] data = Encoding.Unicode.GetBytes(message);
                    EndPoint remotePoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                    listenSocket.SendTo(data, remotePoint);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Close();
            }
        }
        private void SendMessage(string mes, EndPoint remotePoint)
        {
            players.Add(remotePoint);
            byte[] data = Encoding.Unicode.GetBytes(mes);

            foreach(var p in players)
            {
                listenSocket.SendTo(data, p);
            }
            //listenSocket.SendTo(data, remotePoint);

            Console.WriteLine("отправил");
        }

        public void Listening()
        {
            try
            {

                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Any, port);
                Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            
            
                listenSocket.Bind(ipPoint);

                Console.WriteLine("Сервер запущен. Ожидание подключений...");

                while (true)
                {
                    

                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0; // количество полученных байтов
                    byte[] data = new byte[256];// буфер для получаемых данных


                    EndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);

                    do
                    {
                        bytes = listenSocket.ReceiveFrom(data, ref remoteIp);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (listenSocket.Available > 0);



                    IPEndPoint remoteFullIp = remoteIp as IPEndPoint;

                    // выводим сообщение
                    Console.WriteLine("{0}:{1} - {2}", remoteFullIp.Address.ToString(),
                                                    remoteFullIp.Port, builder.ToString());
                    SendMessage($"{remoteFullIp.Address.ToString()} : {remoteFullIp.Port} - {builder.ToString().ToUpper()}", remoteFullIp);

                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                Close();
            }
        }
        private void Close()
        {
            if (listenSocket != null)
            {
                listenSocket.Shutdown(SocketShutdown.Both);
                listenSocket.Close();
                listenSocket = null;
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start();
        }
    }
}
