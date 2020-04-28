using ImageProcessingCore.Helpers;
using ImageProcessingCore.Strategy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.NonLinearFilters
{
	public class SobelOperator : IProcessingStrategy
	{
		public unsafe ImageModel Process(ImageModel input)
		{
			Bitmap output = input.SpatialDomain.Clone(new Rectangle(0, 0, input.SpatialDomain.Width, input.SpatialDomain.Height), input.SpatialDomain.PixelFormat);

			BitmapData bData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, output.PixelFormat);
			BitmapData bDataInput = input.SpatialDomain.LockBits(new Rectangle(0, 0, input.SpatialDomain.Width, input.SpatialDomain.Height), ImageLockMode.ReadWrite, input.SpatialDomain.PixelFormat);
			byte* scan0 = (byte*)bData.Scan0.ToPointer();
			byte* scan0Input = (byte*)bDataInput.Scan0.ToPointer();
			byte bitsPerPixel = ImageHelper.GetBitsPerPixel(bData.PixelFormat);
			byte* data, a0, a1, a2, a3, a4, a5, a6, a7;

			if (bitsPerPixel == 8)
			{
				for (int i = 1; i < bData.Height - 1; ++i)
				{
					for (int j = 1; j < bData.Width - 1; ++j)
					{
						data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
						a0 = scan0Input + (i-1) * bDataInput.Stride + (j-1) * bitsPerPixel / 8;
						a1 = scan0Input + i * bDataInput.Stride + (j-1) * bitsPerPixel / 8;
						a2 = scan0Input + (i+1) * bDataInput.Stride + (j-1) * bitsPerPixel / 8;
						a3 = scan0Input + (i+1) * bDataInput.Stride + j * bitsPerPixel / 8;
						a4 = scan0Input + (i+1) * bDataInput.Stride + (j+1) * bitsPerPixel / 8;
						a5 = scan0Input + i * bDataInput.Stride + (j+1) * bitsPerPixel / 8;
						a6 = scan0Input + (i-1) * bDataInput.Stride + (j+1) * bitsPerPixel / 8;
						a7 = scan0Input + (i-1) * bDataInput.Stride + j * bitsPerPixel / 8;
						data[0] = ImageHelper.Range((int)Math.Floor(Math.Sqrt(( ((a2[0] + 2 * a3[0] + a4[0]) - (a0[0] + 2 * a7[0] + a6[0])) *((a2[0] + 2 * a3[0] + a4[0]) - (a0[0] + 2 * a7[0] + a6[0])) ) + ( ((a0[0] + 2 * a1[0] + a2[0]) - (a6[0] + 2 * a5[0] + a4[0])) *((a0[0] + 2 * a1[0] + a2[0]) - (a6[0] + 2 * a5[0] + a4[0])) ))), 0, 255);
					}
				}

			}
			else
			{
				for (int i = 1; i < bData.Height - 1; ++i)
				{
					for (int j = 1; j < bData.Width - 1; ++j)
					{
						data = scan0 + i * bData.Stride + j * bitsPerPixel / 8;
						a0 = scan0Input + (i - 1) * bDataInput.Stride + (j - 1) * bitsPerPixel / 8;
						a1 = scan0Input + i * bDataInput.Stride + (j - 1) * bitsPerPixel / 8;
						a2 = scan0Input + (i + 1) * bDataInput.Stride + (j - 1) * bitsPerPixel / 8;
						a3 = scan0Input + (i + 1) * bDataInput.Stride + j * bitsPerPixel / 8;
						a4 = scan0Input + (i + 1) * bDataInput.Stride + (j + 1) * bitsPerPixel / 8;
						a5 = scan0Input + i * bDataInput.Stride + (j + 1) * bitsPerPixel / 8;
						a6 = scan0Input + (i - 1) * bDataInput.Stride + (j + 1) * bitsPerPixel / 8;
						a7 = scan0Input + (i - 1) * bDataInput.Stride + j * bitsPerPixel / 8;
						data[0] = ImageHelper.Range((int)Math.Floor(Math.Sqrt((((a2[0] + 2 * a3[0] + a4[0]) - (a0[0] + 2 * a7[0] + a6[0])) * ((a2[0] + 2 * a3[0] + a4[0]) - (a0[0] + 2 * a7[0] + a6[0]))) + (((a0[0] + 2 * a1[0] + a2[0]) - (a6[0] + 2 * a5[0] + a4[0])) * ((a0[0] + 2 * a1[0] + a2[0]) - (a6[0] + 2 * a5[0] + a4[0]))))), 0, 255);
						data[1] = ImageHelper.Range((int)Math.Floor(Math.Sqrt((((a2[1] + 2 * a3[1] + a4[1]) - (a0[1] + 2 * a7[1] + a6[1])) * ((a2[1] + 2 * a3[1] + a4[1]) - (a0[1] + 2 * a7[1] + a6[1]))) + (((a0[1] + 2 * a1[1] + a2[1]) - (a6[1] + 2 * a5[1] + a4[1])) * ((a0[1] + 2 * a1[1] + a2[1]) - (a6[1] + 2 * a5[1] + a4[1]))))), 0, 255);
						data[2] = ImageHelper.Range((int)Math.Floor(Math.Sqrt((((a2[2] + 2 * a3[2] + a4[2]) - (a0[2] + 2 * a7[2] + a6[2])) * ((a2[2] + 2 * a3[2] + a4[2]) - (a0[2] + 2 * a7[2] + a6[2]))) + (((a0[2] + 2 * a1[2] + a2[2]) - (a6[2] + 2 * a5[2] + a4[2])) * ((a0[2] + 2 * a1[2] + a2[2]) - (a6[2] + 2 * a5[2] + a4[2]))))), 0, 255);
					}
				}
			}
			input.SpatialDomain.UnlockBits(bDataInput);
			output.UnlockBits(bData);

			return new ImageModel(output);
		}
	}
}
