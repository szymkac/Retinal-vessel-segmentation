using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RetinalVessel
{
	/// <summary>
	/// Class responsible for vessel segmentation
	/// </summary>
	public class VesselSegmentator
	{
		/// <summary>
		/// Jagged array with pixels values
		/// </summary>
		byte[][] canalPixels;

		/// <summary>
		/// Property of jagged array with pixels values
		/// </summary>
		public byte[][] CanalPixels { get => canalPixels; }

		/// <summary>
		/// Radius of filter window
		/// </summary>
		int windowRadius;

		/// <summary>
		/// Diameter of filter window
		/// </summary>
		int windowDiameter;

		/// <summary>
		/// Length of small line
		/// </summary>
		int smallLineLenght;

		/// <summary>
		/// Value used to calc small line
		/// </summary>
		int smallLineIndexesDiff;

		/// <summary>
		/// Height of image
		/// </summary>
		int height;

		/// <summary>
		/// Width of image
		/// </summary>
		int width;

		/// <summary>
		/// Points pairs list of lines in window
		/// </summary>
		List<PointsPair> linePoints;

		/// <summary>
		/// Jagged array with result image data
		/// </summary>
		public byte[][] Result;

		/// <summary>
		/// Features for svm segmentation
		/// </summary>
		public SVMFeatures[][] SVMFeaturesMatrix;

		/// <summary>
		/// Threshold of pixel power level when pixel belongs to vessel or not
		/// </summary>
		public double Threshold;

		/// <summary>
		/// Type of filtering method
		/// </summary>
		public VesselSegmentatioMethod VesselSegmentatioMethodType;

		/// <summary>
		/// Constructor
		/// </summary>
		public VesselSegmentator()
		{
			windowRadius = 7;
			smallLineLenght = 3;
			Threshold = 2.5;
			VesselSegmentatioMethodType = VesselSegmentatioMethod.Both;
			Init();
		}

		/// <summary>
		/// Initialize class fields
		/// </summary>
		private void Init()
		{
			windowDiameter = windowRadius * 2 + 1;
			linePoints = new List<PointsPair>();
			smallLineIndexesDiff = (smallLineLenght - 1) / 2;

			for (int i = 1; i < windowDiameter - 1; i += 2)
			{
				linePoints.Add(new PointsPair(new Point(i - windowRadius, -windowRadius), new Point(windowRadius - i, windowRadius)));
			}

			for (int i = 1; i < windowDiameter - 1; i += 2)
			{
				linePoints.Add(new PointsPair(new Point(-windowRadius, i - windowRadius), new Point(windowRadius, windowRadius - i)));
			}
		}

		/// <summary>
		/// Setting image on which filter will work
		/// </summary>
		/// <param name="img">Bitmap input image</param>
		/// <param name="invert">Flag that decides whether to invert the pixel values ​​of the image</param>
		/// <param name="canalType">Canal of image to work on</param>
		public void SetInput(Bitmap img, bool invert, CanalType canalType = CanalType.GREEN)
		{
			height = img.Height;
			width = img.Width;
			canalPixels = new byte[height][];
			Result = new byte[height][];
			SVMFeaturesMatrix = new SVMFeatures[height][];

			for (int i = 0; i < height; i++)
			{
				canalPixels[i] = new byte[width];
				Result[i] = new byte[width];
				SVMFeaturesMatrix[i] = new SVMFeatures[width];

				for (int j = 0; j < width; j++)
				{
					byte colorValue = 0;
					switch (canalType)
					{
						case CanalType.RED:
							colorValue = img.GetPixel(j, i).R;
							break;
						case CanalType.GREEN:
							colorValue = img.GetPixel(j, i).G;
							break;
						case CanalType.BLUE:
							colorValue = img.GetPixel(j, i).B;
							break;
					}
					canalPixels[i][j] = invert ? (byte)(byte.MaxValue - colorValue) : colorValue;
				}
			}

		}

		/// <summary>
		/// Getter of inverted canalPixels field
		/// </summary>
		/// <returns>inverted canalPixels jagged array</returns>
		public byte[][] GetReversedCanalPixels()
		{
			byte[][] reversed = new byte[height][];
			for (int i = 0; i < height; i++)
			{
				reversed[i] = new byte[width];
				for (int j = 0; j < width; j++)
				{
					reversed[i][j] = (byte)(byte.MaxValue - canalPixels[i][j]);
				}
			}
			return reversed;
		}

		/// <summary>
		/// Start filtering vessel from image
		/// </summary>
		public void Calculate()
		{
			switch (VesselSegmentatioMethodType)
			{
				case VesselSegmentatioMethod.Thresholding:
					CalculateThresholding();
					break;
				case VesselSegmentatioMethod.SVM:
					CalculateSVMParameters();
					break;
				case VesselSegmentatioMethod.Both:
					CalculateBothThresholdAndSVM();
					break;
			}
		}

		/// <summary>
		/// Calculate parameters for SVM segmentation and filter image by thresholding
		/// </summary>
		private void CalculateBothThresholdAndSVM()
		{
			for (int i = windowRadius; i < height - windowRadius; i++)
			{
				for (int j = windowRadius; j < width - windowRadius; j++)
				{
					double averageWindowGrayScale = GetAverageWindowGrayScale(j, i);
					double largestLineAverageGrayLevel = GetLargestLineAverageGrayLevel(j, i, out double largestSmallLineAverageGrayLevel);
					double pixelPowerOfMainLine = largestLineAverageGrayLevel -averageWindowGrayScale;
					SVMFeaturesMatrix[i][j] = new SVMFeatures()
					{
						PixelGrayLevel = canalPixels[i][j],
						PixelPowerOfMainLine = pixelPowerOfMainLine,
						PixelPowerOfSmallLine = largestSmallLineAverageGrayLevel - averageWindowGrayScale
					};
					Result[i][j] = pixelPowerOfMainLine > Threshold ? byte.MaxValue : byte.MinValue;
				}
			}
		}

		/// <summary>
		/// Calculate parameters for SVM segmentation
		/// </summary>
		private void CalculateSVMParameters()
		{
			for (int i = windowRadius; i < height - windowRadius; i++)
			{
				for (int j = windowRadius; j < width - windowRadius; j++)
				{
					double averageWindowGrayScale = GetAverageWindowGrayScale(j, i);
					double largestLineAverageGrayLevel = GetLargestLineAverageGrayLevel(j, i, out double largestSmallLineAverageGrayLevel);
					SVMFeaturesMatrix[i][j] = new SVMFeatures()
					{
						PixelGrayLevel = canalPixels[i][j],
						PixelPowerOfMainLine = largestLineAverageGrayLevel - averageWindowGrayScale,
						PixelPowerOfSmallLine = largestSmallLineAverageGrayLevel - averageWindowGrayScale
					};
				}
			}
		}

		/// <summary>
		/// Start filtering vessel from image by thresholding
		/// </summary>
		private void CalculateThresholding()
		{
			for (int i = windowRadius; i < height - windowRadius; i++)
			{
				for (int j = windowRadius; j < width - windowRadius; j++)
				{
					double averageWindowGrayScale = GetAverageWindowGrayScale(j, i);
					double largestLineAverageGrayLevel = GetLargestLineAverageGrayLevel(j, i);
					Result[i][j] = largestLineAverageGrayLevel - averageWindowGrayScale > Threshold ? byte.MaxValue : byte.MinValue;
				}
			}
		}

		/// <summary>
		/// Returning average laregest gray scale level from windows filter's lines
		/// </summary>
		/// <param name="x">Column of image pixel from which calculate power line</param>
		/// <param name="y">Row of image pixel from which calculate power line</param>
		/// <param name="averageGrayScaleOfSmallLine">Average grayscale of small line which is perpendicular to retured line</param>
		/// <returns>Power of best pixel line</returns>
		public double GetLargestLineAverageGrayLevel(int x, int y, out double averageGrayScaleOfSmallLine)
		{
			double largest = 0;
			int largestIndex = 0;
			for (int i = 0; i < linePoints.Count; i++)
			{
				PointsPair el = linePoints[i];
				double grayScaleOfLine = GetLineAverageGrayLevel(el.startPoint.X + x, el.startPoint.Y + y, el.endPoint.X + x, el.endPoint.Y + y);
				if (grayScaleOfLine > largest)
				{
					largest = grayScaleOfLine;
					largestIndex = i;
				}
			}

			int perpendicularIndex = Math.Abs(linePoints.Count / 2 - largestIndex);

			PointsPair line = linePoints[perpendicularIndex];
			averageGrayScaleOfSmallLine = GetSmallLineAverageGrayLevel(line.startPoint.X + x, line.startPoint.Y + y, line.endPoint.X + x, line.endPoint.Y + y);
			return largest;
		}

		/// <summary>
		/// Returning average laregest gray scale level from windows filter's lines
		/// </summary>
		/// <param name="x">Column of image pixel from which calculate power line</param>
		/// <param name="y">Row of image pixel from which calculate power line</param>
		/// <returns>Power of best pixel line</returns>
		public double GetLargestLineAverageGrayLevel(int x, int y)
		{
			var averageGrayScaleOfLines = linePoints.Select(el => GetLineAverageGrayLevel(el.startPoint.X + x, el.startPoint.Y + y, el.endPoint.X + x, el.endPoint.Y + y));
			return averageGrayScaleOfLines.Max();
		}

		/// <summary>
		/// Calculate power of single small line, located at normal line - ugly solution :/
		/// </summary>
		/// <param name="x1">Column of line start pixel</param>
		/// <param name="y1">Row of line start pixel</param>
		/// <param name="x2">Column of line end pixel</param>
		/// <param name="y2">Row of line end pixel</param>
		/// <returns>Power of line</returns>
		private double GetSmallLineAverageGrayLevel(int x1, int y1, int x2, int y2)
		{
			double sum = 0;
			int dx = x2 - x1;
			int dy = y2 - y1;
			dx = dx == 0 ? 1 : dx;

			int i = 0;

			for (int x = x1; x < x2 + 1; x++)
			{
				int y = y1 + dy * (x - x1) / dx;
				i++;

				if (i > windowRadius + smallLineIndexesDiff)
					break;

				if (i >= windowRadius - smallLineIndexesDiff)
					sum += canalPixels[y][x];
			}
			return sum / smallLineLenght;
		}

		/// <summary>
		/// Calculate power of single line
		/// </summary>
		/// <param name="x1">Column of line start pixel</param>
		/// <param name="y1">Row of line start pixel</param>
		/// <param name="x2">Column of line end pixel</param>
		/// <param name="y2">Row of line end pixel</param>
		/// <returns>Power of line</returns>
		private double GetLineAverageGrayLevel(int x1, int y1, int x2, int y2)
		{
			double sum = 0;
			int dx = x2 - x1;
			int dy = y2 - y1;
			dx = dx == 0 ? 1 : dx;

			for (int x = x1; x < x2 + 1; x++)
			{
				int y = y1 + dy * (x - x1) / dx;
				sum += canalPixels[y][x];
			}
			return sum / windowDiameter;
		}


		/// <summary>
		/// Calculating Average of gray level in square arond given pixel
		/// </summary>
		/// <param name="x">Column value of pixel</param>
		/// <param name="y">Row of pixel</param>
		/// <returns>Average of gray level</returns>
		private double GetAverageWindowGrayScale(int x, int y)
		{
			double graySummary = 0;
			int k = 0;
			for (int i = y - windowRadius; i <= y + windowRadius; i++)
			{
				for (int j = x - windowRadius; j <= x + windowRadius; j++)
				{
					k++;
					graySummary += canalPixels[i][j];
				}
			}

			return graySummary / (windowDiameter * windowDiameter);
		}
	}
}