using System.Drawing;

namespace RetinalVessel
{
	/// <summary>
	/// Represent pair of point
	/// </summary>
	public class PointsPair
	{
		/// <summary>
		/// First point
		/// </summary>
		public Point startPoint;

		/// <summary>
		/// Second point
		/// </summary>
		public Point endPoint;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="startPoint">First point</param>
		/// <param name="endPoint">Second point</param>
		public PointsPair(Point startPoint, Point endPoint)
		{
			this.startPoint = startPoint;
			this.endPoint = endPoint;
		}
	}
}