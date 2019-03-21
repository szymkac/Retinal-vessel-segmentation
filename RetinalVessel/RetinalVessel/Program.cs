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
			vs.SetInput(new Bitmap(@"C:\Users\szyme\Downloads\im0001.png"), true);

			BitmapWriter.Save(vs.GetReversedCanalPixels(), @"C:\Users\szyme\Downloads\green_canal.png");
			BitmapWriter.Save(vs.CanalPixels, @"C:\Users\szyme\Downloads\inverted_green_canal.png");

			vs.Calculate();

			BitmapWriter.Save(vs.Result, @"C:\Users\szyme\Downloads\result.png");
			Console.ReadKey();
		}	
	}
}