using System;
using System.IO;

[Serializable]
public class Sprite
{
    public Texture2D SpriteTexture;
    public IntPtr SpritePtr;
    public int Sequence;
    public string SpritePath;

    public Sprite(string path)
    {
        SpritePath = path;
        LoadSprite();
    }

    public void LoadSprite()
    {
        FileStream fs = new FileStream(SpritePath, FileMode.Open);
        SpriteTexture = Texture2D.FromStream(ServiceLocator.Current.Get<GraphicsDeviceManager>().GraphicsDevice, fs);
        fs.Dispose();
        SpritePtr = SpriteTexture.GetSharedHandle();
    }
}