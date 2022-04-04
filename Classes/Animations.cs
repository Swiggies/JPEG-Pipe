using Microsoft.Xna.Framework;
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
        new None(),
        new Breathe(),
        new Vibe(),
        new IntenseShake(),
    };

    public static List<Animation> AllAnimations { get => _animations; }

    public class None : Animation
    {
        public override string Name => "None";
        public override Vector2 Execute(float time)
        {
            return Vector2.Zero;
        }
    }

    public class Breathe : Animation
    {
        public override string Name => "Breathe";
        public override Vector2 Execute(float time)
        {
            return new Vector2(0, MathF.Sin(time * 1.5f)) * 10;
        }
    }

    public class Vibe : Animation
    {
        public override string Name => "Vibe";
        public override Vector2 Execute(float time)
        {
            return new Vector2(MathF.Sin(time * 1f), MathF.Sin(time * 1.5f)) * 10;
        }
    }

    public class IntenseShake : Animation
    {
        public override string Name => "Intense Shake";
        public override Vector2 Execute(float time)
        {
            return new Vector2(MathF.Sin(time * 20f), MathF.Sin(time * 10f)) * 10;
        }
    }
}