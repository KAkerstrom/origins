using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Origins
{
    public class Reticle
    {
        private Player player;
        public Point Position { get { return new Point(GridX * Game.CellSize, GridY * Game.CellSize); } }

        public int X { get { return GridX * Game.CellSize; } }
        public int Y { get { return GridY * Game.CellSize; } }
        public bool Grabbing { get; private set; } = false;

        private void Game_GameStep()
        {
            if (Keyboard.IsKeyDown(Key.S) && !Grabbing)
            {
                Dorf dorf = (Dorf)Character.Characters.FirstOrDefault(x => x is Dorf && x.GridX == GridX && x.GridY == GridY);
                if (dorf != null && dorf.Grab())
                {
                    Grabbing = true;
                    return;
                }

                GameObject obj = GameObject.Objects.FirstOrDefault(x => x.GridX == GridX && x.GridY == GridY);
                if (obj is IGrabbable && ((IGrabbable)obj).Grab())
                {
                    Grabbing = true;
                    return;
                }
            }
            else if (Keyboard.IsKeyUp(Key.S) && Grabbing)
            {

                Dorf dorf = (Dorf)Character.Characters.FirstOrDefault(x => x is Dorf && x.GridX == GridX && x.GridY == GridY);
                if (dorf != null && dorf.IsGrabbed)
                {
                    dorf.LetGo();
                    Grabbing = false;
                    return;
                }

                GameObject obj = GameObject.Objects.FirstOrDefault(x => x.GridX == GridX && x.GridY == GridY);
                if (obj is IGrabbable && ((IGrabbable)obj).Grab())
                {
                    ((IGrabbable)obj).LetGo();
                    Grabbing = false;
                    return;
                }
            }
        }

        public int GridX
        {
            get
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || (Grabbing && Game.GridPointOccupied(player.GridX + player.xFacing, player.GridY, true, player)))
                    return player.GridX;
                else
                {
                    if (player.GridX + player.xFacing < 0 || player.GridX + player.xFacing > Tile.Map.GetUpperBound(0))
                        return player.GridX;
                    else
                        return player.GridX + player.xFacing;
                }
            }
        }

        public int GridY
        {
            get
            {
                if (Keyboard.IsKeyDown(Key.LeftShift) || (Grabbing && Game.GridPointOccupied(player.GridX, player.GridY + player.yFacing, true, player)))
                    return player.GridY;
                else
                {
                    if (player.GridY + player.yFacing < 0 || player.GridY + player.yFacing > Tile.Map.GetUpperBound(1))
                        return player.GridY;
                    else
                        return player.GridY + player.yFacing;
                }
            }
        }

        public Tile.TileType ReticleOn { get { return Tile.Map[GridX, GridY].Type; } }

        public Reticle(Player player)
        {
            this.player = player;
            Game.GameStep += Game_GameStep;
        }

        public void Draw(Graphics G)
        {
            if (Grabbing)
                G.DrawImage(Sprites.CharacterSheet, new Rectangle(Position.X, Position.Y, Game.CellSize, Game.CellSize),
                        new Rectangle(0, 112, Game.CellSize, Game.CellSize), GraphicsUnit.Pixel);
            else
                G.DrawImage(Sprites.CharacterSheet, new Rectangle(Position.X, Position.Y, Game.CellSize, Game.CellSize),
                        new Rectangle(0, 80, Game.CellSize, Game.CellSize), GraphicsUnit.Pixel);
        }
    }
}
