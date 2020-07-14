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
    public abstract class GameObject
    {
        public static List<GameObject> Objects = new List<GameObject>();

        protected Rectangle srcRect;

        public int X { get { return GridX * Game.CellSize; } }
        public int Y { get { return GridY * Game.CellSize; } }
        public int GridX { get; protected set; }
        public int GridY { get; protected set; }
        public bool Solid { get; protected set; } = false;
        public bool Foreground { get; protected set; } = false;
        protected Tile.TileType StandingOn { get { return Tile.Map[GridX, GridY].Type; } }

        public GameObject(int gridX, int gridY)
        {
            GridX = gridX;
            GridY = gridY;
            Game.GameStep += Game_GameStep;
        }

        [OnDeserializing]
        protected void OnDeserializing(StreamingContext context)
        {
            Game.GameStep += Game_GameStep;
        }

        protected virtual void Game_GameStep()
        {
            if (this is IGrabbable && ((IGrabbable)this).IsGrabbed)
            {
                GridX = Player.Player1.Reticle.GridX;
                GridY = Player.Player1.Reticle.GridY;
            }
        }

        protected bool WithinBounds(int gridX, int gridY)
        {
            return (gridX >= 0 && gridY >= 0 && gridX <= Tile.Map.GetUpperBound(0) && gridY <= Tile.Map.GetUpperBound(1));
        }

        public abstract void Draw(Graphics G);
        public virtual void Destroy()
        {
            Objects.Remove(this);
            Game.GameStep -= Game_GameStep;
        }
    }
}
