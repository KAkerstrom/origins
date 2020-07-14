using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Origins
{
    public partial class InspectForm : Form
    {
        private Dorf dorf;
        private Timer updateTmr = new Timer();

        public InspectForm(Dorf dorf)
        {
            InitializeComponent();
            this.dorf = dorf;
            Text = dorf.Name;
            UpdateStats();
            updateTmr.Interval = 100;
            updateTmr.Start();
            updateTmr.Tick += UpdateTmr_Tick;
        }

        private void UpdateTmr_Tick(object sender, EventArgs e)
        {
            UpdateStats();
        }

        private void UpdateStats()
        {
            healthTxt.Text = dorf.Health.ToString("0.##");
            staminaTxt.Text = dorf.Stamina.ToString("0.##");
            hungerTxt.Text = dorf.Hunger.ToString("0.##");
            thirstTxt.Text = dorf.Thirst.ToString("0.##");
            stateTxt.Text = dorf.CurrentState.ToString();
            ageTxt.Text = dorf.Age.ToString("0.##");
            intelligenceTxt.Text = dorf.Intelligence.ToString("0.##");
        }
    }
}
