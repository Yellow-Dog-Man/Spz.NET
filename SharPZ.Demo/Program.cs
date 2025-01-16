using System.IO.Compression;
using SharPZ;

namespace SharPZ.Demo;

class Program
{
    static void Main(string[] args)
    {
        // using FileStream spzInput = File.OpenRead("./hornedlizard.spz");
        // using GZipStream gZip = new(spzInput, CompressionMode.Decompress);

        // using FileStream uncompressed = File.OpenWrite("./hornedlizard.spzu");

        // gZip.CopyTo(uncompressed);


        Console.WriteLine(Fixed24.FromFloat(128.99f, 12));
    }
}
