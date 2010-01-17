using System;
using System.Drawing;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Zombie_Sim
{
    abstract class Sentient
    {
        public const int DrawWidth = 5;
        public const int SpotDistance = 25;
        protected static UInt64 IDC= 0;

        protected Color DrawColor;
        protected static Color EraseColor = Color.Black;
        protected Bitmap DrawArea;
        protected static LinkedList<Sentient> Sentients;
        protected static LinkedList<Building> Buildings;
        //will be initialized when a Person or Zombie is 
        //created if it hasn't been initialized already
        protected static Random rnd;

        protected Queue<Sentient> attackers;
        protected Sentient target;
        protected Rectangle location;
        protected int Health;
        protected bool Fighting;
        protected int MoveDistance;
        protected int MaxHealth;
        public UInt64 ID;

        public abstract void Update();
        public abstract void Move();

        public void Draw(bool erase)
        {
            Color c = erase ? EraseColor : DrawColor;
            SolidBrush Brush = new SolidBrush(c);
            Graphics g = Graphics.FromImage(DrawArea);
            g.FillRectangle(Brush, location);
            Brush.Dispose();
            g.Dispose();
        }

        public Rectangle getLocation()
        {
            return location;
        }

        public static void setLists(LinkedList<Sentient> sl, LinkedList<Building> bl)
        {
            Sentients = sl;
            Buildings = bl;
        }

        //Any sentient may wander. The only thing I need to make sure is that the place they're wandering
        //  to isn't a building and isn't off screen. Otherwise, anything goes.
        //ToDo: Make Work Properly - People wander off the sides, left and right, but not top and bottom
        // WHY? It's totally gonna be something stupid too, probably.
        // Fixed - Totally was. Overwrote value of good when checking if the new coordinates with okay
        //   Up/Down wise. A bad Left/Right wise check would be overwritten with a good Up/Down check.
        protected void Wander()
        {            
            int dx;
            int dy;
           
            Rectangle r;
            bool good;
            do
            {
                dx = rnd.Next(MoveDistance + 1);
                dy = rnd.Next(MoveDistance - dx + 1);
                dx = rnd.Next(2) == 0 ? dx : -dx;
                dy = rnd.Next(2) == 0 ? dy : -dy;
                good = true /*MoveDistance >= Math.Sqrt(dx * dx + dy * dy)*/;
                r = new Rectangle(location.Left + dx, location.Top + dy, DrawWidth, DrawWidth);
                good = (r.Left >= 0) && (r.Right <= DrawArea.Width);
                good = good & (r.Top >= 0) && (r.Bottom <= DrawArea.Height);
                LinkedListNode<Building> bn = Buildings.First;
                /*LinkedListNode<Sentient> sn = Sentients.Find(this).Next;*/
                while ((bn != null /*|| sn != null*/) && good)
                {
                    if(bn != null)
                    {
                        good = !(r.IntersectsWith(bn.Value.getSurface()));
                        bn = bn.Next;
                    }
                    /*
                    if(sn != null && good)
                    {
                        good = !(r.IntersectsWith(sn.Value.getLocation()));
                        sn = sn.Next;
                    }
                     */
                }
            } while (!good);
            location = r;
        }

        protected void GoTowardsTarget()
        {
            if (target == null)
            {
                Wander();
                return;
            }
            int xinverse = target.getLocation().Left > location.Left ? 1 : -1;
            int yinverse = target.getLocation().Top > location.Top ? 1 : -1;
            int dx, dy;
            Rectangle r;
            bool good;
            do
            {
                dx = rnd.Next(MoveDistance + 1);
                dy = rnd.Next(MoveDistance - dx + 1);
                dx = xinverse > 0 ? dx : -dx;
                dy = yinverse > 0 ? dy : -dy;
                good = true /*MoveDistance >= Math.Sqrt(dx * dx + dy * dy)*/;
                r = new Rectangle(location.Left + dx, location.Top + dy, DrawWidth, DrawWidth);
                good = (r.Left >= 0) && (r.Right <= DrawArea.Width);
                good = good & (r.Top >= 0) && (r.Bottom <= DrawArea.Height);

                LinkedListNode<Building> bn = Buildings.First;
                while (good && bn != null)
                {
                    if (r.IntersectsWith(bn.Value.getSurface()))
                        good = false;
                    bn = bn.Next;
                }
            } while (!good);

            location = r;
        }

        protected void GoAwayFromTarget()
        {
            if (target == null)
            {
                Wander();
                return;
            }
            int xinverse = target.getLocation().Left > location.Left ? -1 : 1;
            int yinverse = target.getLocation().Top > location.Top ? -1 : 1;
            int dx, dy;
            Rectangle r;
            bool good;
            do
            {
                dx = rnd.Next(MoveDistance + 1);
                dy = rnd.Next(MoveDistance - dx + 1);
                dx = xinverse > 0 ? dx : -dx;
                dy = yinverse > 0 ? dy : -dy;
                good = true /*MoveDistance >= Math.Sqrt(dx * dx + dy * dy)*/;
                r = new Rectangle(location.Left + dx, location.Top + dy, DrawWidth, DrawWidth);
                good = (r.Left >= 0) && (r.Right <= DrawArea.Width);
                good = good & (r.Top >= 0) && (r.Bottom <= DrawArea.Height);

                LinkedListNode<Building> bn = Buildings.First;
                while (good && bn != null)
                {
                    if (r.IntersectsWith(bn.Value.getSurface()))
                        good = false;
                    bn = bn.Next;
                }
            } while (!good);

            location = r;
        }

        //This code is called by an attacking sentient upon the victim sentient
        // should always be used like x.addToAttackers(this);
        public void addToAttackers(Sentient s)
        {
            if(!attackers.Contains(s))
                attackers.Enqueue(s);
            if (!Fighting)
            {
                target = attackers.Dequeue();
                Fighting = true;
            }
        }

        //Remove references from other sentients
        //Possible places there might be a reference are:
        // A sentient's target
        // A sentient's attacker
        public static void removeReference(Sentient s)
        {
            if (s == null)
                return;

            LinkedListNode<Sentient> sn = Sentients.First;
            while (sn != null)
            {
                Sentient snv = sn.Value;
                if (!snv.Equals(s))
                {
                    if (snv.target != null && snv.target.Equals(s))
                        snv.target = null;
                    if (snv.attackers.Contains(s))
                    {
                        if (snv.attackers.Contains(s))
                        {
                            int c = snv.attackers.Count;
                            Sentient x;
                            for (int i = 0; i < c; i++)
                            {
                                x = snv.attackers.Dequeue();
                                if (!x.Equals(s))
                                    snv.attackers.Enqueue(x);
                            }
                        }
                    }
                }
                sn = sn.Next;
            }
            /*
            try
            {
                if (target.Equals(s))
                {
                    target = null;
                    Fighting = false;
                }
                else
                {
                    //loop through attackers, dequeuing everyone, and requeuing everyone but
                    //the one being removed
                    if (attackers.Contains(s))
                    {
                        int c = attackers.Count;
                        Sentient x;
                        for (int i = 0; i < c; i++)
                        {
                            x = attackers.Dequeue();
                            if (x != s)
                                attackers.Enqueue(x);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //I'm sorry
                string m = ex.Message;
            }
             */
        }

        protected void Heal()
        {
            if (Health < MaxHealth)
                Health += 2;
        }
        
        /*
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (obj is Sentient)
            {
                Sentient s = (Sentient)obj;
                return this.ID == s.ID;
            }
            return base.Equals(obj);
        }
         */
        

        //Subtract damage from Health.
        //If Zombie killed, remove references to it from attackers. Remove from Sentients.
        //If Person killed, remove references to it from attackers. Replace with a Zombie.
        public abstract void attack(int damage);

    }
}
