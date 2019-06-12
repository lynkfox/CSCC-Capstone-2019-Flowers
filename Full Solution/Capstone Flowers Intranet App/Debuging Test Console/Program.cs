using System;
using Debuging_Test_Console.Session;
using System.Data;
using MySql.Data.MySqlClient;

namespace Debuging_Test_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var session = new CcnSession();
            var data = new DataTable();

            if (session.isManager == true)
            {

            };

            /*
            Console.WriteLine("The Current Username is: {0} and true/false - manager is: {1}.", session.username, session.isManager);

            var session2 = new CcnSession("lmoore");

            Console.WriteLine("The Current Username is: {0} and true/false - manager is: {1}.", session2.username, session2.isManager);

            var session3 = new CcnSession("brain31");


            Console.WriteLine("The Current Username is: {0} and true/false - manager is: {1}.", session3.username, session3.isManager);


            data = session.GetTable("EMPLOYEE");

            Console.WriteLine("Got The {0} data table, returning {1} rows.", data.TableName, data.Rows.Count);
            Console.ReadLine();
            */

            /*
            MySqlCommand command = new MySqlCommand("ALTER TABLE `INVENTORY` ADD `desc_name` VARCHAR(100) NOT NULL");

            session.SendQry(command);
            */

        }
    }
}
