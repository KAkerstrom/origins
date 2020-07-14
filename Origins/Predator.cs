using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Origins
{
    [Serializable]
    class Predator : Character, IHittable
    {
        public event StatusUpdateDelegate StatusUpdate;

        private enum PredState
        {
            Emerge, Idle, Wander, ChaseDorf, Attack, Hurt
        }

        private PredState currentState = PredState.Emerge;
        private PredState nextState = PredState.Emerge;
        private LocalView localView;

        public double Health { get; private set; }

        public Predator(int gridX, int gridY) : base(gridX, gridY)
        {
            if (Game.GridPointOccupied(gridX, gridY, false, this))
            {
                Destroy();
                return;
            }

            Strength = Game.R.Next(1, 3);
            localView = new LocalView(this, 30);
            spriteIndex = "pred_emerge";
            speed = 0.1;
            Health = 3;
            StatusUpdate += Game.UpdateStatus;
        }

        public void Destroy()
        {
            Characters.Remove(this);
            Game.GameStep -= Step;
            StatusUpdate -= Game.UpdateStatus;
        }

        protected override void Step()
        {
            base.Step();

            if (animTimer % 10 == 0 && ++imageIndex > Sprites.CharacterRects[spriteIndex].Count - 1)
            {
                imageIndex = 0;
                if (currentState == PredState.Emerge)
                    nextState = PredState.Idle;
            }

            if (currentState != nextState)
                stateTimer = 0;
            currentState = nextState;
            int xMove, yMove;
            switch (currentState)
            {
                case PredState.Emerge:
                    ChangeSprite("pred_emerge");
                    break;

                case PredState.Idle:
                    ChangeSprite("pred_idle");
                    if (!IsMoving)
                    {
                        Dorf foundFood = (Dorf)Characters.FirstOrDefault(x => x is Dorf && Math.Abs(x.GridX - GridX) < 5 && Math.Abs(x.GridY - GridY) < 5);
                        if (foundFood != null)
                            nextState = PredState.ChaseDorf;
                        else
                        {
                            bool horizontal = Game.R.Next(2) == 0;
                            Move(horizontal ? Game.R.Next(3) - 1 : 0, horizontal ? 0 : Game.R.Next(3) - 1);
                            nextState = PredState.Wander;
                        }
                    }
                    break;

                case PredState.Wander:
                    ChangeSprite("pred_run");
                    if (!IsMoving)
                        nextState = PredState.Idle;
                    break;

                case PredState.ChaseDorf:
                    ChangeSprite("pred_run");
                    if (!IsMoving)
                    {
                        Dorf foundFood = (Dorf)Characters.FirstOrDefault(x => x is Dorf && Math.Abs(x.GridX - GridX) + Math.Abs(x.GridY - GridY) == 1);
                        if (foundFood != null && StandingOn != Tile.TileType.Water)
                            nextState = PredState.Attack;
                        else
                        {
                            if (localView.MoveToward(localView.GetNearestCharacter(typeof(Dorf)), out xMove, out yMove) && !Game.GridPointOccupied(GridX + xMove, GridY + yMove, true, this))
                                Move(xMove, yMove);
                            else
                            {
                                //Move randomly until it can get to the Dorf
                                bool horizontal = Game.R.Next(2) == 0;
                                Move(horizontal ? Game.R.Next(3) - 1 : 0, horizontal ? 0 : Game.R.Next(3) - 1);
                            }
                        }
                    }
                    break;

                case PredState.Attack:
                    ChangeSprite("pred_attack");
                    if (stateTimer == 0)
                    {
                        Dorf foundFood = (Dorf)Characters.FirstOrDefault(x => x is Dorf && Math.Abs(x.GridX - GridX) + Math.Abs(x.GridY - GridY) == 1);
                        if (foundFood != null)
                        {
                            new SoundPlayer(Properties.Resources.Hit_Hurt2).Play();
                            foundFood.Hit(Strength, this);
                        }
                    }
                    else if (stateTimer > 40)
                        nextState = PredState.Idle;
                    break;

                case PredState.Hurt:
                    ChangeSprite("pred_hurt");
                    if (stateTimer >= 30)
                        nextState = PredState.Idle;
                    break;
            }

        }

        public override void Draw(Graphics G)
        {
            G.DrawImage(Sprites.GetCharacterSprite(Sprites.CharacterRects[spriteIndex][imageIndex], facingRight), X, Y);
        }

        private void ChangeSprite(string sprite)
        {
            if (StandingOn == Tile.TileType.Water && spriteIndex != "pred_emerge")
                sprite = "pred_swim";

            if (spriteIndex != sprite)
            {
                spriteIndex = sprite;
                imageIndex = 0;
            }
        }

        public void Hit(int strength, Character atttacker)
        {
            nextState = PredState.Hurt;
            Hurt(strength);
        }

        /// <summary>
        /// Reduces health by the specified damage, and checks for death.
        /// </summary>
        /// <param name="damage">The damage.</param>
        /// <param name="reason">The reason (phrased as "(name) has died from (reason)...").</param>
        /// <returns>Whether the predator has died from the damage.</returns>
        private bool Hurt(double damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                StatusUpdate?.Invoke($"You have neutralized a predator.", Color.DarkGreen);
                Characters.Remove(this);
                new Poof(GridX, GridY); //Yeah, sure
                Game.GameStep -= Step;
                StatusUpdate -= Game.UpdateStatus;
                return true;
            }
            return false;
        }
    }
}
