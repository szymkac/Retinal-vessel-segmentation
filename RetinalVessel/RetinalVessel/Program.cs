using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RetinalVessel
{
	class Program
	{
		static void Main(string[] args)
		{
			VesselSegmentator vs = new VesselSegmentator();
			vs.SetInput(new Bitmap(@"C:\Users\szyme\Downloads\im0001.png"));

			BitmapWriter.Save(vs.GetReversedCanalPixels(), @"C:\Users\szyme\Downloads\im0002.png");
			BitmapWriter.Save(vs.CanalPixels, @"C:\Users\szyme\Downloads\im0003.png");

			vs.Calculate();
			var svmFeatures = vs.SVMFeaturesMatrix;
			BitmapWriter.Save(vs.Result, @"C:\Users\szyme\Downloads\im0004.png");
			Console.ReadKey();
		}	
	}
}