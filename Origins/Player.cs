using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Media;
using System.Windows.Forms;
using System.Windows.Input;

namespace Origins
{
    [Serializable]
    public class Player : Character
    {
        public enum PlayerAction
        {
            Idle, Run, Swim, Use
        }

        #region Static

        public static Player Player1 = new Player(25, 25);
        private static Image playerSprites;

        static Player()
        {
            try
            {
                playerSprites = new Bitmap($@".\GRAPHIX\PlayerSprites.png");
            }
            catch
            {
                MessageBox.Show("Could not read player image file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        #endregion Static

        private int moveTimer = 0;

        public Reticle Reticle;
        public int xFacing { get; private set; } = 0;
        public int yFacing { get; private set; } = 0;

        public Player(int gridX, int gridY) : base(gridX, gridY)
        {
            speed = 1;
            spriteIndex = "player_idle";
            Reticle = new Reticle(this);
            Strength = 1;
            Game.KeyReleased += Game_KeyRelease;
        }

        private void Game_KeyRelease(char key)
        {
            if (IsMoving)
                return;

            //I don't like this system, what with the key release events and such
            Dictionary<char, char> CtrlChars = new Dictionary<char, char>
            {
                {'\u001a', 'Z' },
                {'\u0018', 'X' },
                {'\u0003', 'C' },
            };
            if (CtrlChars.Keys.Contains(key))
                key = CtrlChars[key];

            key = char.ToUpper(key);
            if (key == 'Z' && !IsMoving && Reticle.ReticleOn != Tile.TileType.Grass && spriteIndex != "player_use")
            {
                ChangeAction("player_use");
                Tile.ChangeTile(Reticle.GridX, Reticle.GridY, Tile.TileType.Grass);
            }
            else if (key == 'X' && !IsMoving && Reticle.ReticleOn != Tile.TileType.Water && GameObject.Objects.Find(x => x.GridX == Reticle.GridX && x.GridY == Reticle.GridY) == null && spriteIndex != "player_use")
            {
                ChangeAction("player_use");
                Tile.ChangeTile(Reticle.GridX, Reticle.GridY, Tile.TileType.Dirt);
            }
            else if (key == 'C' && !IsMoving && StandingOn != Tile.TileType.Water && spriteIndex != "player_punch" && spriteIndex != "player_kick")
            {
                GameObject obj = GameObject.Objects.Find(x => x.GridX == Reticle.GridX && x.GridY == Reticle.GridY && x is IHittable);
                if (obj != null)
                {
                    ChangeAction(Game.R.Next(2) == 0 ? "player_kick" : "player_punch");
                    ((IHittable)obj).Hit(Strength, this);
                }
                else
                {
                    Character character = Characters.Find(x => x is IHittable && x.GridX == Reticle.GridX && x.GridY == Reticle.GridY);
                    if (character != null)
                    {
                        new SoundPlayer(Properties.Resources.Hit_Hurt).Play();
                        ChangeAction(Game.R.Next(2) == 0 ? "player_kick" : "player_punch");
                        ((IHittable)character).Hit(Strength, this);
                    }
                }
            }
            else if (key == 'I')
            {
                Dorf dorf = (Dorf)Characters.FirstOrDefault(x => x is Dorf && x.GridX == Reticle.GridX && x.GridY == Reticle.GridY);
                if(dorf != null)
                {
                    InspectForm inspect = new InspectForm(dorf);
                    inspect.Show();
                }
            }
        }

        public override void Draw(Graphics G)
        {
            G.DrawImage(Sprites.GetCharacterSprite(Sprites.CharacterRects[spriteIndex][imageIndex], facingRight), X, Y);
            Reticle.Draw(G);
        }

        protected override void Step()
        {
            base.Step();

            if (animTimer % 10 == 0 && ++imageIndex > Sprites.CharacterRects[spriteIndex].Count - 1)
            {
                imageIndex = 0;
                if (spriteIndex == "player_use" || spriteIndex == "player_punch" || spriteIndex == "player_kick")
                    ChangeAction("player_idle");
            }

            if (moveTimer > 0)
                moveTimer--;

            if (spriteIndex != "player_ready_attack")
                if (Keyboard.IsKeyDown(Key.Left))
                    Move(-1, 0);
                else if (Keyboard.IsKeyDown(Key.Right))
                    Move(1, 0);
                else if (Keyboard.IsKeyDown(Key.Up))
                    Move(0, -1);
                else if (Keyboard.IsKeyDown(Key.Down))
                    Move(0, 1);

            if (StandingOn == Tile.TileType.Water && spriteIndex != "player_use")
                ChangeAction("player_swim");
            else if ((spriteIndex == "player_idle" || spriteIndex == "player_ready_attack") && Keyboard.IsKeyDown(Key.C) && StandingOn != Tile.TileType.Water)
                ChangeAction("player_ready_attack");
            else if (spriteIndex != "player_use" && spriteIndex != "player_punch" && spriteIndex != "player_kick")
            {
                if (IsMoving)
                    ChangeAction("player_run");
                else
                    ChangeAction("player_idle");
            }
        }

        protected override bool Move(int xMove, int yMove)
        {
            if (!IsMoving)
            {
                if(xFacing != xMove || yFacing != yMove)
                    moveTimer = 12;

                if (moveTimer <= 0 && xFacing == xMove && yFacing == yMove)
                    return base.Move(xMove, yMove);

                xFacing = xMove;
                yFacing = yMove;
                if (xMove > 0) facingRight = true;
                else if (xMove < 0) facingRight = false;
            }
            return false;
        }

        private void ChangeAction(string sprite)
        {
            if (spriteIndex == sprite)
                return;

            spriteIndex = sprite;
            imageIndex = 0;
            animTimer = 0;
        }
    }
}
