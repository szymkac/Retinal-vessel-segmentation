using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RetinalVessel
{
	/// <summary>
	/// Static class containing methods to save and convert byte jagged array to Bitmap
	/// </summary>
	public static class BitmapWriter
	{
		/// <summary>
		/// Save byte jagged array as gray scale image
		/// </summary>
		/// <param name="data"></param>
		/// <param name="name"></param>
		public static void Save(byte[][] data, string name)
		{
			GetBitmap(data).Save(name);
		}

		/// <summary>
		/// Create bitmap image from byte jagged array
		/// </summary>
		/// <param name="data">Image byte data</param>
		/// <returns>Created bitmap</returns>
		public static Bitmap GetBitmap(byte[][] data)
		{
			int width = data[0].Length;
			int height = data.Length;
			int byteIndex = 0;
			byte[] dataBytes = new byte[height * width];
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					dataBytes[byteIndex] = (byte)(((uint)data[y][x]) & 0xFF);
					byteIndex++;
				}
			}
			Color[] palette = new Color[256];
			for (int b = 0; b < 256; b++)
				palette[b] = Color.FromArgb(b, b, b);


			return BuildImage(dataBytes, width, height, width, PixelFormat.Format8bppIndexed, palette, null);
		}

		/// <summary>
		/// Creates a bitmap based on data, width, height, stride and pixel format.
		/// </summary>
		/// <param name="sourceData">Byte array of raw source data</param>
		/// <param name="width">Width of the image</param>
		/// <param name="height">Height of the image</param>
		/// <param name="stride">Scanline length inside the data</param>
		/// <param name="pixelFormat">Pixel format</param>
		/// <param name="palette">Color palette</param>
		/// <param name="defaultColor">Default color to fill in on the palette if the given colors don't fully fill it.</param>
		/// <see cref="https://stackoverflow.com/questions/2185944/why-must-stride-in-the-system-drawing-bitmap-constructor-be-a-multiple-of-4/43967594#43967594"/>
		/// <returns>The new image</returns>
		private static Bitmap BuildImage(byte[] sourceData, int width, int height, int stride, PixelFormat pixelFormat, Color[] palette, Color? defaultColor)
		{
			Bitmap newImage = new Bitmap(width, height, pixelFormat);
			BitmapData targetData = newImage.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, newImage.PixelFormat);
			int newDataWidth = ((Image.GetPixelFormatSize(pixelFormat) * width) + 7) / 8;
			// Compensate for possible negative stride on BMP format.
			Boolean isFlipped = stride < 0;
			stride = Math.Abs(stride);
			// Cache these to avoid unnecessary getter calls.
			int targetStride = targetData.Stride;
			long scan0 = targetData.Scan0.ToInt64();
			for (int y = 0; y < height; y++)
				Marshal.Copy(sourceData, y * stride, new IntPtr(scan0 + y * targetStride), newDataWidth);
			newImage.UnlockBits(targetData);
			// Fix negative stride on BMP format.
			if (isFlipped)
				newImage.RotateFlip(RotateFlipType.Rotate180FlipX);
			// For indexed images, set the palette.
			if ((pixelFormat & PixelFormat.Indexed) != 0 && palette != null)
			{
				ColorPalette pal = newImage.Palette;
				for (int i = 0; i < pal.Entries.Length; i++)
				{
					if (i < palette.Length)
						pal.Entries[i] = palette[i];
					else if (defaultColor.HasValue)
						pal.Entries[i] = defaultColor.Value;
					else
						break;
				}
				newImage.Palette = pal;
			}
			return newImage;
		}
	}
}
