using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Anotarity
{
    public partial class TemplateForm : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public TemplateForm()
        {
            InitializeComponent();
            this.MouseDown += DragForm;
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
    }
}
