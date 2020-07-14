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
    public abstract class Character
    {
        public static List<Character> Characters = new List<Character>();

        protected double x, y;
        protected double speed;
        protected double xSpeed;
        protected double ySpeed;
        protected bool canMove = true;
        protected int animTimer, subTimer, stateTimer = 0;
        protected string spriteIndex;
        protected int imageIndex = 0;
        protected bool facingRight = true;
        protected Tile.TileType StandingOn { get { return Tile.Map[GridX, GridY].Type; } }

        public int X { get { return (int)x; } }
        public int Y { get { return (int)y; } }
        public int GridX { get { return ((X + Game.CellSize / 2)) / Game.CellSize; } }
        public int GridY { get { return ((Y + Game.CellSize / 2)) / Game.CellSize; } }
        public int Strength { get; set; }
        public Point? destination { get; set; }
        public bool IsMoving { get { return destination != null; } }

        public Character(int gridX, int gridY)
        {
            x = gridX * Game.CellSize;
            y = gridY * Game.CellSize;
            Game.GameStep += Step;
            Characters.Add(this);
        }

        [OnDeserializing]
        protected void OnDeserializing(StreamingContext context)
        {
            Game.GameStep += Step;
        }

        protected virtual bool Move(int xMove, int yMove)
        {
            if ((xMove == 0 && yMove == 0) || Math.Abs(xMove) > 1 || Math.Abs(yMove) > 1)
                return false;

            if (xMove > 0) facingRight = true;
            else if (xMove < 0) facingRight = false;

            if (!IsMoving && !Game.GridPointOccupied(GridX + xMove, GridY + yMove, true, this))
            {
                int newGridX = GridX + xMove;
                int newGridY = GridY + yMove;

                if (newGridX <= Tile.Map.GetUpperBound(0)
                    && newGridY <= Tile.Map.GetUpperBound(1)
                    && newGridX >= 0 && newGridY >= 0)
                {
                    xSpeed = speed * xMove;
                    ySpeed = speed * yMove;
                    destination = new Point(X + xMove * Game.CellSize, Y + yMove * Game.CellSize);
                    return true;
                }
            }
            return false;
        }

        protected virtual void Teleport(int gridX, int gridY)
        {
            if (!Tile.PointIsInMap(gridX, gridY))
                return;

            x = gridX * Game.CellSize;
            y = gridY * Game.CellSize;
        }

        protected virtual void Step()
        {
            if (++animTimer > 1000000)
                animTimer = 0;
            if (++subTimer > 1000000)
                animTimer = 0;
            if (++stateTimer > 1000000)
                stateTimer = 1000000;

            if (IsMoving)
            {
                double xSpeedCurrent = xSpeed * Tile.Map[GridX, GridY].SpeedModifier;
                double ySpeedCurrent = ySpeed * Tile.Map[GridX, GridY].SpeedModifier;

                if (Math.Abs(x - destination.Value.X) < Math.Abs(xSpeedCurrent) || (Math.Abs(y - destination.Value.Y) < Math.Abs(ySpeedCurrent)))
                {
                    x = (destination.Value.X / Game.CellSize) * Game.CellSize;
                    y = (destination.Value.Y / Game.CellSize) * Game.CellSize;
                    destination = null;
                }
                else
                {
                    x += xSpeedCurrent;
                    y += ySpeedCurrent;
                }
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

        public abstract void Draw(Graphics G);
    }
}
