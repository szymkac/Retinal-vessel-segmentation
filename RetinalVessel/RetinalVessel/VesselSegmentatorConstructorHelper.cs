using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RetinalVessel
{
	public class VesselSegmentatorConstructorHelper
	{
		public int? windowRadius { get; set; }
		public int? smallLineLenght { get; set; }
		public double? Threshold { get; set; }
		public VesselSegmentatioMethod? VesselSegmentatioMethodType { get; set; }
	}
}
