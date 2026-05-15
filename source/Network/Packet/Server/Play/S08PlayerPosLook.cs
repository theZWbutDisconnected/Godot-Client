using System;
using System.Collections.Generic;

namespace TestClient.Source.Network.Packet.Server.Play;

public class S08PlayerPosLook : IPacket
{
    [Flags]
    public enum EnumFlags
    {
        None = 0,
        X = 1 << 0, // 0x01
        Y = 1 << 1, // 0x02
        Z = 1 << 2, // 0x04
        Yaw = 1 << 3, // 0x08
        Pitch = 1 << 4 // 0x10
    }

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
        Flags = EnumFlagsHelper.FromByte(buf.ReadByte());
    }

    public void Write(PacketBuffer buf)
    {
        /* S2C only, no need to write */
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
    }
}