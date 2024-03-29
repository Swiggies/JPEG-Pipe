using Microsoft.Xna.Framework;

public static class ColorHelper
{
    public static Color Vec3ToColor(this System.Numerics.Vector3 vec)
    {
        return new Color(vec.X, vec.Y, vec.Z);
    }

    public static System.Numerics.Vector3 ColorToVec3(Color c)
    {
        return new System.Numerics.Vector3(c.R / 255f, c.G / 255f, c.B / 255f);
    }

    public static System.Numerics.Vector3 ToVec3(this Color c)
    {
        return new System.Numerics.Vector3(c.R / 255f, c.G / 255f, c.B / 255f);
    }
}