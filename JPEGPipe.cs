using ImGuiNET;
using ImGuiNET.XNA;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace JPEGPipe
{
    public class JPEGPipe : Game
    {
        const float LERP_SPEED = 5.0f;

        private readonly Settings _settings;
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private MicCapture _mc;
        private ImGuiRenderer _guiRenderer;
        private GUI _gui;
        private Texture2D _sprite;
        private Vector2 _spriteOffset;
        private Vector2 _windowStartPos;
        private Color _currentColor;
        private bool _isOverThreshold = false;
        private float _delayTimer;
        private float _colorTimer;
        private bool _gameWindowActive;
        private Animations _animations;

        private Color _inactiveColor
        {
            get
            {
                return ColorHelper.Vec3ToColor(_settings.InactiveColor);
            }
        }

        private Color _activeColor
        {
            get
            {
                return ColorHelper.Vec3ToColor(_settings.ActiveColor);
            }
        }

        public JPEGPipe()
        {
            _settings = Settings.LoadSettings();
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
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

            // Loading from settings
            if (!String.IsNullOrEmpty(_settings.SpritePath))
                LoadSpriteFromPath(_settings.SpritePath);

            _animations = new Animations(_settings.ActiveAnimation, _settings.InactiveAnimation);
            TargetElapsedTime = TimeSpan.FromSeconds(1 / (double)_settings.FPS);

            // GUIRenderer = new ImGUIRenderer(this).Initialize().RebuildFontAtlas();
            _guiRenderer = new ImGuiRenderer(this);
            _guiRenderer.RebuildFontAtlas();
            _gui = new GUI(_settings, _animations, _guiRenderer);
            _currentColor = _inactiveColor;

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
            if (_mc.SmoothedMicVolume > _settings.Threshold)
                _delayTimer = 1;

            _isOverThreshold = (_mc.SmoothedMicVolume > _settings.Threshold) || (_delayTimer > _settings.Delay);
            if (_isOverThreshold)
            {
                _colorTimer = MathHelper.Clamp(_colorTimer + (float)delta * LERP_SPEED, 0, 1);
            }
            else
            {
                _colorTimer = MathHelper.Clamp(_colorTimer - (float)delta * LERP_SPEED, 0, 1);
            }
            _currentColor = Color.Lerp(_inactiveColor, _activeColor, _colorTimer);

            float curTime = (float)gameTime.TotalGameTime.TotalSeconds;
            _spriteOffset = _isOverThreshold ? _animations.ActiveAnimation.Execute(curTime) : _animations.InactiveAnimation.Execute(curTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(0, 255, 0));
            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            if (_sprite != null)
            {
                int scaleLerp = (int)MathHelper.Lerp(-100, 100, _settings.SpriteScale);
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
            _gui.ShowGUI(gameTime, _delayTimer, _mc.SmoothedMicVolume);
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
