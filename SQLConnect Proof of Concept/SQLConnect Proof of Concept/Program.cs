using System;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace SQLConnect_Proof_of_Concept
{
    class Program
    {
        static void Main(string[] args)
        {

            string connectionString = GetConnectionString();

            using (SqlConnection connection = new SqlConnection())
            {
                connection.ConnectionString = connectionString;

                connection.Open();

                Console.WriteLine("State: {0}", connection.State);
                Console.WriteLine("ConnectionString: {0}",
                    connection.ConnectionString);

                /*
                string createQry = @"CREATE TABLE test_table(USERID int,NAME txt, LASTNAME txt, PRIMARY KEY( USERID ));";
                string insertQry = @"INSERT";
                */
            }
        }

        static private string GetConnectionString() 
        {
            // To avoid storing the connection string in code, 
            // you can retrieve it from a configuration file.

            // keeps the connection string outside of the main program.

            return @"Server = 192.168.1.98\MySQL80; User ID = testAccount; Password = GreenFlowerColorWater2;Pooling=false;";
            //return @"Data Source=192.168.1.98,3306;User ID=testAccount;Password=GreenFlowerColorWater2";
        }
       

           
        
    }
}
