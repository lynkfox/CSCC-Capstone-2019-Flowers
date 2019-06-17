using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Debuging_Test_Console
{
    class Program
    {
        static void Main(string[] args)
        {

            /* Testing password based login
             * 
             * and found the error. The SQL query was bad.
             */

            String username = "agoh";
            String password = "tsdfaer"; //bad password
            String goodPW = "test";

            var dTable = new DataTable();

            //Call to database to verify the username.
            // username field is unique

            dTable = CcnSession.GetColumn("EMPLOYEE", "username", "password", username);


            // check to see of the username exists.
            if (dTable.Rows.Count == 1)
            {
                //verify the password
                //if (CcnSession.ChkPassword(password))
                if (CcnSession.ChkPassword(goodPW))
                {
                    Console.WriteLine("Correct Password: Proceede");
                }
                else
                {

                }
            }
            else if (dTable.Rows.Count == 0)
            {

                Console.WriteLine("Incorrect Username or Password");
            }
            else //if dTable.Rows.Count >1
            {
                /* this should never happen - username being unique, and the sql library searches
                 * for exact  strings, not partial matches, this is an edge case error that is here
                 * just in case
                 */
                Console.WriteLine("Something Really Bad Happened.");
            }


            //Set up the username and the Permission level for if Employee or Manager.
            CcnSession.Setup(username);


            Console.WriteLine("IsManager?: " + CcnSession.IsManager + " | ");
            



            /* --- this code was testing it as a static class, for use across all aspects of the app
             * 
             * 
            var data = new DataTable();

            CcnSession.Setup("lmoore");

            if (CcnSession.IsManager == true)
            {
                Console.WriteLine("Totes Manager");
            } else
            {
                Console.WriteLine("Access Denied dude!");
            }
            Console.ReadLine();
            */

            /*  Original testing to make sure the connection worked.
             *  
             *  this was done before making it a static class, so will no longer work as written
             *  
             *  
            Console.WriteLine("The Current Username is: {0} and true/false - manager is: {1}.", session.username, session.isManager);

            var session2 = new CcnSession("lmoore");

            Console.WriteLine("The Current Username is: {0} and true/false - manager is: {1}.", session2.username, session2.isManager);

            var session3 = new CcnSession("brain31");


            Console.WriteLine("The Current Username is: {0} and true/false - manager is: {1}.", session3.username, session3.isManager);


            data = session.GetTable("EMPLOYEE");

            Console.WriteLine("Got The {0} data table, returning {1} rows.", data.TableName, data.Rows.Count);
            Console.ReadLine();
            */



            /* testing the SendQry method by adding new rows to the tale INVENTORY that we had forgoten when creating the initial
             * schema
             * 
             * 
            MySqlCommand command = new MySqlCommand("ALTER TABLE `INVENTORY` ADD `desc_name` VARCHAR(100) NOT NULL");

            session.SendQry(command);
            */

        }
    }
}
