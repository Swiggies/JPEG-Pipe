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

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private MicCapture _mc;
        private ImGuiRenderer _guiRenderer;

        private Texture2D _sprite;
        private Vector2 _spriteDefaultPos;
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

        private Color _inativeColor
        {
            get
            {
                return new Color(_inactiveColorV3.X, _inactiveColorV3.Y, _inactiveColorV3.Z);
            }
        }

        private Color _activeColor
        {
            get
            {
                return new Color(_activeColorV3.X, _activeColorV3.Y, _activeColorV3.Z);
            }
        }


        private float SpriteScale
        {
            get
            {
                return (float)Window.ClientBounds.Height / _sprite.Height;
            }
        }

        public Game1()
        {
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

            FileStream fs = new FileStream(AppDomain.CurrentDomain.BaseDirectory + "susie.png", FileMode.Open);
            _sprite = Texture2D.FromStream(_graphics.GraphicsDevice, fs);
            fs.Dispose();

            _spriteDefaultPos = new Vector2(Window.ClientBounds.Width * 0.5f, Window.ClientBounds.Height * 0.5f);

            _mc = new MicCapture();
            Console.WriteLine(_mc.Mic.State);
            _windowStartPos = Window.Position.ToVector2();

            // GUIRenderer = new ImGUIRenderer(this).Initialize().RebuildFontAtlas();
            _guiRenderer = new ImGuiRenderer(this);
            _guiRenderer.RebuildFontAtlas();


            _currentColor = _inativeColor;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
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
            _spriteOffset = _spriteDefaultPos + (_isOverThreshold ? _activeAnimation.Execute(curTime) : _inactiveAnimation.Execute(curTime));

            base.Update(gameTime);
        }

        protected override async void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0, 255, 0));
            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            _spriteBatch.Draw(_sprite,
            _spriteOffset,
            null,
            _currentColor,
            0f,
            new Vector2(_sprite.Width / 2, _sprite.Height / 2),
            Vector2.One * SpriteScale,
            SpriteEffects.None,
            0f);

            _spriteBatch.End();
            base.Draw(gameTime);

            // GUI Rendering
            if (_gameWindowActive)
            {
                _guiRenderer.BeforeLayout(gameTime);

                ImGui.Begin("", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoCollapse);
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
                        }
                        if (isSelected) ImGui.SetItemDefaultFocus();
                    }
                    ImGui.EndCombo();
                }
                ImGui.ColorEdit3("Inactive Color", ref _inactiveColorV3);
                ImGui.ColorEdit3("Active Color", ref _activeColorV3);
                ImGui.Spacing();
                ImGui.SliderFloat("Threshold", ref _activeThreshold, 0.0f, 1.0f, "");
                ImGui.ProgressBar(_mc.SmoothedMicVolume, new System.Numerics.Vector2(0.0f, 0.0f), "");
                ImGui.SliderFloat("Delay", ref _delayThreshold, 0.0f, 1.0f);
                ImGui.ProgressBar(_delayTimer, new System.Numerics.Vector2(0.0f, 0.0f), "");
                ImGui.End();
                _guiRenderer.AfterLayout();
            }
        }
    }
}
