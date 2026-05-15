namespace TestClient.Source.Physics;

public class AABB
{
    private readonly double _epsilon = 0.0F;
    public double X0;
    public double X1;
    public double Y0;
    public double Y1;
    public double Z0;
    public double Z1;

    public AABB(double x0, double y0, double z0, double x1, double y1, double z1)
    {
        X0 = x0;
        Y0 = y0;
        Z0 = z0;
        X1 = x1;
        Y1 = y1;
        Z1 = z1;
    }

    public AABB Expand(double xa, double ya, double za)
    {
        var x0 = X0;
        var y0 = Y0;
        var z0 = Z0;
        var x1 = X1;
        var y1 = Y1;
        var z1 = Z1;
        if (xa < 0.0F) x0 += xa;
        if (xa > 0.0F) x1 += xa;
        if (ya < 0.0F) y0 += ya;
        if (ya > 0.0F) y1 += ya;
        if (za < 0.0F) z0 += za;
        if (za > 0.0F) z1 += za;
        return new AABB(x0, y0, z0, x1, y1, z1);
    }

    public AABB Grow(double xa, double ya, double za)
    {
        return new AABB(X0 - xa, Y0 - ya, Z0 - za, X1 + xa, Y1 + ya, Z1 + za);
    }

    public double ClipXCollide(AABB c, double xa)
    {
        if (c.Y1 > Y0 && c.Y0 < Y1 && c.Z1 > Z0 && c.Z0 < Z1)
        {
            if (xa > 0.0F && c.X1 <= X0)
            {
                var max = X0 - c.X1 - _epsilon;
                if (max < xa) xa = max;
            }

            if (xa < 0.0F && c.X0 >= X1)
            {
                var max = X1 - c.X0 + _epsilon;
                if (max > xa) xa = max;
            }
        }

        return xa;
    }

    public double ClipYCollide(AABB c, double ya)
    {
        if (c.X1 > X0 && c.X0 < X1 && c.Z1 > Z0 && c.Z0 < Z1)
        {
            if (ya > 0.0F && c.Y1 <= Y0)
            {
                var max = Y0 - c.Y1 - _epsilon;
                if (max < ya) ya = max;
            }

            if (ya < 0.0F && c.Y0 >= Y1)
            {
                var max = Y1 - c.Y0 + _epsilon;
                if (max > ya) ya = max;
            }
        }

        return ya;
    }

    public double ClipZCollide(AABB c, double za)
    {
        if (c.X1 > X0 && c.X0 < X1 && c.Y1 > Y0 && c.Y0 < Y1)
        {
            if (za > 0.0F && c.Z1 <= Z0)
            {
                var max = Z0 - c.Z1 - _epsilon;
                if (max < za) za = max;
            }

            if (za < 0.0F && c.Z0 >= Z1)
            {
                var max = Z1 - c.Z0 + _epsilon;
                if (max > za) za = max;
            }
        }

        return za;
    }

    public bool Intersects(AABB c)
    {
        return c.X1 > X0 && c.X0 < X1 && c.Y1 > Y0 && c.Y0 < Y1 && c.Z1 > Z0 && c.Z0 < Z1;
    }

    public void Move(double xa, double ya, double za)
    {
        X0 += xa;
        Y0 += ya;
        Z0 += za;
        X1 += xa;
        Y1 += ya;
        Z1 += za;
    }
}