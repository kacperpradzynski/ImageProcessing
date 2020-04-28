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
    public class SpectrumFilterOperator : IProcessingStrategy
	{
        private const double NOISE = 0.0001;
        private const double EPISLON = 0.0000001;

        public double FilterAngle { get; set; }
        public double FilterAngleOffset { get; set; }
        public int FilterRadius { get; set; }

        private double _firstLineAngle;
        private double _secondLineAngle;
        public SpectrumFilterOperator(int radius, double angle, double angleOffset)
		{
            angleOffset %= 360;
            angleOffset = angleOffset < 0 ? 360 + angleOffset : angleOffset;
            angle /= 2;

            FilterAngle = (angle + NOISE) * 2 * Math.PI / 360;
            FilterAngleOffset = (angleOffset + NOISE) * 2 * Math.PI / 360;
            FilterRadius = radius;

            _firstLineAngle = FilterAngle + FilterAngleOffset;
            _secondLineAngle = -FilterAngle + FilterAngleOffset;
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

					var widthFactor = Math.Pow((halfImageWidth - j) / FilterRadius, 2);
					var heightFactor = Math.Pow((halfImageHeight - i) / FilterRadius, 2);

                    if (Math.Sqrt(widthFactor + heightFactor) < 1)
                    {
                        frequencyDomainCopy[i, j] = 0;
                        continue;
                    }

                    var firstLineValue = Math.Tan(_firstLineAngle) * (j - halfImageWidth);
                    var secondLineValue = Math.Tan(_secondLineAngle) * (j - halfImageWidth);

                    bool firstHalfStartAssert;
                    bool firstHalfEndAssert;
                    bool secondHalfStartAssert;
                    bool secondHalfEndAssert;

                    var currentValue = i - halfImageHeight;

                    if ((_firstLineAngle - Math.PI / 2 > EPISLON && _secondLineAngle - Math.PI / 2 < EPISLON) ||
                        (_firstLineAngle - 1.5 * Math.PI > EPISLON && _secondLineAngle - 1.5 * Math.PI < EPISLON))
                    {
                        firstHalfStartAssert = firstLineValue > currentValue;
                        firstHalfEndAssert = secondLineValue > currentValue;
                        secondHalfStartAssert = secondLineValue < currentValue;
                        secondHalfEndAssert = firstLineValue < currentValue;
                    }
                    else if (_firstLineAngle - Math.PI > EPISLON && _firstLineAngle - 1.5 * Math.PI < EPISLON)
                    {
                        firstHalfStartAssert = firstLineValue > currentValue;
                        firstHalfEndAssert = secondLineValue < currentValue;
                        secondHalfStartAssert = firstLineValue < currentValue;
                        secondHalfEndAssert = secondLineValue > currentValue;
                    }
                    else
                    {
                        firstHalfStartAssert = firstLineValue < currentValue;
                        firstHalfEndAssert = secondLineValue > currentValue;
                        secondHalfStartAssert = firstLineValue > currentValue;
                        secondHalfEndAssert = secondLineValue < currentValue;
                    }

                    if (!((firstHalfStartAssert && firstHalfEndAssert) || (secondHalfStartAssert && secondHalfEndAssert)))
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
