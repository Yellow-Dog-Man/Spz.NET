using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using ByteSizeLib;
using Spectre.Console;
using Spz.NET;
using Spz.NET.Serialization;

namespace Spz.NET.Demo;

class Program
{
    public static string[] SUPPORTED_EXTENSIONS = [".ply", ".spz"];
    public static string LogFolder;
    public static string LogFile;
    public static StreamWriter? LogWriter;

    static Program()
    {
        LogFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Logs");
        LogFile = Path.Combine(LogFolder, DateTime.Now.ToString("yyyy-mm-dd_hh-mm-ss-fff") + ".log");
    }

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
        if (!Directory.Exists(LogFolder))
            Directory.CreateDirectory(LogFolder);
        
        LogWriter = File.AppendText(LogFile);
        DemoLogger.OnLog += LogMsgFile;

        inputFile = Path.GetFullPath(inputFile);
        outputFile = Path.GetFullPath(outputFile ?? Path.GetFileNameWithoutExtension(inputFile) + (Path.GetExtension(inputFile) == ".ply" ? ".spz" : ".ply"));

        if (!Directory.Exists(Path.GetDirectoryName(inputFile)))
        {
            DemoLogger.Error("The specified path to the input file does not exist.");
            return;
        }

        if (!File.Exists(inputFile))
        {
            DemoLogger.Error("The input file does not exist within the specified path.");
            return;
        }

        if (!Directory.Exists(Path.GetDirectoryName(outputFile)))
        {
            DemoLogger.Error("The specified path to the output file does not exist.");
            return;
        }

        if (File.Exists(outputFile))
        {
            DemoLogger.Warn($"\"{outputFile}\" already exists!");
            bool overwriteOutput = AskFileOverwrite(outputFile);

            if (!overwriteOutput)
            {
                DemoLogger.Log("User has opted to not overwrite output. Aborting.");
                return;
            }
            
            DemoLogger.Warn("Proceeding with file overwrite.");
            
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

        Stopwatch watch = new();
        AnsiConsole.Status().Start("Processing...", ctx =>
        {
            watch.Start();
            Task.Run(async () =>
            {
                while (watch.IsRunning)
                {

                    ctx.Status = $"Processing... {watch.Elapsed:mm\\:ss}";
                    await Task.Delay(10);
                }
            });

            if (inputExtension == ".ply")
            {
                DemoLogger.Log("Reading ply...");
                cloud = SplatSerializer.FromPly(inputFile);
            }
            else if (inputExtension == ".spz")
            {
                DemoLogger.Log("Decompressing spz...");
                cloud = SplatSerializer.FromSpz(inputFile);
            }
            else
                throw new NullReferenceException($"Something terrible has happened and I think you should panic.");

            
            if (outputExtension == ".ply")
            {
                DemoLogger.Log("Writing ply...");
                SplatSerializer.ToPly(cloud, outputFile);
            }
            else if (outputExtension == ".spz")
            {
                DemoLogger.Log("Compressing spz...");
                SplatSerializer.ToSpz(cloud, outputFile);
            }
            watch.Stop();
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

        DemoLogger.Log($"""
            [green bold]:check_mark:[/]  Done!

            Elapsed time: {watch.Elapsed:mm\:ss}

            Wrote file: "{outputFile}"

            Input size: {inputFileSize}
            Output size: {outputFileSize}

            {compressMsg}: {outputDiff} ({Math.Abs(1.0 - (outputFileSize / inputFileSize).Bytes):p2})
            """);
        
        LogWriter.Flush();
        LogWriter.Dispose();

        AnsiConsole.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }


    public static void LogMsgFile(string msgFormatted, object? msg, int logLevel, bool escapeMarkup)
    {
        string textLogMsg = Emoji.Replace(Markup.Remove(msgFormatted));
        LogWriter?.WriteLine(textLogMsg);
    }



    static bool AskFileOverwrite(string path)
    {
        string answer = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"Would you like to overwrite \"{path}\"?")
                .AddChoices(
                    "No",
                    "Yes"
                )
        );

        return answer == "Yes";
    }
}



public static class DemoLogger
{
    static DemoLogger()
    {
        OnLog += LogMsgConsole;
    }
    public static event Action<string, object?, int, bool>? OnLog;
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


    public static void LogMsg(object? msg = null, int logLevel = 0, bool escapeMarkup = false)
    {
        string msgFormatted = FormatMsg(msg, logLevel, escapeMarkup);
        OnLog?.Invoke(msgFormatted, msg, logLevel, escapeMarkup);
    }


    static void LogMsgConsole(string formatted, object? msg, int logLevel, bool escapeMarkup = false) => AnsiConsole.MarkupLine(FormatMsg(msg, logLevel, escapeMarkup));

    public static string FormatMsg(object? msg = null, int logLevel = 0, bool escapeMarkup = false)
    {
        string msgStr = msg?.ToString() ?? "";
        string msgFormatted = escapeMarkup ? Markup.Remove(msgStr) : msgStr;
        DateTime curTime = DateTime.Now;
        string time = $"[grey italic]{curTime.ToString("hh:mm:ss.ffff tt", CultureInfo.InvariantCulture).EscapeMarkup()}[/]";
        string logPrefix = LogPrefix(logLevel);
        

        return $"{time} {logPrefix} {msgFormatted}";
    }

    public static void Log(object? msg = null, bool escapeMarkup = false) => LogMsg(msg, 0, escapeMarkup);

    public static void Warn(object? msg = null, bool escapeMarkup = false) => LogMsg(msg, 1, escapeMarkup);

    public static void Error(object? msg = null, bool escapeMarkup = false) => LogMsg(msg, 2, escapeMarkup);
}