using ImageProcessingCore.FourierTransform;
using ImageProcessingCore.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImageProcessingCore.FourierOperations
{
    public class BandPassFilterOperator : IProcessingStrategy
	{
		private int radius, radius2;
		public BandPassFilterOperator(int radius, int thickness)
		{
			this.radius = radius;
			this.radius2 = radius + thickness;
		}

		public ImageModel Process(ImageModel input)
		{
			Complex[,] frequencyDomainCopy = input.FrequencyDomain.Clone() as Complex[,];
			FFT.SwapQuadrants(frequencyDomainCopy);
			double imageWidth = input.FrequencyDomain.GetUpperBound(0) + 1;
			double imageHeight = input.FrequencyDomain.GetUpperBound(1) + 1;
			double halfImageWidth = imageWidth / 2;
			double halfImageHeight = imageHeight / 2;

			for (int i = 0; i < imageWidth; i++)
			{
				for (int j = 0; j < imageHeight; j++)
				{
					if (i == halfImageHeight && j == halfImageWidth)
					{
						continue;
					}

					var widthFactor = Math.Pow((halfImageWidth - j) / radius, 2);
					var heightFactor = Math.Pow((halfImageHeight - i) / radius, 2);
					var widthFactor2 = Math.Pow((halfImageWidth - j) / radius2, 2);
					var heightFactor2 = Math.Pow((halfImageHeight - i) / radius2, 2);

					if (Math.Sqrt(widthFactor + heightFactor) < 1 || Math.Sqrt(widthFactor2 + heightFactor2) > 1)
					{
						frequencyDomainCopy[i, j] = 0;
					}
				}
			}
			FFT.SwapQuadrants(frequencyDomainCopy);
			return new ImageModel(frequencyDomainCopy, input.SpatialDomain);
		}
	}
}
