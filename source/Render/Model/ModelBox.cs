namespace TestClient.Source.Render.Model;

public readonly struct ModelBox
{
    public readonly float X0;
    public readonly float Y0;
    public readonly float Z0;
    public readonly int W;
    public readonly int H;
    public readonly int D;
    public readonly int TexU;
    public readonly int TexV;
    public readonly bool Mirror;

    public ModelBox(float x0, float y0, float z0, int w, int h, int d, int texU, int texV, bool mirror = false)
    {
        X0 = x0;
        Y0 = y0;
        Z0 = z0;
        W = w;
        H = h;
        D = d;
        TexU = texU;
        TexV = texV;
        Mirror = mirror;
    }
}
