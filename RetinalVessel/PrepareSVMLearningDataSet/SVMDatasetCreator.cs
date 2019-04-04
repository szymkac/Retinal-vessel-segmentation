using RetinalVessel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrepareSVMLearningDataSet
{
	public class SVMDatasetCreator
	{
		static readonly Random rnd = new Random();

		string pathToImagesFolder;

		string pathToResultFile;

		List<string> pathsToExpertImages;

		readonly VesselSegmentator vs;

		int PixelsToGetPerImage { get; set; }

		public SVMDatasetCreator(string pathToImagesFolder, List<string> pathsToExpertImages, string pathToResultFile)
		{
			Init(pathToImagesFolder, pathsToExpertImages, pathToResultFile);
			vs = new VesselSegmentator();
		}

		public SVMDatasetCreator(string pathToImagesFolder, List<string> pathsToExpertImages, string pathToResultFile, VesselSegmentatorConstructorHelper helper)
		{
			Init(pathToImagesFolder, pathsToExpertImages, pathToResultFile);
			vs = new VesselSegmentator(helper);
		}

		private void Init(string pathToImagesFolder, List<string> pathsToExpertImages, string pathToResultFile)
		{
			this.pathToImagesFolder = pathToImagesFolder;
			this.pathsToExpertImages = pathsToExpertImages;
			this.pathToResultFile = pathToResultFile;
			PixelsToGetPerImage = 30;
		}

		public void CreateDataset()
		{
			StreamWriter sw = new StreamWriter(pathToResultFile);

			FileInfo[] images = new DirectoryInfo(pathToImagesFolder).GetFiles("*.jpg");

			foreach (FileInfo image in images)
			{
				List<Point> usedPixels = new List<Point>();
				Console.Write($"{image.Name,7}): ");
				for (int i = 0; i < pathsToExpertImages.Count; i++)
				{
					Bitmap eyeImage = new Bitmap(pathToImagesFolder + $@"\{image.Name}");
					Bitmap expertImage = new Bitmap(pathsToExpertImages[i] + $@"\{image.Name}");
					vs.SetInput(expertImage, false);
					byte[][] expertMask = vs.CanalPixels;
					vs.SetInput(eyeImage);
					byte[][] eye = vs.CanalPixels;

					RandomPixelSelector rps = new RandomPixelSelector(vs.Width, vs.Height, vs.WindowRadius, rnd.Next(int.MaxValue));

					while (usedPixels.Count < PixelsToGetPerImage * 2 * (i + 1))
					{
						//Vessel
						while (usedPixels.Count % 2 == 0)
						{
							Point randomPixel = rps.SelectRandomPixel();
							byte pixelValue = expertMask[randomPixel.Y][randomPixel.X];
							if (expertMask[randomPixel.Y][randomPixel.X] > 175)
							{
								if (!usedPixels.Contains(randomPixel))
								{
									usedPixels.Add(randomPixel);
									SVMFeatures svmFeatures = vs.CalculateSVMInputVectorPerPixel(randomPixel);
									sw.WriteLine($"+1 1:{svmFeatures.PixelPowerOfMainLine} 2:{svmFeatures.PixelPowerOfSmallLine} 3:{svmFeatures.PixelGrayLevel}");
									Console.Write(".");
								}
							}
						}
						//No vessel
						while (usedPixels.Count % 2 == 1)
						{
							Point randomPixel = rps.SelectRandomPixel();
							byte pixelValue = expertMask[randomPixel.Y][randomPixel.X];
							if (expertMask[randomPixel.Y][randomPixel.X] <= 175)
							{
								if (!usedPixels.Contains(randomPixel))
								{
									usedPixels.Add(randomPixel);
									SVMFeatures svmFeatures = vs.CalculateSVMInputVectorPerPixel(randomPixel);
									sw.WriteLine($"-1 1:{svmFeatures.PixelPowerOfMainLine} 2:{svmFeatures.PixelPowerOfSmallLine} 3:{svmFeatures.PixelGrayLevel}");
									Console.Write(".");
								}
							}
						}
					}
				}
				Console.Write("\n");
			}
			sw.Close();
			Console.WriteLine("FINISHED CREATING DATASET!!!");
		}
	}
}
