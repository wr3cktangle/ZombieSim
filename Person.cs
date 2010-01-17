using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace Zombie_Sim
{
    class Person : Sentient
    {
        public const int MOVE_DISTANCE_MAX = 10;
        //public const int SPOT_DISTANCE = 25;
        protected enum MentalState { Calm, Panicked, Aggressive, OnFire, Hunter };
        protected static Color[] ModeColors = new Color[]{Color.Green, Color.Yellow, Color.Blue, Color.Orange, Color.Violet};        
        protected const int WalkDistance = 5;
        protected const int RunDistance = WalkDistance * 3;

        private MentalState DefaultMode;
        private MentalState Mode;
        private int Strength;
        private int Courage;        

        public Person(int x, int y, int h, int s, int c, Bitmap da)
        {
            ID = IDC++;
            attackers = new Queue<Sentient>();
            location = new Rectangle(x, y, DrawWidth, DrawWidth);
            Health = h;
            MaxHealth = Health;
            Strength = s;
            Courage = c;
            Mode = MentalState.Calm;
            DrawColor = ModeColors[(int)Mode];
            DrawArea = da;
            Fighting = false;
            MoveDistance = MOVE_DISTANCE_MAX;
            if (rnd == null)
                rnd = new Random();
        }

        public Person(Rectangle r, int h, int s, int c, Bitmap da)
        {
            ID = IDC++;
            attackers = new Queue<Sentient>();
            location = r;
            Health = h;
            MaxHealth = Health;
            Strength = s;
            Courage = c;
            DefaultMode = MentalState.Calm;
            Mode = DefaultMode;
            DrawColor = ModeColors[(int)Mode];
            DrawArea = da;
            Fighting = false;
            MoveDistance = WalkDistance;
            if (rnd == null)
                rnd = new Random();
        }

        ~Person()
        {
            removeReference(this);
        }

        public override void Move()
        {
            if (!Fighting)
            {
                Heal();
                if (Mode == MentalState.Calm)
                {
                    MoveDistance = WalkDistance;
                    Wander();
                }
                else if (Mode == MentalState.Aggressive)
                {
                    MoveDistance = RunDistance;
                    GoTowardsTarget();
                }
                else if (Mode == MentalState.Panicked)
                {
                    MoveDistance = RunDistance;
                    GoAwayFromTarget();
                }
            }            
        }

        public override void Update()
        {            
            Mode = DefaultMode;
            if (!Fighting)
            {
                int closest = SpotDistance;
                LinkedListNode<Sentient> sn = Sentients.First;
                while (sn != null && attackers.Count == 0)
                {
                    if (sn.Value is Zombie)
                    {
                        if (location.IntersectsWith(sn.Value.getLocation()))
                        {
                            attackers.Enqueue(sn.Value);
                            Mode = MentalState.Aggressive;
                            sn.Value.addToAttackers(this);
                        }
                        else
                        {
                            int dx = location.Left - sn.Value.getLocation().Left;
                            int dy = location.Top - sn.Value.getLocation().Top;
                            int d = (int)Math.Ceiling(Math.Sqrt(dx * dx + dy * dy));
                            if (d < closest)
                            {
                                closest = d;
                                target = sn.Value;
                            }
                        }
                    }
                    sn = sn.Next;
                }
                if (attackers.Count > 0)
                {
                    target = attackers.Dequeue();
                    Fighting = true;
                    Mode = MentalState.Aggressive;
                }
                if (Mode == MentalState.Calm && closest < SpotDistance)
                {
                    if (rnd.Next(1, 10) > Courage)
                        Mode = MentalState.Panicked;
                    else
                        Mode = MentalState.Aggressive;
                }
            }
            else
            {
                if (target == null)
                {
                    Fighting = false;
                    Update();
                    return;
                }
                target.attack((int)Math.Floor(rnd.NextDouble() * Strength));
            }
            DrawColor = ModeColors[(int)Mode];
        }

        public override void attack(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                removeReference(this);
                
                LinkedListNode<Sentient> sn = Sentients.Find(this);
                LinkedListNode<Sentient> zn = new LinkedListNode<Sentient>(new Zombie(location, rnd.Next(3, 11), DrawArea));
                if (sn == null)
                {
                    Sentients.AddLast(zn);
                    Sentients.Remove(this);
                }
                else
                {                
                    Sentients.AddAfter(sn, zn);
                    Sentients.Remove(this);
                }

                Draw(true);
            }
        }
    }
}
