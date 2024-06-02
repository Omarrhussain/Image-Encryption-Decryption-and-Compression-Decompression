using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Windows.Forms;

namespace ImageEncryptCompress
{
    public class ImageBreaker
    {
        public RGBPixel[,] EncryptedImage;
        public byte NumOfBits;
        public TextBox Seedd,Tap;

        public ImageBreaker(RGBPixel[,] encryptedImage,byte numOfBits, TextBox seedd, TextBox tap)
        {
            EncryptedImage = encryptedImage;
            NumOfBits = numOfBits;
            Seedd = seedd;
            Tap = tap;
        }

        public RGBPixel[,] BreakImage()
        {
            RGBPixel[,] bestDecryptedImage = null;
            double maxDeviation = double.MinValue;
            long bestSeed = 0;
            byte bestTapPosition = 0;

            for (long seed = 0; seed < (1L << NumOfBits); seed++)
            {
                for (byte tapPosition = 0; tapPosition < NumOfBits; tapPosition++)
                {
                    RGBPixel[,] decryptedImage = DecryptImage(EncryptedImage, seed, tapPosition, NumOfBits);

                    double deviation = CalculateDeviation(decryptedImage);

                    if (deviation > maxDeviation)
                    {
                        maxDeviation = deviation;
                        bestSeed = seed;
                        bestTapPosition = tapPosition;
                        bestDecryptedImage = decryptedImage;
                    }
                }
            }
            Seedd.Text = Convert.ToString(bestSeed, 2);
            Tap.Text = Convert.ToString(bestTapPosition);
            return bestDecryptedImage;
        }

        private static double CalculateDeviation(RGBPixel[,] image)
        {
            double totalDeviation = 0;
            foreach (var pixel in image)
            {
                totalDeviation += ((pixel.red - 128) * (pixel.red - 128));
                totalDeviation += ((pixel.green - 128) * (pixel.green - 128));
                totalDeviation += ((pixel.blue - 128) * (pixel.blue - 128));

            }
           totalDeviation=Math.Sqrt(totalDeviation);

            double averageDeviation = totalDeviation / (image.GetLength(0)*image.GetLength(1)); 

            return averageDeviation;
        }

        private static RGBPixel[,] DecryptImage(RGBPixel[,] encryptedImage, long seed, byte tapPosition, byte numOfBits)
        {
            RGBPixel[,] decryptedImage = new RGBPixel[encryptedImage.GetLength(0), encryptedImage.GetLength(1)];
            LFSREncryption lfsrEncryption = new LFSREncryption(seed, tapPosition, numOfBits);

            for (int i = 0; i < encryptedImage.GetLength(0); i++)
            {
                for (int j = 0; j < encryptedImage.GetLength(1); j++)
                {
                    decryptedImage[i, j].red = lfsrEncryption.EncryptByte(encryptedImage[i, j].red);
                    decryptedImage[i, j].green = lfsrEncryption.EncryptByte(encryptedImage[i, j].green);
                    decryptedImage[i, j].blue = lfsrEncryption.EncryptByte(encryptedImage[i, j].blue);
                }
            }

            return decryptedImage;
        }
    }
}

