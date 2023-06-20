using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fun1
{
    public partial class Form1 : Form
    {
        Random rand = new Random();
        Thread th;
        Color currentColor;
        bool stopThread = false;
        int leftTime = 255;

        public Form1()
        {
            InitializeComponent();      
            th = new Thread(SetCursorPositionColor);
            th.Start();

            panel1.BackColor = Color.FromArgb(200, panel1.BackColor);
            label3.BackColor = Color.FromArgb(0, label3.BackColor);
        }

        private void SetCursorPositionColor()
        {
            while (!stopThread)
            { 
                Color color = ScreenColorPicker.GetCursorPositionColor();
                BackColor = color;
                currentColor = color;
                label1.ForeColor = color;
                label2.ForeColor = color;
                label3.ForeColor = color;
                label1.Text = $"R:{color.R}\t G:{color.G}\t B:{color.B}";
                //Thread.Sleep(100);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            stopThread = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            leftTime-= 4;

            if (leftTime < 0) 
            {
                label3.BackColor = Color.FromArgb(0, label3.BackColor);
                leftTime = 255; 
                timer1.Stop();
            }
            else label3.BackColor = Color.FromArgb(leftTime, label3.BackColor);            
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Control && e.KeyCode == Keys.C) 
            {
                Clipboard.SetText($"{currentColor.R};{currentColor.G};{currentColor.B}");

                leftTime = 255;
                timer1.Start();
            }
        }
    }

    public class ScreenColorPicker
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("gdi32.dll")]
        public static extern uint GetPixel(IntPtr hdc, int x, int y);

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        public static Color GetCursorPositionColor()
        {
            Point cursorPosition = Cursor.Position;
            IntPtr desktopWindowHandle = IntPtr.Zero;
            IntPtr desktopWindowDC = IntPtr.Zero;

            try
            {
                desktopWindowHandle = GetDesktopWindow();
                desktopWindowDC = GetDC(desktopWindowHandle);

                uint pixelColor = GetPixel(desktopWindowDC, cursorPosition.X, cursorPosition.Y);
                Color color = Color.FromArgb((int)(pixelColor & 0x000000FF), (int)(pixelColor & 0x0000FF00) >> 8, (int)(pixelColor & 0x00FF0000) >> 16);

                return color;
            }
            finally
            {
                if (desktopWindowDC != IntPtr.Zero)
                    ReleaseDC(desktopWindowHandle, desktopWindowDC);
            }
        }
    }
}
