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
	class Program
	{
		static void Main(string[] args)
		{
			string imagesPath = @"D:\all-images\jpg-experimental";
			string expert1Path = @"D:\all-images\jpg-expert1";
			string expert2Path = @"D:\all-images\jpg-expert2";
			string resultPath = "svmData.csv";

			SVMDatasetCreator svmDatasetCreator = new SVMDatasetCreator(imagesPath, new List<string> { expert1Path, expert2Path }, resultPath);
			svmDatasetCreator.CreateDataset();

			Console.ReadKey();
		}
	}
}
