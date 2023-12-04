using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotes_Server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ServerCommunication server = new ServerCommunication("127.0.0.1", 8005);
            server.Start();
        }
    }
}
