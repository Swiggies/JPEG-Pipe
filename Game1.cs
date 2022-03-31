using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using ImGuiNET;
using ImGuiNET.XNA;
using MonoGame.Extended.Tweening;

namespace PNGTube
{
    public class Game1 : Game
    {
        const float LERP_SPEED = 5.0f;

        private readonly Settings _settings;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private MicCapture _mc;
        private ImGuiRenderer _guiRenderer;
        private Texture2D _sprite;
        private Vector2 _spriteOffset;
        private Vector2 _windowStartPos;
        private float _activeThreshold = 0.1f;
        private System.Numerics.Vector3 _inactiveColorV3 = new System.Numerics.Vector3(0.5f, 0.5f, 0.5f);
        private System.Numerics.Vector3 _activeColorV3 = new System.Numerics.Vector3(1.0f, 1.0f, 1.0f);
        private Color _currentColor;
        private bool _isOverThreshold = false;
        private float _delayThreshold = 0.25f;
        private float _delayTimer;
        private float _colorTimer;
        private Animation _inactiveAnimation = new Animations.Breathe();
        private Animation _activeAnimation = new Animations.Breathe();
        private bool _gameWindowActive;
        private float _spriteScale = 0.5f;

        private Color _inativeColor
        {
            get
            {
                return ColorHelper.Vec3ToColor(_inactiveColorV3);
            }
        }

        private Color _activeColor
        {
            get
            {
                return ColorHelper.Vec3ToColor(_activeColorV3);
            }
        }

        public Game1()
        {
            _settings = Settings.LoadSettings();
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            // Uncomment this to lock at 30 FPS
            // this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 144d);
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.PreferredBackBufferWidth = 720;
            _graphics.PreferredBackBufferHeight = 720;
            _graphics.ApplyChanges();

            _mc = new MicCapture();
            //Console.WriteLine(_mc.Mic.State);
            _windowStartPos = Window.Position.ToVector2();

            if (!String.IsNullOrEmpty(_settings.SpritePath))
                LoadSpriteFromPath(_settings.SpritePath);
            _spriteScale = _settings.SpriteScale;
            _activeColorV3 = ColorHelper.ColorToVec3(_settings.ActiveColor);
            _inactiveColorV3 = ColorHelper.ColorToVec3(_settings.InactiveColor);
            _delayThreshold = _settings.Delay;
            _activeThreshold = _settings.Threshold;
            _activeAnimation = Animations.AllAnimations[_settings.ActiveAnimation];
            _inactiveAnimation = Animations.AllAnimations[_settings.InactiveAnimation];

            // GUIRenderer = new ImGUIRenderer(this).Initialize().RebuildFontAtlas();
            _guiRenderer = new ImGuiRenderer(this);
            _guiRenderer.RebuildFontAtlas();
            _currentColor = _inativeColor;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            float delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            MouseState state = Mouse.GetState();
            // if (state.X < 0 || state.Y < 0 || state.X > Window.ClientBounds.Width || state.Y > Window.ClientBounds.Height || !IsActive)
            _gameWindowActive = !(state.X < 0 || state.Y < 0 || state.X > Window.ClientBounds.Width || state.Y > Window.ClientBounds.Height || !IsActive);

            if (Window.Position.ToVector2() != _windowStartPos)
            {
                _windowStartPos = Window.Position.ToVector2();
            }

            _delayTimer = MathHelper.Clamp(_delayTimer -= delta, 0, 1);
            if (_mc.SmoothedMicVolume > _activeThreshold)
                _delayTimer = 1;

            _isOverThreshold = (_mc.SmoothedMicVolume > _activeThreshold) || (_delayTimer > _delayThreshold);
            if (_isOverThreshold)
            {
                _colorTimer = MathHelper.Clamp(_colorTimer + (float)delta * LERP_SPEED, 0, 1);
            }
            else
            {
                _colorTimer = MathHelper.Clamp(_colorTimer - (float)delta * LERP_SPEED, 0, 1);
            }
            _currentColor = Color.Lerp(_inativeColor, _activeColor, _colorTimer);

            float curTime = (float)gameTime.TotalGameTime.TotalSeconds;
            _spriteOffset = _isOverThreshold ? _activeAnimation.Execute(curTime) : _inactiveAnimation.Execute(curTime);

            base.Update(gameTime);
        }

