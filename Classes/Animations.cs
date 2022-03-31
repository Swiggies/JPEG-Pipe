using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
public abstract class Animation
{
    public virtual string Name { get; }
    public abstract Vector2 Execute(float time);
}

public class Animations
{

    private static List<Animation> _animations = new List<Animation>()
    {
        new Breathe(),
        new Vibe(),
        new IntenseShake(),
    };

    public static List<Animation> AllAnimations { get => _animations; }

    public class Breathe : Animation
    {
        public override string Name { get => "Breathe"; }
        public override Vector2 Execute(float time)
        {
            return new Vector2(0, MathF.Sin(time * 1.5f)) * 10;
        }
    }

    public class Vibe : Animation
    {
        public override string Name { get => "Vibe"; }
        public override Vector2 Execute(float time)
        {
            return new Vector2(MathF.Sin(time * 1f), MathF.Sin(time * 1.5f)) * 10;
        }
    }

    public class IntenseShake : Animation
    {
        public override string Name { get => "Intense Shake"; }
        public override Vector2 Execute(float time)
        {
            return new Vector2(MathF.Sin(time * 20f), MathF.Sin(time * 10f)) * 10;
        }
    }
}