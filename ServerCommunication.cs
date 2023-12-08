using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Quotes_Server
{
    internal class ServerCommunication

    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();
        private Quotes quotes = new Quotes();
        private int quotesSentToClient;
        private readonly int maxClients = 3; // макс. кол-во одновременных клиентов на соккете
        private string Password = "trufakin";
        private string Login = "ilya";
        IPEndPoint ipPoint;
        private Socket listenSocket;
        private readonly int maxQuotesPerClient = 5; // макс. кол-во цитат на клиента
        private Dictionary<Socket, string> connectedClients = new Dictionary<Socket, string>();
        private int clientIdCounter = 0;
        private object lockObject = new object(); // для блокировки доступа нескольких потоков к изменяемому объекту

        //consoleThread = new Thread(ConsoleInput);


        public ServerCommunication(string ipAddress = "127.0.0.1", int port = 8005)
        {
            try
            {
                ipPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Ошибка IP-адреса: " + ex.Message);
                logger.Info($"Ошибка IP-адреса:  {ex.Message}  /{DateTime.Now}/");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine("Ошибка в аргументах: " + ex.Message);
                logger.Info($"Ошибка в аргументах:  {ex.Message}  /{DateTime.Now}/");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка: " + ex.Message);
                logger.Info($"Произошла ошибка:  {ex.Message}  /{DateTime.Now}/");
            }
        }



        public void Start()
        {
            try
            {
                listenSocket.Bind(ipPoint);
                listenSocket.Listen(10);
                Console.WriteLine("Server start listen...");
                logger.Info($"Server start listen:  /{DateTime.Now}/");


                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    AddClient(handler);

                    Console.WriteLine($"Client connected:  {connectedClients[handler]} IP({handler.RemoteEndPoint})");
                    logger.Info($"Client connected:  {connectedClients[handler]} IP({handler.RemoteEndPoint}) + /{DateTime.Now}/");
                    Console.WriteLine($"\tList of connected clients: ");
                    foreach (var clients in connectedClients)
                    {
                        Console.WriteLine($"\t- {clients.Value} IP({clients.Key.RemoteEndPoint})");
                    }

                    ThreadPool.QueueUserWorkItem(new WaitCallback(HandleClient), handler);
                    //Console.WriteLine("Создан поток: " + clientThread.GetHashCode());

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                logger.Info($"Произошла ошибка при запуске сервера: {ex.Message}  /{DateTime.Now}/");
            }
        }



        // Добавление клиента и его идентификатора в словарь connectedClients
        private void AddClient(Socket clientSocket)
        {
            lock (lockObject)
            {
                clientIdCounter++;
                connectedClients.Add(clientSocket, "Client ID#" + clientIdCounter.ToString());
            }
        }





        private void HandleClient(object obj)
        {
            Socket handler = (Socket)obj;
            quotesSentToClient = 0;
            try
            {

                while (true)
                {
                    byte[] data = new byte[256];
                    StringBuilder receivedString = new StringBuilder();
                    int receivedBytes;

                    do
                    {
                        receivedBytes = handler.Receive(data);
                        receivedString.Append(Encoding.Unicode.GetString(data, 0, receivedBytes));
                    } while (handler.Available > 0);

                    string[] StringParts = receivedString.ToString().Split(':');

                    if (StringParts[0] != "timeQuiet") // когда клиент запрашивает время в автоматическом режиме, не выводим об этом инфо в консоль
                    {
                        Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + StringParts[0] + $"  (from {connectedClients[handler]})");
                        logger.Info($"Сообщение {StringParts[0]}  (получено от  {connectedClients[handler]})  /{DateTime.Now}/");
                    }


                    if (StringParts[0] == "login") // когда клиент подключается и присылает логин и пароль
                    {
                        Console.WriteLine(DateTime.Now.ToShortTimeString() + ": " + StringParts[0] + $"  (from {connectedClients[handler]})" + $"Идентификация клиента: {StringParts[0]} : {StringParts[1]} : {StringParts[2]}");
                        logger.Info($"Сообщение {StringParts[0]}  (получено от  {connectedClients[handler]})  + Идентификация клиента: {StringParts[0]} : {StringParts[1]} : {StringParts[2]} /{DateTime.Now}/");

                        if (connectedClients.Count > maxClients)
                        {
                            StringParts[0] = "maxсonnectionlimit";
                        }

                        if (!(StringParts[1] == "ilya" && StringParts[2] == "trufakin"))
                        {
                            StringParts[0] = "reject";
                        }
                    }

                    string response = ProcessRequest(StringParts[0]); // обработка строки запроса от клиента

                    handler.Send(Encoding.Unicode.GetBytes(response)); // отправка ответа клиенту

                    if (response == "Closing" || StringParts[0] == "reject" || StringParts[0] == "maxсonnectionlimit") // отработка запроса клиента на закрытие соединения
                    {
                        Console.Write($"{connectedClients[handler]} - Closing connection...");
                        logger.Info($"Соединение с клиентом {connectedClients[handler]} завершается: /{DateTime.Now}/");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connectedClients.Remove(handler);
                handler.Shutdown(SocketShutdown.Both);
                logger.Info($"Соединение с клиентом завершено: /{DateTime.Now}/");
                Console.WriteLine("Connection closed");
                handler.Close();

            }



        }


        private string ProcessRequest(string request)
        {
            string response = string.Empty;

            switch (request.Trim().ToLower())
            {
                case "getquote":

                    string str = quotes.GetQuote();
                    if (quotesSentToClient > maxQuotesPerClient)
                    {
                        Console.WriteLine($"Кол-во запрашиваемых цитат ({quotesSentToClient}), для клиента, превышает  заданный уровень - {maxQuotesPerClient}. В получении цитат отказано.");
                        logger.Info($"Кол-во запрашиваемых цитат ({quotesSentToClient}), для клиента, превышает  заданный уровень - {maxQuotesPerClient}. /{DateTime.Now}/ ");
                        response = ($"Кол-во запрашиваемых цитат ({quotesSentToClient}), для клиента, превышает  заданный уровень - {maxQuotesPerClient}. /{DateTime.Now}/ ");
                    }
                    else
                    {
                        Console.WriteLine("Цитата: " + str);
                        logger.Info($"Цитата: {str}  /{DateTime.Now}/ ");
                        response = str;
                        quotesSentToClient++; // счетчик инкрементальный числа цитат.
                    }

                    break;

                case "time":
                    response = DateTime.Now.ToString();
                    break;

                case "login":
                    response = ($"Идентификация клиента осуществлена: {request}");
                    break;

                case "reject":
                    response = ($"Идентификация клиента не осуществлена.");
                    break;

               case "maxсonnectionlimit":
                    response = ($"Сервер перегружен. Попробуйте присоединиться позже.");
                    break;


                case "timequiet":
                    response = DateTime.Now.ToString();
                    break;

                case "info":
                    response = Environment.OSVersion.ToString();
                    break;

                case "get":
                    Console.WriteLine("Запрошен ручной ответ, напишите что-нибудь клиенту: ");
                    response = Console.ReadLine();
                    break;

                case "bye":
                    response = "Closing";
                    break;

                default:
                    response = "Invalid command";
                    break;
            }

            return response;
        }
    }
}
