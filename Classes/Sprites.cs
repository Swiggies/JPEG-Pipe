using System;
using System.Collections.Generic;

[Serializable]
public class Sprites
{
    public Texture2D CurrentSprite;

    public List<Sprite> SpriteSequence = new List<Sprite>();

    public Sprites()
    {
    }

    public void CreateNewSprite(string path)
    {
        SpriteSequence.Add(new Sprite(path));
        CurrentSprite = SpriteSequence[0].SpriteTexture;
    }
}