using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Origins
{
    [Serializable]
    public class Dorf : Character, IHittable, IGrabbable
    {
        public event StatusUpdateDelegate StatusUpdate;

        #region Enums

        public enum DorfState
        {
            Idle, Walk, StartEat, Eat, Hurt, RunAway, GoToWater, GoToLand, GoToFood, Grabbed, Die
        }

        #endregion Enums

        public static int Amount { get; private set; }

        private string name;
        private bool isMale;
        private double age;
        private int generation;
        private double health, stamina, hunger, thirst, strength, intelligence, radiation;
        private int awarenessTimer = Game.R.Next(50);
        private DorfState currentState = DorfState.Idle;
        private DorfState nextState = DorfState.Idle;
        private LocalView localView;
        private Character attacker;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public DorfState CurrentState
        {
            get { return currentState; }
        }

        public double Health
        {
            get { return health; }
            set { health = LimitValue(value, 0, 100); }
        }

        public double Stamina
        {
            get { return stamina; }
            set { stamina = LimitValue(value, 0, 100); }
        }

        public double Thirst
        {
            get { return thirst; }
            set { thirst = LimitValue(value, 0, 100); }
        }

        public double Hunger
        {
            get { return hunger; }
            set { hunger = LimitValue(value, 0, 100); }
        }

        public double Intelligence
        {
            get { return intelligence; }
            set { intelligence = LimitValue(value, 0, 100); }
        }

        public double Speed
        {
            get { return speed; }
            set { speed = LimitValue(value, 0.1, 20); }
        }

        public double Age { get { return age; } }
        public int Generation { get { return generation; } }
        public double Radiation { get { return radiation; } }
        public bool IsGrabbed { get; private set; }

        public Dorf(int gridX, int gridY) : base(gridX, gridY)
        {
            speed = 0.2;
            isMale = Game.R.Next(2) == 0;
            generation = 1;
            health = stamina = 100;
            hunger = 0;
            strength = Game.R.Next(21) + Game.R.Next(21) + Game.R.Next(20) + 1; //Bell curve between 1-60
            intelligence = Game.R.Next(5, 15);
            radiation = 0;
            BasicInitialization();
        }

        public Dorf(int gridX, int gridY, Egg egg) : base(gridX, gridY)
        {
            speed = egg.Speed;
            isMale = Game.R.Next(2) == 0;
            generation = egg.Generation;
            health = stamina = 100;
            hunger = egg.Hunger;
            strength = egg.Strength;
            intelligence = egg.Intelligence;
            radiation = egg.Radiation;
            StatusUpdate?.Invoke($"{name} was born!", Color.Navy);
            BasicInitialization();
        }

        private void BasicInitialization()
        {
            name = GenerateName(isMale);
            age = 0;
            spriteIndex = "dorf_idle";
            Amount++;
            localView = new LocalView(this, (int)intelligence);
            StatusUpdate += Game.UpdateStatus;
        }

        private string GenerateName(bool isMale)
        {
            string vowels = "aeiou";
            string consonants = "bcdfghjklmnpqrstvwxz";
            int nameLength = Game.R.Next(4, 8);
            bool addVowel = !isMale;
            string output = string.Empty;

            for (int i = 0; i < nameLength; i++)
            {
                if (addVowel)
                    output += vowels[Game.R.Next(vowels.Length)];
                else
                    output += consonants[Game.R.Next(consonants.Length)];

                addVowel = !addVowel;
            }

            //My name isn't possible, so I'm making it possible
            if (output == "kile")
                output = "kyle";

            return char.ToUpper(output[0]) + output.Substring(1);
        }

        protected override void Step()
        {
            base.Step();
            stateTimer++;
            age += 0.001;
            Thirst += (StandingOn == Tile.TileType.Water) ? -0.4 : 0.02;
            Hunger += 0.01;
            if (Thirst == 100 && Hurt(0.02, "thirst")) return;
            if (Hunger == 100 && Hurt(0.02, "hunger")) return;
            //if (Stamina == 0 && Hurt(0.005, "exhaustion")) return;

            if (animTimer % 10 == 0 && ++imageIndex > Sprites.CharacterRects[spriteIndex].Count - 1)
                imageIndex = 0;

            int xMove, yMove;
            UpdateCurrentState();
            switch (currentState)
            {
                case DorfState.Idle:
                    ChangeSprite("dorf_idle");
                    if (++awarenessTimer % 50 == 0)
                        MakeNewDecision();
                    break;

                case DorfState.Walk:
                    ChangeSprite("dorf_run");
                    if (!IsMoving)
                        nextState = DorfState.Idle;
                    break;

                case DorfState.Hurt:
                    ChangeSprite("dorf_hurt");
                    if (stateTimer > 20)
                        nextState = DorfState.RunAway;
                    break;

                case DorfState.RunAway:
                    ChangeSprite("dorf_run_away");

                    if (!IsMoving)
                    {
                        if (stateTimer > 400)
                            nextState = DorfState.Idle;
                        else
                        {
                            localView.MoveAway(attacker, out xMove, out yMove);
                            if (Game.GridPointOccupied(GridX + xMove, GridY + yMove, true, this))
                            {
                                bool horizontal = Game.R.Next(2) == 0;
                                Move(horizontal ? Game.R.Next(3) - 1 : 0, horizontal ? 0 : Game.R.Next(3) - 1);
                            }
                            else
                                Move(xMove, yMove);
                        }
                    }
                    break;

                case DorfState.GoToFood:
                    ChangeSprite("dorf_run");
                    if (!IsMoving)
                    {
                        Food foundFood = (Food)GameObject.Objects.FirstOrDefault(x => x is Food && x.GridX == GridX && x.GridY == GridY);
                        if (foundFood != null)
                            nextState = DorfState.StartEat;

                        if (localView.MoveToward(typeof(Food), out xMove, out yMove) && !Game.GridPointOccupied(GridX + xMove, GridY + yMove, true, this))
                            Move(xMove, yMove);
                        else
                        {
                            bool horizontal = Game.R.Next(2) == 0;
                            Move(horizontal ? Game.R.Next(3) - 1 : 0, horizontal ? 0 : Game.R.Next(3) - 1);
                        }
                    }
                    break;

                case DorfState.GoToWater:
                    ChangeSprite("dorf_run");
                    if (!IsMoving)
                    {
                        if (localView.MoveToward(Tile.TileType.Water, out xMove, out yMove) && !Game.GridPointOccupied(GridX + xMove, GridY + yMove, true, this))
                            Move(xMove, yMove);
                        else if (!IsMoving)
                            nextState = DorfState.Idle;
                    }
                    break;

                case DorfState.GoToLand:
                    ChangeSprite("dorf_run");
                    if (!IsMoving)
                    {
                        if (localView.MoveToward(Tile.TileType.Grass, out xMove, out yMove) && !Game.GridPointOccupied(GridX + xMove, GridY + yMove, true, this))
                            Move(xMove, yMove);
                        else if (!IsMoving)
                            nextState = DorfState.Idle;
                    }
                    break;

                case DorfState.StartEat:
                    ChangeSprite("dorf_eat"); //Temporary, change to eating sprite
                    Food food = (Food)GameObject.Objects.FirstOrDefault(x => x is Food && x.GridX == GridX && x.GridY == GridY);
                    if (food != null) //just in case something happened to the food
                    {
                        Eat(food);
                        nextState = DorfState.Eat;
                    }
                    else
                        nextState = DorfState.Idle;
                    break;

                case DorfState.Eat:
                    ChangeSprite("dorf_eat"); //Temporary, change to eating sprite
                    //new SoundPlayer(Properties.Resources.eat).Play();
                    if (stateTimer > 50)
                        nextState = DorfState.Idle;
                    break;

                case DorfState.Grabbed:
                    ChangeSprite("dorf_idle");
                    Teleport(Player.Player1.Reticle.GridX, Player.Player1.Reticle.GridY);
                    break;

                case DorfState.Die:
                    ChangeSprite("dorf_die");
                    break;
            }
        }

        private void Eat(Food food)
        {
            Health += food.HealthRegen;
            Stamina += food.StaminaRegen;
            Hunger -= food.HungerRegen;
            food.Destroy();
        }

        /// <summary>
        /// Reduces health by the specified damage, and checks for death.
        /// </summary>
        /// <param name="damage">The damage.</param>
        /// <param name="reason">The reason (phrased as "[name] has died from [reason]...").</param>
        /// <returns>Whether the dorf has died from the damage.</returns>
        private bool Hurt(double damage, string reason)
        {
            Health -= damage;
            if (Health <= 0)
            {
                Amount--;
                Characters.Remove(this);
                StatusUpdate?.Invoke($"{name} has died from {reason}...", Color.Red);
                if (Amount == 0)
                    StatusUpdate?.Invoke($"GENOCIDE: There are no Dorfs left!", Color.DarkRed);
                new Poof(GridX, GridY);
                new SoundPlayer(Properties.Resources.death).Play();
                Game.GameStep -= Step;
                StatusUpdate -= Game.UpdateStatus;
                return true;
            }
            return false;
        }

        public void Destroy()
        {
            Amount--;
            Characters.Remove(this);
            Game.GameStep -= Step;
            StatusUpdate -= Game.UpdateStatus;
        }

        private void MakeNewDecision()
        {
            if (!IsMoving)
            {
                if (thirst > 70 - Game.R.Next(20) && localView.InRadius(Tile.TileType.Water, (int)intelligence))
                    nextState = DorfState.GoToWater;

                else if (hunger > 70 - Game.R.Next(20) && localView.InRadius(typeof(Food), (int)intelligence))
                    nextState = DorfState.GoToFood;

                else if (StandingOn == Tile.TileType.Water && localView.InRadius(Tile.TileType.Grass, (int)intelligence))
                    nextState = DorfState.GoToLand;

                else if (Game.R.Next(3) == 0)
                {
                    bool horizontal = Game.R.Next(2) == 0;
                    Move(horizontal ? Game.R.Next(3) - 1 : 0, horizontal ? 0 : Game.R.Next(3) - 1);
                    nextState = DorfState.Walk;
                }
                else
                {
                    nextState = DorfState.Idle;
                    if (Game.R.Next(1000) == 0 && !Game.GridPointOccupied(GridX, GridY, true, this))
                        new Egg(this);
                }
            }
        }

        public override void Draw(Graphics G)
        {
            G.DrawImage(Sprites.GetCharacterSprite(Sprites.CharacterRects[spriteIndex][imageIndex], facingRight), X, Y);

            //Debug info
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                G.DrawString(name, new Font("Arial", 5), new SolidBrush(Color.Black), X, Y - 5);
                G.DrawString(currentState.ToString(), new Font("Arial", 5), new SolidBrush(Color.Black), X, Y + Game.CellSize + 9);
                Healthbar.Draw(G, X, Y + Game.CellSize, 2, Game.CellSize, health, 100, health > 40 ? Color.White : Color.Yellow, Color.Red);
                //Healthbar.Draw(G, X, Y + Game.CellSize + 3, 2, Game.CellSize, stamina, 100, stamina > 30 ? Color.White : Color.Yellow, Color.Green);
                Healthbar.Draw(G, X, Y + Game.CellSize + 3, 2, Game.CellSize, hunger, 100, hunger < 60 ? Color.White : Color.Yellow, Color.SandyBrown);
                Healthbar.Draw(G, X, Y + Game.CellSize + 6, 2, Game.CellSize, thirst, 100, thirst < 70 ? Color.White : Color.Yellow, Color.SkyBlue);
                //G.DrawRectangle(Pens.DeepSkyBlue, X - (float)(Intelligence * Game.CellSize), Y - (float)(Intelligence * Game.CellSize), (float)(Intelligence * 2 + 1) * Game.CellSize, (float)(Intelligence * 2 + 1) * Game.CellSize + 1);
            }
        }

        private void ChangeSprite(string sprite)
        {
            if (StandingOn == Tile.TileType.Water)
            {
                if (currentState == DorfState.Hurt || currentState == DorfState.RunAway)
                    sprite = "dorf_swim_hurt";
                else
                    sprite = "dorf_swim";
            }

            if (spriteIndex != sprite)
            {
                spriteIndex = sprite;
                imageIndex = 0;
            }
        }

        private void UpdateCurrentState()
        {
            if (currentState != nextState)
            {
                currentState = nextState;

                switch (currentState)
                {
                    case DorfState.GoToFood:
                    case DorfState.GoToLand:
                    case DorfState.GoToWater:
                    case DorfState.Walk: ChangeSprite("dorf_run"); break;
                    case DorfState.Idle: ChangeSprite("dorf_idle"); break;
                    case DorfState.Hurt: ChangeSprite("dorf_hurt"); break;
                    case DorfState.RunAway: ChangeSprite("dorf_run_away"); break;
                }

                stateTimer = 0;
            }
        }

        public void Hit(int strength, Character attacker)
        {
            this.attacker = attacker;
            nextState = DorfState.Hurt;
            Hurt(strength * 10, attacker is Player ? "murder" : "a predator");
        }

        protected override bool Move(int xMove, int yMove)
        {
            if (base.Move(xMove, yMove))
            {
                Stamina -= (StandingOn == Tile.TileType.Water) ? 1 : 0.5;
                return true;
            }
            return false;
        }

        public bool Grab()
        {
            if (IsMoving)
                return false;

            nextState = DorfState.Grabbed;
            IsGrabbed = true;
            return true;
        }

        public void LetGo()
        {
            if (IsGrabbed)
            {
                IsGrabbed = false;
                nextState = DorfState.Idle;
            }
        }
    }
}
