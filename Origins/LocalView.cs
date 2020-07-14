using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Origins
{
    [Serializable]
    class LocalView
    {
        private Character character;
        private int radius;
        public List<Dorf> Dorfs = new List<Dorf>();
        public List<Player> Players = new List<Player>();
        public List<GameObject> Objects = new List<GameObject>();
        public Tile.TileType[,] Tiles = new Tile.TileType[5, 5];

        public bool HasDorf { get { return Dorfs.Count > 0; } }
        public bool HasWater
        {
            get
            {
                for (int i = 0; i <= Tiles.GetUpperBound(0); i++)
                    for (int j = 0; j <= Tiles.GetUpperBound(1); j++)
                        if (Tiles[i, j] == Tile.TileType.Water)
                            return true;
                return false;
            }
        }
        public bool HasLand
        {
            get
            {
                for (int i = 0; i <= Tiles.GetUpperBound(0); i++)
                    for (int j = 0; j <= Tiles.GetUpperBound(1); j++)
                        if (Tiles[i, j] != Tile.TileType.Water)
                            return true;
                return false;
            }
        }

        public LocalView(Character character, int radius)
        {
            this.character = character;
            Tiles = new Tile.TileType[radius * 2 + 1, radius * 2 + 1];
            UpdateMapView(radius);
        }

        public void UpdateMapView(int radius)
        {
            if (radius <= 0)
                throw new Exception("Cannot create a LocalView object with a range of less than 1 block.");

            this.radius = radius;

            Tiles = new Tile.TileType[radius * 2 + 1, radius * 2 + 1];
            for (int i = 0; i <= radius * 2; i++)
                for (int j = 0; j <= radius * 2; j++)
                    if (character.GridX + (i - radius) < 0 || character.GridY + (j - radius) < 0
                        || character.GridX + i > Tile.Map.GetUpperBound(0) || character.GridY + j > Tile.Map.GetUpperBound(1))
                        Tiles[i, j] = Tile.TileType.None;
                    else
                        Tiles[i, j] = Tile.Map[character.GridX + (i - radius), character.GridY + (j - radius)].Type;
        }

        public bool MoveToward(Tile.TileType type, out int xMove, out int yMove)
        {
            UpdateMapView(radius);
            xMove = 0;
            yMove = 0;

            if (Tiles[Tiles.GetUpperBound(0) / 2, Tiles.GetUpperBound(1) / 2] == type)
                return false;

            List<Tuple<int, int>> directions = new List<Tuple<int, int>>();
            Random R = new Random();
            for (int r = 0; r < radius; r++)
            //Make this more efficient - This code works "from the inside out", but repeatedly re-checks the inside
            {
                for (int i = -r; i <= r; i++)
                    for (int j = -r; j <= r; j++)
                    {
                        if (Tiles[Tiles.GetUpperBound(0) / 2 + i, Tiles.GetUpperBound(1) / 2 + j] == type)
                        {
                            if (i != 0 && j != 0)
                            {
                                directions.Add(new Tuple<int, int>(Math.Sign(i), 0));
                                directions.Add(new Tuple<int, int>(0, Math.Sign(j)));
                            }
                            else
                                directions.Add(new Tuple<int, int>(Math.Sign(i), Math.Sign(j)));
                        }
                    }

                while (directions.Count > 0)
                {
                    int chosenIndex = R.Next(directions.Count);
                    Tuple<int, int> chosenDir = directions[chosenIndex];
                    if (!Game.GridPointOccupied(character.GridX + chosenDir.Item1, character.GridY + chosenDir.Item2, true, character))
                    {
                        xMove = chosenDir.Item1;
                        yMove = chosenDir.Item2;
                        return true;
                    }
                    else
                        directions.RemoveAt(chosenIndex);
                }
            }

            return false;
        }

        public bool MoveToward(Character goToCharacter, out int xMove, out int yMove)
        {
            xMove = 0;
            yMove = 0;

            if (goToCharacter == null)
                return false;

            xMove = Math.Sign(goToCharacter.GridX - character.GridX);
            yMove = Math.Sign(goToCharacter.GridY - character.GridY);

            if (xMove != 0 && yMove != 0)
            {
                if (Game.R.Next(2) == 0) xMove = 0;
                else yMove = 0;
            }

            return true;
        }

        public bool MoveAway(Tile.TileType type, out int xMove, out int yMove)
        {
            UpdateMapView(radius);
            xMove = 0;
            yMove = 0;

            if (Tiles[Tiles.GetUpperBound(0) / 2, Tiles.GetUpperBound(1) / 2] == type)
                return false;

            List<Tuple<int, int>> directions = new List<Tuple<int, int>>();
            Random R = new Random();
            for (int r = 0; r < radius; r++)
            {
                //Make this more efficient - This code works "from the inside out", but repeatedly re-checks the inside
                for (int i = -r; i <= r; i++)
                    for (int j = -r; j <= r; j++)
                        if (Tiles[Tiles.GetUpperBound(0) / 2 + i, Tiles.GetUpperBound(1) / 2 + j] == type)
                        {
                            if (i != 0 && j != 0)
                            {
                                directions.Add(new Tuple<int, int>(-Math.Sign(i), 0));
                                directions.Add(new Tuple<int, int>(0, -Math.Sign(j)));
                            }
                            else
                                directions.Add(new Tuple<int, int>(-Math.Sign(i), -Math.Sign(j)));
                        }

                if (directions.Count > 0)
                {
                    Tuple<int, int> chosenDir = directions[R.Next(directions.Count)];
                    xMove = chosenDir.Item1;
                    yMove = chosenDir.Item2;
                    return true;
                }
            }

            return false;
        }

        public bool MoveAway(Character attacker, out int xMove, out int yMove)
        {
            xMove = 0;
            yMove = 0;

            if (attacker == null)
                return false;

            xMove = Math.Sign(character.GridX - attacker.GridX);
            yMove = Math.Sign(character.GridY - attacker.GridY);

            if (xMove != 0 && yMove != 0)
            {
                if (Game.R.Next(2) == 0) xMove = 0;
                else yMove = 0;
            }

            return true;
        }

        public Character GetNearestCharacter(Type charType)
        {
            return Character.Characters.FindAll(x => x != character && x.GetType() == charType).OrderBy(x => Math.Sqrt(Math.Pow(x.X - character.X, 2) + Math.Pow(x.Y - character.Y, 2))).FirstOrDefault();
        }

        public bool MoveToward(Type objectType, out int xMove, out int yMove)
        {
            xMove = 0;
            yMove = 0;

            GameObject nearestObject = GameObject.Objects.FindAll(x => x.GetType() == objectType).OrderBy(x => Math.Sqrt(Math.Pow(x.X - character.X, 2) + Math.Pow(x.Y - character.Y, 2))).FirstOrDefault();

            if (nearestObject == null || Math.Sqrt((Math.Pow(character.GridX - nearestObject.GridX, 2) + Math.Pow(character.GridY - nearestObject.GridY, 2))) > radius)
                return false;

            xMove = Math.Sign(nearestObject.X - character.X);
            yMove = Math.Sign(nearestObject.Y - character.Y);

            if (xMove != 0 && yMove != 0)
            {
                if (Game.R.Next(2) == 0)
                    xMove = 0;
                else
                    yMove = 0;
            }

            return true;
        }

        public bool InRadius(Type objectType, int radius)
        {
            GameObject nearestObject = GameObject.Objects.FindAll(x => x.GetType() == objectType).OrderBy(x => Math.Sqrt(Math.Pow(x.X - character.X, 2) + Math.Pow(x.Y - character.Y, 2))).FirstOrDefault();
            return (nearestObject != null && Math.Sqrt((Math.Pow(character.GridX - nearestObject.GridX, 2) + Math.Pow(character.GridY - nearestObject.GridY, 2))) <= radius);
        }

        public bool InRadius(Tile.TileType tileType, int radius)
        {
            UpdateMapView(radius);
            for (int i = 0; i < Tiles.GetUpperBound(0); i++)
                for (int j = 0; j < Tiles.GetUpperBound(1); j++)
                    if (Tiles[i, j] == tileType)
                        return true;

            return false;
        }
    }
}
