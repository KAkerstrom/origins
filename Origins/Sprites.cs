//
//
//
//
//
//
// Don't look here, it's garbage code that needs to be un-garbage-ified
//
//
//
//
//
//
//
//
//

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
    public static class Sprites
    {
        public static Image CharacterSheet { get; private set; }
        public static Image TileSheet { get; private set; }
        public static Dictionary<Tile.TileType, Dictionary<Tile.Adjacency, Rectangle>> TileRects { get; private set; } = new Dictionary<Tile.TileType, Dictionary<Tile.Adjacency, Rectangle>>();
        public static Dictionary<string, List<Rectangle>> CharacterRects { get; private set; } = new Dictionary<string, List<Rectangle>>();
        public static Dictionary<string, Rectangle> FoodRects { get; private set; } = new Dictionary<string, Rectangle>();
        public static List<Rectangle> TreeRects { get; private set; } = new List<Rectangle>();

        static Sprites()
        {
            LoadSpritesheets();

            LoadGrassRects();
            LoadDirtRects();

            LoadTreeRects();
            LoadFoodRects();

            LoadCharacterRects("dorf");
            LoadCharacterRects("player");
            LoadCharacterRects("pred");
        }

        private static void LoadSpritesheets()
        {
            try
            {
                TileSheet = new Bitmap($@".\GRAPHIX\tiles\tilesheet.png");
                CharacterSheet = new Bitmap($@".\GRAPHIX\PlayerSprites.png");
            }
            catch
            {
                MessageBox.Show("Could not read image files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private static void LoadGrassRects()
        {
            try
            {
                //Parse the edge file for the grass sprites
                TileRects.Add(Tile.TileType.Grass, new Dictionary<Tile.Adjacency, Rectangle>());
                string grassFile = File.ReadAllText($@".\GameFiles\GrassEdges.txt");
                grassFile = grassFile.Substring(grassFile.IndexOf('\n') + 1); //Remove the first line
                foreach (string line in grassFile.Split('\n'))
                {
                    //Parse the directions
                    Tile.Adjacency adj = new Tile.Adjacency();
                    adj.N = line[0] == '1';
                    adj.E = line[1] == '1';
                    adj.S = line[2] == '1';
                    adj.W = line[3] == '1';

                    //Parse the rectangle and add
                    Rectangle rect = new Rectangle();
                    rect.X = int.Parse(line.Remove(0, 5).Substring(0, line.Remove(0, 5).IndexOf(' ')));
                    rect.Y = int.Parse(line.Remove(0, 5).Substring(line.Remove(0, 5).IndexOf(' ') + 1));
                    rect.Width = rect.Height = Game.CellSize;

                    TileRects[Tile.TileType.Grass].Add(adj, rect);
                }
            }
            catch
            {
                MessageBox.Show("Could not read the grass edges text file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private static void LoadDirtRects()
        {
            try
            {
                //Parse the edge file for the grass sprites
                TileRects.Add(Tile.TileType.Dirt, new Dictionary<Tile.Adjacency, Rectangle>());
                string dirtFile = File.ReadAllText($@".\GameFiles\DirtEdges.txt");
                dirtFile = dirtFile.Substring(dirtFile.IndexOf('\n') + 1); //Remove the first line
                foreach (string line in dirtFile.Split('\n'))
                {
                    //Parse the directions
                    Tile.Adjacency adj = new Tile.Adjacency();
                    adj.N = line[0] == '1';
                    adj.E = line[1] == '1';
                    adj.S = line[2] == '1';
                    adj.W = line[3] == '1';

                    //Parse the rectangle and add
                    Rectangle rect = new Rectangle();
                    rect.X = int.Parse(line.Remove(0, 5).Substring(0, line.Remove(0, 5).IndexOf(' ')));
                    rect.Y = int.Parse(line.Remove(0, 5).Substring(line.Remove(0, 5).IndexOf(' ') + 1));
                    rect.Width = rect.Height = Game.CellSize;

                    TileRects[Tile.TileType.Dirt].Add(adj, rect);
                }
            }
            catch
            {
                MessageBox.Show("Could not read the dirt edges text file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private static void LoadCharacterRects(string character)
        {
            character = character.ToLower();
            try
            {
                using (StreamReader sr = new StreamReader($@".\GameFiles\{character}Rects.txt"))
                {
                    while (!sr.EndOfStream)
                    {
                        string action = sr.ReadLine();
                        string coords = sr.ReadLine();
                        CharacterRects.Add($"{character}_" + action, new List<Rectangle>());

                        do
                        {
                            string xCoord = coords.Substring(0, coords.IndexOf(' '));
                            string yCoord = coords.Substring(coords.IndexOf(' ') + 1);
                            CharacterRects[$"{character}_" + action].Add(new Rectangle(int.Parse(xCoord), int.Parse(yCoord), Game.CellSize, Game.CellSize));
                            coords = sr.ReadLine();
                        }
                        while (!string.IsNullOrWhiteSpace(coords));
                    }
                }
            }
            catch
            {
                MessageBox.Show($"Could not read the {character}Rects text file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private static void LoadFoodRects()
        {
            try
            {
                using (StreamReader sr = new StreamReader($@".\GameFiles\foodRects.txt"))
                {
                    while (!sr.EndOfStream)
                    {
                        string action = sr.ReadLine();
                        string coords = sr.ReadLine();
                        FoodRects.Add("Food_" + action, new Rectangle());

                        do
                        {
                            string xCoord = coords.Substring(0, coords.IndexOf(' '));
                            string yCoord = coords.Substring(coords.IndexOf(' ') + 1);
                            FoodRects["Food_" + action] = new Rectangle(int.Parse(xCoord), int.Parse(yCoord), Game.CellSize, Game.CellSize);
                            coords = sr.ReadLine();
                        }
                        while (!string.IsNullOrWhiteSpace(coords));
                    }
                }
            }
            catch
            {
                MessageBox.Show("Could not read the foodRects text file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private static void LoadTreeRects()
        {
            try
            {
                foreach (string line in File.ReadAllText($@".\GameFiles\treeRects.txt").Split('\n'))
                {
                    if (line.Contains('*'))
                    {
                        string newLine = line;
                        int xCoord = int.Parse(newLine.Substring(0, newLine.IndexOf(' ')));
                        newLine = newLine.Remove(0, newLine.IndexOf(' '));
                        int yCoord = int.Parse(newLine.Substring(0, newLine.IndexOf('*')));
                        TreeRects.Add(new Rectangle(xCoord, yCoord, Game.CellSize, Game.CellSize * 2));
                    }
                    else
                    {
                        int xCoord = int.Parse(line.Substring(0, line.IndexOf(' ')));
                        int yCoord = int.Parse(line.Substring(line.IndexOf(' ') + 1));
                        TreeRects.Add(new Rectangle(xCoord, yCoord, Game.CellSize, Game.CellSize));
                    }
                }
            }
            catch
            {
                MessageBox.Show("Could not read the treeRects text file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        public static Image GetCharacterSprite(Rectangle srcRect, bool facingRight)
        {
            Image sprite = new Bitmap(Game.CellSize, Game.CellSize);
            Graphics.FromImage(sprite).DrawImage(CharacterSheet, new Rectangle(0, 0, Game.CellSize, Game.CellSize), srcRect, GraphicsUnit.Pixel);
            if (!facingRight)
                sprite.RotateFlip(RotateFlipType.RotateNoneFlipX);

            return sprite;
        }
    }
}
