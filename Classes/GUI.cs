using ImGuiNET;
using ImGuiNET.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;


class GUI
{
    private readonly Settings _settings;
    private readonly Animations _animations;
    private readonly ImGuiRenderer _guiRenderer;

    public Action LoadImageBtnPress;
    public Action SetFrameRate;

    public GUI(Settings settings, Animations animations, ImGuiRenderer guiRenderer)
    {
        _settings = settings;
        _animations = animations;
        _guiRenderer = guiRenderer;
    }

    public void ShowGUI(GameTime gameTime, float delayTimer, float micVolume)
    {
        _guiRenderer.BeforeLayout(gameTime);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0f, 0f, 0f, 0.6f));
        ImGui.Begin("Settings", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
        // Docks GUI to the side of the window
        // ImGui.SetWindowPos(new System.Numerics.Vector2(0, 0));
        // ImGui.SetWindowSize(new System.Numerics.Vector2(350, 720));
        ImGui.Text("Moods");
        // Inactive Animation
        if (ImGui.BeginCombo("Inactive", _animations.InactiveAnimation.Name))
        {
            for (int i = 0; i < Animations.AllAnimations.Count; i++)
            {
                var a = Animations.AllAnimations[i];
                bool isSelected = Animations.AllAnimations[i] == _animations.InactiveAnimation;
                if (ImGui.Selectable(a.Name, isSelected))
                {
                    _animations.InactiveAnimation = Animations.AllAnimations[i];
                    _settings.InactiveAnimation = i;
                }
                if (isSelected) ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }

        // Active Animation
        if (ImGui.BeginCombo("Active", _animations.ActiveAnimation.Name))
        {
            for (int i = 0; i < Animations.AllAnimations.Count; i++)
            {
                var a = Animations.AllAnimations[i];
                bool isSelected = Animations.AllAnimations[i] == _animations.ActiveAnimation;
                if (ImGui.Selectable(a.Name, isSelected))
                {
                    _animations.ActiveAnimation = Animations.AllAnimations[i];
                    _settings.ActiveAnimation = i;
                }
                if (isSelected) ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }
        ImGui.ColorEdit3("Inactive Color", ref _settings.InactiveColor);
        ImGui.ColorEdit3("Active Color", ref _settings.ActiveColor);
        ImGui.Spacing();
        ImGui.SliderFloat("Threshold", ref _settings.Threshold, 0.0f, 1.0f);
        ImGui.ProgressBar(micVolume, new System.Numerics.Vector2(0.0f, 0.0f), "");
        ImGui.SliderFloat("Delay", ref _settings.Delay, 0.0f, 1.0f);
        ImGui.ProgressBar(delayTimer, new System.Numerics.Vector2(0.0f, 0.0f), "");
        ImGui.Spacing();
        if (ImGui.Button("Load Image"))
        {
            // LoadSprite();
            LoadImageBtnPress?.Invoke();
        }
        ImGui.SliderFloat("Sprite Scale", ref _settings.SpriteScale, 0.0f, 1.0f);
        // _settings.SpriteScale = _spriteScale;
        if (ImGui.Button("Save"))
        {
            _settings.SaveSettings();
        }

        ImGui.Spacing();
        // Advanced Settings
        if (ImGui.CollapsingHeader("Advanced Settings"))
        {
            if (ImGui.SliderInt("FPS", ref _settings.FPS, 15, 60))
            {
                SetFrameRate?.Invoke();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor();
        _guiRenderer.AfterLayout();
    }
}