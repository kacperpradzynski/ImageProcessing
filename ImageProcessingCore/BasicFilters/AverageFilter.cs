using ImageProcessingCore.Helpers;
using ImageProcessingCore.Strategy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.BasicFilters
{
	public class AverageFilter : IProcessingStrategy
	{
		private int maskSize;
		public AverageFilter(int maskSize)
		{
			this.maskSize = maskSize % 2 == 0 ? maskSize + 1 : maskSize;
		}
		public unsafe Bitmap Process(Bitmap input)
		{
			Bitmap output = input.Clone(new Rectangle(0, 0, input.Width, input.Height), input.PixelFormat);
			int halfMaskSize = maskSize / 2;
			int maskSizeSquare = maskSize * maskSize;

			BitmapData bData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, output.PixelFormat);
			BitmapData bDataInput = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadWrite, input.PixelFormat);
			byte* scan0 = (byte*)bData.Scan0.ToPointer();
			byte* scan0Input = (byte*)bDataInput.Scan0.ToPointer();
			byte bitsPerPixel = ImageHelper.GetBitsPerPixel(bData.PixelFormat);
			byte* data, maskData;
			int mask = 0;
			int maskRed = 0, maskGreen = 0, maskBlue = 0;
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
								mask += maskData[0];
							}
						}
						data[0] = (byte)(mask / maskSizeSquare);
						mask = 0;
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
								maskRed += maskData[0];
								maskGreen += maskData[1];
								maskBlue += maskData[2];
							}
						}
						data[0] = (byte)(maskRed / maskSizeSquare);
						data[1] = (byte)(maskGreen / maskSizeSquare);
						data[2] = (byte)(maskBlue / maskSizeSquare);
						maskRed = 0;
						maskGreen = 0;
						maskBlue = 0;
					}
				}
			}
			input.UnlockBits(bDataInput);
			output.UnlockBits(bData);

			return output;
		}
	}
}
