using System;
using System.Diagnostics;

namespace ChatDisentanglement
{
    class Program
    {
        static void Main(string[] args)
        {
            Debug.WriteLine(args.Length + " ");

            if (args.Length != 3)
            {
                Debug.WriteLine("Arguments Required.");
            }

            string train_path = args[0];
            string test_path = args[1];
            int threshold = Int32.Parse(args[2]);

            var Model = new model();
            Model.train(train_path, test_path, threshold);
        }
    }
}
