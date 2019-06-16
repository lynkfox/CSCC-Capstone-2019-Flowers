/* This class is designed to hold all the information needed to access a mySQL database in the background for a 
 * session. This includes the permission level of the user (currently Employee or Managerm based on the table)
 * it also holds the connection data, so that it is always the same.
 * 
 * each function that can retrieve or send data to the mySQL database deals with the connection internally
 * it opens and closes it to make sure there are no extra open connections floating around
 * 
 * 
 * Designed by: Anthony Goh (lynkfox on GitHub)
 * For: Columbus State Community College, CSCI-2999 capstone, 2019.
 */



using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;

namespace login
{
    static class CcnSession
    {
        //public properties
        static public bool IsManager { get; set; }
        static private string Username { get; set;  }



        /* Initializes the username and the permission level for the current login session.
         * 
         * takes parameters of the username and the password
         * 
         * checks to make sure the password is correct.
         * 
         * this should be called immediately after login.
         */

        static public void Setup(string user, string password)
        {
            Username = user;
            IsManager = Permission();

        }



        /* This connection is currently hardcoded in.
         * 
         * TO DO - move connection to an external file to be read in
         */
        private static MySqlConnectionStringBuilder cnnStr = new MySqlConnectionStringBuilder
        {
            Server = "cscc-capstone-flowers.czo7kmlutdp9.us-east-2.rds.amazonaws.com",
            Database = "capstoneFlowers",
            UserID = "flowersAdmin",
            Password = "SolFlowerGreenWater2",
            Port = 3306

        };






        /* This function can be used to determine if the current employee has permission to access
         * whatever it is being asked of. 
         * 
         * checks the password
         * 
         * Returns True if a manager, false if not.
         * 
         * Delibertly private - It would undoubtedly be better to make isManager flag private
         * and to use this function to return a bool externally, at this time we're leaving it as is 
         * in order to streamline a few external processes. 
         * 
         * This may change in the future.
         */
        static private bool Permission()
        {

            string sql = "SELECT password, type FROM EMPLOYEE WHERE username = '" + Username + "';";
            MySqlDataReader rdr = null;
            int i = 0;

            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {
                cnn.Open();
                var cmd = new MySqlCommand(sql, cnn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Console.WriteLine("Checking Data. Username: " + Username + " | AcctType: " + rdr.GetString(i));
                    i++;
                    // this bit is just in case the command somehow draws back more than one username of the same name. 
                    // the username col in this table is set to unique, so this shouldn't happen. 
                    // unless - where the username is similar like: abcd and abcde - 
                    // check into this!!!!
                    if (rdr.GetString(0) == "Manager")
                    {

                        return true;

                    }
                    else
                    {
                        return false;
                    }
                }

                // put in an exception for if rdr.count >1?

                return false;
            }


        }


