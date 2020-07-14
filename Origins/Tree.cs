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
    class Tree : GameObject, IHittable
    {
        private int health = Game.R.Next(3, 6);
        private int seedTimer = Game.R.Next(5000) + 5000;

        public Tree(int gridX, int gridY) : base(gridX, gridY)
        {
            //Currently doesn't allow for a tree to be directly above another tree
            if (Tile.Map[gridX, gridY].Type != Tile.TileType.Grass || Objects.Any(x => x != this && x.GridX == gridX && (x.GridY == gridY || x.GridY == gridY - 1 || x.GridY == GridY + 1)) || Game.GridPointOccupied(gridX, gridY, false, this))
                return;

            Foreground = true;
            Solid = true;
            srcRect = Sprites.TreeRects[Game.R.Next(Sprites.TreeRects.Count)];
            Objects.Add(this);
            Game.GameStep += Game_GameStep;
        }

        public override void Draw(Graphics G)
        {
            G.DrawImage(Sprites.TileSheet, new Rectangle(X, Y + (Game.CellSize / 2) - srcRect.Height, srcRect.Width, srcRect.Height), srcRect, GraphicsUnit.Pixel);
        }

        public void Hit(int strength, Character attacker)
        {
            health -= strength;
            if (health <= 0)
            {
                new Stump(GridX, GridY);
                Destroy();
            }
        }

        public override void Destroy()
        {
            base.Destroy();
        }
    }
}
