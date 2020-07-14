using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;


namespace Origins
{
    public delegate void GameStepDelegate();
    public delegate void KeyReleasedDelegate(char key);
    public delegate void StatusUpdateDelegate(string status, System.Drawing.Color color);

    static class Game
    {
        #region Events

        public static event GameStepDelegate GameStep;
        public static event KeyReleasedDelegate KeyReleased;
        public static event StatusUpdateDelegate StatusUpdate;

        #endregion Events

        #region Private Fields

        private const int CELL_SIZE = 16;
        private static bool paused = true;
        private static double scale = 2;
        private static int year;
        private static Timer stepTmr = new Timer();
        private static bool debug = false;


        #endregion Private Fields

        #region Public Properties

        public static Random R = new Random();
        public static int CellSize { get { return CELL_SIZE; } }
        public static int ViewHeight { get; private set; }
        public static int ViewWidth { get; private set; }
        public static bool Paused
        {
            get { return paused; }
            set { paused = value; }
        }
        public static int Year
        {
            get { return year; }
            set { year = value; }
        }
        public static double Scale
        {
            get { return scale; }
            set { scale = value; }
        }
        public static int View_X { get { return Player.Player1.X - (ViewWidth + CellSize) / 2; } }
        public static int View_Y { get { return Player.Player1.Y - (ViewHeight + CellSize) / 2; } }

        #endregion Public Properties

        #region Constructor

        static Game()
        {
            StartGame();
        }

        #endregion Constructor

        #region Public Methods

        public static void StartGame()
        {
            for (int i = 0; i < 10; i++)
                new Dorf(R.Next(Tile.Map.GetUpperBound(0)), R.Next(Tile.Map.GetUpperBound(1)));
            for (int i = 0; i < 30; i++)
                new Tree(R.Next(Tile.Map.GetUpperBound(0)), R.Next(Tile.Map.GetUpperBound(1)));
            for (int i = 0; i < 20; i++)
                new Food(R.Next(Tile.Map.GetUpperBound(0)), R.Next(Tile.Map.GetUpperBound(1)));

            stepTmr.Interval = 5;
            stepTmr.Start();
            stepTmr.Tick += StepTmr_Tick;
        }

        public static void UpdateStatus(string status, System.Drawing.Color color)
        {
            StatusUpdate?.Invoke(status, color);
        }

        public static void SetDisplaySize(int width, int height)
        {
            ViewWidth = width;
            ViewHeight = height;
        }

        public static void DrawScreen(Graphics G)
        {
            G.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            G.TranslateTransform(-(Player.Player1.X * (float)scale - ViewWidth / 2 + CellSize / 2) - (CellSize / 2) * (float)scale, -(Player.Player1.Y * (float)scale - ViewHeight / 2 + CellSize / 2) - (CellSize / 2) * (float)scale);
            G.ScaleTransform((float)scale, (float)scale);

            G.DrawImage(Tile.MapImage, 0, 0);

            foreach (GameObject obj in GameObject.Objects.FindAll(x => !x.Foreground))
                obj.Draw(G);

            foreach (Character character in Character.Characters)
                character.Draw(G);

            foreach (GameObject obj in GameObject.Objects.FindAll(x => x.Foreground))
                obj.Draw(G);
        }

        public static void ReleaseKey(char key)
        {
            if (Paused)
                return;

            if (key == 'k' && scale < 10)
                scale += 0.5;
            else if (key == '½' && scale > 1)
                scale -= 0.5;

            if (char.ToUpper(key) == '1')
                new Dorf(Player.Player1.Reticle.GridX, Player.Player1.Reticle.GridY);
            if (char.ToUpper(key) == '2')
                new Predator(Player.Player1.Reticle.GridX, Player.Player1.Reticle.GridY);
            if (char.ToUpper(key) == '3')
                new Stump(Player.Player1.Reticle.GridX, Player.Player1.Reticle.GridY);
            if (char.ToUpper(key) == '4')
                new Tree(Player.Player1.Reticle.GridX, Player.Player1.Reticle.GridY);
            if (char.ToUpper(key) == '5')
                foreach (Dorf dorf in Character.Characters.FindAll(x => x is Dorf))
                    new Egg(dorf);

            KeyReleased?.Invoke(key);
        }

        public static void Zoom(bool zoomIn)
        {
            if (zoomIn && scale < 10)
                scale += 0.5;
            else if (!zoomIn && scale > 1)
                scale -= 0.5;
        }

        #endregion Public Methods


