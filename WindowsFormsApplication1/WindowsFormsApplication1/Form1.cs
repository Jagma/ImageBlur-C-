using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Bitmap map { get; set; }
        private int thrds = 4;

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
                pictureBox1.Image =Image.FromFile(ofd.FileName);
            map = (Bitmap)pictureBox1.Image;
            button2.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            thrds = (int)numericUpDown1.Value;
            int SCALE = 3;
            Bitmap[] parts = new Bitmap[thrds];
            int size = map.Height / thrds;
            int prt = 0;
            //Splits the map into parts
            for(int i=0; i<thrds;i++)
            {
                parts[i] = map.Clone(new Rectangle(0,(prt*size),map.Width,size),PixelFormat.DontCare);
                prt++;
            }
            Graphics g = Graphics.FromImage(map);
            g.Clear(Color.Black);
            prt = 0;
            int m = 0;
            Thread[] threads = new Thread[thrds];
            foreach(Bitmap b in parts)
            {
                threads[m] = new Thread(() => blurBlock(SCALE, b,b));
                threads[m].Start();
                m++;
            }

            foreach (Thread t in threads)
                t.Join();
            foreach (Bitmap s in parts)
            {
                 g.DrawImage(s, new Point(0, prt*size));
                 s.Dispose();
                 prt++;
            }
            MessageBox.Show(map.GetPixel(5,5).A.ToString());
            pictureBox2.Image = (Image)map;
            map.Save(@"image.png", ImageFormat.Png);
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
        }
    }
}
