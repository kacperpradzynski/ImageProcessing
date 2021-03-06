﻿using ImageProcessingCore.Helpers;
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
    public class BrightnessOperator : IProcessingStrategy
    {
		private int brightness;
		public BrightnessOperator(int brightness)
		{
			this.brightness = brightness;
		}
		public unsafe ImageModel Process(ImageModel input)
		{
			Bitmap output = input.SpatialDomain.Clone(new Rectangle(0, 0, input.SpatialDomain.Width, input.SpatialDomain.Height), input.SpatialDomain.PixelFormat);

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
						data[0] = ImageHelper.Range(data[0] + brightness, 0, 255);
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
						data[0] = ImageHelper.Range(data[0] + brightness, 0, 255);
						data[1] = ImageHelper.Range(data[1] + brightness, 0, 255);
						data[2] = ImageHelper.Range(data[2] + brightness, 0, 255);
					}
				}
			}
			output.UnlockBits(bData);

			return new ImageModel(output);
		}
	}
}
