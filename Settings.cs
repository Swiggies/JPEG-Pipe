using Newtonsoft.Json;
using Microsoft.Xna.Framework;
using System.IO;
using System;

[System.Serializable]
public class Settings
{
    public string SpritePath;
    public Color ActiveColor = Color.White;
    public Color InactiveColor = Color.LightGray;
    public float SpriteScale = 0.5f;
    public float Threshold = 0.1f;
    public float Delay = 0.1f;

    public static Settings LoadSettings()
    {
        if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "/settings.json"))
        {
            using (StreamReader file = File.OpenText(AppDomain.CurrentDomain.BaseDirectory + "/settings.json"))
            {
                JsonSerializer s = new JsonSerializer();
                return (Settings)s.Deserialize(file, typeof(Settings));
            }
        }
        return new Settings();
    }

    public void SaveSettings()
    {
        using (StreamWriter file = File.CreateText(AppDomain.CurrentDomain.BaseDirectory + "/settings.json"))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(file, this);
        }
    }
}