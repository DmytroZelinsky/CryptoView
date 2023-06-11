using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using Microsoft.VisualBasic;
using Models;
using Newtonsoft.Json;
using SuperConvert.Extensions;
using System.Data;
using System.Net.Http.Json;
using System.Xml;

namespace CryptoView.Services
{
	public class PriceService
	{
		public async Task<List<float>> GetPriceForecast(string id)
		{
			var context = new MLContext();

			var client = new HttpClient();

			var responseString = await client.GetStringAsync("https://api.coincap.io/v2/assets/" + id + "/history?interval=d1");

			var jsonDynamic = JsonConvert.DeserializeObject<dynamic>(responseString)!;

			for(int i = 0; i < jsonDynamic.data.Count; ++i)
			{
				jsonDynamic.data[i].priceUsd = float.Parse(jsonDynamic.data[i].priceUsd.ToString(), System.Globalization.CultureInfo.InvariantCulture);
			}
			string jsonString = jsonDynamic.ToString();

			XmlNode xml = JsonConvert.DeserializeXmlNode("{records:{record:" + jsonString + "}}");
			XmlDocument xmldoc = new XmlDocument();
			xmldoc.LoadXml(xml.InnerXml);
			XmlReader xmlReader = new XmlNodeReader(xml);
			DataSet dataSet = new DataSet();
			dataSet.ReadXml(xmlReader);
			var dataTable = dataSet.Tables[1];

			//Datatable to CSV
			var lines = new List<string>();
			string[] columnNames = dataTable.Columns.Cast<DataColumn>().
											  Select(column => column.ColumnName).
											  ToArray();
			var header = string.Join(",", columnNames);
			lines.Add(header);
			var valueLines = dataTable.AsEnumerable()
							   .Select(row => string.Join(",", row.ItemArray));
			lines.AddRange(valueLines);

			var filePath = AppDomain.CurrentDomain.BaseDirectory + "priceData.csv";

			File.WriteAllLines(filePath, lines);

			var data = context.Data.LoadFromTextFile<PriceData>(filePath,
				hasHeader: true, separatorChar: ',');

			var pipeline = context.Forecasting.ForecastBySsa(
				nameof(PriceForecast.Forecast),
				nameof(PriceData.PriceUsd),
				windowSize: 5,
				seriesLength: 10,
				trainSize: 1800,
				horizon: 8);

			var model = pipeline.Fit(data);

			var forecastingEngine = model.CreateTimeSeriesEngine<PriceData, PriceForecast>(context);

			var forecasts = forecastingEngine.Predict();

			return forecasts.Forecast.ToList();
		}
	}
}
