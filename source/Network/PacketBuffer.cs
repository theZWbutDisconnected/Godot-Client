using System;
using System.IO;
using System.Text;

namespace TestClient.Source.Network;

public class PacketBuffer(Stream stream)
{
    public Stream GetInternalStream() => stream;
    public static int GetVarIntSize(int input)
    {
        for (var i = 1; i < 5; ++i)
            if ((input & (-1 << (i * 7))) == 0)
                return i;

        return 5;
    }

    public void WriteByte(byte value)
    {
        stream.WriteByte(value);
    }

    public int ReadByte()
    {
        return stream.ReadByte();
    }

    public void WriteBytes(byte[] array)
    {
        WriteVarInt(array.Length);
        stream.Write(array, 0, array.Length);
    }

    public void WriteRawBytes(byte[] array)
    {
        stream.Write(array, 0, array.Length);
    }

    public byte[] ReadBytes(int length)
    {
        var array = new byte[length];
        stream.ReadExactly(array, 0, length);
        return array;
    }

    public void WriteVarInt(int value)
    {
        while ((value & -128) != 0)
        {
            stream.WriteByte((byte)((value & 0x7F) | 0x80));
            value >>>= 7;
        }

        stream.WriteByte((byte)value);
    }

    public int ReadVarInt()
    {
        var i = 0;
        var j = 0;
        while (true)
        {
            var b0 = stream.ReadByte();
            if (b0 == -1)
                throw new EndOfStreamException("Unexpected end of stream while reading VarInt");
            i |= (b0 & 0x7F) << (j++ * 7);
            if (j > 5) throw new InvalidDataException("VarInt too big");
            if ((b0 & 0x80) == 0) break;
        }

        return i;
    }


    public void WriteString(string str)
    {
        var abyte = Encoding.UTF8.GetBytes(str);

        if (abyte.Length > 32767)
            throw new InvalidOperationException(
                $"String too big (was {abyte.Length} bytes encoded, max 32767)");

        WriteVarInt(abyte.Length);
        stream.Write(abyte, 0, abyte.Length); // 直接写字节，不走WriteBytes（WriteBytes会多写一次长度）
    }

    public string ReadChatComponent()
    {
        return ReadString(32767);
    }

    public string ReadString(int maxLength)
    {
        var i = ReadVarInt();

        if (i > maxLength * 4)
            throw new InvalidDataException(
                $"The received encoded string buffer length is longer than maximum allowed ({i} > {maxLength * 4})");
        if (i < 0)
            throw new InvalidDataException(
                "The received encoded string buffer length is less than zero! Weird string!");
        var bytes = ReadBytes(i);
        var s = Encoding.UTF8.GetString(bytes);

        if (s.Length > maxLength)
            throw new InvalidDataException(
                $"The received string length is longer than maximum allowed ({i} > {maxLength})");
        return s;
    }

    public void WriteShort(int value)
    {
        var bytes = new byte[2];
        bytes[0] = (byte)((value >> 8) & 0xFF);
        bytes[1] = (byte)(value & 0xFF);
        stream.Write(bytes, 0, 2);
    }

    public void WriteUnsignedShort(int value)
    {
        var bytes = new byte[2];
        bytes[0] = (byte)((value >> 8) & 0xFF);
        bytes[1] = (byte)(value & 0xFF);
        stream.Write(bytes, 0, 2);
    }

    public int ReadShort()
    {
        var bytes = ReadBytes(2);
        return (bytes[0] << 8) | bytes[1];
    }

    public int ReadUnsignedShort()
    {
        var bytes = ReadBytes(2);
        return (bytes[0] << 8) | bytes[1];
    }

    public int ReadInt()
    {
        var bytes = ReadBytes(4);
        return (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
    }

    public int ReadSignedByte()
    {
        return (sbyte)stream.ReadByte();
    }

    public bool ReadBoolean()
    {
        return stream.ReadByte() != 0;
    }
}