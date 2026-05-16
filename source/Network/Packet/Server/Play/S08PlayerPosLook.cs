using System;
using System.Collections.Generic;

namespace TestClient.Source.Network.Packet.Server.Play;

public class S08PlayerPosLook : IPacket
{
    public enum EnumFlags
    {
        X = 0,
        Y = 1,
        Z = 2,
        Yaw = 3,
        Pitch = 4
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
        Flags = EnumFlagsHelper.Unpack(buf.ReadByte());
    }

    public void Write(PacketBuffer buf)
    {
        /* S2C only, no need to write */
    }
    
    public static class EnumFlagsHelper
    {
        public static int GetMask(EnumFlags flag)
        {
            return 1 << (int)flag;
        }

        public static bool IsSet(EnumFlags flag, int flags)
        {
            return (flags & GetMask(flag)) == GetMask(flag);
        }

        public static HashSet<EnumFlags> Unpack(int flags)
        {
            var set = new HashSet<EnumFlags>();
            foreach (EnumFlags enumFlag in Enum.GetValues(typeof(EnumFlags)))
            {
                if (IsSet(enumFlag, flags))
                {
                    set.Add(enumFlag);
                }
            }
            return set;
        }
    }
}