using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Origins
{
    [Serializable]
    public class Tile
    {
        #region Enums & Structs

        public enum TileType
        {
            None, Water, Grass, Dirt
        }

        public enum TileLayer
        {
            Background, Foreground, Item
        }

        public struct Adjacency
        {
            public bool N, W, E, S;
        }

        #endregion Enums & Structs

        #region Static

        private const int size = 15; //Needs to be 15 (not 16) for some reason

        public static Tile[,] Map { get; set; }
        public static Image MapImage { get; set; }

        static Tile()
        {
            InitializeMap(50, 50);
            UpdateFullImage();
        }

        #endregion Static

        #region Private Fields

        private TileType type;
        private bool drawBg = false;
        private Rectangle bgRect;
        private Rectangle fgRect;

        #endregion Private Fields

        #region Public Properties

        public double SpeedModifier { get; set; } = 1;
        public TileType Type
        {
            get { return type; }
            private set { SetType(value); }
        }

        #endregion Public Properties

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Tile"/> class.
        /// </summary>
        /// <param name="type">The tile type.</param>
        public Tile(TileType type)
        {
            Type = type;
            fgRect.Width = fgRect.Height = bgRect.Width = bgRect.Height = size;
        }

        #endregion Constructors

        #region Private Methods

        /// <summary>
        /// Sets the tile type and applies attributes for that type.
        /// </summary>
        /// <param name="type">The tile type.</param>
        private void SetType(TileType type)
        {
            this.type = type;
            switch (type)
            {
                case TileType.Water:
                    SpeedModifier = 0.4;
                    fgRect.X = bgRect.X = 51; //water
                    fgRect.Y = bgRect.Y = 17;
                    break;
                case TileType.Grass:
                    SpeedModifier = 1;
                    fgRect.X = bgRect.X = 85; //grass
                    fgRect.Y = bgRect.Y = 17;
                    break;
                case TileType.Dirt:
                    SpeedModifier = 1.5;
                    fgRect.X = bgRect.X = 136; //dirt
                    fgRect.Y = bgRect.Y = 170;
                    break;
            }
        }

        /// <summary>
        /// Creates a new tile-map.
        /// </summary>
        private static void InitializeMap(int xMapSize, int yMapSize)
        {
            if (xMapSize < 1 || yMapSize < 1)
                throw new Exception("Cannot create a map with less than 1 tile.");

            Map = new Tile[xMapSize, yMapSize];

            for (int i = 0; i < xMapSize; i++)
                for (int j = 0; j < yMapSize; j++)
                {
                    if (i < 2 || j < 2 || j > yMapSize - 2 || i > xMapSize - 2)
                        Map[i, j] = new Tile(TileType.Water);
                    else
                    {
                        int xPara = Math.Abs(i - Map.GetUpperBound(0) / 2);
                        int yPara = Math.Abs(j - Map.GetUpperBound(1) / 2);
                        Map[i, j] = new Tile(Game.R.Next(xPara < yPara ? yPara : xPara) < 10 && Game.R.Next(30) > 0 ? TileType.Grass : TileType.Water);
                    }
                }
        }

        private static void UpdateFullImage()
        {
            for (int i = 0; i <= Map.GetUpperBound(0); i++)
                for (int j = 0; j <= Map.GetUpperBound(1); j++)
                    UpdateAdjacency(i, j);

            MapImage = new Bitmap((Map.GetUpperBound(0) + 1) * Game.CellSize, (Map.GetUpperBound(1) + 1) * Game.CellSize);
            using (Graphics G = Graphics.FromImage(MapImage))
            {
                for (int i = 0; i <= Map.GetUpperBound(0); i++)
                    for (int j = 0; j <= Map.GetUpperBound(1); j++)
                        Map[i, j].Draw(G, i * Game.CellSize, j * Game.CellSize);
            }
        }

        private static void UpdateImagePart(int gridX, int gridY)
        {
            if (!PointIsInMap(gridX, gridY))
                return;

            UpdateAdjacency(gridX, gridY);
            if (PointIsInMap(gridX - 1, gridY)) UpdateAdjacency(gridX - 1, gridY);
            if (PointIsInMap(gridX + 1, gridY)) UpdateAdjacency(gridX + 1, gridY);
            if (PointIsInMap(gridX, gridY - 1)) UpdateAdjacency(gridX, gridY - 1);
            if (PointIsInMap(gridX, gridY + 1)) UpdateAdjacency(gridX, gridY + 1);

            MapImage = new Bitmap(MapImage);
            using (Graphics G = Graphics.FromImage(MapImage))
            {
                Map[gridX, gridY].Draw(G, gridX * Game.CellSize, gridY * Game.CellSize);
                if (PointIsInMap(gridX - 1, gridY)) Map[gridX - 1, gridY].Draw(G, (gridX - 1) * Game.CellSize, gridY * Game.CellSize);
                if (PointIsInMap(gridX + 1, gridY)) Map[gridX + 1, gridY].Draw(G, (gridX + 1) * Game.CellSize, gridY * Game.CellSize);
                if (PointIsInMap(gridX, gridY - 1)) Map[gridX, gridY - 1].Draw(G, gridX * Game.CellSize, (gridY - 1) * Game.CellSize);
                if (PointIsInMap(gridX, gridY + 1)) Map[gridX, gridY + 1].Draw(G, gridX * Game.CellSize, (gridY + 1) * Game.CellSize);
            }
        }

        private static void UpdateAdjacency(int gridX, int gridY)
        {
            if (!PointIsInMap(gridX, gridY))
                return;

            if (Map[gridX, gridY].Type == TileType.Grass)
            {
                Adjacency adj = GetAdjacency(gridX, gridY, TileType.Grass);
                Adjacency dirtAdj = GetAdjacency(gridX, gridY, TileType.Dirt);
                adj.N |= dirtAdj.N;
                adj.W |= dirtAdj.W;
                adj.E |= dirtAdj.E;
                adj.S |= dirtAdj.S;
                Map[gridX, gridY].fgRect = Sprites.TileRects[TileType.Grass][adj];

                Map[gridX, gridY].drawBg = !(adj.N && adj.E && adj.W && adj.S);
                if (Map[gridX, gridY].drawBg)
                {
                    adj = GetAdjacency(gridX, gridY, TileType.Water);
                    if (adj.N || adj.E || adj.W || adj.S)
                        Map[gridX, gridY].bgRect = new Rectangle(51, 17, size, size); //water
                    else
                        Map[gridX, gridY].bgRect = new Rectangle(136, 170, size, size); //dirt
                }
            }
            else if (Map[gridX, gridY].Type == TileType.Dirt)
            {
                Adjacency adj = GetAdjacency(gridX, gridY, TileType.Dirt);
                Map[gridX, gridY].fgRect = Sprites.TileRects[TileType.Dirt][adj];

                Map[gridX, gridY].drawBg = !(adj.N && adj.E && adj.W && adj.S);
                if (Map[gridX, gridY].drawBg)
                {
                    adj = GetAdjacency(gridX, gridY, TileType.Water);
                    if (adj.N || adj.E || adj.W || adj.S)
                        Map[gridX, gridY].bgRect = new Rectangle(51, 17, size, size); //water
                    else
                        Map[gridX, gridY].bgRect = new Rectangle(136, 272, size, size); //grass
                }
            }
        }

        public static void LoadMap(Tile[,] map)
        {
            Map = map;
            UpdateFullImage();
        }

        private static Adjacency GetAdjacency(int gridX, int gridY, TileType type)
        {
            Adjacency adj = new Adjacency();
            adj.N = (gridY > 0) && type == Map[gridX, gridY - 1].Type;
            adj.W = (gridX > 0) && type == Map[gridX - 1, gridY].Type;
            adj.E = (gridX < Map.GetUpperBound(0)) && type == Map[gridX + 1, gridY].Type;
            adj.S = (gridY < Map.GetUpperBound(1)) && type == Map[gridX, gridY + 1].Type;
            return adj;
        }

        /// <summary>
        /// Checks for the initial placement of a WaterFlow at a point on the map, and places it.
        /// </summary>
        /// <param name="gridX">The grid x.</param>
        /// <param name="gridY">The grid y.</param>
        private static void CheckWaterFlow(int gridX, int gridY)
        {
            if (Map[gridX, gridY].type != TileType.Dirt)
                return;

            Adjacency adj = GetAdjacency(gridX, gridY, TileType.Water);
            if (adj.N || adj.W || adj.E || adj.S)
                new WaterFlow(gridX, gridY);
        }

        #endregion Private Methods

        #region Public Methods

        public void Draw(Graphics G, int X, int Y)
        {
            if (drawBg)
                G.DrawImage(Sprites.TileSheet, new Rectangle(X, Y, Game.CellSize, Game.CellSize), bgRect, GraphicsUnit.Pixel);

            G.DrawImage(Sprites.TileSheet, new Rectangle(X, Y, Game.CellSize, Game.CellSize), fgRect, GraphicsUnit.Pixel);
        }

        public static bool PointIsInMap(int gridX, int gridY)
        {
            return (gridX >= 0 || gridY >= 0 || gridX <= Map.GetUpperBound(0) || gridY <= Map.GetUpperBound(1));
        }

        public static void ChangeTile(int gridX, int gridY, TileType type)
        {
            //Don't allow tiles to be changed on the outer border - water needed for predator spawning
            if (gridX < 1 || gridY < 1 || gridX > Map.GetUpperBound(0) - 1 || gridY > Map.GetUpperBound(1) - 1 || Map[gridX, gridY].Type == type)
                return;

            Map[gridX, gridY].Type = type;

            if (type == TileType.Dirt)
                CheckWaterFlow(gridX, gridY);

            UpdateImagePart(gridX, gridY);
        }

        #endregion Public Methods
    }
}
