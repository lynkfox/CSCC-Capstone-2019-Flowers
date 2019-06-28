using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            string store="1001-Col";
            for(int i = 1; i<=20; i++)
            {
                for(int z = 0; z<6; z++)
                {
                    switch(z)
                    {
                        case 0:
                            store = "2000-Pitt";
                            break;
                        case 1:
                            store = "2001-Pitt";
                            break;
                        case 2:
                            store = "3000-Det";
                            break;
                        case 3:
                            store = "3001-Det";
                            break;
                        case 4:
                            store = "4000-Indy";
                            break;
                        case 5:
                            store = "4001-Indy";
                            break;

                    }

                    int y = RandomNumber(100, 500);
                    Console.WriteLine("INSERT INTO `capstoneFlowers`.`store_inventory` (`item_id`,`qty`,`location`) VALUES(" + i + ", " + y + ", '" + store + "');");
                }
                
            }
        }

        
        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }
    }
}
