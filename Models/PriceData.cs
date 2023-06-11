using Microsoft.ML.Data;

namespace Models
{
	public class PriceData
	{
		[LoadColumn(2)]
		public DateTime Date { get; set; }

		[LoadColumn(0)]
		public float PriceUsd { get; set; }
	}
}
