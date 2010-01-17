using System;
using System.Drawing;
using System.Collections.Generic;
using System.Text;

namespace Zombie_Sim
{
    class Zombie : Sentient
    {
        public const int MOVE_DISTANCE_MAX = 7;
        public const int Strength = 8;

        public Zombie(int x, int y, int h, Bitmap da )
        {
            ID = IDC++;
            attackers = new Queue<Sentient>();
            location = new Rectangle(x, y, DrawWidth, DrawWidth);
            Health = h;
            MaxHealth = Health;
            DrawArea = da;
            DrawColor = Color.Red;
            Fighting = false;
            MoveDistance = MOVE_DISTANCE_MAX;
            if (rnd == null)
                rnd = new Random();

        }

        //ZOMBIE BITCHES!
        public Zombie(Rectangle r, int h, Bitmap da)
        {
            ID = IDC++;
            attackers = new Queue<Sentient>();
            location = r;
            Health = h;
            MaxHealth = Health;
            DrawArea = da;
            DrawColor = Color.Red;
            Fighting = false;
            MoveDistance = MOVE_DISTANCE_MAX;
            if (rnd == null)
                rnd = new Random();
        }

        ~Zombie()
        {
            removeReference(this);
        }

        public override void Move()
        {
            if (!Fighting)
            {

                Heal();
                if (target == null)
                    Wander();
                else
                {
                    GoTowardsTarget();                    
                }
            }            
        }

        public override void Update()
        {
            //
            if (!Fighting && attackers.Count > 0)
            {
                target = attackers.Dequeue();
                Fighting = true;
            }
            if (!Fighting)
            {
                int closest = SpotDistance;
                LinkedListNode<Sentient> sn = Sentients.First;
                while (sn != null && attackers.Count == 0)
                {
                    if (sn.Value is Person)
                    {
                        if (location.IntersectsWith(sn.Value.getLocation()))
                        {
                            attackers.Enqueue(sn.Value);
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

        }

        public override void attack(int damage)
        {
            Health -= damage;
            if (Health <= 0)
            {
                removeReference(this);
                
                Sentients.Remove(this);
                Draw(true);
            }
        }
    }
}
