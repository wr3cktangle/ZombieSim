using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Zombie_Sim
{
    public partial class frmControls : Form
    {
        public frmControls()
        {
            InitializeComponent();
            lblHelp.Text = "Command              Key \n" +
               "------------------------------------ \n" +
               "This Screen           c \n" +
               "Speed Up              +  \n" +
               "Speed Down          -  \n" +
               "Reset                     r  \n" +
               "Start/Pause       Space\n" +
               "Quit                     Esc \n\n\n" +
               "Resize the window and Reset the game \n" +
               "to change the play area size";
        }

        private void frmControls_Load(object sender, EventArgs e)
        {

        }

        private void btnHide_Click(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}