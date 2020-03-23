using ImageProcessingCore.Strategy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageProcessingCore.Helpers;

namespace ImageProcessingCore.BasicImageOperations
{
    public class NegativeOperator : IProcessingStrategy
    {
        public unsafe Bitmap Process(Bitmap input)
        {
            Bitmap output = input.Clone(new Rectangle(0, 0, input.Width, input.Height), input.PixelFormat);

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
						data[0] = (byte)~data[0];
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
						data[0] = (byte)~data[0];
						data[1] = (byte)~data[1];
						data[2] = (byte)~data[2];
					}
				}
			}
			output.UnlockBits(bData);

			return output;
        }
    }
}
