using ImageProcessingCore.Helpers;
using ImageProcessingCore.Strategy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.LinearFilters
{
	public class ConvolutionFilter : IProcessingStrategy
	{
		private int[] mask;
		private int maskSize;
		public ConvolutionFilter(int[] mask)
		{
			this.mask = mask;
			this.maskSize = (int)Math.Sqrt(mask.Length);
		}
		public unsafe ImageModel Process(ImageModel input)
		{
			Bitmap output = input.SpatialDomain.Clone(new Rectangle(0, 0, input.SpatialDomain.Width, input.SpatialDomain.Height), input.SpatialDomain.PixelFormat);
			int halfMaskSize = maskSize / 2;

			BitmapData bData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, output.PixelFormat);
			BitmapData bDataInput = input.SpatialDomain.LockBits(new Rectangle(0, 0, input.SpatialDomain.Width, input.SpatialDomain.Height), ImageLockMode.ReadWrite, input.SpatialDomain.PixelFormat);
			byte* scan0 = (byte*)bData.Scan0.ToPointer();
			byte* scan0Input = (byte*)bDataInput.Scan0.ToPointer();
			byte bitsPerPixel = ImageHelper.GetBitsPerPixel(bData.PixelFormat);
			byte* data, maskData;
			int index = 0, value = 0, valueRed = 0, valueGreen = 0, valueBlue = 0;
			if (bitsPerPixel == 8)
			{
				for (int i = halfMaskSize; i < bData.Height - halfMaskSize; ++i)
				{
					for (int j = halfMaskSize; j < bData.Width - halfMaskSize; ++j)
					{
						data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
						for (int k = -halfMaskSize; k <= halfMaskSize; k++)
						{
							for (int l = -halfMaskSize; l <= halfMaskSize; l++)
							{
								maskData = scan0Input + (i + k) * bDataInput.Stride + (j + l) * bitsPerPixel / 8;
								value += maskData[0] * mask[index];
								index++;
							}
						}
						data[0] = ImageHelper.Range(value, 0, 255);
						index = 0;
						value = 0;
					}
				}

			}
			else
			{
				for (int i = halfMaskSize; i < bData.Height - halfMaskSize; ++i)
				{
					for (int j = halfMaskSize; j < bData.Width - halfMaskSize; ++j)
					{
						data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
						for (int k = -halfMaskSize; k <= halfMaskSize; k++)
						{
							for (int l = -halfMaskSize; l <= halfMaskSize; l++)
							{
								maskData = scan0Input + (i + k) * bDataInput.Stride + (j + l) * bitsPerPixel / 8;
								valueRed += maskData[0] * mask[index];
								valueGreen += maskData[1] * mask[index];
								valueBlue += maskData[2] * mask[index];
								index++;
							}
						}
						data[0] = ImageHelper.Range(valueRed, 0, 255);
						data[1] = ImageHelper.Range(valueGreen, 0, 255);
						data[2] = ImageHelper.Range(valueBlue, 0, 255);
						index = 0;
						valueRed = 0;
						valueGreen = 0;
						valueBlue = 0;
					}
				}
			}
			input.SpatialDomain.UnlockBits(bDataInput);
			output.UnlockBits(bData);

			return new ImageModel(output);
		}
	}
}
