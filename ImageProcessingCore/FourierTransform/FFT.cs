using ImageProcessingCore.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.FourierTransform
{
    class FFT
    {
        public static unsafe Complex[,] Transform(Bitmap spatialDomain, PixelFormat pixelFormat)
        {
            Complex[,] output = new Complex[spatialDomain.Width, spatialDomain.Height];
            BitmapData bData = spatialDomain.LockBits(new Rectangle(0, 0, spatialDomain.Width, spatialDomain.Height), ImageLockMode.ReadWrite, spatialDomain.PixelFormat);
            byte bitsPerPixel = ImageHelper.GetBitsPerPixel(bData.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();

            if (bitsPerPixel == 8)
            {
                for (int i = 0; i < bData.Height; ++i)
                {
                    for (int j = 0; j < bData.Width; ++j)
                    {
                        byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
                        output[i, j] = data[0];
                    }
                }
                for (int i = 0; i < output.GetUpperBound(0) + 1; i++)
                {
                    FFTRow(output, i);
                }
                for (int i = 0; i < output.GetUpperBound(1) + 1; i++)
                {
                    FFTColumn(output, i);
                }
            }
            spatialDomain.UnlockBits(bData);
            return output;
        }

        public static unsafe Bitmap InverseTransform(Complex[,] frequencyDomain, PixelFormat pixelFormat, Bitmap paletteHelper)
        {
            Complex[,] frequencyDomainCopy = frequencyDomain.Clone() as Complex[,];
            Bitmap output = paletteHelper.Clone(new Rectangle(0, 0, paletteHelper.Width, paletteHelper.Height), paletteHelper.PixelFormat);

            BitmapData bData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, output.PixelFormat);
            byte bitsPerPixel = ImageHelper.GetBitsPerPixel(bData.PixelFormat);
            byte* scan0 = (byte*)bData.Scan0.ToPointer();

            if (bitsPerPixel == 8)
            {
                for (int i = 0; i < frequencyDomainCopy.GetUpperBound(1) + 1; i++)
                {
                    ConjugateColumn(frequencyDomainCopy, i);
                    FFTColumn(frequencyDomainCopy, i);
                    ConjugateColumn(frequencyDomainCopy, i);
                    ScaleColumn(frequencyDomainCopy, i);
                }
                for (int i = 0; i < frequencyDomainCopy.GetUpperBound(0) + 1; i++)
                {
                    ConjugateRow(frequencyDomainCopy, i);
                    FFTRow(frequencyDomainCopy, i);
                    ConjugateRow(frequencyDomainCopy, i);
                    ScaleRow(frequencyDomainCopy, i);
                }
                for (int i = 0; i < bData.Height; ++i)
                {
                    for (int j = 0; j < bData.Width; ++j)
                    {
                        byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
                        data[0] = ImageHelper.Range((int)Math.Floor(frequencyDomainCopy[i, j].Real), 0, 255);
                    }
                }
            }
            output.UnlockBits(bData);
            return output;
        }

        private static void FFTRow(Complex[,] buffer, int row)
        {
            int bits = (int)Math.Log(buffer.GetUpperBound(0) + 1, 2);

            for (int j = 1; j < buffer.GetUpperBound(0) + 1; j++)
            {
                int swapPos = BitReverse(j, bits);
                if (swapPos <= j)
                {
                    continue;
                }
                var temp = buffer[row, j];
                buffer[row, j] = buffer[row, swapPos];
                buffer[row, swapPos] = temp;
            }

            for (int N = 2; N <= buffer.GetUpperBound(0) + 1; N <<= 1)
            {
                for (int i = 0; i < buffer.GetUpperBound(0) + 1; i += N)
                {
                    for (int k = 0; k < N / 2; k++)
                    {

                        int evenIndex = i + k;
                        int oddIndex = i + k + (N / 2);
                        var even = buffer[row, evenIndex];
                        var odd = buffer[row, oddIndex];

                        double term = -2 * Math.PI * k / (double)N;
                        Complex exp = new Complex(Math.Cos(term), Math.Sin(term)) * odd;

                        buffer[row, evenIndex] = even + exp;
                        buffer[row, oddIndex] = even - exp;

                    }
                }
            }
        }

        private static void FFTColumn(Complex[,] buffer, int column)
        {
            int bits = (int)Math.Log(buffer.GetUpperBound(1) + 1, 2);

            for (int j = 1; j < buffer.GetUpperBound(1) + 1; j++)
            {
                int swapPos = BitReverse(j, bits);
                if (swapPos <= j)
                {
                    continue;
                }
                var temp = buffer[j, column];
                buffer[j, column] = buffer[swapPos, column];
                buffer[swapPos, column] = temp;
            }

            for (int N = 2; N <= buffer.GetUpperBound(1) + 1; N <<= 1)
            {
                for (int i = 0; i < buffer.GetUpperBound(1) + 1; i += N)
                {
                    for (int k = 0; k < N / 2; k++)
                    {

                        int evenIndex = i + k;
                        int oddIndex = i + k + (N / 2);
                        var even = buffer[evenIndex, column];
                        var odd = buffer[oddIndex, column];

                        double term = -2 * Math.PI * k / (double)N;
                        Complex exp = new Complex(Math.Cos(term), Math.Sin(term)) * odd;

                        buffer[evenIndex, column] = even + exp;
                        buffer[oddIndex, column] = even - exp;

                    }
                }
            }
        }

        private static int BitReverse(int n, int bits)
        {
            int reversedN = n;
            int count = bits - 1;

            n >>= 1;
            while (n > 0)
            {
                reversedN = (reversedN << 1) | (n & 1);
                count--;
                n >>= 1;
            }

            return ((reversedN << count) & ((1 << bits) - 1));
        }

        private static void ConjugateRow(Complex[,] buffer, int row)
        {
            for (int j = 0; j < buffer.GetUpperBound(0) + 1; j++)
            {
                buffer[row, j] = Complex.Conjugate(buffer[row, j]);
            }
        }

        private static void ConjugateColumn(Complex[,] buffer, int column)
        {
            for (int j = 0; j < buffer.GetUpperBound(1) + 1; j++)
            {
                buffer[j, column] = Complex.Conjugate(buffer[j, column]);
            }
        }

        private static void ScaleRow(Complex[,] buffer, int row)
        {
            int size = buffer.GetUpperBound(0) + 1;
            for (int j = 0; j < size; j++)
            {
                buffer[row, j] = buffer[row, j]/size;
            }
        }

        private static void ScaleColumn(Complex[,] buffer, int column)
        {
            int size = buffer.GetUpperBound(1) + 1;
            for (int j = 0; j < size; j++)
            {
                buffer[j, column] = buffer[j, column] / size;
            }
        }

        public static double LogNormalize(double value, double max, double expectedMax)
        {
            return Math.Log(1 + value) / Math.Log(1 + max) * expectedMax;
        }

        public static void SwapQuadrants(Complex [,] frequencyDomain)
        {
            int size = frequencyDomain.GetUpperBound(0) + 1;
            for(int x = 0; x < size / 2; x++)
            {
                for (int y = 0; y < size / 2; y++)
                {
                    Complex tmp = frequencyDomain[x, y];
                    frequencyDomain[x, y] = frequencyDomain[x + size / 2, y + size / 2];
                    frequencyDomain[x + size / 2, y + size / 2] = tmp;
                }
            }
            for (int x = size / 2; x < size; x++)
            {
                for (int y = 0; y < size / 2; y++)
                {
                    Complex tmp = frequencyDomain[x, y];
                    frequencyDomain[x, y] = frequencyDomain[x - size / 2, y + size / 2];
                    frequencyDomain[x - size / 2, y + size / 2] = tmp;
                }
            }
        }
    }
}
