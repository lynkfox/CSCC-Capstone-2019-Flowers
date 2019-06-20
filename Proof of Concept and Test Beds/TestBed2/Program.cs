using System;

namespace TestBed2
{
    class Program
    {
        static void Main(string[] args)
        {
            string fname, lname, pw, username;


            /*
            Console.WriteLine("First Name: ");
            fname = Console.ReadLine();
            Console.WriteLine("Last Name: ");
            lname = Console.ReadLine();
            Console.WriteLine("Desired PW: ");
            pw = Console.ReadLine();

            username = CcnSession.CcnSession.CreateUser(fname, lname, pw);

            if (username != null)
                Console.WriteLine("Success: Username is " + username);
            else
                Console.WriteLine("Something went wrong)");
            */

            

            Console.WriteLine("Username: ");
            username = Console.ReadLine();
            Console.WriteLine("PW: ");
            pw = Console.ReadLine();

            CcnSession.CcnSession.Setup(username, pw);

            if (CcnSession.CcnSession.PwCorrect)
                Console.WriteLine("Success. Correct PW");
            else
                Console.WriteLine("Failure. Wrong PW");

            Console.ReadLine();
            





            
        }
    }
}
