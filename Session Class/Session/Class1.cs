﻿using System;
using System.Data;
using MySql.Data.MySqlClient;


/*
 * This class is to keep the connection completely seperate and out of all other parts of the code
 * 
 * Because it is also going to be used in the Capstone Project (CSCC2019) as part of the permisions functions
 * for managers vrs employees on the intranet, it holds session data based on the employee login
 * 
 * That permission data (IsManager == bool) can be used to allow access to various other parts of the app
 * (say, Acct_Payable, other employee data)
 * 
 * Current Constructors
 * 
 * CcnSession() - defaults to null username and false manager
 * CcnSession(username) - sets the username, checks permission of that username against the database
 * 
 * Current Public Methods and Properties:
 * 
 * CcnSession.username (get and set)
 * CcnSession.isManager (get only - true = Manager)
 * 
 * CcnSession.GetTable(tableName)
 * CcnSession.GetTable(tableName, orderBy)
 * CcnSession.GetTable(tableName, orderBy, whereCol, whereValue)
 * 
 * CcnSession.GetCol(tableName, colName)
 * CcnSession.GetCol(tableName, colName, whereValue)
 * 
 * CcnSession.SendQry(mysql qry string)
 * 
 * 
 */
namespace Session
{
    public class CcnSession
    {
        public bool isManager { get; }
        public string username { get; set; }

        /* This connection is currently hardcoded in.
         * 
         * TO DO - move connection to an extenral file to be read in
         */
        private MySqlConnectionStringBuilder cnnStr = new MySqlConnectionStringBuilder
        {
            Server = "cscc-capstone-flowers.czo7kmlutdp9.us-east-2.rds.amazonaws.com",
            Database = "cscc-capstone-flowers",
            UserID = "flowersAdmin",
            Password = "SolFlowerGreenWater2",
            Port = 3306

        };

        public CcnSession()
        {
            isManager = false;
            username = null;
            
        }

        public CcnSession(string user)
        {
            username = user;
            isManager = Permission();
        }




        /* This funciton can be used to determine if the current employee has permission to access
         * whatever it is being asked of. 
         * 
         * Returns True if a manager, false if not.
         * 
         * Delibertly private - It would undoubtedly be better to make isManager flag private
         * and to use this function to return a bool externally, at this time we're leaving it as is 
         * in order to streamline a few external processes. 
         * 
         * This may change in the future.
         */
        private bool Permission()
        {
            
            string sql = "SELECT type FROM EMPLOYEE WHERE username = '" + username + "';";
            MySqlDataReader rdr = null;
            int i = 0;

            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {
                cnn.Open();
                var cmd = new MySqlCommand(sql, cnn);
                rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    Console.WriteLine("Checking Data. Username: " + username + " | AcctType: " + rdr.GetString(i));
                    i++;
                    // this bit is just in case the command somehow draws back more than one username of the same name. 
                    // the username col in this table is set to unique, so this should never happen.
                }

                // put in an exception for if rdr.count >1?

                if (rdr.GetString(0) == "Manager")
                {
                    /* if this becomes a public class, may need to add an isManager=True flag here
                     * in order to set the flag within the object for other uses internally
                     */
                    
                    return true;

                }
                else
                {
                    
                    return false;
                }
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
        public DataTable GetTable(string tableName)
        {
            var tableData = new DataTable();


            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {
                    string sql = "SELECT * FROM " + tableName + ";";

                    //logging
                    Console.WriteLine("Connecting... ");

                    var cmd = new MySqlCommand(sql, cnn);

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
        public DataTable GetTable(string tableName, string orderBy)
        {
            var tableData = new DataTable();


            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {
                    string sql = "SELECT * FROM " + tableName + "ORDER BY" +orderBy+ ";";

                    //logging
                    Console.WriteLine("Connecting... ");

                    var cmd = new MySqlCommand(sql, cnn);

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
        public DataTable GetTable(string tableName, string orderBy, string whCol, string whVal)
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
         * doesn not currently work with BETWEEN query
         */
        public DataTable GetColumn(string tableName, string colName)
        {
            var tableData = new DataTable();


            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {
                    string sql = "SELECT "+colName+ " FROM " + tableName + ";";

                    //logging
                    Console.WriteLine("Connecting... ");

                    var cmd = new MySqlCommand(sql, cnn);

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
        public DataTable GetColumn(string tableName, string colName, string whereVal)
        {
            var tableData = new DataTable();


            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {
                    string sql = "SELECT " + colName + " FROM " + tableName + " WHERE " +colName+"="+whereVal+";";

                    //logging
                    Console.WriteLine("Connecting... ");

                    var cmd = new MySqlCommand(sql, cnn);

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
         */
        public void SendQry(MySqlCommand cmd)
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
                    cmd.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    //mostly for debugging at this point.
                    Console.WriteLine("Error: {0}", ex.Message);
                }
                finally
                {
                    if (cnn != null) cnn.Close();
                }

            }

        }
    }



}

