using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace Spz.NET.Helpers;

public static class BinaryHelpers
{
    // TODO: Make this less bad.
    public static string ReadLine(this BinaryReader reader)
    {
        StringBuilder builder = new();
        for (;;)
        {
            char curChar = (char)reader.ReadByte();
            if (curChar == '\n')
                break;

            builder.Append(curChar);
        }

        return builder.ToString();
    }

    public static string? GetLine(this BinaryReader reader, string text, bool exactMatch = true)
    {
        string curLine;

        do
        {
            try
            {
                curLine = reader.ReadLine();
            }
            catch (EndOfStreamException)
            {
                return "";
            }
        }
        while(exactMatch ? curLine != text : !curLine.Contains(text));

        return curLine;
    }


    public static void WriteLine(this BinaryWriter writer, string text)
    {
        byte[] textBytes = Encoding.UTF8.GetBytes(text + '\n');
        writer.Write(textBytes, 0, textBytes.Length);
    }



    // These read and write helpers were basically just lifted from the .NET source and backported. All my homies love spans.
    public static void Read(this BinaryReader reader, Span<byte> buffer)
    {
        byte[] array = ArrayPool<byte>.Shared.Rent(buffer.Length);

        try
        {
            reader.Read(array, 0, buffer.Length);
            array.AsSpan()[..buffer.Length].CopyTo(buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }


    public static void Write(this BinaryWriter writer, ReadOnlySpan<byte> buffer)
    {
        byte[] array = ArrayPool<byte>.Shared.Rent(buffer.Length);
        try
        {
            buffer.CopyTo(array);
            writer.Write(array, 0, buffer.Length);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(array);
        }
    }
}