using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Threading;

namespace ImageBlur
{
    public partial class Form1 : Form
    {
        Bitmap bitmap;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if(ofd.ShowDialog()== DialogResult.OK)
                pictureBox1.Image = Image.FromFile( ofd.FileName);
        }

        private void btnBlur_Click(object sender, EventArgs e)
        {
            const int SCALE = 3; //Scale at which the image blurs for example 3x3
            int NumOfThreads = (int)numThreads.Value;
            Image image = pictureBox1.Image;

            Bitmap bitmap = new Bitmap(image.Width, image.Height);
            Thread[] threads = new Thread[NumOfThreads];

            /*  for(int i=0; i<NumOfThreads; i++)
              {
                  threads[0] = new Thread(() => getColours(SCALE, image, ref bitmap));
                  threads[0].Start();
              }*/
            ThreadStart starter = () => getColours(SCALE, image, ref bitmap);  
            Thread thread1 = new Thread(starter);
            thread1.Start();
            thread1.Join();
            

            pictureBox2.Image = bitmap;
            pictureBox2.Image.Save(@"Z:\Users\Jagma\Desktop\Blurs\blur.jpeg", ImageFormat.Jpeg);

        }

        public static void getColours(int SCALE, Image image, ref Bitmap bitmap)
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height),
                    new Rectangle(0, 0, image.Width, image.Height), GraphicsUnit.Pixel);
            }
            // Loop through the image with the SCALE cells.
            for (var yy = 0; yy < image.Height && yy < image.Height; yy += SCALE)
            {

                for (var xx = 0; xx < image.Width && xx < image.Width; xx += SCALE)
                {
                    var cellColors = new List<Color>();

                    // Store each color from the scale cells into cellColors.
                    for (var y = yy; y < yy + SCALE && y < image.Height; y++)
                    {
                        for (var x = xx; x < xx + SCALE && x < image.Width; x++)
                        {
                            cellColors.Add(bitmap.GetPixel(x, y));
                        }
                    }

                    // Get the average red, green, and blue values.
                    var averageRed = cellColors.Aggregate(0, (current, color) => current + color.R) / cellColors.Count;
                    var averageGreen = cellColors.Aggregate(0, (current, color) => current + color.G) / cellColors.Count;
                    var averageBlue = cellColors.Aggregate(0, (current, color) => current + color.B) / cellColors.Count;

                    var averageColor = Color.FromArgb(averageRed, averageGreen, averageBlue);

                    // Go BACK over the scale cells and set each pixel to the average color.
                    for (var y = yy; y < yy + SCALE && y < image.Height; y++)
                    {
                        for (var x = xx; x < xx + SCALE && x < image.Width; x++)
                        {
                            bitmap.SetPixel(x, y, averageColor);
                        }
                    }
                }
            }
         //   return bitmap;
        }

    }
}
