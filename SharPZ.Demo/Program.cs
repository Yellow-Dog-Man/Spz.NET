using System.IO.Compression;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using SharPZ;

namespace SharPZ.Demo;

class Program
{
    static void Main(string[] args)
    {
        Quaternion realQuat = Quaternion.CreateFromYawPitchRoll(0.3f, -0.6f, 2.1f);
        QuantizedQuat quantizedQuat = Quaternion.CreateFromYawPitchRoll(0.3f, -0.6f, 2.1f);

        

        args = ["./input.spz"];
        string input;
        if (args.Length > 0)
            input = Path.GetFullPath(args[0]);
        else
        {
            Console.WriteLine("An input file is required.");
            return;
        }
            

        if (!Directory.Exists(Path.GetDirectoryName(input)))
        {
            Console.WriteLine("Path doesn't exist!");
            return;
        }

        string extension = Path.GetExtension(input);

        if (extension == ".ply")
        {
            var cloud = SplatSerializer.FromPly(input);
            cloud.ToPly("./output.ply");
        }
        else if (extension == ".spz")
        {
            var cloud = SplatSerializer.FromSpz(input);
            cloud.Unpack().ToPly("./output.ply");
        }


    }
}
