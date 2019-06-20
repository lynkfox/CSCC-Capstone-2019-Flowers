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




/* TO DO LIST
 * 
 * Exceptions instead of Booleans. Throw a custom exception
 * 
 * More Detailed sql methods? (ie: GetInventory(Store)?
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using CcnSession.Properties;

namespace CcnSession
{
    public class SQL
    {
        //public properties - used to check permissions and defaults

        // Consider changing IsManager to an int? For more than just 2 levels of permission? Future proofing.
        static public bool IsManager { get; set; }
        static public bool PwCorrect { get; set; }
        static public string DefaultStore { get; set; }


        static private string Username { get; set; }

        static private string address, database, userid, password;

        private static MySqlConnectionStringBuilder cnnStr = new MySqlConnectionStringBuilder
        {

        };


        /* Initializes session.
         * 
         * takes parameters of the username and the password
         * 
         * sets the Username inside the static object, to be used within various other functions used during
         * the session
         * 
         * sets the permission level (IsManager true/false)
         * 
         * checks to make sure the password is correct, sets a bool for later (so as not to store pw)
         * 
         * stores the DefaultStore string for use in the main program.
         * 
         * 
         * 
         * this should be called immediately after login, and ONLY after login. Never again.
         */

        static public void Setup(string user, string password)
        {
            SetupConnection();

            Username = user;

            PwCorrect = ChkPassword(password);

            DefaultStore = GetStore();


            if (PwCorrect)
            {
                Permission();
            }
            else
            {
                IsManager = false;
            }
        }

        /* Safety function. Cleans up the static class for logout (but not program exit). 
         * 
         * Needs to be called at every position that can log out. Setup **should** overwrite for a new user, but
         * its better to be safe and call this.
         * 
         */

        static public void Cleanup()
        {
            Username = null;
            PwCorrect = false;
            IsManager = false;
            DefaultStore = null;
        }


        /* This function reads in the connection data from the file in the resources and adds its information to the 
         * properties needed for the connection builder.
         * 
         * to do: encrypt and decrypt the file for additional protection
         */

        static private void SetupConnection()
        {

            string[] rows = Resources.connection.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);


            address = rows[0];
            database = rows[1];
            userid = rows[2];
            password = rows[3];

            MySqlConnectionStringBuilder cnn = new MySqlConnectionStringBuilder
            {

                Server = address,
                Database = database,
                UserID = userid,
                Password = password,
                Port = 3306

            };

            cnnStr = cnn;
        }


        

        /* Checks the password against the database, and returns true if valid, false if not.
         * 
         * to do - add exception handling.
         * 
         * Passwords are stored with a 24bit Hash and a 24bit salt. We will pull the salt and store
         * it in a string, then generate the hash from the inputed pw and the salt, comparing that to the hash
         * (without ever storing it in a variable of its own other than the reader).
         */
        public static bool ChkPassword(string pw)
        {

            byte[] salt;
            string hash;
            var saltData = new DataTable();
            var hashData = new DataTable();
            saltData = GetColumn("EMPLOYEE", "salt", "username", Username);
            hashData = GetColumn("EMPLOYEE", "password", "username", Username);

            salt = Convert.FromBase64String(saltData.Rows[0]["salt"].ToString());
            hash = hashData.Rows[0]["password"].ToString();

            if (hash == Convert.ToBase64String(PasswordHash.ComputeHash(pw, salt)))
            {
                return true;
            }
            else
            {
                return false;
            }

        }



        /* This function can be used to determine if the current employee has permission to access
         * whatever it is being asked of. 
         * 
         * checks the password
         * 
         * checks for if the user has the Manager access level, and sets appropriately.
         * 
         * 
         * 
         * This may change in the future.
         */
        private static void Permission()
        {

            string sql = "SELECT type FROM EMPLOYEE WHERE username = '" + Username + "';";
            MySqlDataReader rdr = null;
            int i = 0;

            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {
                try
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

                            IsManager = true;

                        }
                        else
                        {
                            IsManager = false;
                        }



                    }

                    // put in an exception for if rdr.count >1?
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);

                }
                finally
                {
                    if (cnn != null) cnn.Close();
                }



            }


        }


        /* CreateUser takes the employees first and last name and desired pw.
         * 
         * will automatically generate a unique username (first initial, last name + numbers as needed)
         * 
         * will generate a salt for the pw, and store the salt and the resulting hash in the database, along with the firstname/lastname
         * username. Additional information will need to be stored by other means
         * 
         * returns the generated username as a string.
         * 
         * 
         * if this errors out in some way, it will should return null - check when using that username !null.
         */

        public static string CreateUser(string fName, string lName, string pw)
        {

            // outputs the date in format equal to the rest of the table
            var today = DateTime.Today.ToString("yyyy-MM-dd");

            // automatically create the username as first initial, last name - all lowercase
            string username = fName[0] + lName;
            username = username.ToLower();

            /* if there happen to be 2 people with the same first initial/last name combo
             * then this section will add a number to the end of the username.
             */

            int i = 1;

            while (CheckUsername(username))
            {

                if (i > 1 && i < 10)
                {
                    /* if there happen to be more than 2 people with the same first initial, last name
                     * then we remove the 1 (the last char fo the string) and add the new incrimimented i
                     * to the username (so username2, then username3, ect)
                     */
                    username = username.Substring(0, username.Length - 1);
                }
                else if (i >= 10)
                {
                    /* Let's be real here. If there are more than 10 people with the exact same first
                     * initial and last name, there there is either nepotism or something very weird going on
                     * but just in case, we're removing 2 numbers if it gets above 10 for i.
                     * 
                     * we're not going to check for 3 numbers. Something is messed up, contact IT
                     */
                    username = username.Substring(0, username.Length - 2);
                }
                username += i; // add the iteration number (starting at 1!!!) to the end of the preset username.
                i++;
            }

            //Generate the salt and the hash
            var passwordSalt = PasswordHash.GenerateSalt();
            var passwordHash = PasswordHash.ComputeHash(pw, passwordSalt);

            //convert the hash and salt to a string for database storage
            string saltString = Convert.ToBase64String(passwordSalt);
            string hashString = Convert.ToBase64String(passwordHash);

            //setup the sql string for insertion

            string sql = "INSERT INTO EMPLOYEE (first_name, last_name, username, password, salt, hired, location) VALUES ('" + fName + "','" + lName + "','" + username + "','" + hashString + "','" + saltString + "','" + today + "', '" + DefaultStore + "')";

            if (SendQry(new MySqlCommand(sql)))
            {
                return username;
            }
            else
            {
                return null;
            }

        }



        /* Private function for checking the username if it is unique or not
         */
        private static bool CheckUsername(string username)
        {

            if (GetColumn("EMPLOYEE", "username", username).Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static string GetStore()
        {
            var data = new DataTable();
            data = GetColumn("EMPLOYEE", "location", "username", Username);

            string store = data.Rows[0]["location"].ToString();
            return store;
        }

        /* Method for changing the pw in the database.
         * 
         * Pulls the username from the current session. Overloaded version takes a username as a second argument.
         * 
         * Note. This method JUST changes the pw (and the hash and the salt, of course).
         * 
         * Be sure to ChkPassword BEFORE calling this function.
         * 
         * returns true if successful, false if not.
         */

        public static bool ChangePassword(string newPW)
        {
            var salt = PasswordHash.GenerateSalt();
            var hash = PasswordHash.ComputeHash(newPW, salt);

            string saltString = Convert.ToBase64String(salt);
            string hashString = Convert.ToBase64String(hash);

            //if the PWs are the same, then we don't want to change it!

            // really needs exception handling here.
            if (ChkPassword(newPW))
            { return false; }



            string sql = "UPDATE EMPLOYEE SET password = '" + hashString + "', salt = '" + saltString + "' WHERE username = '" + Username + "';";

            if (SendQry(new MySqlCommand(sql)))
            {
                return true;
            }
            else
            {
                return false;
            }



        }






        /* Generic SQL functions to Follow
         */


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
                    string sql = "SELECT " + colName + " FROM " + tableName + " WHERE " + colName + "= '" + whereVal + "' ;";

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

        static public DataTable GetColumn(string tableName, string colName, string whereCol, string whereVal)
        {
            var tableData = new DataTable();


            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {
                    string sql = "SELECT " + colName + " FROM " + tableName + " WHERE " + whereCol + "= '" + whereVal + "' ;";

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
                    }
                    else
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

