using System.CommandLine;
using Spz.NET;
using Spz.NET.Serialization;

namespace Spz.NET.Demo;

class Program
{
    public static string[] SUPPORTED_EXTENSIONS = [".ply", ".spz"];

    static int Main(string[] args)
    {
        RootCommand root = new("Demonstrates converting between .ply and .spz files.");

        Option<string> input = new(
            ["--input", "-i"],
            "Path to the input file (.ply or .spz supported)");
        input.IsRequired = true;
        root.AddOption(input);
        
        Option<string> output = new(
            ["--output", "-o"],
            () => "./output.spz",
            "Path to the output file (.ply or .spz supported)");
        root.AddOption(output);

        root.SetHandler(Execute, input, output);

        return root.Invoke(args);
    }

    public static void Execute(string inputFile, string outputFile)
    {
        inputFile = Path.GetFullPath(inputFile);
        outputFile = Path.GetFullPath(outputFile);

        if (!Directory.Exists(Path.GetDirectoryName(inputFile)))
        {
            Console.WriteLine("The specified path to the input file does not exist.");
        }

        if (!File.Exists(inputFile))
        {
            Console.WriteLine("The input file does not exist within the specified path.");
            return;
        }


        if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
        {
            Console.WriteLine("The specified path to the output file does not exist.");
        }
        

        string inputExtension = Path.GetExtension(inputFile);
        string outputExtension = Path.GetExtension(outputFile);

        if (!SUPPORTED_EXTENSIONS.Contains(inputExtension))
        {
            Console.WriteLine($"Input file has unsupported extension: {inputExtension}");
            return;
        }

        if (!SUPPORTED_EXTENSIONS.Contains(outputExtension))
        {
            Console.WriteLine($"Output file has unsupported extension: {outputExtension}");
            return;
        }


        GaussianCloud? cloud;
        if (inputExtension == ".ply")
        {
            cloud = SplatSerializer.FromPly(inputFile);
        }
        else if (inputExtension == ".spz")
        {
            var spz = SplatSerializer.FromSpz(inputFile);
            cloud = spz.Unpack();
        }
        else
            throw new NullReferenceException($"Something terrible has happened and I think you should panic.");

        
        if (outputExtension == ".ply")
        {
            SplatSerializer.ToPly(cloud, outputFile);
        }
        else if (outputExtension == ".spz")
        {
            SplatSerializer.ToSpz(cloud.Pack(), outputFile);
        }

    }
}
