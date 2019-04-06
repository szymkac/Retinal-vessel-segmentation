using RetinalVessel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace PrepareSVMLearningDataSet
{
	public class SVMDatasetCreator
	{
		static readonly Random rnd = new Random();

		string pathToImagesFolder;

		string pathToTrainingResultFile;

		List<string> pathsToExpertImages;

		readonly VesselSegmentator vs;

		public int PixelsToGetPerImage { get; set; }

		public string PathToTestingResultFile { get; set; }

		List<Point> usedPixels;

		RandomPixelSelector rps;

		public SVMDatasetCreator(string pathToImagesFolder, List<string> pathsToExpertImages, string pathToTrainingResultFile)
		{
			Init(pathToImagesFolder, pathsToExpertImages, pathToTrainingResultFile);
			vs = new VesselSegmentator();
		}

		public SVMDatasetCreator(string pathToImagesFolder, List<string> pathsToExpertImages, string pathToTrainingResultFile, VesselSegmentatorConstructorHelper helper)
		{
			Init(pathToImagesFolder, pathsToExpertImages, pathToTrainingResultFile);
			vs = new VesselSegmentator(helper);
		}

		private void Init(string pathToImagesFolder, List<string> pathsToExpertImages, string pathToTrainingResultFile)
		{
			this.pathToImagesFolder = pathToImagesFolder;
			this.pathsToExpertImages = pathsToExpertImages;
			this.pathToTrainingResultFile = pathToTrainingResultFile;
			PixelsToGetPerImage = 30;
		}

		public void CreateDataset()
		{
			StreamWriter trainingSW = new StreamWriter(pathToTrainingResultFile);
			StreamWriter testingSW = string.IsNullOrEmpty(PathToTestingResultFile) ? null : new StreamWriter(PathToTestingResultFile);
			FileInfo[] images = new DirectoryInfo(pathToImagesFolder).GetFiles("*.jpg");
			int amountOfResultsFiles = string.IsNullOrEmpty(PathToTestingResultFile) ? 1 : 2;


			foreach (FileInfo image in images)
			{
				usedPixels = new List<Point>();
				Console.Write($"{image.Name,7}): ");

				for (int writerNumber = 1; writerNumber <= amountOfResultsFiles; writerNumber++)
				{
					StreamWriter sw;
					if (writerNumber == 2)
					{
						sw = testingSW;
						Console.Write("\n\t TESTING SET: ");
					}
					else
					{
						sw = trainingSW;
						Console.Write("\n\tTRAINING SET: ");
					}

					for (int expertNumber = 0; expertNumber < pathsToExpertImages.Count; expertNumber++)
					{
						Console.Write($"\n\t\tEXPERT-{expertNumber}: ");
						byte[][] expertMask = SetVSInputForImage(image.Name, pathsToExpertImages[expertNumber]);

						int usedPixelAmountLimiter = usedPixels.Count + (PixelsToGetPerImage * 2);
						while (usedPixels.Count < usedPixelAmountLimiter)
						{
							while (usedPixels.Count % 2 == 0)
								AddPointForDataSet(p => expertMask[p.Y][p.X] > 175, sw);

							while (usedPixels.Count % 2 == 1)
								AddPointForDataSet(p => expertMask[p.Y][p.X] <= 175, sw, false);
						}
					}
				}
				Console.Write("\n");
			}
			trainingSW.Close();
			if (testingSW != null)
				testingSW.Close();

			Console.WriteLine("\nFINISHED CREATING DATASET!!!");
		}

		private byte[][] SetVSInputForImage(string imgName, string expertFolderPath)
		{
			vs.SetInput($@"{expertFolderPath}\{imgName}", false);
			byte[][] expertMask = vs.CanalPixels;
			vs.SetInput($@"{pathToImagesFolder}\{imgName}");
			rps = new RandomPixelSelector(vs.Width, vs.Height, vs.WindowRadius, rnd.Next(int.MaxValue));

			return expertMask;
		}

		private void AddPointForDataSet(Func<Point, bool> condition, StreamWriter writer, bool datasetValue = true)
		{
			Point randomPixel = rps.SelectRandomPixel();

			if (condition(randomPixel))
			{
				if (!usedPixels.Contains(randomPixel))
				{
					usedPixels.Add(randomPixel);
					SVMFeatures svmFeatures = vs.CalculateSVMInputVectorPerPixel(randomPixel);
					writer.WriteLine($"{(datasetValue ? "+1" : "-1")} 1:{svmFeatures.PixelPowerOfMainLine} 2:{svmFeatures.PixelPowerOfSmallLine} 3:{svmFeatures.PixelGrayLevel}");
					Console.Write(".");
				}
			}
		}
	}
}
