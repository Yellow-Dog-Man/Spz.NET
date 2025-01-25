using System.CommandLine;
using ByteSizeLib;
using Spectre.Console;
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
        
        Option<string?> output = new(
            ["--output", "-o"],
            "Path to the output file (.ply or .spz supported)");
        root.AddOption(output);

        root.SetHandler(Execute, input, output);

        return root.Invoke(args);
    }

    public static void Execute(string inputFile, string? outputFile)
    {
        inputFile = Path.GetFullPath(inputFile);
        outputFile = Path.GetFullPath(outputFile ?? Path.GetFileNameWithoutExtension(inputFile) + (Path.GetExtension(inputFile) == ".ply" ? ".spz" : ".ply"));

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

        ByteSize inputFileSize = default;
        ByteSize outputFileSize = default;

        AnsiConsole.Status().Start("Processing...", ctx =>
        {
            if (inputExtension == ".ply")
            {
                DemoLogger.WriteLine("Reading ply...");
                cloud = SplatSerializer.FromPly(inputFile);
            }
            else if (inputExtension == ".spz")
            {
                DemoLogger.WriteLine("Decompressing spz...");
                var spz = SplatSerializer.FromSpz(inputFile);
                cloud = spz.Unpack();
            }
            else
                throw new NullReferenceException($"Something terrible has happened and I think you should panic.");

            
            if (outputExtension == ".ply")
            {
                DemoLogger.WriteLine("Writing ply...");
                SplatSerializer.ToPly(cloud, outputFile);
            }
            else if (outputExtension == ".spz")
            {
                DemoLogger.WriteLine("Compressing spz...");
                SplatSerializer.ToSpz(cloud.Pack(), outputFile);
            }

            FileInfo inputInfo = new(inputFile);
            FileInfo outputInfo = new(outputFile);

            inputFileSize = ByteSize.FromBytes(inputInfo.Length);
            outputFileSize = ByteSize.FromBytes(outputInfo.Length);
        });

        ByteSize outputDiff = outputFileSize - inputFileSize;
        int diffSign = Math.Sign(outputDiff.Bytes);

        string compressMsg = diffSign switch
        {
            -1 => "[green bold]Compressed[/]",
            0  => "[yellow bold]No size difference[/]",
            1  => "[red bold]Decompressed[/]",
            _  => ""
        };

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"""
            [green bold]:check_mark:[/]  Done!

            Wrote file: "{outputFile}"

            Input size: {inputFileSize}
            Output size: {outputFileSize}

            {compressMsg}: {outputDiff} ({Math.Abs(1.0 - (outputFileSize / inputFileSize).Bytes):p2})
            """);
    }
}



public static class DemoLogger
{
    public static string LogPrefix(int logLevel)
    {
        return logLevel switch
        {
            0 => "[green bold][[INFO]][/]",
            1 => "[yellow bold][[WARN]][/]",
            2 => "[red bold][[ERROR]][/]",
            _ => throw new NotImplementedException("Bad log level. >:(")
        };
    }


    public static void WriteLine(object? msg, int logLevel = 0)
    {
        string? msgStr = msg?.ToString();
        AnsiConsole.MarkupLine($"{LogPrefix(logLevel)} {msgStr?.EscapeMarkup()}");
    }
}