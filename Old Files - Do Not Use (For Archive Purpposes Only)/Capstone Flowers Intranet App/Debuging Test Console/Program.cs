using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Debuging_Test_Console
{
    class Program
    {
        static void Main(string[] args)
        {

            /*--- This code is for testing the new user creation screen.
             * 
             * god - damn.t     again.  ... bad sql command. i'm getting sick of myself
             * 
             */
            /*
            string fName = "Anthony";
            string lName = "Goh";
            string password = "test1";
            string store = "1000-Col";


            // outputs the date in format equal to the rest of the table
            var today = DateTime.Today.ToString("yyyy-MM-dd");
            string username = fName[0] + lName;
            int i = 1;

            while (checkUsername(username))
            {

                /* if there happen to be 2 people with the same first initial/last name combo
                 * then this section will add a number to the end of the username.
                 

                if (i > 1 && i < 10)
                {
                    /* if there happen to be more than 2 people with the same first initial, last name
                     * then we remove the 1 (the last char fo the string) and add the new incrimimented i
                     * to the username (so username2, then username3, ect)
                     
                    username = username.Substring(0, username.Length - 1);
                }
                else if (i >= 10)
                {
                    /* Let's be real here. If there are more than 10 people with the exact same first
                     * initial and last name, there there is either nepotism or something very weird going on
                     * but just in case, we're removing 2 numbers if it gets above 10 for i.
                     * 
                     * we're not going to check for 3 numbers. Something is messed up, contact IT
                     
                    username = username.Substring(0, username.Length - 2);
                }
                username += i; // add the iteration number (starting at 1!!!) to the end of the preset username.
                i++;
            }

            //Set up the SQL insertion string.

            string cmd = "INSERT INTO EMPLOYEE (first_name, last_name, username, password, hired, location) VALUES ('" + fName + "','" + lName + "','" + username + "','" + password + "','" + today + "','" + store + "')";

            if (CcnSession.SendQry(new MySqlCommand(cmd)))
            {
                Console.WriteLine("User Added: Username: " + username + " | Date Hired: " + today);
            }else
            {
                Console.WriteLine("Returned 0 Lines - User Not Added");
            }


                bool checkUsername(string un)
            {

                if (CcnSession.GetColumn("EMPLOYEE", "username", un).Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            */




            //
            //
            //



            /* Testing password based login
             * 
             * and found the error. The SQL query was bad. 
             

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
                 *//*
                Console.WriteLine("Something Really Bad Happened.");
            }


            //Set up the username and the Permission level for if Employee or Manager.
            CcnSession.Setup(username);


            Console.WriteLine("IsManager?: " + CcnSession.IsManager + " | ");
            
            /*




            //
            //
            //





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





            //
            //
            //



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




            //
            //
            //





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
