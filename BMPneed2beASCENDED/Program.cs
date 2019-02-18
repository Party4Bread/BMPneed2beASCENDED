using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Console = Colorful.Console;
using Colorful;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;
namespace BMPneed2beASCENDED
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename = "C:\\Users\\solsa\\Downloads\\73065890_p0_master1200.jpg";//"C:\\Users\\solsa\\OneDrive\\Pictures\\558122_325400004221410_33691947_n.jpg";
            Bitmap sourceBitmap = new Bitmap(Image.FromFile(filename));
            var resultBitmap = sourceBitmap;//somecontrast(sourceBitmap);

            //resultBitmap.Save("what.bmp");
            int width = 200, height = resultBitmap.Height * width / resultBitmap.Width;
            var destImage = new Bitmap(
                resultBitmap.GetThumbnailImage(width, height, null, IntPtr.Zero));
            destImage = colorclustcompress(destImage, 15);
            //Console.BackgroundColor = Color.Transparent;
            for(int i = 0; i < destImage.Height; i++)
            {
                for(int j = 0; j < destImage.Width; j++)
                {
                    Console.Write("■", destImage.GetPixel(j, i));
                    //Thread.Sleep(10);
                }
                Console.WriteLine();
            }
            
        }
        public static Bitmap colorclustcompress(Bitmap src, int colorcnt)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            List<Color> colorset = new List<Color>();

            for (int i = 0; i < src.Height; i++)
            {
                for (int j = 0; j < src.Width; j++)
                {
                    Color cc = src.GetPixel(j, i);
                    if (!colorset.Contains(cc))
                    {
                        colorset.Add(cc);
                    }
                }
            }

            Color[] evcolors = new Color[colorcnt];
            Color[] prevevcolors = new Color[colorcnt];
            int[] labels = new int[colorset.Count];
            bool chksame()
            {
                foreach (var i in evcolors)
                {
                    if (!prevevcolors.Contains(i)) return false;
                }
                return true;
            }
            void calclabel()
            {
                for(int i = 0; i < colorset.Count; i++)
                {
                    int minival = int.MaxValue;
                    int minidx = 0;
                    for(int j = 0; j < colorcnt; j++)
                    {
                        int diff = Math.Abs(colorset[i].R - evcolors[j].R);
                        diff += Math.Abs(colorset[i].G - evcolors[j].G);
                        diff += Math.Abs(colorset[i].B - evcolors[j].B);
                        if (diff < minival)
                        {
                            minival = diff;
                            minidx = j;
                        }
                    }
                    labels[i] = minidx;
                }
            }
            void calcevcolor()
            {
                int[,] evpoint = new int[colorcnt, 3];
                int[] lblcnt = new int[colorcnt];
                for(int i = 0; i < labels.Length; i++)
                {
                    lblcnt[labels[i]]++;
                    evpoint[labels[i], 0] += colorset[i].R;
                    evpoint[labels[i], 1] += colorset[i].G;
                    evpoint[labels[i], 2] += colorset[i].B;
                }
                for(int i = 0; i < colorcnt; i++)
                {
                    if (lblcnt[i] == 0) continue;
                    evcolors[i] = Color.FromArgb(evpoint[i,0]/lblcnt[i], evpoint[i, 1] / lblcnt[i], evpoint[i, 2] / lblcnt[i]);
                }
            }
            for(int i = 0; i < colorcnt; i++)
            {
                evcolors[i]= HlsToRgb(360 * i / colorcnt, 0.5, 0.5);
            }
            while (!chksame())
            {
                evcolors.CopyTo(prevevcolors,0);
                calclabel();
                calcevcolor();
            }
            for (int i = 0; i < src.Height; i++)
            {
                for (int j = 0; j < src.Width; j++)
                {
                    Color k = src.GetPixel(j, i);
                    int l = -1;
                    while (colorset[++l] != k);
                    dst.SetPixel(j, i, evcolors[labels[l]]);
                }
            }
            return dst;
        }
        public static Bitmap somecontrast(Bitmap sourceBitmap)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                            sourceBitmap.Width, sourceBitmap.Height),
                            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);


            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];


            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);


            sourceBitmap.UnlockBits(sourceData);


            double contrastLevel = Math.Pow((100.0 + 20.0) / 100.0, 2);


            double blue = 0;
            double green = 0;
            double red = 0;


            for (int k = 0; k + 4 < pixelBuffer.Length; k += 4)
            {
                blue = ((((pixelBuffer[k] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                green = ((((pixelBuffer[k + 1] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                red = ((((pixelBuffer[k + 2] / 255.0) - 0.5) *
                            contrastLevel) + 0.5) * 255.0;


                if (blue > 255)
                { blue = 255; }
                else if (blue < 0)
                { blue = 0; }


                if (green > 255)
                { green = 255; }
                else if (green < 0)
                { green = 0; }


                if (red > 255)
                { red = 255; }
                else if (red < 0)
                { red = 0; }


                pixelBuffer[k] = (byte)blue;
                pixelBuffer[k + 1] = (byte)green;
                pixelBuffer[k + 2] = (byte)red;
            }


            Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);


            BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
                                        resultBitmap.Width, resultBitmap.Height),
                                        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);


            Marshal.Copy(pixelBuffer, 0, resultData.Scan0, pixelBuffer.Length);
            resultBitmap.UnlockBits(resultData);
            return resultBitmap;
        }

        // Convert an HLS value into an RGB value.
        public static Color HlsToRgb(double h, double l, double s)
        {
            double p2;
            if (l <= 0.5) p2 = l * (1 + s);
            else p2 = l + s - l * s;

            double p1 = 2 * l - p2;
            double double_r, double_g, double_b;
            if (s == 0)
            {
                double_r = l;
                double_g = l;
                double_b = l;
            }
            else
            {
                double_r = QqhToRgb(p1, p2, h + 120);
                double_g = QqhToRgb(p1, p2, h);
                double_b = QqhToRgb(p1, p2, h - 120);
            }

            // Convert RGB to the 0 to 255 range.
            return Color.FromArgb((int)(double_r * 255.0), (int)(double_g * 255.0), (int)(double_b * 255.0));
        }

        private static double QqhToRgb(double q1, double q2, double hue)
        {
            if (hue > 360) hue -= 360;
            else if (hue < 0) hue += 360;

            if (hue < 60) return q1 + (q2 - q1) * hue / 60;
            if (hue < 180) return q2;
            if (hue < 240) return q1 + (q2 - q1) * (240 - hue) / 60;
            return q1;
        }
    }
}
