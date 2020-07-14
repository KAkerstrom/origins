using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Origins
{
    [Serializable]
    class Food : GameObject, IHittable, IGrabbable
    {
        #region Static

        private static int countTimer;
        private static int foodCount;

        static Food()
        {
            foodCount = 0;
            countTimer = 1000;
            Game.GameStep += Static_Game_GameStep;
        }

        private static void Static_Game_GameStep()
        {
            if (--countTimer <= 0)
            {
                foodCount = Objects.Count(x => x is Food);
                countTimer = 1000;
            }
        }

        #endregion Static

        private int seedTimer = Game.R.Next(2000) + 1000;
        private Color color1, color2, averageColor;
        private Image foodImg = new Bitmap(Game.CellSize, Game.CellSize);

        public bool IsGrabbed { get; private set; } = false;

        public double HealthRegen
        {
            get { return averageColor.R / 5; } //0 - 51
        }

        public double StaminaRegen
        {
            get { return averageColor.G / 5; } //0 - 51
        }

        public double HungerRegen
        {
            get { return averageColor.G / 3 + 15; } //15 - 100
        }

        public Food(int gridX, int gridY) : base(gridX, gridY)
        {
            if (StandingOn != Tile.TileType.Grass || !WithinBounds(gridX, gridY) || Objects.Any(x => x != this && x.GridX == gridX && x.GridY == gridY))
                return;

            color1 = Color.FromArgb(Game.R.Next(256), Game.R.Next(256), Game.R.Next(256));
            color2 = Color.FromArgb(Game.R.Next(256), Game.R.Next(256), Game.R.Next(256));
            averageColor = GetAverageColor(color1, color2);
            CreateImage(color1, color2);

            Solid = false;
            Objects.Add(this);
        }

        public Food(int gridX, int gridY, Color color1, Color color2) : base(gridX, gridY)
        {
            if (!WithinBounds(gridX, gridY) || StandingOn == Tile.TileType.Water || Objects.FirstOrDefault(x => x.GridX == gridX && x.GridY == gridY) != null)
                return;
            this.color1 = color1;
            this.color2 = color2;
            averageColor = GetAverageColor(color1, color2);
            CreateImage(color1, color2);

            Solid = false;
            Objects.Add(this);
        }

        private Color GetAverageColor(Color color1, Color color2)
        {
            return Color.FromArgb((color1.R + color2.R) / 2, (color1.G + color2.G) / 2, (color1.B + color2.B) / 2);
        }

        private void CreateImage(Color color1, Color color2)
        {
            Rectangle baseRect = Sprites.FoodRects["Food_base"];

            Rectangle petalRect = Sprites.FoodRects["Food_petals1"];
            Bitmap petals1 = new Bitmap(Game.CellSize, Game.CellSize);
            Graphics petalGraphics = Graphics.FromImage(petals1);
            petalGraphics.DrawImage(Sprites.TileSheet, new Rectangle(0, 0, Game.CellSize, Game.CellSize), petalRect, GraphicsUnit.Pixel);
            petals1 = Colorize(petals1, color1);

            petalRect = Sprites.FoodRects["Food_petals2"];
            Bitmap petals2 = new Bitmap(Game.CellSize, Game.CellSize);
            petalGraphics = Graphics.FromImage(petals2);
            //copy petals1 instead of redrawing
            petalGraphics.DrawImage(Sprites.TileSheet, new Rectangle(0, 0, Game.CellSize, Game.CellSize), petalRect, GraphicsUnit.Pixel);
            petals2 = Colorize(petals2, color2);

            petalGraphics = Graphics.FromImage(foodImg);
            petalGraphics.DrawImage(Sprites.TileSheet, new Rectangle(0, 0, baseRect.Width, baseRect.Height), baseRect, GraphicsUnit.Pixel);
            petalGraphics.DrawImage(petals1, 0, 0);
            petalGraphics.DrawImage(petals2, 0, 0);
        }

        protected override void Game_GameStep()
        {
            base.Game_GameStep();
            if (!IsGrabbed && --seedTimer <= 0)
            {
                seedTimer = Game.R.Next(2000) + 1000;
                for (int i = -2; i <= 2; i++)
                    for (int j = -2; j <= 2; j++)
                        if (Game.R.Next(foodCount + 10) == 0)
                            new Food(GridX + i, GridY + j, GetMutatedColor(color1), GetMutatedColor(color2));
            }
        }

        private int LimitRGBValue(int value)
        {
            if (value > 255)
                return 255 - (value - 255);
            else if (value < 0)
                return -value;
            else
                return value;
        }

        private Color GetMutatedColor(Color original)
        {
            int[] components = new int[3];
            components[0] = LimitRGBValue(original.R + Game.R.Next(61) - 30);
            components[1] = LimitRGBValue(original.G + Game.R.Next(61) - 30);
            components[2] = LimitRGBValue(original.B + Game.R.Next(61) - 30);

            return Color.FromArgb(components[0], components[1], components[2]);
        }

        private Bitmap Colorize(Bitmap original, Color newColor)
        {
            for (int i = 0; i < original.Width; i++)
                for (int j = 0; j < original.Height; j++)
                {
                    if (original.GetPixel(i, j).A != 0)
                        original.SetPixel(i, j, Color.FromArgb(
                            (int)(newColor.R),
                            (int)(newColor.G),
                            (int)(newColor.B)));
                }
            return original;
        }

        public override void Draw(Graphics G)
        {
            G.DrawImage(foodImg, X, Y);
        }

        public void Hit(int strength, Character attacker)
        {
            Destroy();
        }

        public bool Grab()
        {
            IsGrabbed = true;
            return true;
        }

        public void LetGo()
        {
            if (IsGrabbed)
            {
                IsGrabbed = false;

                if (StandingOn == Tile.TileType.Water)
                    Destroy();
                else
                {
                    Food conflictingFood = (Food)Objects.FirstOrDefault(x => x is Food && x != this && x.GridX == GridX && x.GridY == GridY);
                    if (conflictingFood != null)
                    {
                        Destroy();
                        conflictingFood.Destroy();
                        new Food(GridX, GridY, GetAverageColor(conflictingFood.color1, color1), GetAverageColor(conflictingFood.color2, color2));
                    }
                }
            }
        }
    }
}
