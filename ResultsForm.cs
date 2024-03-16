using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Anotarity.Models;
using Anotarity.Exploits;
using Anotarity.Scanners;

namespace Anotarity
{
    public partial class ResultsForm : Form
    {
        public Device[] ExploitedDevices = new Device[0];
        private Int32 PageIndex = 0;

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public ResultsForm()
        {
            InitializeComponent();
            this.MouseDown += DragForm;
            this.StartPosition = FormStartPosition.Manual;
            PreviousButton.Visible = false;
            NextButton.Visible = false;
            panel1.MouseDown += DragForm;
            panel2.MouseDown += DragForm;
            panel3.MouseDown += DragForm;
            panel4.MouseDown += DragForm;
            panel5.MouseDown += DragForm;
            panel6.MouseDown += DragForm;
            panel7.MouseDown += DragForm;
            label1.MouseDown += DragForm;
            label2.MouseDown += DragForm;
            label3.MouseDown += DragForm;
            label5.MouseDown += DragForm;
            pictureBox1.MouseDown += DragForm;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void DragForm(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        public void InitLoad()
        {
            if (ExploitedDevices == null || ExploitedDevices.Length == 0)
            {
                ModelLabel.Text = String.Empty;
                AddressLabel.Text = String.Empty;
                ResultIcon.Image = Anotarity.Properties.Resources.NoFoundIcon;
                ResultLabel.ForeColor = Color.FromArgb(0, 142, 208);
                ResultLabel.Text = "НЕ НАМЕРИХМЕ НИЩО";
                ExploitLabel.Text = "Не бяха намерени никакви устройства";
                DescriptionLabel.Text = String.Empty;
                AdviceLabel.Text = String.Empty;
                DescriptionTitle.Visible = false;
                CountLabel.Text = String.Empty; ;
                AdviceTitle.Visible = false;
            }
            else LoadPage(0);
        }

        public void LoadPage(Int32 Index)
        {
            Device Target = ExploitedDevices[Index];
            ResultLabel.ForeColor = Color.FromArgb(169, 250, 102);
            ModelLabel.Text = (Target.Model == Model.NVMS1000 || Target.Model == Model.NVMS9000) ? "TVT " + Target.Model.ToString() : Target.Model.ToString();
            AddressLabel.Text = Target.EndPoint.ToString();
            if ((Target.Exploited && Target.Exploits.Length > 0))
            {
                ResultIcon.Image = Anotarity.Properties.Resources.ThreatIcon;
                ResultLabel.ForeColor = Color.FromArgb(245, 73, 93);
                ResultLabel.Text = "НАМЕРЕНА ЗАПЛАХА";
                DescriptionTitle.Visible = true;
                AdviceTitle.Visible = true;
                if (Target.Exploits.Length > 0 && Target.Exploits[0] == ExploitType.DahuaBackdoor)
                {
                    ExploitLabel.Text = "2017 Bashis Backdoor";
                    DescriptionLabel.Text = @"Във вашето устройство има слабост от тип Backdoor. Злонамерен нападател може да заобиколи автентикацията и да влезе в системата без нужда от парола. ";
                    AdviceLabel.Text = "   1. Свържете се с фирмата или търговеца (vendor), от когото сте закупили устройството си и помолете за актуализация на firmware-а\r\n" +
                                       "   2. Сменете паролите на всички акаунти.";
                }
                else if (Target.Exploits.Length > 0 && Target.Exploits[0] == ExploitType.HiSiliconPathTraversal)
                {
                    ExploitLabel.Text = "CVE-2020-24219";
                    DescriptionLabel.Text = @"Във вашето устройство има слабост от тип Directory Traversal. Злонамерен нападател може да заобиколи автентикацията и да влезе в системата без нужда от парола. ";
                    AdviceLabel.Text = "   1. Свържете се с фирмата или търговеца (vendor), от когото сте закупили устройството си и помолете за актуализация на firmware-а\r\n" +
                                       "   2. Сменете паролите на всички акаунти.";
                }
                else if (Target.Exploits.Length > 0 && Target.Exploits[0] == ExploitType.NVMS9000Backdoor)
                {
                    ExploitLabel.Text = "2018 Bashis Backdoor";
                    DescriptionLabel.Text = @"Във вашето устройство има слабост от тип Backdoor. Злонамерен нападател може да заобиколи автентикацията и да влезе в системата без нужда от парола. ";
                    AdviceLabel.Text = "   1. Свържете се с фирмата или търговеца (vendor), от когото сте закупили устройството си и помолете за актуализация на firmware-а\r\n" +
                                       "   2. Сменете паролите на всички акаунти.";
                }
                else if (Target.Exploits.Length > 0 && Target.Exploits[0] == ExploitType.NVMS1000PathTraversal)
                {
                    ExploitLabel.Text = "CVE-2019-20085";
                    DescriptionLabel.Text = @"Във вашето устройство има слабост от тип Directory Traversal. Злонамерен нападател може директно да получи паролата на всеки акаунт.";
                    AdviceLabel.Text = "   1. Свържете се с фирмата или търговеца (vendor), от когото сте закупили устройството си и помолете за актуализация на firmware-а\r\n" +
                                       "   2. Сменете паролите на всички акаунти.";
                }
            }

            else
            {
                ResultIcon.Image = Anotarity.Properties.Resources.SafeIcon;
                ResultLabel.Text = "ЗАЩИТЕНО УСТРОЙСТВО";
                ExploitLabel.Text = "Не е намерена заплаха";
                DescriptionLabel.Text = String.Empty;
                AdviceLabel.Text = String.Empty;
                DescriptionTitle.Visible = false;
                CountLabel.Text = String.Empty; ;
                AdviceTitle.Visible = false;
            }

            if (Index == 0) PreviousButton.Visible = false;
            else PreviousButton.Visible = true;

            if (Index == ExploitedDevices.Length - 1) NextButton.Visible = false;
            else NextButton.Visible = true;

            CountLabel.Text = (Index + 1).ToString() + "/" + ExploitedDevices.Length;
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            LoadPage(++PageIndex);
        }

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            LoadPage(--PageIndex);
        }
    }
}
