﻿using ImageProcessingCore.Helpers;
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
	public class MedianFilter : IProcessingStrategy
	{
		private int maskSize;
		public MedianFilter(int maskSize)
		{
			this.maskSize = maskSize % 2 == 0 ? maskSize + 1 : maskSize;
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
			List<int> mask = new List<int>();
			List<int> maskRed = new List<int>(), maskGreen = new List<int>(), maskBlue = new List<int>();
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
								maskData = scan0Input + ( i + k ) * bDataInput.Stride + ( j + l ) * bitsPerPixel / 8;
								mask.Add(maskData[0]);
							}
						}
						data[0] = (byte)mask.OrderBy(n => n).ElementAt(mask.Count / 2);
						mask.Clear();
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
								maskRed.Add(maskData[0]);
								maskGreen.Add(maskData[1]);
								maskBlue.Add(maskData[2]);
							}
						}
						data[0] = (byte)maskRed.OrderBy(n => n).ElementAt(maskRed.Count / 2);
						data[1] = (byte)maskGreen.OrderBy(n => n).ElementAt(maskGreen.Count / 2);
						data[2] = (byte)maskBlue.OrderBy(n => n).ElementAt(maskBlue.Count / 2);
						maskRed.Clear();
						maskGreen.Clear();
						maskBlue.Clear();
					}
				}
			}
			input.SpatialDomain.UnlockBits(bDataInput);
			output.UnlockBits(bData);

			return new ImageModel(output);
		}
	}
}
