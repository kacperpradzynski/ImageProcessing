using ImageProcessingCore.Helpers;
using ImageProcessingCore.Strategy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.BasicImageOperations
{
	public class ContrastOperator : IProcessingStrategy
	{
		private int contrast;
		public ContrastOperator(int contrast)
		{
			this.contrast = contrast;
		}
		public unsafe Bitmap Process(Bitmap input)
		{
			Bitmap output = input.Clone(new Rectangle(0, 0, input.Width, input.Height), input.PixelFormat);
			double contrastValue = Math.Pow((100.0 + contrast) / 100.0, 2);
			BitmapData bData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, output.PixelFormat);
			byte bitsPerPixel = ImageHelper.GetBitsPerPixel(bData.PixelFormat);
			byte* scan0 = (byte*)bData.Scan0.ToPointer();

			if (bitsPerPixel == 8)
			{
				for (int i = 0; i < bData.Height; ++i)
				{
					for (int j = 0; j < bData.Width; ++j)
					{
						byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
						data[0] = ImageHelper.Range((int)((((((double)data[0]/(double)255) - 0.5) * contrastValue) + 0.5 ) * 255), 0, 255);
					}
				}

			}
			else
			{
				for (int i = 0; i < bData.Height; ++i)
				{
					for (int j = 0; j < bData.Width; ++j)
					{
						byte* data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
						data[0] = ImageHelper.Range((int)((((((double)data[0] / (double)255) - 0.5) * contrastValue) + 0.5) * 255), 0, 255);
						data[1] = ImageHelper.Range((int)((((((double)data[1] / (double)255) - 0.5) * contrastValue) + 0.5) * 255), 0, 255);
						data[2] = ImageHelper.Range((int)((((((double)data[2] / (double)255) - 0.5) * contrastValue) + 0.5) * 255), 0, 255);
					}
				}
			}
			output.UnlockBits(bData);

			return output;
		}
	}
}
