/* This class is designed to hold all the information needed to access a mySQL database in the background for a 
 * session. This includes the permission level of the user (currently Employee or Managerm based on the table)
 * it also holds the connection data, so that it is always the same.
 * 
 * each function that can retrieve or send data to the mySQL database deals with the connection internally
 * it opens and closes it to make sure there are no extra open connections floating around
 * 
 * a note on exceptions. 
 * 
 * Due to the limited time in the summer semester for putting together a project of this magnitude, I have
 * elected to go with a cheap and dirty exception method: just continue to throw the ex upward.
 * 
 * in the main program I handle catches with message boxes to display what went wrong. No, it is not ideal, 
 * and no it is not necessarily the best practice, but it works and gets the job done, plus
 * when I break something in the RDS I can see the message easier than having to make a console testbed for it.
 * 
 * Designed by: Anthony Goh (lynkfox on GitHub)
 * For: Columbus State Community College, CSCI-2999 capstone, su2019.
 */




/* TO DO LIST
 * 
 * More Detailed Exceptions (custom?)
 * 
 * perhaps change some bool functions to throw custom message if false.
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
        public static string Username { get; set; }
        public static bool CurEmp { get; set; }


        //Private properties, variables

        // these remain private, as they are not really needed outside of the class

        private static string address, database, userid, password;

        private static MySqlConnectionStringBuilder cnnStr = new MySqlConnectionStringBuilder
        {
           
        };

        /* Public Functions */

        /* Housekeeping Functions
         * 
         * these functions are setup and cleanup of the static object
         */
        /* SQL.Setup(username, password) - No Overloads
         * 
         * Initializes session.
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

            try
            {
                SetupConnection();
                if(CheckUsername(user))
                {
                    Username = user;
                } else
                {
                    throw new Exception("Those records do not match our database.");
                }
                
                
                


                if (ChkPassword(password))
                {
                    PwCorrect = true;
                    Permission();
                    DefaultStore = GetStore();
                }
                else
                {
                    IsManager = false;
                    CurEmp = false;
                    PwCorrect = false;
                    throw new Exception("Those records do not match our database.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        /* SQL.Cleanup() - No overloads.
         * 
         * Safety function. Cleans up the static class for logout (but not program exit). 
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
            CurEmp = false;
        }

        /* User (Employee Database) functions
         * 
         * these functions are for creating, and updating new user entries in the employee database
         */

        /* SQL.ChkPassword(password) - No Overloads
         * 
         * Checks the password against the database, and returns true if valid, false if not.
         * 
         * to do - add exception handling.
         * 
         * Passwords are stored with a 24bit Hash and a 24bit salt. We will pull the salt and store
         * it in a string, then generate the hash from the inputed pw and the salt, comparing that to the hash
         * (without ever storing it in a variable of its own other than the reader).
         */
        public static bool ChkPassword(string pw)
        {

            try
            {
                byte[] salt;
                string hash;
                var saltData = new DataTable();
                var hashData = new DataTable();
                saltData = GetColumn("employee", "salt", "username", Username);
                hashData = GetColumn("employee", "password", "username", Username);

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
            } catch (Exception ex)
            {
                throw ex;
            }
            

        }

        /* SQL.CreateUser(fname, lname, desiredPW) - no overloads
         * 
         * CreateUser takes the employees first and last name and desired pw.
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
            var today = DateTime.Today.ToString("yyyy-MM-dd hh:mm tt");

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

            try
            {
                string sql = "INSERT INTO employee (first_name, last_name, username, password, salt, hired, location) VALUES ('" + fName + "','" + lName + "','" + username + "','" + hashString + "','" + saltString + "','" + today + "', '" + DefaultStore + "')";

                if (SendQry(new MySqlCommand(sql)))
                {
                    return username;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            

        }

        /* SQL.CheckUsername(username) - no overloads
         * 
         * returns bool if username exists (good for creating new user to prevent duplicate usernames and error in the
         * sql insert or to make sure the username exists before calling the database for the passwordhash
         */
        public static bool CheckUsername(string username)
        {
            try
            {
                if (GetColumn("employee", "username", username).Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            } catch (Exception ex)
            {
                throw ex;
            }
            
        }

        /* SQL.ChangePassword(newPW) - no overloads
         * 
         * Method for changing the pw in the database.
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



            string sql = "UPDATE employee SET password = '" + hashString + "', salt = '" + saltString + "' WHERE username = '" + Username + "';";

            if (SendQry(new MySqlCommand(sql)))
            {
                return true;
            }
            else
            {
                return false;
            }



        }

        /* SQL.GetEmpNumber(username)
         * 
         * returns an employee number. 
         * 
         * important because for safety reasons (to prevent errors or miss data) we use the emp number for all
         * update and modification functions. The empNum is the Primary Key in the database, unique, and so much
         * less trouble than possibly mispelling usernames.
         *
         * 
         */

        public static int GetEmpNum(string user)
        {
            try
            {
                int.TryParse(GetColumn("employee", "emp_num", "username", user).Rows[0]["emp_num"].ToString(), out int empNum);

                
                if (empNum ==0)
                {
                    throw new Exception("No employee found.");
                } else
                {
                    return empNum;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            
        }

        public static void UpdateFname(int empNum, string fname)
        {
            
            try
            {
                string sql = "UPDATE employee SET first_name = '" + fname + "' WHERE emp_num = '" + empNum + "';";
                SendQry(new MySqlCommand(sql));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static void UpdateLname(int empNum, string lname)
        {
            
            try
            {
                string sql = "UPDATE employee SET last_name = '" + lname + "' WHERE emp_num = '" + empNum + "';";
                SendQry(new MySqlCommand(sql));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void UpdateStreet(int empNum, string street)
        {
            
            try
            {
                string sql = "UPDATE employee SET street = '" + street + "' WHERE emp_num = '" + empNum + "';";
                SendQry(new MySqlCommand(sql));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void UpdateCity(int empNum, string city)
        {
            
            try
            {
                string sql = "UPDATE employee SET city = '" + city + "' WHERE emp_num = '" + empNum + "';";
                SendQry(new MySqlCommand(sql));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void UpdateState(int empNum, string st)
        {
            
            try
            {
                string sql = "UPDATE employee SET state = '" + st + "' WHERE emp_num = '" + empNum + "';";
                SendQry(new MySqlCommand(sql));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void UpdateZip(int empNum, int zip)
        {
           
            try
            {
                string sql = "UPDATE employee SET zip = '" + zip + "' WHERE emp_num = '" + empNum + "';";
                SendQry(new MySqlCommand(sql));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Promote(int empNum)
        {
            
            try
            {
                string sql = "UPDATE employee SET type = 'Manager' WHERE emp_num = '" + empNum + "';";
                SendQry(new MySqlCommand(sql));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Demote(int empNum)
        {
            
            try
            {
                string sql = "UPDATE employee SET type = 'Employee' WHERE emp_num = '" + empNum + "';";
                SendQry(new MySqlCommand(sql));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Changestore(int empNum, string store)
        {
            
            try
            {
                string sql = "UPDATE employee SET location = '" + store + "' WHERE emp_num = '" + empNum + "';";
                SendQry(new MySqlCommand(sql));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void ChangePay(int empNum, double newPay)
        {
            
            try
            {
                string sql = "UPDATE employee SET pay = '" + newPay + "' WHERE emp_num = '" + empNum + "';";
                SendQry(new MySqlCommand(sql));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void Terminate(int empNum)
        {
            
            
            try
            {
                var today = DateTime.Today.ToString("yyyy-MM-dd hh:mm tt");
                string sql = "UPDATE `employee` SET `type` = 'Terminated', `terminated` = '" + today + "' WHERE (`emp_num` = '" + empNum + "');";
                SendQry(new MySqlCommand(sql));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataTable GetEmployee(int empNum)
        {
            
            try
            {
                var data = GetTable("employee", "emp_num", empNum.ToString());
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /* Order Database Functions
         * 
         * these functions all have to do with retrieving and viewing orders
         * 
         */


        /* SQL.GetOrders( ... several overloads)
         * 
         * at its basic returns orders for the default store of the user.
         * 
         * overloads can return for a different store, or for specific customers, or even specific orders
         */

        public static DataTable GetOrders()
        {
            try
            {
                string sql = "SELECT * FROM order_history WHERE location = '" + DefaultStore + "';";
                return SelectQry(sql);
            } catch (Exception ex)
            {
                throw ex;
            }
            

        }

        public static DataTable GetOrders(string store)
        {
            try
            {
                string sql = "SELECT * FROM order_history WHERE location = '" + store + "';";
                return SelectQry(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            


        }

        public static DataTable GetOrders(string store, string email)
        {
            try
            {
                string acctNum = GetAcctNum(email).ToString();
                string sql = "SELECT * FROM order_history WHERE location = '" + store + "' AND email = '"+acctNum+"';";
                return SelectQry(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            


        }

        public static DataTable GetOrders(int custNum)
        {

            try
            {
                string sql = "SELECT * FROM order_history WHERE acct_num = '" + custNum + "';";
                return SelectQry(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static DataTable GetOrders(string store, int custNum)
        {
            try
            {
                string sql = "SELECT * FROM order_history WHERE location = '" + store + "' AND acct_num = '"+custNum+"';";
                return SelectQry(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /*
         * valid status:
         * 
         * Ordered, Recieved, Processed, Out for Delivery, Delivered, Canceled
         * 
         * Paid, Not Paid
         */
        public static void ChangeStatus(int orderNum, string status)
        {

        }

        public static void ChangeDate(int orderNum, string date)
        {

        }

        public static void ChangeDelAddy(int orderNum, string address)
        {

        }

        public static void AddItem(int orderNum, int itemNum, int qty)
        {

        }

        public static void AdjustQty(int orderNum, int itemNum, int qty)
        {

        }

        public static int GetAcctNum(string email)
        {
            try
            {
                int.TryParse(GetColumn("customer", "acct_num", "email", email).Rows[0]["acct_num"].ToString(), out int acctNum);


                if (acctNum == 0)
                {
                    throw new Exception("No employee found.");
                }
                else
                {
                    return acctNum;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static int GetAcctNum(string fName, string lName)
        {
            try
            {
                string sql = "SELECT acct_num FROM customer WHERE first_name ='" + fName + "' AND last_name = '" + lName + "';";
                int.TryParse(SelectQry(sql).Rows[0]["acct_num"].ToString(), out int acctNum);


                if (acctNum == 0)
                {
                    throw new Exception("No employee found.");
                }
                else
                {
                    return acctNum;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /* Store Inventory Functions
         * 
         * these functions deal with the inventory database. 
         * 
         * they DO NOT deal with the item database - that stuff is NOT adjusted by the store app
         * and at this point is to be dealt with by HQ when new items are brought to sale.
         * 
         */

        /* SQL.GetInventory( ... several overloads)
         * 
         * at its basic returns orders for the default store of the user.
         * 
         * overloads can return for a different store, or for specific items
         */
        public static DataTable GetInventory()
        {
            try
            {
                return SelectQry("SELECT items.item, items.item_name, items.purchase_cost, items.sell_cost, items.category, store_inventory.qty FROM items  INNER JOIN store_inventory WHERE store_inventory.item_id = items.item_id AND location = '" + DefaultStore + "'; ");
            }
            catch(Exception ex)
            {
                throw ex;
            }
            


           

        }

        public static DataTable GetInventory(string store)
        {
            try
            {
                return SelectQry("SELECT items.item, items.item_name, items.purchase_cost, items.sell_cost, items.category, store_inventory.qty FROM items  INNER JOIN store_inventory WHERE store_inventory.item_id = items.item_id AND location = '" + store + "'; ");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            


        }

        public static DataTable GetInventory(string store, int itemNum)
        {
            try
            {
                
                return SelectQry("SELECT items.item, items.item_name, items.purchase_cost, items.sell_cost, items.category, store_inventory.qty FROM items  INNER JOIN store_inventory WHERE store_inventory.item_id = items.item_id AND location = '" + store + "' AND items.item_id = '"+itemNum+"'; ");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            

        }

        public static int GetItemId(string itemName)
        {
            try
            {
                var data = new DataTable();
                string sql = "SELECT item_id FROM items WHERE item_name ='" + itemName + "';";
                try
                {
                    data = SelectQry(sql);
                }
                catch 
                {
                    throw new Exception("No item found with that name");
                }
                int.TryParse(data.Rows[0]["item_id"].ToString(), out int item_id);

                return item_id;
            } catch(Exception ex)
            {
                throw ex;
            }
        }

        /* SQL.ChangeQty(itemNum, qty)
         * 
         * this updates the item qty at the store - positive or negative - and spits out an error message if the
         * item is going to go negative.
         * 
         * autmatically uses the users default store. CANNOT MODIFY OTHER STORES INVENTORY
         */
        public static void ChangeQty(int itemNum, int qty)
        {
            string sql= "UPDATE store_inventory SET qty = '"+qty+"' WHERE item_id='"+itemNum+"' AND location = '"+DefaultStore+"';";
           
            try
            {
                if(SendQry(new MySqlCommand(sql)))
                {
                    Console.WriteLine("Command sent, Qty Updated.");
                }
            } catch (Exception ex)
            {
                throw ex;
            }

        }

        /* SQL.ItemChgStore(itemNum, store, amount)
         * 
         * moves an amount of item from default store to new store
         * 
         */

        public static void ItemChgStore(int itemName, string store, int amt)
        {

        }



        /* Accounting methods
         * 
         * these methods handle the Accts Rec, Accts Payable, and basic financial reporting tools for the
         * default store. They ONLY affect the default store, and cannot be used to see other stores information
         */

        /*SQL.GetAcRec(... several overloads
         * 
         * this method retrieves the accounts recievables for the store, or a specific one, or a specific users)
         */
        public static DataTable GetAcRec()
        {
            var data = new DataTable();


            return data;

        }

        /* valid types are invoice and account
         */
        public static DataTable GetAcRec(int idNum, string type)
        {
            var data = new DataTable();


            return data;

        }

        public static DataTable GetAcRec(string startDate, string endDate)
        {
            var data = new DataTable();


            return data;

        }

        public static int AmtOwed(int invoiceNum)
        {
            int owed = 0;
            return owed;
        }

        public static void AddPay(int invoiceNum, int pmt)
        {

        }

        public static void NewAcRec(int acctNum, int orderNum, int owed)
        {

        }

        public static DataTable AcctPay()
        {
            var data = new DataTable();


            return data;

        }

        public static DataTable AcctPay(int id)
        {
            var data = new DataTable();


            return data;

        }

        public static DataTable AcctPay(string vendor, int invoiceNum)
        {
            var data = new DataTable();


            return data;

        }

        public static DataTable AcctPay(string vendor)
        {
            var data = new DataTable();


            return data;

        }

        public static DataTable AcctPay(string startDate, string endDate)
        {
            var data = new DataTable();


            return data;

        }

        public static int AcctPayOwed(int id)
        {
            int owed = 0;
            return owed;
        }

        public static int AcctPayOwed(string vendor, int invoiceNum)
        {
            int owed = 0;
            return owed;
        }

        public static void AcctPayPmt(string vendor, int invoiceNum, int pmt)
        {

        }

        /* still Accounting Methods - but these are the Report Generation Methods
         * 
         * again - these all only deal with the current store of the logged in manager.
         * 
         * this information can be provided to corporate account department for corporate level reports
         */

        public static DataTable GeneralLedger()
        {
            var data = new DataTable();


            return data;

        }

        public static DataTable ProfitReport()
        {
            var data = new DataTable();


            return data;

        }

        public static DataTable LossReport()
        {
            var data = new DataTable();


            return data;

        }

        public static DataTable CashReport()
        {
            var data = new DataTable();


            return data;

        }

        public static void RunReports()
        {

        }









        /*Private Internal Functions */

        /* GetStore() - no overloads - private - internal use Only
         * 
         * used to get the store of the logged in account.
         */

        private static string GetStore()
        {
            try
            {
                var data = new DataTable();
                data = GetColumn("employee", "location", "username", Username);

                string store = data.Rows[0]["location"].ToString();
                return store;

            } catch (Exception ex)
            {
                throw ex;
            }
            
        }
        
        /* SetupConnection() - Internal, Private, No Overloads
         * 
         * This function reads in the connection data from the file in the resources and adds its information to the 
         * properties needed for the connection builder.
         * 
         * to do: encrypt and decrypt the file for additional protection
         */

        private static void SetupConnection()
        {

            string[] rows = Resources.connection.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            /* I acknowledge this is a dirty way to do this. There is no error checking to make sure that the
             * file is in the correct order, nor is it a deliniated list. This is fast and effective for the moment
             * mostly being used as a current method until I figure out how to encrypt that plain text resource file :/
             */
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
                Port = 3306,
                AllowZeroDateTime = true

            };

            cnnStr = cnn;
        }

        /* Permission() - No Overloads - Private - Internal use Only
         * 
         * This function can be used to determine if the current employee has permission to access
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

            string sql = "SELECT type FROM employee WHERE username = '" + Username + "';";
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
                            CurEmp = true;

                        } else if (rdr.GetString(0) == "Terminated")
                        {
                            CurEmp = false;
                            IsManager = false;
                        }
                        else
                        {
                            IsManager = false;
                            CurEmp = true;
                        }
                        



                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                    throw ex;
                }
                finally
                {
                    if (cnn != null) cnn.Close();
                }



            }


        }

        /* GetOrderItems(ordernum) - no overloads - private - internal use only
         * 
         * for retrieving the line items out of the order_items database.
         * 
         * only used in the GetOrder functions - no use for just the line items without the additional
         * order information in the order_history database
         * 
         */

        private static DataTable GetOrderItems(int orderNum)
        {
            var data = new DataTable();

            return data;
        }





        /* Generic SQL functions to Follow
         * 
         * The goal is to not use these functions outside of the class. Because these are some of the generic connection
         * types were going to need over and over, then they are functions. 
         * 
         *
         * 
         * they are available for public use for edge cases however
         */


        /* SQL.GetTable(... Multiple overloads)
         * 
         * This function is designed to get any table from the connection.
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
            try
            {
                string sql = "SELECT * FROM " + tableName;
                return SelectQry(sql);
            } catch (Exception ex)
            {
                throw ex;
            }
            


        }
        static public DataTable GetTable(string tableName, string orderBy)
        {
            try
            {
                string sql = "SELECT * FROM " + tableName + "ORDER BY" + orderBy + ";";
                return SelectQry(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }
        static public DataTable GetTable(string tableName, string whCol, string whVal)
        {
            try
            {
                string sql = "SELECT * FROM " + tableName + " WHERE " + whCol + "=" + whVal + ";";
                return SelectQry(sql);
            }
            catch (Exception ex)
            {
                throw ex;
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
         * does not currently work with BETWEEN query (tho you could jury rig it with
         * FROM table BETWEEN value=start, value=end; as one string in the tableName.
         * 
         * don't do this. just use sendqry instead.
         */
        static public DataTable GetColumn(string tableName, string colName)
        {
            try
            {
                string sql = "SELECT " + colName + " FROM " + tableName + ";";
                return SelectQry(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            


        }
        static public DataTable GetColumn(string tableName, string colName, string whereVal)
        {
            try
            {
                string sql = "SELECT " + colName + " FROM " + tableName + " WHERE " + colName + "= '" + whereVal + "' ;";
                return SelectQry(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            


        }

        static public DataTable GetColumn(string tableName, string colName, string whereCol, string whereVal)
        {
            try
            {
                string sql = "SELECT " + colName + " FROM " + tableName + " WHERE " + whereCol + "= '" + whereVal + "' ;";
                return SelectQry(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            

        }

        /* SendQry(2 overloads)
         * 
         * 
         * For Executing non table getting queries (INSERT, ALTER, UPDATE)
         * 
         * takes a string, and puts it into a MySqlCommand 
         * 
         * or
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
                    throw ex;
                }
                finally
                {
                    if (cnn != null) cnn.Close();
                }

            }

        }

        static public bool SendQry(string sql)
        {

            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {


                    //logging
                    Console.WriteLine("Connecting... ");

                    // assigns the connection to the command, so when executeNonQry fires it knows what connection to use
                    var cmd = new MySqlCommand(sql, cnn);
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
                    throw ex;
                }
                finally
                {
                    if (cnn != null) cnn.Close();
                }

            }

        }


        /* SelectQry(string), no overloads
         * 
         * this is the most basic function here, returns a datatable from a Select string.
         * 
         * all GetTable and GetColumn functions use it, they just build the string for you
         * 
         * this is public in case need something that doesn't fit in the GetTable, GetColumn overloads
         */

        static public DataTable SelectQry(string sql)
        {
            var tableData = new DataTable();
            using (var cnn = new MySqlConnection(cnnStr.ConnectionString))
            {

                try
                {
                    

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
                    throw ex;
                }
                finally
                {
                    if (cnn != null) cnn.Close();
                }

            }
        }

    }



}

