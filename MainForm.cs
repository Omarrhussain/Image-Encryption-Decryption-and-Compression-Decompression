using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageEncryptCompress
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        RGBPixel[,] ImageMatrix;
        public TimeSpan totalcomprtime;

        public TimeSpan totaldecomprtime;

        string openedFilePath;

        public void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                openedFilePath = openFileDialog1.FileName;
                ImageMatrix = ImageOperations.OpenImage(openedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, pictureBox1);


                int width = ImageOperations.GetWidth(ImageMatrix);
                int height = ImageOperations.GetHeight(ImageMatrix);
                txtWidth.Text = width.ToString();
                txtHeight.Text = height.ToString();


            }
        }

        private void btnEncrypt_Click_1(object sender, EventArgs e)
        {
            if (ImageMatrix == null)
            {
                MessageBox.Show("Please open an image first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            long intialSeed = 0;
            string InitialSeedString = InitialSeed.Text;
            bool containsCharacter = InitialSeedString.Any(c => c != '0' && c != '1' );
            if (containsCharacter)//  Seed contains chars
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (char c in InitialSeedString)
                {
                    string binaryChar = Convert.ToString(c, 2).PadLeft(8, '0');
                    stringBuilder.Append(binaryChar);
                }
                InitialSeedString = stringBuilder.ToString();
                InitialSeed.Text = InitialSeedString;
                intialSeed = Convert.ToInt64(InitialSeedString, 2);
            }
            else //  Seed contains only 0 & 1
            {
                intialSeed = Convert.ToInt64(InitialSeedString, 2);
            }
            byte tapPosition = Convert.ToByte(TapPosition.Text);
            byte NumOfBits = (byte)InitialSeedString.Length;
            try
            {
                if (tapPosition >= NumOfBits)
                {
                    throw new InvalidOperationException("Tap position cannot be greater than or equal to the number of bits.");
                }
                EncryptImage(intialSeed, tapPosition, NumOfBits);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.Image.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
            }
            

        }
        private async void EncryptImage(long seed, byte tapPosition, byte bits)
        {
            Stopwatch watch = new Stopwatch();
            LFSREncryption lfsrEncryption = new LFSREncryption(seed, tapPosition, bits);
            watch.Start();
            for (int i = 0; i < ImageMatrix.GetLength(0); i++)  //O(N*N)
            {
                await Task.Run(() =>
                {
                    for (int j = 0; j < ImageMatrix.GetLength(1); j++)
                    {
                        ImageMatrix[i, j].red = lfsrEncryption.EncryptByte(ImageMatrix[i, j].red);
                        ImageMatrix[i, j].green = lfsrEncryption.EncryptByte(ImageMatrix[i, j].green);
                        ImageMatrix[i, j].blue = lfsrEncryption.EncryptByte(ImageMatrix[i, j].blue);
                    }
                }).
                ContinueWith((ant) =>
                {
                    string te = watch.Elapsed.ToString(@"m\:ss");
                    lblTimeTaken.Text = "Time Taken : " + te;
                }
                , TaskScheduler.FromCurrentSynchronizationContext());
            }
            watch.Stop();
            totalcomprtime = watch.Elapsed;
            totaldecomprtime = watch.Elapsed;
            string tsOut = watch.Elapsed.ToString(@"m\:ss");
            lblTimeTaken.Text = "Time Taken : " + tsOut;
            ImageOperations.DisplayImage(ImageMatrix, pictureBox2);
        }






        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void Compress_Click(object sender, EventArgs e)
        {
            lblTimeTaken.Text = "Loading... ";

            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Compression C = new Compression();
            C.comp(ImageMatrix, openedFilePath, InitialSeed.Text, TapPosition.Text);

            
            stopwatch.Stop();
            totalcomprtime += stopwatch.Elapsed;
      
            string tsOut = totalcomprtime.ToString(@"m\:ss");
            lblTimeTaken.Text = "Time Taken : " + tsOut;
            //MessageBox.Show($"Compression completed in {stopwatch.ElapsedMilliseconds / 1000} seconds.", "Compression Time");


        }



        private void decompress_Click(object sender, EventArgs e)
        {
            lblTimeTaken.Text = "Loading... ";
            RGBPixel[,] Image;
            Compression DC = new Compression();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Image = DC.Decomp();

            stopwatch.Stop();
            totaldecomprtime += stopwatch.Elapsed;

            InitialSeed.Text = Compression.initialseed;

            TapPosition.Text = Compression.taposition; 

            string tsOut = totaldecomprtime.ToString(@"m\:ss");
            lblTimeTaken.Text = "Time Taken : " + tsOut;

            ImageOperations.DisplayImage(Image, pictureBox2);

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
            saveFileDialog1.RestoreDirectory = true;
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox2.Image.Save(saveFileDialog1.FileName, ImageFormat.Bmp);
            }
            ImageMatrix = Image;
        }

        private async void BtnBreaker_Click(object sender, EventArgs e)
        {
            lblTimeTaken.Text = "Loading.....";

            // Check if an image is loaded
            if (ImageMatrix == null)
            {
                MessageBox.Show("Please open an image first.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Parse the number of bits from the input field
            byte numOfBits;
            if (!byte.TryParse(numofbits.Text, out numOfBits))
            {
                MessageBox.Show("Please enter a valid number of bits.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ImageBreaker breaker = new ImageBreaker(ImageMatrix, numOfBits, InitialSeed, TapPosition);
            // Start a stopwatch to measure time taken for decryption
            Stopwatch watch = new Stopwatch();
            watch.Start();

            // Decrypt the image asynchronously
            RGBPixel[,] decryptedImage = await Task.Run(() => breaker.BreakImage());

            // Stop the stopwatch and display the time taken
            watch.Stop();
            string elapsedTime = watch.Elapsed.ToString(@"m\:ss");
            lblTimeTaken.Text = "Time Taken : " + elapsedTime;

            // Display the decrypted image if decryption was successful
            if (decryptedImage != null)
            {
                ImageOperations.DisplayImage(decryptedImage, pictureBox2);
            }
            else
            {
                // Show an error message if decryption failed
                MessageBox.Show("Failed to decrypt the image. No matching combination found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }



}



