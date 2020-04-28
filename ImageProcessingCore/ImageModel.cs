using ImageProcessingCore.FourierTransform;
using ImageProcessingCore.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore
{
    public class ImageModel
    {
        public Bitmap SpatialDomain { get; set; }
        public Bitmap PhaseImage { get; set; }
        public Bitmap MagnitudeImage { get; set; }
        public Complex[,] FrequencyDomain { get; set; }
        public PixelFormat pixelFormat { get; set; }

        public ImageModel(Bitmap spatialDomain, Complex[,] frequencyDomain)
        {
            SpatialDomain = spatialDomain;
            FrequencyDomain = frequencyDomain;
            pixelFormat = spatialDomain.PixelFormat;
            GenerateFrequencyDomainImages();
        }
        public ImageModel(Bitmap spatialDomain)
        {
            SpatialDomain = spatialDomain;
            pixelFormat = spatialDomain.PixelFormat;
            FrequencyDomain = FFT.Transform(spatialDomain, pixelFormat);
            GenerateFrequencyDomainImages();
        }
        public ImageModel(Complex[,] frequencyDomain, Bitmap paletteHelper)
        {
            FrequencyDomain = frequencyDomain;
            pixelFormat = paletteHelper.PixelFormat;
            SpatialDomain = FFT.InverseTransform(frequencyDomain, pixelFormat, paletteHelper);
            GenerateFrequencyDomainImages();
        }

        private unsafe void GenerateFrequencyDomainImages()
        {
            FFT.SwapQuadrants(FrequencyDomain);
            Bitmap magnitude = SpatialDomain.Clone(new Rectangle(0, 0, SpatialDomain.Width, SpatialDomain.Height), SpatialDomain.PixelFormat);
            Bitmap phase = SpatialDomain.Clone(new Rectangle(0, 0, SpatialDomain.Width, SpatialDomain.Height), SpatialDomain.PixelFormat);

            BitmapData bDataMagnitude = magnitude.LockBits(new Rectangle(0, 0, magnitude.Width, magnitude.Height), ImageLockMode.ReadWrite, magnitude.PixelFormat);
            byte* scan0Magnitude = (byte*)bDataMagnitude.Scan0.ToPointer();

            BitmapData bDataPhase = phase.LockBits(new Rectangle(0, 0, phase.Width, phase.Height), ImageLockMode.ReadWrite, phase.PixelFormat);
            byte* scan0Phase = (byte*)bDataPhase.Scan0.ToPointer();

            byte bitsPerPixel = ImageHelper.GetBitsPerPixel(bDataMagnitude.PixelFormat);

            List<double> phaseValues = new List<double>();
            List<double> magnitudeValues = new List<double>();
            for (int i = 0; i < FrequencyDomain.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < FrequencyDomain.GetUpperBound(1) + 1; j++)
                {
                    phaseValues.Add(FrequencyDomain[i, j].Phase);
                    magnitudeValues.Add(FrequencyDomain[i, j].Magnitude);
                }
            }
            double phaseMax = phaseValues.Max();
            double magnitudeMax = magnitudeValues.Max();

            for (int i = 0; i < bDataMagnitude.Height; ++i)
            {
                for (int j = 0; j < bDataMagnitude.Width; ++j)
                {
                    byte* dataMagnitude = scan0Magnitude + i * bDataMagnitude.Stride + j * bitsPerPixel / 8;
                    byte* dataPhase = scan0Phase + i * bDataPhase.Stride + j * bitsPerPixel / 8;
                    dataMagnitude[0] = (byte)FFT.LogNormalize(FrequencyDomain[i, j].Magnitude, magnitudeMax, 255);
                    dataPhase[0] = (byte)FFT.LogNormalize(FrequencyDomain[i, j].Phase, phaseMax, 255);
                }
            }

            magnitude.UnlockBits(bDataMagnitude);
            phase.UnlockBits(bDataPhase);

            MagnitudeImage = magnitude;
            PhaseImage = phase;
            FFT.SwapQuadrants(FrequencyDomain);
        }
    }
}
