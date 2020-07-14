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
    class Stump : GameObject, IHittable, IGrabbable
    {
        Rectangle imageRect;
        public bool IsGrabbed { get; private set; }
        public int Health { get; private set; }

        public Stump(int gridX, int gridY) : base(gridX, gridY)
        {
            imageRect = new Rectangle(901, Game.R.Next(2) == 0 ? 323 : 340, Game.CellSize, Game.CellSize);
            Solid = true;
            Health = 5;
            Objects.Add(this);
            Game.GameStep += Game_GameStep;
        }

        public override void Draw(Graphics G)
        {
            G.DrawImage(Sprites.TileSheet, new Rectangle(X, Y , Game.CellSize, Game.CellSize), imageRect, GraphicsUnit.Pixel);
        }

        public bool Grab()
        {
            IsGrabbed = true;
            return true;
        }

        public void Hit(int strength, Character attacker)
        {
            Health -= strength;
            if (Health <= 0)
                Destroy();
        }

        public void LetGo()
        {
            IsGrabbed = false;
        }
    }
}
