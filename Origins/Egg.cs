using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Origins
{
    [Serializable]
    public class Egg : GameObject, IHittable, IGrabbable
    {
        public event StatusUpdateDelegate StatusUpdate;

        private int eggTimer = Game.R.Next(1000) + 5000;
        private int animTimer = 0;
        private List<Rectangle> rects = new List<Rectangle>
        {
            new Rectangle(96, 128, Game.CellSize, Game.CellSize),
            new Rectangle(112, 128, Game.CellSize, Game.CellSize),
            new Rectangle(128, 128, Game.CellSize, Game.CellSize),
        };
        private int imageIndex = 0;

        public double Health { get; private set; }
        public double Stamina { get; private set; }
        public double Thirst { get; private set; }
        public double Hunger { get; private set; }
        public double Strength { get; private set; }
        public double Intelligence { get; private set; }
        public double Speed { get; private set; }
        public int Generation { get; private set; }
        public double Radiation { get; private set; }
        public bool IsGrabbed { get; private set; }

        public Egg(Dorf parent) : base(parent.GridX, parent.GridY)
        {
            if (Tile.Map[GridX, GridY].Type != Tile.TileType.Grass || Objects.Any(x => x.GridX == parent.GridX && x.GridY == parent.GridY))
                return;

            Stamina = parent.Stamina;
            Hunger = parent.Hunger;
            Thirst = parent.Thirst;
            Generation = parent.Generation + 1;
            Radiation = parent.Radiation;

            Strength = parent.Strength;
            Intelligence = parent.Intelligence;
            Speed = parent.Speed;

            imageIndex = 0;
            Solid = true;
            StatusUpdate += Game.UpdateStatus;
            Objects.Add(this);
        }

        protected override void Game_GameStep()
        {
            base.Game_GameStep();

            if (++animTimer % 20 == 0)
            {
                animTimer = 0;
                if (StandingOn == Tile.TileType.Water)
                    imageIndex = (imageIndex != 1) ? 1 : 2;
                else
                    imageIndex = 0;
            }

            if (!IsGrabbed && --eggTimer <= 0 && !Character.Characters.Any(x => x.GridX == GridX && x.GridY == GridY))
            {
                new Dorf(GridX, GridY);
                Destroy();
            }
        }

        public override void Draw(Graphics G)
        {
            G.DrawImage(Sprites.CharacterSheet,new Rectangle(X, Y, Game.CellSize, Game.CellSize), rects[imageIndex],  GraphicsUnit.Pixel);
        }

        public void Hit(int strength, Character attacker)
        {
            Health -= strength;
            if (Health <= 0)
            {
                StatusUpdate?.Invoke($"Was that really necessary?", Color.Red);
                Objects.Remove(this);
                new Poof(GridX, GridY);
                Game.GameStep -= Game_GameStep;
                StatusUpdate -= Game.UpdateStatus;
            }
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
                GameObject obj = Objects.FirstOrDefault(x => x != this && x.GridX == GridX && x.GridY == GridY);
                if (obj != null)
                    obj.Destroy();
            }
        }

        protected double LimitValue(double value, double min, double max)
        {
            if (value < min)
                return min;
            if (value > max)
                return max;
            return value;
        }
    }
}
