using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
       
        #region variables
        public Bitmap map { get; set; }
        private int thrds = 4;
        private const int  MAX_THREADS = 64;
        private const int SCALE = 3;
        private const string BENCHFILE = "Benchmark.txt";
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
                pictureBox1.Image =Image.FromFile(ofd.FileName);
            map = (Bitmap)pictureBox1.Image;
            button2.Enabled = true;
        }

        private void blurImage(int thrds)
        {
            if (pictureBox2.Image != null)
                pictureBox2.Image = null;
            Bitmap[] parts = new Bitmap[thrds];
            int size = map.Height / thrds;
            int prt = 0;
            //Splits the map into parts
            for (int i = 0; i < thrds; i++)
            {
                parts[i] = map.Clone(new Rectangle(0, (prt * size), map.Width, size), PixelFormat.DontCare);
                prt++;
            }
            Graphics g = Graphics.FromImage((Image)map);
            g.Clear(Color.LawnGreen);
            prt = 0;
            int m = 0;
            Thread[] threads = new Thread[thrds];
            foreach (Bitmap b in parts)
            {
                threads[m] = new Thread(() => blurBlock(SCALE, b, b));
                threads[m].Start();
                m++;
            }

            foreach (Thread t in threads)
                t.Join();
            foreach (Bitmap s in parts)
            {
                g.DrawImage(s, new Point(0, prt * size));
                s.Dispose();
                prt++;
            }
            pictureBox2.Image = (Image)map;
            map.Save(@"image.png", ImageFormat.Png);
        }

        private void blurImage(int thrd, bool fnl)
        {
            if (pictureBox2.Image != null)
                pictureBox2.Image = null;
            Bitmap[] parts = new Bitmap[thrds];
            int size = map.Height / thrds;
            int prt = 0;
            int fnlSize = map.Height - ((thrds - 1) * size);
            //Splits the map into parts
            for (int i = 0; i < thrds-1; i++)
            {
                parts[i] = map.Clone(new Rectangle(0, (prt * size), map.Width, size), PixelFormat.DontCare);
                prt++;
            }
            parts[parts.Length-1] = map.Clone(new Rectangle(0, (prt * size), map.Width, fnlSize), PixelFormat.DontCare);
            Graphics g = Graphics.FromImage((Image)map);
            g.Clear(Color.LawnGreen);
            prt = 0;
            int m = 0;
            Thread[] threads = new Thread[thrds];
            foreach (Bitmap b in parts)
            {
                threads[m] = new Thread(() => blurBlock(SCALE, b, b));
                threads[m].Start();
                m++;
            }

            foreach (Thread t in threads)
                t.Join();
            foreach (Bitmap s in parts)
            {
                g.DrawImage(s, new Point(0, prt * size));
                s.Dispose();
                prt++;
            }
            pictureBox2.Image = (Image)map;
            map.Save(@"image.png", ImageFormat.Png);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            map = (Bitmap)pictureBox1.Image;
            thrds = (int)numericUpDown1.Value;
            //blurImage(thrds);
            blurImage(thrds, true);
        }
        
        private static void blurBlock(int SCALE, Image image , Bitmap bitmap) {
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

        private void Form1_Load(object sender, EventArgs e)
        {
            button2.Enabled = false;
            pgrBar.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            map = (Bitmap)pictureBox1.Image;
            thrds = (int)numericUpDown1.Value;
            bool stopper = false;
            while(map.Height % thrds != 0)
            {
                if (thrds >= 64 || stopper)
                {
                    thrds--;
                    stopper = true;
                }
                else if (!stopper)
                    thrds++;
            }
            blurImage(thrds);
        }

        private void btnBench_Click(object sender, EventArgs e)
        {
            pgrBar.Visible = true;
            Stopwatch watch = new Stopwatch();
            StreamWriter txt = new StreamWriter(BENCHFILE);
            pgrBar.Value = 0;
            map = (Bitmap)pictureBox1.Image;

            txt.WriteLine("Benchmark for:" +map.Width+"x"+map.Height);
            for(int i=1; i<=MAX_THREADS; i++)
            {
                watch.Reset();
                watch.Start();
                blurImage(i, true);
                watch.Stop();
                txt.WriteLine("Thread{0}: {1}ms", i.ToString(), watch.Elapsed.TotalMilliseconds);
                pgrBar.Value = (int)Math.Round(((double)i / MAX_THREADS) * 100);
            }
            MessageBox.Show("Benchmark saved in: " + BENCHFILE);
            txt.Close();
        }
    }
}
