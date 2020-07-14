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
    class Poof : GameObject
    {
        private int animTimer = 0;
        private int imageIndex = 0;

        public Poof(int gridX, int gridY) : base(gridX, gridY)
        {
            Objects.Add(this);
            Game.GameStep += Game_GameStep;
        }

        protected override void Game_GameStep()
        {
            if (++animTimer % 5 == 0 && ++imageIndex > Sprites.CharacterRects["dorf_die"].Count - 1)
                Destroy();
        }

        public override void Draw(Graphics G)
        {
            G.DrawImage(Sprites.GetCharacterSprite(Sprites.CharacterRects["dorf_die"][imageIndex], true), X, Y);
        }
    }
}
