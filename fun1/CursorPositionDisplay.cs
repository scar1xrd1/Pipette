using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fun1
{
    public partial class CursorPositionDisplay : Form
    {
        private Bitmap screenshot;
        private Rectangle zoomArea;
        private float zoomLevel = 2.0f;

        // Импортируем функции Windows API
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern int BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        public CursorPositionDisplay()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Захватываем скриншот всего экрана
            screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Получаем контекст устройства для формы
            IntPtr hdcForm = e.Graphics.GetHdc();

            // Получаем контекст устройства для экрана
            IntPtr hdcScreen = GetDC(IntPtr.Zero);

            // Копируем изображение экрана на изображение формы
            BitBlt(hdcForm, 0, 0, Width, Height, hdcScreen, 0, 0, 13369376);

            // Освобождаем контекст устройства
            ReleaseDC(IntPtr.Zero, hdcScreen);

            // Рисуем приближенную область на форме
            if (zoomArea != null && !zoomArea.IsEmpty)
            {
                using (Bitmap zoomedInImage = new Bitmap(zoomArea.Width, zoomArea.Height))
                {
                    using (Graphics g = Graphics.FromImage(zoomedInImage))
                    {
                        g.DrawImage(screenshot, new Rectangle(Point.Empty, zoomArea.Size), zoomArea, GraphicsUnit.Pixel);
                    }

                    e.Graphics.DrawImage(zoomedInImage, Point.Empty);
                }
            }

            // Освобождаем контекст устройства формы
            e.Graphics.ReleaseHdc(hdcForm);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // Получаем координаты курсора на экране
            float x = e.X / zoomLevel;
            float y = e.Y / zoomLevel;

            // Определяем область для приближенного изображения
            int zoomAreaWidth = (int)(Width / zoomLevel);
            int zoomAreaHeight = (int)(Height / zoomLevel);
            int zoomAreaX = (int)(x - zoomAreaWidth / 2);
            int zoomAreaY = (int)(y - zoomAreaHeight / 2);
            zoomArea = new Rectangle(zoomAreaX, zoomAreaY, zoomAreaWidth, zoomAreaHeight);

            // Обновляем форму
            Invalidate();
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {

        }
    }
}
