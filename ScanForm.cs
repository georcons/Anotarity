using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Windows.Forms;

using Anotarity.Scanners;

namespace Anotarity
{
    public partial class ScanForm : Form
    {
        public IPAddress StartRange, EndRange;


        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool ReleaseCapture();

        LoadIndicator Indicator;

        public ScanForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.MouseDown += DragForm;
            panel1.MouseDown += DragForm;
            panel2.MouseDown += DragForm;
            panel3.MouseDown += DragForm;
            panel4.MouseDown += DragForm;
            panel5.MouseDown += DragForm;
            panel6.MouseDown += DragForm;
            panel7.MouseDown += DragForm;
            panel8.MouseDown += DragForm;
            panel9.MouseDown += DragForm;
            label1.MouseDown += DragForm;
            label2.MouseDown += DragForm;
            label3.MouseDown += DragForm;
            label4.MouseDown += DragForm;
            label5.MouseDown += DragForm;
            LoadLabel.MouseDown += DragForm;
            LoadedLine.MouseDown += DragForm;
            Line1.MouseDown += DragForm;
            Line2.MouseDown += DragForm;
            Line3.MouseDown += DragForm;
            pictureBox1.MouseDown += DragForm;
            Indicator = new LoadIndicator(LoadPanel, LoadedLine);
            Indicator.SetLines(Line1, Line2, Line3);
            Indicator.SetLoadLabel(LoadLabel);
            Indicator.Start();
            LoadedLine.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void ScanForm_Load(object sender, EventArgs e)
        {
            Scanner sc = new Scanner(StartRange, EndRange);
            Thread ProgressThread = new Thread(() => 
            {
                while (!sc.HasFinished())
                {
                    Indicator.Percent = sc.GetProgress();
                    Thread.Sleep(100);
                }
                Indicator.Percent = 100;
                ShowResults(sc.GetOutput());
            });

            ProgressThread.Start();
            sc.Begin();
        }

        private void DragForm(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void ShowResults(Device[] result)
        {
            this.Invoke(new Action(() => 
            { 
                ResultsForm Result = new ResultsForm();
                Result.Location = this.Location;
                Result.ExploitedDevices = result;
                this.Hide();
                Result.FormClosed += (s, args) => this.Close();
                Result.Show();
                Result.Invoke(new Action(() => { Result.InitLoad(); }));
            }));
        }
    }

    public class LoadIndicator
    {
        public double Percent
        {
            set
            {
                if (value < 0) LoadedCoef = 0;
                else if (value > 100) LoadedCoef = 1;
                else LoadedCoef = value / 100;

            }
        }

        Panel[] Lines;
        Panel Container, LoadLine;
        Label LoadLabel;
        System.Windows.Forms.Timer InnerTimer;
        int[] PossibleSpeeds = new int[] { 10, 20, 11, 19, 22 };
        int[] Speeds, Delays;
        int Count = 0;
        double LoadedCoef = 0;
        Random r = new Random();

        public LoadIndicator(Panel Container, Panel LoadLine)
        {
            this.Container = Container;
            this.LoadLine = LoadLine;
            InnerTimer = new System.Windows.Forms.Timer();
            InnerTimer.Interval = 10;
            InnerTimer.Tick += TimerTick;
        }

        public void SetLines(params Panel[] Lines)
        {
            this.Lines = Lines;
        }

        public void SetLoadLabel(Label LoadLabel)
        {
            this.LoadLabel = LoadLabel;
        }

        public void Start()
        {
            LoadLine.Dock = DockStyle.Left;
            LoadLine.Width = 0;
            Speeds = new int[Lines.Length];
            Delays = new int[Lines.Length];
            for (int i = 0; i < Lines.Length; i++)
            {
                Lines[i].Location = new Point(Container.Width + Lines[i].Width, Lines[i].Location.Y);
                Speeds[i] = PossibleSpeeds[r.Next(0, PossibleSpeeds.Length - 1)];
                Delays[i] = r.Next(0, 14);
            }
            InnerTimer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Count >= Delays[i]) Lines[i].Location = new Point(Lines[i].Location.X - Speeds[i], Lines[i].Location.Y);
                if (Lines[i].Location.X <= -Lines[i].Width)
                {
                    
                    Speeds[i] = PossibleSpeeds[r.Next(0, PossibleSpeeds.Length - 1)];
                    Lines[i].Location = new Point(Container.Width + Lines[i].Width, Lines[i].Location.Y);
                }
            }

            LoadLine.Width = (int)(Container.Width * LoadedCoef);
            LoadLabel.Text = ((int)(100 * LoadedCoef)).ToString() + "%";


            Count++;
        }
    }
}
