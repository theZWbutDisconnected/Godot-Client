using System;
using System.Collections.Generic;

namespace TestClient.Source.Network.Packet.Server.Play;


public class S08PlayerPosLook : IPacket
{
    public double X { get; private set; }
    public double Y { get; private set; }
    public double Z { get; private set; }
    public float Yaw { get; private set; }
    public float Pitch { get; private set; }
    public HashSet<EnumFlags> Flags { get; private set; }

    public void Read(PacketBuffer buf)
    {
        X = buf.ReadDouble();
        Y = buf.ReadDouble();
        Z = buf.ReadDouble();
        Yaw = buf.ReadFloat();
        Pitch = buf.ReadFloat();
        byte flagByte = (byte)buf.ReadByte();
        Flags = EnumFlagsHelper.FromByte(flagByte);
    }

    public void Write(PacketBuffer buf)
    {
        buf.WriteDouble(X);
        buf.WriteDouble(Y);
        buf.WriteDouble(Z);
        buf.WriteFloat(Yaw);
        buf.WriteFloat(Pitch);
        buf.WriteByte(EnumFlagsHelper.ToByte(Flags));
    }

    [Flags]
    public enum EnumFlags
    {
        None = 0,
        X = 1 << 0,      // 0x01
        Y = 1 << 1,      // 0x02
        Z = 1 << 2,      // 0x04
        Yaw = 1 << 3,    // 0x08
        Pitch = 1 << 4   // 0x10
    }

    public static class EnumFlagsHelper
    {
        public static HashSet<EnumFlags> FromByte(byte value)
        {
            var set = new HashSet<EnumFlags>();
            foreach (EnumFlags flag in Enum.GetValues(typeof(EnumFlags)))
            {
                if (flag == EnumFlags.None) continue;
                if ((value & (byte)flag) != 0)
                    set.Add(flag);
            }
            return set;
        }

        public static byte ToByte(HashSet<EnumFlags> flags)
        {
            byte value = 0;
            if (flags != null)
            {
                foreach (var flag in flags)
                    value |= (byte)flag;
            }
            return value;
        }
    }
}