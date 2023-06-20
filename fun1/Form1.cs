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
        CopyFormat copyFormat = CopyFormat.RGB;
        Random rand = new Random();
        Thread th;
        Color currentColor;
        bool stopThread = false;
        bool KILLThread = false;
        int leftTime = 255;

        public Form1()
        {
            InitializeComponent();      
            th = new Thread(SetCursorPositionColor);
            th.Start();

            panel1.BackColor = Color.FromArgb(200, panel1.BackColor);
            label3.BackColor = Color.FromArgb(0, label3.BackColor);
            label5.Text = $"Копировать в формате: {copyFormat.ToString()}";
        }

        private bool IsCursorOnForm(Form form)
        {
            Point cursorPosition = Cursor.Position;  // Получаем текущую позицию курсора
            Rectangle formBounds = form.Bounds;      // Получаем границы формы

            // Проверяем, содержит ли прямоугольник формы координаты курсора
            if (formBounds.Contains(cursorPosition))
            {
                return true;  // Курсор находится на форме
            }
            else
            {
                return false; // Курсор не находится на форме
            }
        }

        private void SetCursorPositionColor()
        {
            while(!KILLThread)
            {
                while (!stopThread)
                {
                    Color color = ScreenColorPicker.GetCursorPositionColor();

                    if (!IsCursorOnForm(this))
                    {
                        BackColor = color;
                        currentColor = color;
                        label1.ForeColor = color;
                        label2.ForeColor = color;
                        label3.ForeColor = color;
                        label4.ForeColor = color;
                        label5.ForeColor = color;
                        label1.Text = $"R:{color.R}\t G:{color.G}\t B:{color.B}";

                        if (currentColor.R >= 128 && currentColor.G >= 128 && currentColor.B >= 128)
                        {
                            panel1.BackColor = Color.FromArgb(200, Color.Black);
                            label4.BackColor = panel1.BackColor;
                        }
                        else
                        {
                            panel1.BackColor = Color.FromArgb(200, Color.White);
                            label4.BackColor = panel1.BackColor;
                        }
                    }
                }
            }            
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            th.Abort();
            KILLThread = true;
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
            if (e.KeyCode == Keys.S)
            {
                stopThread = stopThread ? false : true;
                label4.Visible = label4.Visible ? false : true;
            }

            if(e.Control && e.KeyCode == Keys.C) 
            {
                if (currentColor.R >= 128 && currentColor.G >= 128 && currentColor.B >= 128) label3.BackColor = Color.Black;
                else label3.BackColor = Color.White;

                if(copyFormat == CopyFormat.Image)
                {
                    Bitmap bitmap = CreateSquareBitmap(1024, currentColor);
                    Clipboard.SetImage(bitmap);
                }
                else if(copyFormat == CopyFormat.RGB) Clipboard.SetText($"{currentColor.R};{currentColor.G};{currentColor.B}");
                else if(copyFormat == CopyFormat.HSL)
                {
                    double h, s, l;
                    RGBtoHSL(currentColor.R, currentColor.G, currentColor.B, out h, out s, out l);
                    Clipboard.SetText($"{Math.Round(h, 0)}°;{Math.Round(s, 0)}%;{Math.Round(l, 0)}%");
                }

                leftTime = 255;
                timer1.Start();
            }
            else if(e.KeyCode == Keys.C)
            {
                var values = (CopyFormat[])Enum.GetValues(typeof(CopyFormat));
                int currentIndex = 0;
                 
                for (int i = 0; i < values.Length; i++) if (copyFormat == values[i]) currentIndex = i;

                if (currentIndex + 1 == values.Length) copyFormat = values[0];
                else copyFormat = values[currentIndex+1];

                label5.Text = $"Копировать в формате: {copyFormat.ToString()}";
            }
        }

        public static void RGBtoHSL(int r, int g, int b, out double h, out double s, out double l)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));

            double hue, saturation, lightness;
            double diff = 0;

            // Определение оттенка
            if (max == min)
            {
                hue = 0; // Оттенок не имеет значения, если цвет - оттенок серого
            }
            else
            {
                diff = max - min;

                if (max == rd)
                {
                    hue = ((gd - bd) / diff) % 6;
                }
                else if (max == gd)
                {
                    hue = ((bd - rd) / diff) + 2;
                }
                else // max == bd
                {
                    hue = ((rd - gd) / diff) + 4;
                }

                hue *= 60; // Перевод оттенка в градусы
            }

            // Определение светлоты
            lightness = (max + min) / 2;

            // Определение насыщенности
            if (lightness <= 0.5)
            {
                saturation = diff / (max + min);
            }
            else
            {
                saturation = diff / (2 - max - min);
            }

            h = hue;
            s = saturation;
            l = lightness;
        }

        private static Bitmap CreateSquareBitmap(int size, Color color)
        {
            Bitmap bitmap = new Bitmap(size, size);

            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                using (Brush brush = new SolidBrush(color))
                {
                    graphics.FillRectangle(brush, 0, 0, size, size);
                }
            }

            return bitmap;
        }
    }

    public enum CopyFormat
    {
        Image,
        RGB,
        HSL
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
