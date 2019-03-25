namespace RetinalVessel
{
	/// <summary>
	/// Propably temporaty class for store data for svm
	/// </summary>
	public class SVMFeatures
	{
		/// <summary>
		/// Difference between average gray scale level of main filter line and average gray scale of window
		/// </summary>
		public double PixelPowerOfMainLine;

		/// <summary>
		/// Difference between average gray scale level of line small filter line and average gray scale of window
		/// </summary>
		public double PixelPowerOfSmallLine;

		/// <summary>
		/// Gray scale value of pixel
		/// </summary>
		public byte PixelGrayLevel;
	}
}
