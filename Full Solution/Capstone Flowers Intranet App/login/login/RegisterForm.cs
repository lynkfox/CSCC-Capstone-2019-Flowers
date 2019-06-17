using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace login
{
    public partial class RegisterForm : Form
    {
        public RegisterForm()
        {
            InitializeComponent();
        }
            private void RegisterForm_Load(object sender, EventArgs e)
            {
                // remove the focus from the textboxes
                this.ActiveControl = label1;
            }

            private void textBoxFirstname_Enter(object sender, EventArgs e)
            {
                String fname = textBoxFirstname.Text;
                if (fname.ToLower().Trim().Equals("first name"))
                {
                    textBoxFirstname.Text = "";
                    textBoxFirstname.ForeColor = Color.Black;
                }
            }

            private void textBoxFirstname_Leave(object sender, EventArgs e)
            {
                String fname = textBoxFirstname.Text;
                if (fname.ToLower().Trim().Equals("first name") || fname.Trim().Equals(""))
                {
                    textBoxFirstname.Text = "first name";
                    textBoxFirstname.ForeColor = Color.Gray;
                }
            }

            private void textBoxLastname_Enter(object sender, EventArgs e)
            {
                String lname = textBoxLastname.Text;
                if (lname.ToLower().Trim().Equals("last name"))
                {
                    textBoxLastname.Text = "";
                    textBoxLastname.ForeColor = Color.Black;
                }
            }

            private void textBoxLastname_Leave(object sender, EventArgs e)
            {
                String lname = textBoxLastname.Text;
                if (lname.ToLower().Trim().Equals("last name") || lname.Trim().Equals(""))
                {
                    textBoxLastname.Text = "last name";
                    textBoxLastname.ForeColor = Color.Gray;
                }
            }

          

            private void textBoxPassword_Enter(object sender, EventArgs e)
            {
                String password = textBoxPassword.Text;
                if (password.ToLower().Trim().Equals("password"))
                {
                    textBoxPassword.Text = "";
                    textBoxPassword.UseSystemPasswordChar = true;
                    textBoxPassword.ForeColor = Color.Black;
                }
            }

            private void textBoxPassword_Leave(object sender, EventArgs e)
            {
                String password = textBoxPassword.Text;
                if (password.ToLower().Trim().Equals("password") || password.Trim().Equals(""))
                {
                    textBoxPassword.Text = "password";
                    textBoxPassword.UseSystemPasswordChar = false;
                    textBoxPassword.ForeColor = Color.Gray;
                }
            }

            private void textBoxPasswordConfirm_Enter(object sender, EventArgs e)
            {
                String cpassword = textBoxPasswordConfirm.Text;
                if (cpassword.ToLower().Trim().Equals("confirm password"))
                {
                    textBoxPasswordConfirm.Text = "";
                    textBoxPasswordConfirm.UseSystemPasswordChar = true;
                    textBoxPasswordConfirm.ForeColor = Color.Black;
                }
            }

            private void textBoxPasswordConfirm_Leave(object sender, EventArgs e)
            {
                String cpassword = textBoxPasswordConfirm.Text;
                if (cpassword.ToLower().Trim().Equals("confirm password") ||
                    cpassword.ToLower().Trim().Equals("password") ||
                    cpassword.Trim().Equals(""))
                {
                    textBoxPasswordConfirm.Text = "confirm password";
                    textBoxPasswordConfirm.UseSystemPasswordChar = false;
                    textBoxPasswordConfirm.ForeColor = Color.Gray;
                }
            }

            private void labelClose_Click(object sender, EventArgs e)
            {
                //this.Close();
                Application.Exit();
            }


            private void labelClose_MouseEnter(object sender, EventArgs e)
            {
                labelClose.ForeColor = Color.Black;
            }

            private void labelClose_MouseLeave(object sender, EventArgs e)
            {
                labelClose.ForeColor = Color.White;
            }

        private void buttonCreateAccount_Click(object sender, EventArgs e)
        {
            // add a new user

            string fName = textBoxFirstname.Text;
            string lName = textBoxLastname.Text;
            string password = textBoxPassword.Text;
            string store = list_store.Text;


            // outputs the date in format equal to the rest of the table
            var today = DateTime.Today.ToString("yyyy-MM-dd");

            // automatically create the username as first initial, last name - all lowercase
            string username = fName[0] + lName;
            username = username.ToLower();



            /* if there happen to be 2 people with the same first initial/last name combo
             * then this section will add a number to the end of the username.
             */

            int i = 1;

            while(checkUsername(username))
            {

                

                if (i>1 && i<10)
                {
                    /* if there happen to be more than 2 people with the same first initial, last name
                     * then we remove the 1 (the last char fo the string) and add the new incrimimented i
                     * to the username (so username2, then username3, ect)
                     */
                    username = username.Substring(0, username.Length - 1);
                } else if (i>=10)
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

            //Set up the SQL insertion string.

            string sql = "INSERT INTO EMPLOYEE (first_name, last_name, username, password, hired, location) VALUES ('" + fName + "','" + lName + "','" + username + "','" + password + "','" + today + "','" + store+"')";

            

            //Check if Default Values are left in any of the boxes
            if (!checkTextBoxesValues())
            {
                //check if password and confirm pw match
                // check if the password equal the confirm password
                if (textBoxPassword.Text.Equals(textBoxPasswordConfirm.Text))
                {
                    //make sure the command returns true (ie: at least 1 row was affected)
                    if(CcnSession.SendQry(new MySqlCommand(sql)))
                    {
                        MessageBox.Show("Your Account Has Been Created. Your username is " +username+ ". Please remember this for your records.", "Account Created", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        //Return to the Login Screen
                        this.Hide();
                        LoginForm loginform = new LoginForm();
                        loginform.Show();
                    }else
                    {
                        MessageBox.Show("Unable to make a connection at this time. Please try again later.", "General Fault", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                    }

                    
                } else
                {
                    MessageBox.Show("Wrong Confirmation Password", "Password Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                }

            } else
            {
                MessageBox.Show("Enter Your Informations First", "Empty Data", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
            }


            /* original code, left for archive purposes
                DB db = new DB();
                MySqlCommand command = new MySqlCommand("INSERT INTO `Login_SignUP`(`firstname`, `lastname`, `emailaddress`, `username`, `password`) VALUES (@fn, @ln, @email, @usn, @pass)", db.getConnection());

                command.Parameters.Add("@fn", MySqlDbType.VarChar).Value = textBoxFirstname.Text;
                command.Parameters.Add("@ln", MySqlDbType.VarChar).Value = textBoxLastname.Text;
                command.Parameters.Add("@email", MySqlDbType.VarChar).Value = textBoxEmail.Text;
                command.Parameters.Add("@usn", MySqlDbType.VarChar).Value = textBoxUsername.Text;
                command.Parameters.Add("@pass", MySqlDbType.VarChar).Value = textBoxPassword.Text;

                // open the connection
                db.openConnection();

                // check if the textboxes contains the default values 
                if (!checkTextBoxesValues())
                {
                    // check if the password equal the confirm password
                    if (textBoxPassword.Text.Equals(textBoxPasswordConfirm.Text))
                    {
                        // check if this username already exists
                        if (checkUsername())
                        {
                            MessageBox.Show("This Username Already Exists, Select A Different One", "Duplicate Username", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                        }
                        else
                        {
                            // execute the query
                            if (command.ExecuteNonQuery() == 1)
                            {
                                MessageBox.Show("Your Account Has Been Created", "Account Created", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("ERROR");
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Wrong Confirmation Password", "Password Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                    }

                }
                else
                {
                    MessageBox.Show("Enter Your Informations First", "Empty Data", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                }



                // close the connection
                db.closeConnection();

            */





            }


            // check if the username already exists
            public bool checkUsername(string username)
        {

            if (CcnSession.GetColumn("EMPLOYEE", "username", username).Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
            /* original code, kept for archive purposes
                DB db = new DB();

                String username = textBoxUsername.Text;

                DataTable table = new DataTable();

                MySqlDataAdapter adapter = new MySqlDataAdapter();

                MySqlCommand command = new MySqlCommand("SELECT * FROM `Login_SignUP` WHERE `username` = @usn", db.getConnection());

                command.Parameters.Add("@usn", MySqlDbType.VarChar).Value = username;

                adapter.SelectCommand = command;

                adapter.Fill(table);

                // check if this username already exists in the database
                if (table.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
                */



        }

            // check if the textboxes contains the default values
            public Boolean checkTextBoxesValues()
            {
                String fname = textBoxFirstname.Text;
                String lname = textBoxLastname.Text;
                String pass = textBoxPassword.Text;
            String store = list_store.Text;

                if (fname.Equals("first name") || lname.Equals("last name") ||
                    store.Equals("0000 - Please Select a Store")
                    || pass.Equals("password"))
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            private void labelGoToLogin_MouseEnter(object sender, EventArgs e)
            {
                labelGoToLogin.ForeColor = Color.Yellow;
            }

            private void labelGoToLogin_MouseLeave(object sender, EventArgs e)
            {
                labelGoToLogin.ForeColor = Color.White;
            }

            private void labelGoToLogin_Click(object sender, EventArgs e)
            {
                this.Hide();
                LoginForm loginform = new LoginForm();
                loginform.Show();
            }


    }
    }
