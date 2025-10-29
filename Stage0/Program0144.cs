
namespace Stage0
{
    partial class Program
    {
        private static void Main(string[] args)
        {
            Welcome0144();
            Console.ReadKey();
            WelcomeYYYY();
            Console.ReadKey();
        }

        private static void WelcomeYYYY()
        {
            Console.WriteLine("Hello Mr. Nobody"); 
            Console.WriteLine("Press any key to continue");
        }

        private static void Welcome0144()
        {
            Console.WriteLine("Enter your name: ");
            string name = Console.ReadLine() ?? "ERROR";
            Console.WriteLine("{0}, welcome to my first console application", name);
            Console.WriteLine("Press any key to continue");
        }
    }
}