        private static void StepTmr_Tick(object sender, EventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Home))
                debug = true;
            else if (Keyboard.IsKeyDown(Key.End))
                debug = false;

            if (Paused)
                return;

            if (R.Next(1000 + (GameObject.Objects.FindAll(x => x is Tree).Count * 10)) == 0)
                new Tree(R.Next(Tile.Map.GetUpperBound(0)), R.Next(Tile.Map.GetUpperBound(1)));

            if (R.Next((2000 - (Dorf.Amount * 10)) >= 10 ? (3000 - (Dorf.Amount * 10)) : 10) == 0)
            {
                int direction = R.Next(4);
                switch (direction)
                {
                    case 0: new Predator(R.Next(Tile.Map.GetUpperBound(0)), 0); break;
                    case 1: new Predator(R.Next(Tile.Map.GetUpperBound(0)), Tile.Map.GetUpperBound(1)); break;
                    case 2: new Predator(0, R.Next(Tile.Map.GetUpperBound(1))); break;
                    case 3: new Predator(Tile.Map.GetUpperBound(0), R.Next(Tile.Map.GetUpperBound(1))); break;
                }
            }

            GameStep?.Invoke();
        }

        public static bool GridPointOccupied(int gridX, int gridY, bool onlySolid = true, object toIgnore = null)
        {
            //This can probably be cleaned up quite a bit
            foreach (GameObject obj in GameObject.Objects)
                if (obj != toIgnore && (obj.Solid || !onlySolid) && obj.GridX == gridX && obj.GridY == gridY)
                    if (obj is IGrabbable)
                    {
                        if (!((IGrabbable)obj).IsGrabbed)
                            return true;
                    }
                    else
                        return true;

            foreach (Character character in Character.Characters)
                if (character != toIgnore && (character is IGrabbable ? !((IGrabbable)character).IsGrabbed : true)) //should fix this, it's gross
                {
                    if (character.GridX == gridX && character.GridY == gridY)
                        return true;

                    if (character.destination != null && character.destination == new Point(gridX * CellSize, gridY * CellSize))
                        return true;
                }

            return false;
        }

        public static void TogglePaused()
        {
            Paused = !Paused;
        }

        public static bool SaveGame()
        {
            try
            {
                using (Stream stream = File.Open("./SaveFiles/Objects.bin", FileMode.Create))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(stream, GameObject.Objects);
                }

                using (Stream stream = File.Open("./SaveFiles/Characters.bin", FileMode.Create))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(stream, Character.Characters.FindAll(x => !(x is Player)));
                }

                using (Stream stream = File.Open("./SaveFiles/Tiles.bin", FileMode.Create))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(stream, Tile.Map);
                }

                using (Stream stream = File.Open("./SaveFiles/Flows.bin", FileMode.Create))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(stream, WaterFlow.Flows);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Save failed.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool LoadGame()
        {
            try
            {
                using (Stream stream = File.Open("./SaveFiles/Objects.bin", FileMode.Open))
                {
                    for (int i = GameObject.Objects.Count - 1; i >= 0; i--)
                        GameObject.Objects[i].Destroy();

                    BinaryFormatter bf = new BinaryFormatter();
                    GameObject.Objects = (List<GameObject>)bf.Deserialize(stream);
                }

                using (Stream stream = File.Open("./SaveFiles/Characters.bin", FileMode.Open))
                {
                    for (int i = Character.Characters.Count - 1; i >= 0; i--)
                    {
                        //Could definitely be handled better, but this is a last minute thing
                        if (Character.Characters[i] is Dorf)
                            ((Dorf)Character.Characters[i]).Destroy();
                        else if (Character.Characters[i] is Predator)
                            ((Predator)Character.Characters[i]).Destroy();
                    }

                    BinaryFormatter bf = new BinaryFormatter();
                    Character.Characters = Character.Characters.FindAll(x => x is Player);
                    Character.Characters.AddRange((List<Character>)bf.Deserialize(stream));
                }

                using (Stream stream = File.Open("./SaveFiles/Tiles.bin", FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    Tile.LoadMap((Tile[,])bf.Deserialize(stream));
                }

                using (Stream stream = File.Open("./SaveFiles/Flows.bin", FileMode.Open))
                {
                    for (int i = WaterFlow.Flows.Count - 1; i >= 0; i--)
                        WaterFlow.Flows[i].Destroy();

                    BinaryFormatter bf = new BinaryFormatter();
                    WaterFlow.Flows = (List<WaterFlow>)bf.Deserialize(stream);
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Load failed.\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
