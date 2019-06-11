using System;
using Debuging_Test_Console.Session;

namespace Debuging_Test_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var session = new CcnSession();

            Console.WriteLine("The Current Username is: {0} and true/false - manager is: {1}.", session.username, session.isManager);

            var session2 = new CcnSession("lmoore");

            Console.WriteLine("The Current Username is: {0} and true/false - manager is: {1}.", session2.username, session2.isManager);

            var session3 = new CcnSession("brain31");


            Console.WriteLine("The Current Username is: {0} and true/false - manager is: {1}.", session3.username, session3.isManager);

            Console.ReadLine();

        }
    }
}
