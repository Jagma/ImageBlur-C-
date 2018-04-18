using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageBlur
{
    public partial class Form1 : Form
    {
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
            Bitmap img = new Bitmap(pictureBox1.Image);
            Bitmap blurPic = new Bitmap(img.Width, img.Height);

            Int32 avgR = 0, avgG = 0, avgB = 0;
            Int32 blurPixelCount = 0;

            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    Color pixel = img.GetPixel(x, y);
                    avgR += pixel.R;
                    avgG += pixel.G;
                    avgB += pixel.B;

                    blurPixelCount++;
                }
            }

            avgR = avgR / blurPixelCount;
            avgG = avgG / blurPixelCount;
            avgB = avgB / blurPixelCount;

            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    blurPic.SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
                }
            }

            pictureBox2.Image = blurPic;
        }
    }
}
