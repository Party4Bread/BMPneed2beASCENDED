using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Console = Colorful.Console;
//using Colorful;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;

namespace BMPneed2beASCENDED
{
    class Program
    {
        #region p/invoke things
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;

            public COORD(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct CONSOLE_FONT_INFO_EX
        {
            public uint cbSize;
            public uint nFont;
            public COORD dwFontSize;
            public int FontFamily;
            public int FontWeight;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string FaceName;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern Int32 SetCurrentConsoleFontEx(IntPtr ConsoleOutput,bool MaximumWindow,ref CONSOLE_FONT_INFO_EX ConsoleCurrentFontEx);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        extern static bool GetCurrentConsoleFontEx(IntPtr hConsoleOutput,bool bMaximumWindow,ref CONSOLE_FONT_INFO_EX lpConsoleCurrentFont);
        
        private enum StdHandle
        {
            OutputHandle = -11
        }

        [DllImport("kernel32")]
        private static extern IntPtr GetStdHandle(StdHandle index);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleModes dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleModes lpMode);

        [Flags]
        private enum ConsoleModes : uint
        {
            ENABLE_PROCESSED_INPUT = 0x0001,
            ENABLE_LINE_INPUT = 0x0002,
            ENABLE_ECHO_INPUT = 0x0004,
            ENABLE_WINDOW_INPUT = 0x0008,
            ENABLE_MOUSE_INPUT = 0x0010,
            ENABLE_INSERT_MODE = 0x0020,
            ENABLE_QUICK_EDIT_MODE = 0x0040,
            ENABLE_EXTENDED_FLAGS = 0x0080,
            ENABLE_AUTO_POSITION = 0x0100,

            ENABLE_PROCESSED_OUTPUT = 0x0001,
            ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
            ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004,
            DISABLE_NEWLINE_AUTO_RETURN = 0x0008,
            ENABLE_LVB_GRID_WORLDWIDE = 0x0010
        }
        #endregion
        static void Main(string[] args)
        {
            Console.Title = "Realtime Touhou! by P4B";
            /*
            string videofile = "C:\\Users\\BREADY\\Videos\\badapple.mp4", fps = "30";
            if (Directory.Exists("badapplefr")) Directory.Delete("badapplefr", true);
            Thread.Sleep(1000);
            Directory.CreateDirectory("badapplefr");
            //int;
            var psi = new ProcessStartInfo("G:\\ffmpeg\\bin\\ffmpeg", 
                    $"-i \"{videofile}\" -vf fps={fps} badapplefr\\%010d.jpg -hide_banner");
            Process.Start(psi).WaitForExit();
            return;
            */
            Bitmap basePic = new Bitmap(Image.FromFile($"badapplefr\\{1:D10}.jpg"));
            int width = 100, height = basePic.Height * width / basePic.Width;

            CONSOLE_FONT_INFO_EX ConsoleFontInfo = new CONSOLE_FONT_INFO_EX();
            var hnd = GetStdHandle(StdHandle.OutputHandle);
            ConsoleFontInfo.cbSize = (uint)Marshal.SizeOf(ConsoleFontInfo);
            ConsoleModes mod;
            GetConsoleMode(hnd, out mod);
            SetConsoleMode(hnd, mod & ~(ConsoleModes.ENABLE_AUTO_POSITION|ConsoleModes.ENABLE_MOUSE_INPUT));
            GetCurrentConsoleFontEx(hnd, false, ref ConsoleFontInfo);
            ConsoleFontInfo.dwFontSize.Y = 5;
            Console.CursorVisible = false;
            SetCurrentConsoleFontEx(hnd, false, ref ConsoleFontInfo);
            int bufsize = 10000 - 1;
            Console.SetWindowSize(width * 2, height);
            Console.SetBufferSize(bufsize, bufsize);
            Console.ResetColor();
            //Console.ReplaceAllColorsWithDefaults();
            //Console.BackgroundColor = Color.White;
            Console.BackgroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.Black;

            //IntPtr hwndhwnd = GetConsoleWindow();
            //SetWindowPos(hwndhwnd, IntPtr.Zero, Screen.AllScreens[0].Bounds.Left, Screen.AllScreens[0].Bounds.Top, 1337, 1337, 1);

            //RECT ks = new RECT();
            //GetWindowRect(hwndhwnd, out ks);
            //Rectangle bounds = new Rectangle(ks.Left, ks.Top, ks.Right - ks.Left, ks.Bottom - ks.Top);
            int frame = 0;
            string filename;

            //Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            //var pt = new Point(bounds.Left, bounds.Top);
            //Graphics g = Graphics.FromImage(bitmap);
            string[][] stringframe = new string[6572][];
            long curprg = 0;
            var locker = new object();
            Parallel.For(1, 6572, (i) =>
            {
                filename = $"badapplefr\\{i:D10}.jpg";
                //Console.Title = "touhou realtime buffered frames : " + frame.ToString();
                using (Bitmap sourceBitmap = new Bitmap(Image.FromFile(filename)))
                {
                    using (Bitmap ti = (Bitmap)sourceBitmap.GetThumbnailImage(width, height, null, IntPtr.Zero))
                    {
                        using (Bitmap destImage = forcedpalette(ti, new Color[] { Color.White, Color.Black }))
                        {
                            stringframe[i-1] = stringblackwhiteoutput(destImage).Split('\n');
                            //Console.Write(stringframe[i]);
                            //Console.Write(stringblackwhiteoutput(destImage));
                        }
                    }
                }
                Interlocked.Increment(ref curprg);
                Console.Title = $"touhou realtime preloading.... {Interlocked.Read(ref curprg)}/6572";                
            });

            int garo = bufsize / width /2;
            Console.Title = "touhou realtime preloading....";
            for (int i=0;i< stringframe.Length/garo; i++)
            {
                for(int j = 0; j < height; j++)
                {
                    int k;
                    for (k = 0; k < garo; k++)
                    {
                        Console.Write(stringframe[garo * i + k][j]);
                    }
                    Console.Title = $"touhou realtime predrawing.... {garo * i}-{garo * (i+1)}/6572";
                    Console.WriteLine();
                }
            }
            
            //for(int i=0;i<bufsize/height;i++)
            /*
            foreach(var kh in stringframe)
            {
                Console.Write(kh);
            }
            */
            //Console.Title = "touhou realtime playing: " + frame.ToString();
            int curframe = 0;
            var sp = new System.Media.SoundPlayer("badsong.wav");
            //sp.PlaySync();
            sp.Load();
            sp.Play();
            Thread.Sleep(1600);
            Stopwatch sk = Stopwatch.StartNew();
            while (curframe < stringframe.Length)
            {
                if (height * (curframe / garo) + 23 > 9990) break;
                Console.WindowTop = height * (curframe / garo) + 23;
                Console.WindowLeft = (width * (curframe % garo)) * 2;
                //Thread.Sleep(1000);
                //Console.Clear();
                //Console.Write(stringframe[curframe]);
                Console.Title = "touhou realtime playing : " + curframe.ToString();
                curframe = (int)(sk.ElapsedMilliseconds * 30.0 / 1000.0);
            }
            
            
            /*
            psi = new ProcessStartInfo("G:\\ffmpeg\\bin\\ffmpeg",
                    $"-i {filename} tmpsound.aac");

            psi = new ProcessStartInfo("G:\\ffmpeg\\bin\\ffmpeg",
                    $"-framerate {0} -i rendered\\%10d.jpg -i tmpsound.aac {DateTime.Now.ToString()}.mp4");
            Process.Start(psi).WaitForExit();
            */
        }
        public static string stringblackwhiteoutput(Bitmap dat)
        {
            //Console.ResetColor();
            //Console.ReplaceAllColorsWithDefaults();
            const char blackchar = '■',whitechar= '\u25A1';
            string buf = "";
            Color lstclr = dat.GetPixel(0, 0);
            for (int i = 0; i < dat.Height; i++)
            {
                for (int j = 0; j < dat.Width; j++)
                {
                    var tmp = dat.GetPixel(j, i);
                    if (tmp.R == 255)
                    {
                        buf += whitechar;
                    }
                    else
                    {
                        buf += blackchar;
                    }
                }
                buf += '\n';//Environment.NewLine;
            }
            return buf;
        }
        public static void consoleimageoutput(Bitmap dat)
        {
            //Console.ResetColor();
            //Console.ReplaceAllColorsWithDefaults();
            const char thatchar = '■';
            string buf = "";
            Color lstclr = dat.GetPixel(0, 0);
            for (int i = 0; i < dat.Height; i++)
            {
                for (int j = 0; j < dat.Width; j++)
                {
                    var tmp = dat.GetPixel(j, i);
                    if (lstclr != tmp)
                    {
                        Console.Write(buf, lstclr);
                        lstclr = tmp;
                        buf = "";// +thatchar;
                    }
                    buf += thatchar;
                    //Thread.Sleep(10);
                }
                buf += '\n';
                //Console.WriteLine();
            }
            Console.Write(buf, lstclr);
        }
        public static Bitmap colorclustcompress(Bitmap src, int colorcnt)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            Color[] clist = new Color[src.Width*src.Height];
            int hi = 0;
            for (int i = 0; i < src.Height; i++)
            {
                for (int j = 0; j < src.Width; j++)
                {
                    Color cc = src.GetPixel(j, i);
                    clist[hi++]=cc;
                }
            }
            var colorset = clist.Distinct().ToArray();
            Color[] evcolors = new Color[colorcnt];
            Color[] prevevcolors = new Color[colorcnt];
            int[] labels = new int[colorset.Length];
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
                for(int i = 0; i < colorset.Length; i++)
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
                    if (lblcnt[i] == 0)
                    {
                        evcolors[i] = Color.White;
                    }
                    else {
                        evcolors[i] = Color.FromArgb(evpoint[i, 0] / lblcnt[i], evpoint[i, 1] / lblcnt[i], evpoint[i, 2] / lblcnt[i]);
                    }
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
            Graphics g = Graphics.FromImage(dst);
            // Set the image attribute's color mappings
            ColorMap[] colorMap = new ColorMap[colorset.Length];
            for(int i = 0; i < colorset.Length; i++)
            {
                colorMap[i] = new ColorMap();
                colorMap[i].OldColor = colorset[i];
                colorMap[i].NewColor = evcolors[labels[i]];
            }
            ImageAttributes attr = new ImageAttributes();
            attr.SetRemapTable(colorMap);
            // Draw using the color map
            Rectangle rect = new Rectangle(0, 0, dst.Width, dst.Height);
            g.DrawImage(src, rect, 0, 0, rect.Width, rect.Height, GraphicsUnit.Pixel, attr);
            g.Save();
            return dst;
        }
        public static Bitmap forcedpalette(Bitmap src, Color[] evcolors)
        {
            Bitmap dst = new Bitmap(src.Width, src.Height);
            Color[] clist = new Color[src.Width * src.Height];
            int hi = 0;
            for (int i = 0; i < src.Height; i++)
            {
                for (int j = 0; j < src.Width; j++)
                {
                    Color cc = src.GetPixel(j, i);
                    clist[hi++] = cc;
                }
            }
            var colorset = clist.Distinct().ToArray();
            int[] labels = new int[colorset.Length];
            for (int i = 0; i < colorset.Length; i++)
            {
                int minival = int.MaxValue;
                int minidx = 0;
                for (int j = 0; j <evcolors.Length; j++)
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
            Graphics g = Graphics.FromImage(dst);
            // Set the image attribute's color mappings
            ColorMap[] colorMap = new ColorMap[colorset.Length];
            for (int i = 0; i < colorset.Length; i++)
            {
                colorMap[i] = new ColorMap();
                colorMap[i].OldColor = colorset[i];
                colorMap[i].NewColor = evcolors[labels[i]];
            }
            ImageAttributes attr = new ImageAttributes();
            attr.SetRemapTable(colorMap);
            // Draw using the color map
            Rectangle rect = new Rectangle(0, 0, dst.Width, dst.Height);
            g.DrawImage(src, rect, 0, 0, rect.Width, rect.Height, GraphicsUnit.Pixel, attr);
            g.Save();
            g.Dispose();
            return dst;
        }
        public static Bitmap somecontrast(Bitmap sourceBitmap,double cl)
        {
            BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
                            sourceBitmap.Width, sourceBitmap.Height),
                            ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];

            Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
            sourceBitmap.UnlockBits(sourceData);
            double contrastLevel = Math.Pow((100.0 + cl) / 100.0, 2);
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
            double p2 = 0.75;// 1/2 * 3/2
            double p1 = 1.0 - p2;
            double double_r = QqhToRgb(p1, p2, h + 120),
                double_g = QqhToRgb(p1, p2, h), 
                double_b = QqhToRgb(p1, p2, h - 120);
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
