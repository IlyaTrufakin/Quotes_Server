using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Quotes_Server
{
    public class Program
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();


        static void Main(string[] args)
        {
            try
            {
                logger.Info($"Приложение запущено: /{DateTime.Now}/");
                ServerCommunication server = new ServerCommunication("127.0.0.1", 8005);
                server.Start();
  
                logger.Info($"Приложение завершило работу: /{DateTime.Now}/");
                LogManager.Shutdown();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Произошла ошибка: " + ex.Message);
            }
            finally
            {
                logger.Info($"Приложение завершило работу: /{DateTime.Now}/");
                LogManager.Shutdown();
            }
        }
    }
}
