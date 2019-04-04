using System;
using System.Drawing;

namespace PrepareSVMLearningDataSet
{
	public class RandomPixelSelector
	{
		int xMax;
		int yMax;
		int imageSkipBorder;
		Random rnd;

		public RandomPixelSelector(int width, int height, int imageSkipBorder, int seed)
		{
			xMax = width - imageSkipBorder;
			yMax = height - imageSkipBorder;
			this.imageSkipBorder = imageSkipBorder;
			rnd = new Random(seed);
		}

		public Point SelectRandomPixel()
		{
			return new Point(rnd.Next(imageSkipBorder, xMax), rnd.Next(imageSkipBorder, yMax));
		}
	}
}
