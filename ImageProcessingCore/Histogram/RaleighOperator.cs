using ImageProcessingCore.BasicImageOperations;
using ImageProcessingCore.Helpers;
using ImageProcessingCore.Strategy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.Histogram
{
    public class RaleighOperator : IProcessingStrategy
	{
		private int rMin, rMax, gMin, gMax, bMin, bMax;
		public RaleighOperator(int rMin, int rMax, int gMin, int gMax, int bMin, int bMax)
		{
			this.rMin = rMin;
			this.rMax = rMax;
			this.gMin = gMin;
			this.gMax = gMax;
			this.bMin = bMin;
			this.bMax = bMax;
		}
		public unsafe ImageModel Process(ImageModel input)
		{
			Bitmap output = input.SpatialDomain.Clone(new Rectangle(0, 0, input.SpatialDomain.Width, input.SpatialDomain.Height), input.SpatialDomain.PixelFormat);
			List<int[]> histogram = HistogramGenerator.Generate(input.SpatialDomain);
			BitmapData bData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, output.PixelFormat);
			byte bitsPerPixel = ImageHelper.GetBitsPerPixel(bData.PixelFormat);
			byte* scan0 = (byte*)bData.Scan0.ToPointer();
			int[] red = new int[256];
			int[] green = new int[256];
			int[] blue = new int[256];
			double[] logRed = new double[256];
			double[] logGreen = new double[256];
			double[] logBlue = new double[256];
			int N = input.SpatialDomain.Width * input.SpatialDomain.Height;

			if (bitsPerPixel == 8)
			{
				for (int i = 0; i < histogram[0].Length; ++i)
				{
					logRed[i] = Math.Log(1.0/((double)histogram[0].Take(i + 1).Sum() / (double)N));
				}

				List<double> ignoreInfinity = new List<double>(logRed);
				ignoreInfinity.RemoveAll(item => double.IsInfinity(item) || item == 0);
				logRed = logRed.Select(x => x.Equals(Double.PositiveInfinity) ? ignoreInfinity.Max() : x).ToArray();
				double alpha = Math.Sqrt(((rMax-rMin)*(rMax-rMin))/(2* ignoreInfinity.Max()));

				for (int i = 0; i < histogram[0].Length; ++i)
				{
					red[i] = ImageHelper.Range(rMin + (int)Math.Floor(Math.Sqrt(2 * alpha * alpha * logRed[i])), 0, 255);
				}

				for (int i = 0; i < bData.Height; ++i)
				{
					for (int j = 0; j < bData.Width; ++j)
					{
						byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
						data[0] = (byte)red[data[0]];
					}
				}
			}
			else
			{
				for (int i = 0; i < histogram[0].Length; ++i)
				{
					logRed[i] = Math.Log(1.0 / ((double)histogram[0].Take(i + 1).Sum() / (double)N));
					logGreen[i] = Math.Log(1.0 / ((double)histogram[1].Take(i + 1).Sum() / (double)N));
					logBlue[i] = Math.Log(1.0 / ((double)histogram[2].Take(i + 1).Sum() / (double)N));
				}

				List<double> ignoreInfinityRed = new List<double>(logRed);
				ignoreInfinityRed.RemoveAll(item => double.IsInfinity(item) || item == 0);
				double maxRed = ignoreInfinityRed.Max();
				logRed = logRed.Select(x => x.Equals(Double.PositiveInfinity) ? maxRed : x).ToArray();
				double alphaRed = Math.Sqrt(((rMax - rMin) * (rMax - rMin)) / (2 * maxRed));
				
				List<double> ignoreInfinityGreen = new List<double>(logGreen);
				ignoreInfinityGreen.RemoveAll(item => double.IsInfinity(item) || item == 0);
				double maxGreen = ignoreInfinityGreen.Max();
				logGreen = logGreen.Select(x => x.Equals(Double.PositiveInfinity) ? maxGreen : x).ToArray();
				double alphaGreen = Math.Sqrt(((gMax - gMin) * (gMax - gMin)) / (2 * maxGreen));
				
				List<double> ignoreInfinityBlue = new List<double>(logBlue);
				ignoreInfinityBlue.RemoveAll(item => double.IsInfinity(item) || item == 0);
				double maxBlue = ignoreInfinityBlue.Max();
				logBlue = logBlue.Select(x => x.Equals(Double.PositiveInfinity) ? maxBlue : x).ToArray();
				double alphaBlue = Math.Sqrt(((bMax - bMin) * (bMax - bMin)) / (2 * maxBlue));


				for (int i = 0; i < histogram[0].Length; ++i)
				{
					red[i] = ImageHelper.Range(rMin + (int)Math.Floor(Math.Sqrt(2 * alphaRed * alphaRed * logRed[i])), 0, 255);
					green[i] = ImageHelper.Range(gMin + (int)Math.Floor(Math.Sqrt(2 * alphaGreen * alphaGreen * logGreen[i])), 0, 255);
					blue[i] = ImageHelper.Range(bMin + (int)Math.Floor(Math.Sqrt(2 * alphaBlue * alphaBlue * logBlue[i])), 0, 255);
				}

				for (int i = 0; i < bData.Height; ++i)
				{
					for (int j = 0; j < bData.Width; ++j)
					{
						byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
						data[0] = (byte)red[data[0]];
						data[1] = (byte)green[data[1]];
						data[2] = (byte)blue[data[2]];
					}
				}
			}
			output.UnlockBits(bData);
			NegativeOperator negative = new NegativeOperator();

			return negative.Process(new ImageModel(output, input.FrequencyDomain));
		}
	}
}
