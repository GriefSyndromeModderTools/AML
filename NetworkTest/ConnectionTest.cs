using AGSO.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkTest
{
    class ConnectionTest
    {
        static void Main()
        {
            Console.WriteLine("c/s:");
            if (Console.ReadKey().Key == ConsoleKey.C)
            {
                Connection conn = new Connection(128);

                conn.Buffer.WriteByte(123);
                Console.WriteLine("IP:");
                conn.Send(Console.ReadLine(), 10800);
                conn.Close();

                Console.WriteLine("Sent.");
            }
            else
            {
                Connection conn = new Connection(128);
                conn.Bind(10800);

                Remote r;
                while (!conn.Receive(out r))
                {
                    Thread.Sleep(1);
                }
                conn.Close();

                Console.WriteLine("Recieved: {0}.", conn.Buffer.ReadByte());
            }
        }
    }
}
