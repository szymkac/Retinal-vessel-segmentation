using System;
using System.Collections.Generic;

namespace PrepareSVMLearningDataSet
{
	class Program
	{
		static void Main(string[] args)
		{
			string imagesPath = @"D:\all-images\jpg-experimental";
			string expert1Path = @"D:\all-images\jpg-expert1";
			string expert2Path = @"D:\all-images\jpg-expert2";
			string resultPath = "svmTraining.csv";

			SVMDatasetCreator svmDatasetCreator = new SVMDatasetCreator(imagesPath, new List<string> { expert1Path, expert2Path }, resultPath)
			{
				PathToTestingResultFile = @"svmTesting.csv"
			};

			svmDatasetCreator.CreateDataset();

			Console.ReadKey();
		}
	}
}
