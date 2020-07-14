using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace Origins
{
    struct Status
    {
        public Status(int interval, string text, Color color)
        {
            Interval = interval;
            Text = text;
            TextColor = color;
        }

        public int Interval;
        public string Text;
        public Color TextColor;
    }

    public partial class MainForm : Form
    {
        private Timer drawTmr = new Timer();
        private List<Status> statuses = new List<Status>();
        public static SoundPlayer bgm = new SoundPlayer(Properties.Resources.theme);

        public MainForm()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.UserPaint, true);
            MinimumSize = new Size(Width, Height);
            MaximumSize = new Size(Width, Height);
            BackgroundImage = new Bitmap(Properties.Resources.water, new Size(Game.CellSize, Game.CellSize));
            Game.SetDisplaySize(ClientRectangle.Width, ClientRectangle.Height);
            bgm.PlayLooping();

            drawTmr.Interval = 10;
            drawTmr.Start();
            drawTmr.Tick += DrawTmr_Tick;
            MouseWheel += MainForm_MouseWheel;
            Game.StatusUpdate += Game_StatusUpdate;
        }

        private void Game_StatusUpdate(string status, Color color)
        {
            statuses.Add(new Status(500, status, color));
        }

        private void MainForm_MouseWheel(object sender, MouseEventArgs e)
        {
            Game.Zoom(Math.Sign(e.Delta) > 0);
        }

        private void DrawTmr_Tick(object sender, EventArgs e)
        {
            //Just do the status stuff here for now
            for (int i = statuses.Count - 1; i >= 0; i--)
            {
                statuses[i] = new Status(statuses[i].Interval - 1, statuses[i].Text, statuses[i].TextColor);
                if (statuses[i].Interval <= 0)
                    statuses.RemoveAt(i);
            }

            Invalidate();
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            Game.DrawScreen(e.Graphics);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            Game.SetDisplaySize(ClientRectangle.Width, ClientRectangle.Height);
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            Game.ReleaseKey((char)e.KeyValue);

            if ((char)e.KeyValue == 'Q')
               Game.SaveGame();

            if ((char)e.KeyValue == 'W')
                Game.LoadGame();
        }

        private void statusPb_Paint(object sender, PaintEventArgs e)
        {
            if (Game.Paused)
                e.Graphics.DrawString("PAUSED", new Font("Arial", 12), new SolidBrush(Color.Black), 5, 5);
            else
                for (int i = 0; i < statuses.Count; i++)
                {
                    SolidBrush brush = new SolidBrush(Color.FromArgb((statuses[i].Interval > 255 ? 255 : statuses[i].Interval), statuses[i].TextColor));
                    e.Graphics.DrawString(statuses[i].Text, new Font("Arial", 12), brush, 16, 16 + (16 * i));
                }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.P && !titlePnl.Visible)
                Game.TogglePaused();

            if (e.KeyCode == Keys.F11 && !titlePnl.Visible)
            {
                if(FormBorderStyle == FormBorderStyle.Sizable)
                {
                    FormBorderStyle = FormBorderStyle.None;
                    WindowState = FormWindowState.Maximized;
                }
                else
                {
                    FormBorderStyle = FormBorderStyle.Sizable;
                    WindowState = FormWindowState.Normal;
                }
            }

            if (e.KeyCode == Keys.Escape && MessageBox.Show("Are you sure you want to exit?", "Exit Game?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Close();
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            startBtn.Text = "Continue";

            bgm.Stop();
            Game.Paused = false;
            titlePnl.Visible = false;
            MaximumSize = new Size(0, 0);
            MinimumSize = new Size(0, 0);
            Focus();
        }

        private void exitBtn_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tutorialBtn_Click(object sender, EventArgs e)
        {
            Tutorial tut = new Tutorial();
            tut.ShowDialog();
        }
    }
}
