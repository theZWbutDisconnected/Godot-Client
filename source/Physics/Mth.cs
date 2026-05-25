namespace TestClient.Source.Physics;

public static class Mth
{
    private static readonly int[] MultiplyDeBruijnBitPosition =
    {
        0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5,
        10, 9
    };

    public static double LengthSquared(double p_211593_, double p_211594_, double p_211595_)
    {
        return p_211593_ * p_211593_ + p_211594_ * p_211594_ + p_211595_ * p_211595_;
    }

    private static int CalculateLogBaseTwoDeBruijn(int value)
    {
        value = IsPowerOfTwo(value) ? value : RoundUpToPowerOfTwo(value);
        return MultiplyDeBruijnBitPosition[(int)((value * 125613361L) >> 27) & 31];
    }

    public static int CalculateLogBaseTwo(int value)
    {
        return CalculateLogBaseTwoDeBruijn(value) - (IsPowerOfTwo(value) ? 0 : 1);
    }

    public static int RoundUpToPowerOfTwo(int value)
    {
        var i = value - 1;
        i = i | (i >> 1);
        i = i | (i >> 2);
        i = i | (i >> 4);
        i = i | (i >> 8);
        i = i | (i >> 16);
        return i + 1;
    }

    private static bool IsPowerOfTwo(int value)
    {
        return value != 0 && (value & (value - 1)) == 0;
    }

    public static float WrapAngle(float value)
    {
        value = value % 360.0F;

        if (value >= 180.0F)
        {
            value -= 360.0F;
        }

        if (value < -180.0F)
        {
            value += 360.0F;
        }

        return value;
    }

    public static float InterpolateRotation(float par1, float par2, float par3)
    {
        float f;
        for (f = par2 - par1; f < -180.0F; f += 360.0F) ;
        while (f >= 180.0F) f -= 360.0F;
        return par1 + par3 * f;
    }
}