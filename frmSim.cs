using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Zombie_Sim
{
    public partial class frmSim : Form
    {
        private Bitmap DrawArea;
        private Random rnd;

        private LinkedList<Sentient> Sentients;
        private LinkedList<Building> Buildings;
        private frmControls FormControls;

        private const int MAXINTERVAL = 1000;
        private const int MININTERVAL = 100;
        private const double ZOMBIE_PERCENTAGE = .1;
        private const int OFFSET = 6;
        private const int MINSENTIENTS = 100;
        private const int MAXSENTIENTS = 500;
        private const int MINBUILDINGS = 250;
        private const int MAXBUILDINGS = 500;

        public frmSim()
        {
            InitializeComponent();
        }

        private void frmSim_Load(object sender, EventArgs e)
        {
            Buildings = new LinkedList<Building>();
            Sentients = new LinkedList<Sentient>();
            FormControls = new frmControls();
            rnd = new Random();
            InitializeDrawArea();
            this.BackgroundImage = DrawArea;
            genBuildings(rnd.Next(MINBUILDINGS, MAXBUILDINGS));
            genSentients(rnd.Next(MINSENTIENTS, MAXSENTIENTS));            
            Sentient.setLists(Sentients, Buildings);
            this.DoubleBuffered = true;
        }

        private void InitializeDrawArea()
        {
            DrawArea = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //DrawArea = new Bitmap(this.ClientRectangle.Width - (OFFSET * 2), this.ClientRectangle.Height - (OFFSET * 2) - Menus.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g;

            g = Graphics.FromImage(DrawArea);
            // clear the drawing area to background color
            g.Clear(Color.Black);
        }

        private void genBuildings(int max)
        {
            int minSpacing = Sentient.DrawWidth;
            int minW = Sentient.DrawWidth * 3;
            int minH = minW;
            int maxW = DrawArea.Width / 5;
            int maxH = DrawArea.Height / 5;
            if (minW > maxW)
                minW /= 2;
            if (minH > maxH)
                minH /= 2;
            for(int i = 0; i < max; i++)
            {     
                //random rectangle
                int x = rnd.Next(DrawArea.Width);
                int y = rnd.Next(DrawArea.Height);
                int h = rnd.Next(minH,maxH);
                int w = rnd.Next(minW, maxW);
                Rectangle r = new Rectangle(x, y, w, h);

                //loop through list of buildings making sure the rectangle doesn't intersect with any other building
                //and that there will be a gap of (Sentients.DrawWidth * 2) between them
                //This is written so that there aren't necessarily max number of buildings on purpose,
                //only max number of chances of buildings
                LinkedListNode<Building> bn = Buildings.First;
                bool good = true;
                while (bn != null && good)
                {
                    Rectangle temp = bn.Value.getSurface();
                    temp.Inflate(minSpacing, minSpacing);                    
                    good = !r.IntersectsWith(temp);
                    bn = bn.Next;
                }
                if (good)
                {
                    Buildings.AddLast(new Building(r, DrawArea));
                    Buildings.Last.Value.Draw(false);
                }
            }
        }

        //generate max number of sentients
        //can be either Zombies or People
        //the first Sentient is always a Zombie, to ensure that at least 1 Zombie is created
        //randomly place in the DrawArea, making sure not to intersect with Buildings
        //and other Sentients
        private void genSentients(int max)
        {             
            Rectangle r;

            for (int i = 0; i < max; i++)
            {
                r = getStartSpot();
                //Created a function to generate a valid start location.
                //do
                //{                    
                //    x = rnd.Next(DrawArea.Width);
                //    y = rnd.Next(DrawArea.Height);
                //    r = new Rectangle(x, y, Sentient.DrawWidth, Sentient.DrawWidth);
                //    good = (x + Sentient.DrawWidth < DrawArea.Width) && (y + Sentient.DrawWidth < DrawArea.Height); 
                //    LinkedListNode<Building> bn = Buildings.First;
                //    while (bn != null && good)
                //    {
                //        good = !r.IntersectsWith(bn.Value.getSurface());
                //        bn = bn.Next;
                //    }
                //    /* Decided to allow Sentient overlapping. Will make combat easier to detect.
                //     * Plus, it caused a metric shit-ton of problems with the wander code.
                //    LinkedListNode<Sentient> sn = Sentients.First;
                //    while (sn != null && good)
                //    {
                //        good = !r.IntersectsWith(sn.Value.getLocation());
                //        sn = sn.Next;
                //    }
                //     */
                //} while (!good);
                int h = rnd.Next(10, 20);
                if (rnd.NextDouble() < ZOMBIE_PERCENTAGE || i == 1)
                {
                    Sentients.AddLast(new Zombie(r, h, DrawArea));
                }
                else
                {
                    int s = rnd.Next(4,10);
                    int c = rnd.Next(1,10);
                    Sentients.AddLast(new Person(r,h,s,c,DrawArea));
                }
                Sentients.Last.Value.Draw(false);
            }
            /*
            if (Sentients.Count > MAXSENTIENTS)
            {
                MessageBox.Show(Sentients.Count.ToString());
                tmrGame.Enabled = false;
            }
             */
        }

        private void genPerson()
        {
            Rectangle r = getStartSpot();

            int h = rnd.Next(10, 20);
            int s = rnd.Next(4, 10);
            int c = rnd.Next(1, 10);
            Sentients.AddLast(new Person(r, h, s, c, DrawArea));

        }

        private void genZombie()
        {
            Rectangle r = getStartSpot();
            int h = rnd.Next(10, 20);
            Sentients.AddLast(new Zombie(r, h, DrawArea));
        }

        private Rectangle getStartSpot()
        {
            bool good;
            int x, y;
            Rectangle r;

            do
            {
                x = rnd.Next(DrawArea.Width);
                y = rnd.Next(DrawArea.Height);
                r = new Rectangle(x, y, Sentient.DrawWidth, Sentient.DrawWidth);
                good = (x + Sentient.DrawWidth < DrawArea.Width) && (y + Sentient.DrawWidth < DrawArea.Height);
                LinkedListNode<Building> bn = Buildings.First;
                while (bn != null && good)
                {
                    good = !r.IntersectsWith(bn.Value.getSurface());
                    bn = bn.Next;
                }
            } while (!good);

            return r;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        /*
        // paint event - stolen mostly
        private void frmSim_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawImageUnscaled(DrawArea, OFFSET, OFFSET + Menus.Height);
            //g.DrawImage(DrawArea, OFFSET, OFFSET + Menus.Height, DrawArea.Width, DrawArea.Height);
            g.Dispose();
        }
         * */

        protected override void OnPaint(PaintEventArgs e)
        {
            /*Graphics g = e.Graphics;
            g.DrawImageUnscaled(DrawArea, OFFSET, OFFSET + Menus.Height);
            //g.DrawImage(DrawArea, OFFSET, OFFSET + Menus.Height, DrawArea.Width, DrawArea.Height);
            g.Dispose();
             */
            this.BackgroundImage = DrawArea;
            
        }

        // free up resources on program exit - stolen
        private void frmSim_Closed(object sender, FormClosedEventArgs e)
        {
            //DrawArea.Dispose();
            Buildings.Clear();
            Sentients.Clear();
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tmrGame.Enabled = true;
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tmrGame.Enabled = false;
        }

        // 1. Update
        // 2. Erase
        // 3. Move
        // 4. Draw
        // 5. ???
        // 6. Profit!
        private void tmrGame_Tick(object sender, EventArgs e)
        {
            LinkedListNode<Sentient> sn = Sentients.First;
            while (sn != null)
            {
                sn.Value.Update();
                sn.Value.Draw(true);
                sn = sn.Next;
            }
            sn = Sentients.First;
            while (sn != null)
            {
                //sn.Value.Draw(true);
                sn.Value.Move();
                sn.Value.Draw(false);
                sn = sn.Next;
            }
            this.Invalidate();
            System.GC.Collect();
        }

        //reset the game
        private void reset()
        {
            tmrGame.Enabled = false;
            //clear the collections and image
            //DrawArea.Dispose();
            Buildings.Clear();
            Sentients.Clear();
            //force garbage collection now
            System.GC.Collect();

            //start anew
            InitializeDrawArea();
            genBuildings(rnd.Next(MINBUILDINGS, MAXBUILDINGS));
            genSentients(rnd.Next(MINSENTIENTS, MAXSENTIENTS));
            this.Invalidate();
        }

        private void resetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            reset();
        }

        private void controlsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormControls.Show();
        }

        private void frmSim_KeyDown(object sender, KeyEventArgs e)
        {
            
            //+ = increase speed
            if (e.KeyCode == Keys.Add)
            {
                if (tmrGame.Interval > MININTERVAL)
                    tmrGame.Interval -= 50;
            }
            //- = decrease speed
            else if (e.KeyCode == Keys.Subtract)
            {
                if (tmrGame.Interval < MAXINTERVAL)
                    tmrGame.Interval += 50;
            }
            //r = reset game
            else if (e.KeyCode == Keys.R)
            {
                reset();
            }
            //space bar = run/pause game  
            else if (e.KeyCode == Keys.Space)
            {
                tmrGame.Enabled = !tmrGame.Enabled;
            }
            //Esc = quit
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
            //z = Add Zombie
            else if (e.KeyCode == Keys.Z)
            {
                genZombie();
            }
            //x = Add Person
            else if (e.KeyCode == Keys.X)
            {
                genPerson();
            }
            //q = Kill all Zombies
            else if (e.KeyCode == Keys.Q)
            {
                LinkedListNode<Sentient> sn = Sentients.First;
                LinkedListNode<Sentient> snTemp;
                while (sn != null)
                {
                    if (sn.Value is Zombie)
                    {
                        snTemp = sn.Next;
                        sn.Value.Draw(true);
                        Sentients.Remove(sn);
                        sn = snTemp;
                    }
                    else
                        sn = sn.Next;
                }
                System.GC.Collect();
            }
            //w = Kill all Zombies
            else if (e.KeyCode == Keys.W)
            {
                LinkedListNode<Sentient> sn = Sentients.First;
                LinkedListNode<Sentient> snTemp;
                while (sn != null)
                {
                    if (sn.Value is Person)
                    {
                        snTemp = sn.Next;
                        sn.Value.Draw(true);
                        Sentients.Remove(sn);
                        sn = snTemp;
                    }
                    else
                        sn = sn.Next;
                }
                System.GC.Collect();
            }
            //c = Show controls window
            else if (e.KeyCode == Keys.C)
            {
                FormControls.Show();
            }
        }

        //outdated
        private void frmSim_KeyPress(object sender, KeyPressEventArgs e)
        {
            //broken after adding mouse input
            ////+ = increase speed
            //if (e.KeyChar.ToString().Equals("+"))
            //{
            //    if (tmrGame.Interval > MININTERVAL)
            //        tmrGame.Interval -= 50;
            //}
            ////- = decrease speed
            //else if (e.KeyChar.ToString().Equals("-"))
            //{
            //    if (tmrGame.Interval < MAXINTERVAL)
            //        tmrGame.Interval += 50;
            //}
            ////r = reset game
            //else if (e.KeyChar.ToString().Equals("r"))
            //{
            //    reset();
            //}
            ////space bar = run/pause game  
            //else if (e.KeyChar.ToString().Equals(" "))
            //{
            //    tmrGame.Enabled = !tmrGame.Enabled;
            //}
            ////Esc = quit
            //else if ((int)e.KeyChar == (int)Keys.Escape)
            //{
            //    this.Close();
            //}   
            ////z = Add Zombie
            //else if (e.KeyChar.ToString().Equals("z"))
            //{
            //    genZombie();
            //}
            ////x = Add Person
            //else if (e.KeyChar.ToString().Equals("x"))
            //{
            //    genPerson();
            //}
            ////q = Kill all Zombies
            //else if (e.KeyChar.ToString().Equals("q"))
            //{
            //    LinkedListNode<Sentient> sn = Sentients.First;
            //    LinkedListNode<Sentient> snTemp;
            //    while (sn != null)
            //    {
            //        if (sn.Value is Zombie)
            //        {
            //            snTemp = sn.Next;
            //            sn.Value.Draw(true);
            //            Sentients.Remove(sn);
            //            sn = snTemp;
            //        }
            //        else
            //            sn = sn.Next;
            //    }
            //    System.GC.Collect();
            //}
            ////w = Kill all Zombies
            //else if (e.KeyChar.ToString().Equals("w"))
            //{
            //    LinkedListNode<Sentient> sn = Sentients.First;
            //    LinkedListNode<Sentient> snTemp;
            //    while (sn != null)
            //    {
            //        if (sn.Value is Person)
            //        {
            //            snTemp = sn.Next;
            //            sn.Value.Draw(true);
            //            Sentients.Remove(sn);
            //            sn = snTemp;
            //        }
            //        else
            //            sn = sn.Next;
            //    }
            //    System.GC.Collect();
            //}
            //else if (e.KeyChar.ToString().Equals("c"))
            //{
            //    FormControls.Show();
            //}


        }

        private void frmSim_MouseDown(object sender, MouseEventArgs e)
        {
            //MessageBox.Show(e.X.ToString() + ", " + e.Y.ToString());
            Rectangle r = new Rectangle(e.X, e.Y, Sentient.DrawWidth, Sentient.DrawWidth);
            bool good = (e.X + Sentient.DrawWidth < DrawArea.Width) && (e.Y + Sentient.DrawWidth < DrawArea.Height);
            LinkedListNode<Building> bn = Buildings.First;
            while (bn != null && good)
            {
                good = !r.IntersectsWith(bn.Value.getSurface());
                bn = bn.Next;
            }
            if (good)
            {
                int h = rnd.Next(10, 20);
                if (e.Button == MouseButtons.Left)
                {
                    Sentients.AddLast(new Zombie(r, h, DrawArea));
                }
                else if (e.Button == MouseButtons.Right)
                {
                    int s = rnd.Next(1, 10);
                    int c = rnd.Next(1, 10);
                    Sentients.AddLast(new Person(r, h, s, c, DrawArea));
                }
                Sentients.Last.Value.Draw(false);
            }
        }
    }
}