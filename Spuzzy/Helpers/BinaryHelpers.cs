using System.Buffers;
using System.Text;

namespace Spuzzy.Helpers;

public static class BinaryHelpers
{
    const int BUFFER_CHUNK_COUNT = 8192;
    private static readonly ArrayPool<byte> pool = ArrayPool<byte>.Create();



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



    // public static void Read(this Stream stream, Span<byte> bytes)
    // {
    //     byte[] buffer = pool.Rent(READ_CHUNK_COUNT);
    //     Span<byte> bufferSpan = buffer;

    //     int len = bytes.Length;
    //     int curIndex = 0;

    //     while (curIndex < len)
    //     {
    //         int bytesRead = stream.Read(buffer, 0, READ_CHUNK_COUNT);
    //         bufferSpan[..bytesRead].CopyTo(bytes[curIndex..]);
    //         curIndex += bytesRead;
    //     }

    //     pool.Return(buffer);
    // }


    public static void Read(this BinaryReader reader, Span<byte> bytes)
    {
        byte[] buffer = pool.Rent(BUFFER_CHUNK_COUNT);
        Span<byte> bufferSpan = buffer;

        int len = bytes.Length;
        int curIndex = 0;

        while (curIndex < len)
        {
            int curLength = Math.Min(bytes.Length - curIndex, BUFFER_CHUNK_COUNT);
            int bytesRead = reader.Read(buffer, 0, curLength);
            Span<byte> byteSlice = bytes.Slice(curIndex);
            bufferSpan[..bytesRead].CopyTo(byteSlice);
            curIndex += bytesRead;
        }

        pool.Return(buffer);
    }



    public static void Write(this BinaryWriter writer, Span<byte> bytes)
    {
        byte[] buffer = pool.Rent(BUFFER_CHUNK_COUNT);
        Span<byte> bufferSpan = buffer;

        int len = bytes.Length;
        int curIndex = 0;

        Span<byte> byteSlice;
        while (curIndex < len)
        {
            int curLength = Math.Min(bytes.Length - curIndex, BUFFER_CHUNK_COUNT);
            byteSlice = bytes.Slice(curIndex, curLength);
            byteSlice.CopyTo(bufferSpan[..curLength]);
            writer.Write(buffer, 0, curLength);
            curIndex += curLength;
        }

        pool.Return(buffer);
    }
}