        /* This function is designed to get any table from the connection.
         * 
         * Overloaded version can order the table by a custom ORDER BY
         * 
         * Overloaded version can be ordered by, where col = value
         * 
         * Both functions return NULL data if they catch an exception, and log it to the console
         * 
         * in live, check for Null when using these functions, if return null then throw an error.
         * 
         * does where val does not currently work with BETWEEN
         */
        static public DataTable GetTable(string tableName)
        {
            var tableData = new DataTable();


            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {
                    string sql = "SELECT * FROM " + tableName;

                    //logging
                    Console.WriteLine("Connecting... ");

                    var cmd = new MySqlCommand(sql, cnn);
                    cnn.Open();
                    //logging
                    Console.WriteLine("Connection:  {0}", cnn.State);
                    Console.WriteLine("Sending Command: {0}", sql);

                    using (MySqlDataAdapter data = new MySqlDataAdapter(cmd)) 
                    {
                        data.Fill(tableData);
                    }

                    return tableData;
                }
                catch (Exception ex)
                {
                    //mostly for debugging at this point.
                    Console.WriteLine("Error: {0}", ex.Message);
                    return null;
                }
                finally
                {
                    if (cnn != null) cnn.Close();
                }

            }

        }
        static public DataTable GetTable(string tableName, string orderBy)
        {
            var tableData = new DataTable();


            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {
                    string sql = "SELECT * FROM " + tableName + "ORDER BY" + orderBy + ";";

                    //logging
                    Console.WriteLine("Connecting... ");

                    var cmd = new MySqlCommand(sql, cnn);
                    cnn.Open();
                    //logging
                    Console.WriteLine("Connection:  {0}", cnn.State);
                    Console.WriteLine("Sending Command: {0}", sql);

                    using (MySqlDataAdapter data = new MySqlDataAdapter(cmd))
                    {
                        data.Fill(tableData);
                    }

                    return tableData;
                }
                catch (Exception ex)
                {
                    //mostly for debugging at this point.
                    Console.WriteLine("Error: {0}", ex.Message);
                    return null;
                }
                finally
                {
                    if (cnn != null) cnn.Close();
                }

            }

        }
        static public DataTable GetTable(string tableName, string orderBy, string whCol, string whVal)
        {
            var tableData = new DataTable();


            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {
                    string sql = "SELECT * FROM " + tableName + "ORDER BY" + orderBy + " WHERE " + whCol + "=" + whVal + ";";

                    //logging
                    Console.WriteLine("Connecting... ");

                    var cmd = new MySqlCommand(sql, cnn);
                    cnn.Open();
                    //logging
                    Console.WriteLine("Connection:  {0}", cnn.State);
                    Console.WriteLine("Sending Command: {0}", sql);

                    using (MySqlDataAdapter data = new MySqlDataAdapter(cmd))
                    {
                        data.Fill(tableData);
                    }

                    return tableData;
                }
                catch (Exception ex)
                {
                    //mostly for debugging at this point.
                    Console.WriteLine("Error: {0}", ex.Message);
                    return null;
                }
                finally
                {
                    if (cnn != null) cnn.Close();
                }

            }

        }

        /* same as GetTable, except takes the table name and the column name as arguments, and only returns
         * that single column
         * 
         * single overload with a third string, WHERE query
         * 
         * session.GetColumn(tablename, columnName, valueOfCol)
         * 
         * session.GetColumn(ACCT_REC, acct_num, "10003")
         * 
         * does not currently work with BETWEEN query
         */
        static public DataTable GetColumn(string tableName, string colName)
        {
            var tableData = new DataTable();


            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {
                    string sql = "SELECT " + colName + " FROM " + tableName + ";";

                    //logging
                    Console.WriteLine("Connecting... ");

                    var cmd = new MySqlCommand(sql, cnn);
                    cnn.Open();
                    //logging
                    Console.WriteLine("Connection:  {0}", cnn.State);
                    Console.WriteLine("Sending Command: {0}", sql);

                    using (MySqlDataAdapter data = new MySqlDataAdapter(cmd))
                    {
                        data.Fill(tableData);
                    }

                    return tableData;
                }
                catch (Exception ex)
                {
                    //mostly for debugging at this point.
                    Console.WriteLine("Error: {0}", ex.Message);
                    return null;
                }
                finally
                {
                    if (cnn != null) cnn.Close();
                }

            }

        }
        static public DataTable GetColumn(string tableName, string colName, string whereVal)
        {
            var tableData = new DataTable();


            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {
                    string sql = "SELECT " + colName + " FROM " + tableName + " WHERE " + colName + "=" + whereVal + ";";

                    //logging
                    Console.WriteLine("Connecting... ");

                    var cmd = new MySqlCommand(sql, cnn);
                    cnn.Open();
                    //logging
                    Console.WriteLine("Connection:  {0}", cnn.State);
                    Console.WriteLine("Sending Command: {0}", sql);

                    using (MySqlDataAdapter data = new MySqlDataAdapter(cmd))
                    {
                        data.Fill(tableData);
                    }

                    return tableData;
                }
                catch (Exception ex)
                {
                    //mostly for debugging at this point.
                    Console.WriteLine("Error: {0}", ex.Message);
                    return null;
                }
                finally
                {
                    if (cnn != null) cnn.Close();
                }

            }

        }

        /* For Executing non table getting queries (INSERT, ALTER)
         * 
         * takes a MySqlCommand - built with the MySqlCommand.CommandText functions
         * 
         * using something like:
         * 
         *          cmd.CommandText = "INSERT INTO testTable(ID, FIRST_NAME, LAST_NAME, EMAIL) VALUES(?ID, ?FIRST_NAME, ?LAST_NAME, ?EMAIL)";
                    cmd.Parameters.Add("?ID", MySqlDbType.Int32).Value = ID;
                    cmd.Parameters.Add("?FIRST_NAME", MySqlDbType.Text).Value = firstName;
                    cmd.Parameters.Add("?LAST_NAME", MySqlDbType.Text).Value = lastName;
                    cmd.Parameters.Add("?EMAIL", MySqlDbType.Text).Value = email;

            * Alternatively, it the  SQL Query to be sent does not need to be rebuilt like this often,
            * simply store it as a string.
            * 
            *       cmd.CommandText+"INSERT INTO Customer(" +id + firstName + lastName + email+ ")";

            returns a boolean, of True if the command is successful (ie, returns more than 0 rows), or false
            if the command is not successful (returns 0 rows)
         */
        static public bool SendQry(MySqlCommand cmd)
        {

            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {


                    //logging
                    Console.WriteLine("Connecting... ");

                    // assigns the connection to the command, so when executeNonQry fires it knows what connection to use
                    cmd.Connection = cnn;
                    cnn.Open();

                    //logging
                    Console.WriteLine("Connection:  {0}", cnn.State);
                    Console.WriteLine("Sending Command: {0}", cmd);

                    //Sends the command, as defined by the string builder externally.
                    if (cmd.ExecuteNonQuery() >= 1)
                    {
                        return true;
                    } else
                    {
                        return false;
                    }

                }
                catch (Exception ex)
                {
                    //mostly for debugging at this point.
                    Console.WriteLine("Error: {0}", ex.Message);
                    return false;
                }
                finally
                {
                    if (cnn != null) cnn.Close();
                }

            }

        }
    }



}

