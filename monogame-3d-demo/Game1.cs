using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace demo3D
{
    public class Game1 : Game
    {
        public RenderTarget2D RenderTarget;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private Texture2D pixel;

        // Screen dimensions
        private const int ScreenWidth = 640;
        private const int ScreenHeight = 480;

        // Map (1 = wall, 0 = empty space)
        private int[,] map = new int[,]
        {
            {1,1,1,1,1,1,1,1,1,1},
            {1,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,1},
            {1,0,0,1,1,1,0,0,0,1},
            {1,0,0,1,0,0,0,0,0,1},
            {1,0,0,1,0,0,0,1,0,1},
            {1,0,0,0,0,0,0,1,0,1},
            {1,0,0,0,0,0,0,0,0,1},
            {1,0,0,0,0,0,0,0,0,1},
            {1,1,1,1,1,1,1,1,1,1}
        };

        // Player
        private float playerX = 5f;
        private float playerY = 5f;
        private float playerAngle = 0f; // facing along the X axis

        // Settings
        private const float MovementSpeed = 0.05f;
        private const float TurningSpeed = 0.002f;
        private const float Fov = MathHelper.Pi / 3f; // 60° field of view

        private MouseState prevMouse;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            prevMouse = Mouse.GetState();

            this.TargetElapsedTime = TimeSpan.FromSeconds(1.0 / 60.0f);
            base.Initialize();

            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;

            RenderTarget = new RenderTarget2D(GraphicsDevice, ScreenWidth, ScreenHeight);
            graphics.ApplyChanges();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Create a 1x1 white pixel for drawing
            pixel = new Texture2D(GraphicsDevice, 1, 1);
            pixel.SetData([Color.White]);
        }

        protected override void Update(GameTime gameTime)
        {
            var keyboard = Keyboard.GetState();
            var mouse = Mouse.GetState();

            if (keyboard.IsKeyDown(Keys.Escape))
                Exit();

            // WASD movement
            float newX = playerX;
            float newY = playerY;

            if (keyboard.IsKeyDown(Keys.W))
            {
                newX += (float)Math.Cos(playerAngle) * MovementSpeed;
                newY += (float)Math.Sin(playerAngle) * MovementSpeed;
            }
            if (keyboard.IsKeyDown(Keys.S))
            {
                newX += -(float)Math.Cos(playerAngle) * MovementSpeed;
                newY += -(float)Math.Sin(playerAngle) * MovementSpeed;
            }
            if (keyboard.IsKeyDown(Keys.A))
            {
                newX += (float)Math.Cos(playerAngle - MathHelper.PiOver2) * MovementSpeed;
                newY += (float)Math.Sin(playerAngle - MathHelper.PiOver2) * MovementSpeed;
            }
            if (keyboard.IsKeyDown(Keys.D))
            {
                newX += (float)Math.Cos(playerAngle + MathHelper.PiOver2) * MovementSpeed;
                newY += (float)Math.Sin(playerAngle + MathHelper.PiOver2) * MovementSpeed;
            }

            // Collision check
            if (map[(int)newX, (int)playerY] == 0)
                playerX = newX;
            if (map[(int)playerX, (int)newY] == 0)
                playerY = newY;

            // Mouse rotation
            int deltaX = mouse.X - prevMouse.X;
            playerAngle += deltaX * TurningSpeed;

            // Recenter the mouse
            Mouse.SetPosition(ScreenWidth / 2, ScreenHeight / 2);
            prevMouse = Mouse.GetState();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            this.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp);

            // Draw sky and floor
            spriteBatch.Draw(pixel, new Rectangle(0, 0, ScreenWidth, ScreenHeight / 2),
                new Color(50, 50, 100)); // Dark blue sky
            spriteBatch.Draw(pixel, new Rectangle(0, ScreenHeight / 2, ScreenWidth, ScreenHeight / 2),
                new Color(40, 40, 40)); // Gray floor

            // RAYCASTING - for each horizontal pixel
            for (int x = 0; x < ScreenWidth; x++)
            {
                // Calculate ray angle
                float normalizedX = (x / (float)ScreenWidth) - 0.5f; // -0.5 to 0.5
                float rayUhel = playerAngle + normalizedX * Fov;

                // Ray direction
                float rayDirX = (float)Math.Cos(rayUhel);
                float rayDirY = (float)Math.Sin(rayUhel);

                // STEPPING - simple wall search
                float rayX = playerX;
                float rayY = playerY;
                float stepSize = 0.02f; // Step size
                float vzdalenost = 0;
                bool narazil = false;

                // Maximum 100 steps (safety limit)
                for (int krok = 0; krok < 500; krok++)
                {
                    rayX += rayDirX * stepSize;
                    rayY += rayDirY * stepSize;
                    vzdalenost += stepSize;

                    // Check if we are outside the map
                    int mapX = (int)rayX;
                    int mapY = (int)rayY;

                    if (mapX < 0 || mapX >= map.GetLength(0) ||
                        mapY < 0 || mapY >= map.GetLength(1))
                        break;

                    // Did we hit a wall?
                    if (map[mapX, mapY] > 0)
                    {
                        narazil = true;
                        break;
                    }
                }

                if (narazil)
                {
                    // Fish-eye correction (important!)
                    vzdalenost *= (float)Math.Cos(rayUhel - playerAngle);

                    // Wall height on screen
                    float vyskaZdi = (ScreenHeight / vzdalenost) * 0.6f;

                    // Wall position
                    int zahajenoY = (int)(ScreenHeight / 2 - vyskaZdi / 2);
                    int vyska = (int)vyskaZdi;

                    // Wall color by distance (darker = farther)
                    float brightness = Math.Max(0, 1f - (vzdalenost / 10f));

                    // Different colors by direction (north/south vs east/west)
                    Color barva;
                    float frac = rayX - (float)Math.Floor(rayX);
                    if (frac < 0.5f)
                        barva = new Color(brightness, 0, 0); // Red
                    else
                        barva = new Color(0, brightness, 0); // Green

                    // Draw vertical line
                    spriteBatch.Draw(pixel, new Rectangle(x, zahajenoY, 1, vyska), barva);
                }
            }

            // Debug info
            spriteBatch.DrawString(Content.Load<SpriteFont>("Font"),
                $"Position: ({playerX:F1}, {playerY:F1})\nAngle: {MathHelper.ToDegrees(playerAngle):F0}°\n",
                new Vector2(10, 10), Color.Yellow);

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
