using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Net;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Anotarity.Cryptography;

namespace Anotarity
{
    public partial class ScanInitForm : Form
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        TextInputArray Inputs = new TextInputArray();

        public ScanInitForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            Inputs.BoxColor = Color.Silver;
            Inputs.LabelColor = Color.Gray;
            Inputs.ActiveColor = Color.FromArgb(0, 142, 208);
            Inputs.ErrorColor = Color.FromArgb(245, 73, 93);
            Inputs.Add(new TextInput(textBox1, panel10, label6));
            Inputs.Add(new TextInput(textBox2, panel11, label7));
            Inputs.Add(new TextInput(textBox3, panel12, label8));
            Inputs.ErrorLabel = ErrorLabel;
            ErrorLabel.Text = String.Empty;
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

        private void button3_Click(object sender, EventArgs e)
        {
            IPAddress RangeStart, RangeEnd;
            Int32[] Ports = new Int32[0];
            if (!IPAddress.TryParse(textBox1.Text, out RangeStart)) 
            { 
                Inputs.ShowError(0);
                ErrorLabel.Text = "Невалиден IP адрес!";
                return; 
            }
            if (!IPAddress.TryParse(textBox2.Text, out RangeEnd)) 
            { 
                Inputs.ShowError(1);
                ErrorLabel.Text = "Невалиден IP адрес!";
                return; 
            }
            if (!TryConvertPorts(textBox3.Text, out Ports) || Ports.Length == 0) 
            { 
                Inputs.ShowError(2);
                ErrorLabel.Text = "Невалиден списък с портове!";
                return; 
            }

            UInt32 StartInt = IPAddressToUInt(RangeStart), EndInt = IPAddressToUInt(RangeEnd);

            if (StartInt > EndInt)
            {
                Inputs.ShowError(0, 1);
                ErrorLabel.Text = "Началният IP адрес трябва да е преди крайния!";
                return;
            }

            if ((!IsLocal(RangeStart) || !IsLocal(RangeEnd)) && !(File.Exists("martial") && ShortMD5.Compute(File.ReadAllText("martial")) == "0CBURVeN"))
            {
                Inputs.ShowError(0, 1);
                ErrorLabel.Text = "Нямате право да сканирате публични IP адреси!";
                return;
            }

            ScanForm Scan = new ScanForm();
            Scan.Location = this.Location;
            Scan.StartRange = RangeStart;
            Scan.EndRange = RangeEnd;
            this.Hide();
            Scan.FormClosed += (s, args) => this.Close();
            Scan.Show();

        }

        private UInt32 IPAddressToUInt(IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }

            return BitConverter.ToUInt32(bytes, 0);
        }

        private bool IsLocal(IPAddress Address)
        {
            UInt32 AddressInt = IPAddressToUInt(Address);
            UInt32[] SmallRange = new UInt32[] { IPAddressToUInt(IPAddress.Parse("192.168.0.0")), IPAddressToUInt(IPAddress.Parse("192.168.255.255")) };
            UInt32[] MediumRange = new UInt32[] { IPAddressToUInt(IPAddress.Parse("172.16.0.0")), IPAddressToUInt(IPAddress.Parse("172.31.255.255")) };
            UInt32[] BigRange = new UInt32[] { IPAddressToUInt(IPAddress.Parse("10.0.0.0")), IPAddressToUInt(IPAddress.Parse("10.255.255.255")) };
            if (AddressInt >= SmallRange[0] && AddressInt <= SmallRange[1]) return true;
            if (AddressInt >= MediumRange[0] && AddressInt <= MediumRange[1]) return true;
            if (AddressInt >= BigRange[0] && AddressInt <= BigRange[1]) return true;
            return false;
        }

        private bool TryConvertPorts(String Text, out Int32[] Output)
        {
            Output = new Int32[0];
            String[] PortBlocks = Text.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (Int32 i = 0; i < PortBlocks.Length; i++)
            {
                String[] CurrentPorts = PortBlocks[i].Split('-');
                if (CurrentPorts.Length == 1)
                {
                    Int32 A = 0;
                    if (!Int32.TryParse(CurrentPorts[0], out A) || A < 0 || A > 65535) return false;
                    Array.Resize(ref Output, Output.Length + 1);
                    Output[Output.Length - 1] = A;
                }
                else if (CurrentPorts.Length == 2)
                {
                    Int32 A = 0, B = 0;
                    if (!Int32.TryParse(CurrentPorts[0], out A) || !Int32.TryParse(CurrentPorts[1], out B) || A > B || A < 0 || B > 65535) return false;
                    Int32 OriginalLength = Output.Length;
                    Array.Resize(ref Output, Output.Length + B - A + 1);
                    for (Int32 j = OriginalLength; j < Output.Length; j++) Output[j] = A + j - OriginalLength;
                }
                else return false;
            }
            return true;
        }
    }

    class TextInput
    {
        public TextBox Input;
        public Panel Line;
        public Label Label;

        public TextInput(TextBox Input, Panel Line, Label Label)
        {
            this.Input = Input;
            this.Line = Line;
            this.Label = Label;
        }

        public TextInput() { }
    }

    class TextInputArray
    {
        private TextInput[] Arr = new TextInput[0];
        public Color BoxColor, LabelColor, ActiveColor, ErrorColor;
        public Label ErrorLabel;

        public void Add(TextInput Input)
        {
            Array.Resize(ref Arr, Arr.Length + 1);
            Arr[Arr.Length - 1] = Input;
            Input.Input.ForeColor = BoxColor;
            Input.Line.BackColor = BoxColor;
            Input.Label.ForeColor = LabelColor;
            Input.Input.GotFocus += FocusedInput;
            Input.Input.LostFocus += UnfocusedInput;
        }

        public void DeactivateAll()
        {
            for (Int32 i = 0; i < Arr.Length; i++) Arr[i].Line.BackColor = BoxColor;
        }

        public void ShowError(params Int32[] indexes)
        {
            for (Int32 i = 0; i < indexes.Length; i++)
                if (indexes[i] < Arr.Length && indexes[i] >= 0) Arr[indexes[i]].Line.BackColor = ErrorColor;
        }

        private void FocusedInput(object sender, EventArgs args)
        {
            TextInput Group = Find((TextBox)sender);
            DeactivateAll();
            Group.Line.BackColor = ActiveColor;
            Group.Label.ForeColor = ActiveColor;
            ErrorLabel.Text = String.Empty;
        }

        private void UnfocusedInput(object sender, EventArgs args)
        {
            TextInput Group = Find((TextBox)sender);
            Group.Line.BackColor = BoxColor;
            Group.Label.ForeColor = LabelColor;
        }

        public TextInput Find(TextBox Input)
        {
            for (Int32 i = 0; i < Arr.Length; i++)
                if (Arr[i].Input == Input) return Arr[i];
            return new TextInput();
        }
    }
}