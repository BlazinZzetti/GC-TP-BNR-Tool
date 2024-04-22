using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GC_tp_bnr_Tool
{
    public partial class Form1 : Form
    {
        private byte[] tpFileBytes;
        public Form1()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 2;
        }

        private int Display8BitAs5BitColor(int originalIntColor, int startBit, int endBit)
        {
            var startBitSize = (float)(Math.Pow(2, startBit) - 1);
            var endBitSize = (float)(Math.Pow(2, endBit) - 1);
            
            return (int)Math.Floor((originalIntColor / startBitSize) * endBitSize);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Generate16bitPalette();
        }

        private void Generate16bitPalette()
        {
            int size = 32;
            Bitmap palette = new Bitmap(size * 8, size * 4);

            for (int r = 0; r < size; r++)
            {
                for (int g = 0; g < size; g++)
                {
                    for (int b = 0; b < size; b++)
                    {
                        var rx = (r % 8);
                        var ry = (r / 8); 
                        palette.SetPixel(b +(size * rx), g + (size * ry),
                            Color.FromArgb(
                                Display8BitAs5BitColor(r, 5, 8),
                                Display8BitAs5BitColor(g, 5, 8),
                                Display8BitAs5BitColor(b, 5, 8)));
                    }
                }
            }

            pictureBox1.Image = palette;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string filename = String.Empty;
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "tp File (*.tp)|*.tp";
                var result = ofd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
                {
                    filename = ofd.FileName;
                }
            }

            if (!string.IsNullOrEmpty(filename))
            {
                tpFileBytes = File.ReadAllBytes(filename);
                LoadTpFileBytesToPicture();

            }
        }

        private void LoadTpFileBytesToPicture()
        {
            if(tpFileBytes != null && (tpFileBytes.Length == 2048 || tpFileBytes.Length == 6144))
            {
                var width = 32 * tpFileBytes.Length / 2048;
                var height = 32;
                var tpFileData = new ushort[width, height];
                for (int i = 0; i < tpFileBytes.Length; i += 2)
                {
                    var y = i / 2;
                    var dataX = y % width;
                    var dataY = y / width;
                    tpFileData[dataX, dataY] =
                        BitConverter.ToUInt16(new byte[2] { tpFileBytes[i + 1], tpFileBytes[i] }, 0);
                }
                
                Bitmap tpIcon = new Bitmap(width, height);

                int xpos = 0;
                int ypos = 0;
                int xCount = 0;
                int yCount = 0;

                for (int i = 0; i < tpFileData.Length; i++)
                {
                    var dataX = i % width;
                    var dataY = i / width;
                    var color = tpFileData[dataX, dataY];
                    if ((color & 0x8000) == 0x8000)
                    {
                        //Alpha is 1 so show color.

                        string hexColor = color.ToString("x8");
                        var r = (color & 0x7C00) >> 10;
                        var g = (color & 0x03E0) >> 5;
                        var b = (color & 0x001F);

                        tpIcon.SetPixel(xpos, ypos,
                            Color.FromArgb(
                                Display8BitAs5BitColor(r, 5, 8),
                                Display8BitAs5BitColor(g, 5, 8),
                                Display8BitAs5BitColor(b, 5, 8)));
                    }

                    xCount++;
                    xpos++;
                    if (xCount >= 4)
                    {
                        xCount = 0;
                        xpos -= 4;
                        yCount++;
                        ypos++;
                        if (yCount >= 4)
                        {
                            ypos -= 4;
                            yCount = 0;
                            xpos += 4;
                        }
                    }

                    if (xpos == width)
                    {
                        //New Line
                        xpos = 0;
                        ypos += 4;
                    }
                }

                var outputScale = comboBox1.SelectedIndex + 1;
                var processedImage = new Bitmap(width * outputScale, height * outputScale);

                using (Graphics g = Graphics.FromImage(processedImage))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    g.DrawImage(tpIcon, 0, 0, width * outputScale, height * outputScale);
                }

                pictureBox1.Image = processedImage;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                string filename = String.Empty;
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "PNG File (*.png)|*.png";
                    var result = sfd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(sfd.FileName))
                    {
                        filename = sfd.FileName;
                        using (System.IO.FileStream fstream = new System.IO.FileStream(filename, System.IO.FileMode.Create))
                        {
                            pictureBox1.Image.Save(fstream, System.Drawing.Imaging.ImageFormat.Png);
                            fstream.Close();
                        }
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Export tp.
            if (tpFileBytes != null && tpFileBytes.Length > 0)
            {
                string filename = String.Empty;
                using (var sfd = new SaveFileDialog())
                {
                    sfd.Filter = "tp File (*.tp)|*.tp";
                    var result = sfd.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(sfd.FileName))
                    {
                        filename = sfd.FileName;
                        File.WriteAllBytes(filename, tpFileBytes);
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //import PNG.
            string filename = String.Empty;
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "PNG File (*.png)|*.png";
                var result = ofd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(ofd.FileName))
                {
                    filename = ofd.FileName;
                    pictureBox1.Image = new Bitmap(filename);
                }
            }
            
            //Convert into tp bytes
            var imageData = pictureBox1.Image as Bitmap;
            
            var xpos = 0;
            var ypos = 0;
            var xCount = 0;
            var yCount = 0;
            var byteIntr = 0;
            tpFileBytes = new byte[imageData.Width * imageData.Height * 2];

            while (imageData.Size.Height * imageData.Size.Width *2 != byteIntr)
            {
                var pixelColor = imageData.GetPixel(xpos, ypos);
                var r = pixelColor.R;
                var g = pixelColor.G;
                var b = pixelColor.B;
                var a = pixelColor.A;

                var r2 = Display8BitAs5BitColor(r, 8, 5);
                var g2 = Display8BitAs5BitColor(g, 8, 5);
                var b2 = Display8BitAs5BitColor(b, 8, 5);
                var tpColor = ((a > 0) ? 0x8000 : 0x0000);

                r2 = r2 << 10;
                g2 = g2 << 5;

                tpColor = tpColor | (UInt16)r2 | (UInt16)g2 | (UInt16)b2;
                
                byte tpByte1 = (byte)((tpColor & 0xFF00) >> 8);
                byte tpByte2 = (byte)((tpColor & 0x00FF));
                
                string tpString = tpColor.ToString("X");
                string tpB1String = tpByte1.ToString("X");
                string tpB2String = tpByte2.ToString("X");
                
                tpFileBytes[byteIntr++] = tpByte1;
                tpFileBytes[byteIntr++] = tpByte2;

                xCount++;
                xpos++;
                if (xCount >= 4)
                {
                    xCount = 0;
                    xpos -= 4;
                    yCount++;
                    ypos++;
                    if (yCount >= 4)
                    {
                        ypos -= 4;
                        yCount = 0;
                        xpos += 4;
                    }
                }

                if (xpos == imageData.Width)
                {
                    //New Line
                    xpos = 0;
                    ypos += 4;
                }
            }

            pictureBox1.Image = null;
            LoadTpFileBytesToPicture();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTpFileBytesToPicture();
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            //Who is changing the size?
        }
    }
}