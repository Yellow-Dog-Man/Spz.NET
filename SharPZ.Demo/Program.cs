using System.IO.Compression;
using System.Runtime.InteropServices;
using SharPZ;

namespace SharPZ.Demo;

class Program
{
    static void Main(string[] args)
    {
        string input = "./input.ply";

        if (args.Length > 0)
            input = Path.GetFullPath(args[0]);
            

        if (!Directory.Exists(Path.GetDirectoryName(input)))
        {
            Console.WriteLine("Path doesn't exist!");
            return;
        }

        GaussianCloud cloud = GaussianCloud.FromPly(input);


        cloud.ToPly("./output.ply");
    }
}