        protected override async void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0, 255, 0));
            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            if (_sprite != null)
            {
                int scaleLerp = (int)MathHelper.Lerp(-100, 100, _spriteScale);
                int scale = 720 - scaleLerp;
                Rectangle destRect = new Rectangle(scaleLerp / 2, scaleLerp / 2, scale, scale);
                Rectangle sourceRect = new Rectangle(0, 0, _sprite.Width, _sprite.Height);

                _spriteBatch.Draw(_sprite, destRect, sourceRect, _currentColor, 0, _spriteOffset, SpriteEffects.None, 0);
            }
            _spriteBatch.End();
            base.Draw(gameTime);

            // GUI Rendering
            if (_gameWindowActive)
            {
                DrawGUI(gameTime);
            }
        }


        public void DrawGUI(GameTime gameTime)
        {
            _guiRenderer.BeforeLayout(gameTime);

            ImGui.PushStyleColor(ImGuiCol.WindowBg, new System.Numerics.Vector4(0f, 0f, 0f, 0.6f));
            ImGui.Begin("Settings", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
            // Docks GUI to the side of the window
            // ImGui.SetWindowPos(new System.Numerics.Vector2(0, 0));
            // ImGui.SetWindowSize(new System.Numerics.Vector2(350, 720));
            ImGui.Text("Moods");
            // Inactive Animation
            if (ImGui.BeginCombo("Inactive", _inactiveAnimation.Name))
            {
                for (int i = 0; i < Animations.AllAnimations.Count; i++)
                {
                    var a = Animations.AllAnimations[i];
                    bool isSelected = Animations.AllAnimations[i] == _inactiveAnimation;
                    if (ImGui.Selectable(a.Name, isSelected))
                    {
                        _inactiveAnimation = Animations.AllAnimations[i];
                        _settings.InactiveAnimation = i;
                    }
                    if (isSelected) ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }

            // Active Animation
            if (ImGui.BeginCombo("Active", _activeAnimation.Name))
            {
                for (int i = 0; i < Animations.AllAnimations.Count; i++)
                {
                    var a = Animations.AllAnimations[i];
                    bool isSelected = Animations.AllAnimations[i] == _activeAnimation;
                    if (ImGui.Selectable(a.Name, isSelected))
                    {
                        _activeAnimation = Animations.AllAnimations[i];
                        _settings.ActiveAnimation = i;
                    }
                    if (isSelected) ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            if (ImGui.ColorEdit3("Inactive Color", ref _inactiveColorV3))
                _settings.InactiveColor = _inativeColor;
            if (ImGui.ColorEdit3("Active Color", ref _activeColorV3))
                _settings.ActiveColor = _activeColor;
            ImGui.Spacing();
            if (ImGui.SliderFloat("Threshold", ref _activeThreshold, 0.0f, 1.0f))
                _settings.Threshold = _activeThreshold;
            ImGui.ProgressBar(_mc.SmoothedMicVolume, new System.Numerics.Vector2(0.0f, 0.0f), "");
            if (ImGui.SliderFloat("Delay", ref _delayThreshold, 0.0f, 1.0f))
                _settings.Delay = _delayThreshold;
            ImGui.ProgressBar(_delayTimer, new System.Numerics.Vector2(0.0f, 0.0f), "");
            ImGui.Spacing();
            if (ImGui.Button("Load Image"))
            {
                LoadSprite();
            }
            if (ImGui.SliderFloat("Sprite Scale", ref _spriteScale, 0.0f, 1.0f))
                _settings.SpriteScale = _spriteScale;
            if (ImGui.Button("Save"))
            {
                _settings.SaveSettings();
            }
            ImGui.End();
            ImGui.PopStyleColor();
            _guiRenderer.AfterLayout();
        }

        private void LoadSprite()
        {
            System.Windows.Forms.OpenFileDialog of = new System.Windows.Forms.OpenFileDialog();
            of.Filter = "Image Files|*.jpeg;*.png;*.jpg";
            if (of.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                LoadSpriteFromPath(of.FileName);
            }
        }

        private void LoadSpriteFromPath(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            _sprite = Texture2D.FromStream(_graphics.GraphicsDevice, fs);
            fs.Dispose();
            _settings.SpritePath = path;
        }
    }
}
