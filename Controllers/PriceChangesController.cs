using CryptoView.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoView.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PriceChangesController : ControllerBase
	{
		public PriceService PriceService { get; set; }
		public PriceChangesController() 
		{
			PriceService = new PriceService();
		}

		[HttpGet]
		public async Task<IActionResult> GetPriceForecast(string id) 
		{
			return Ok(await PriceService.GetPriceForecast(id));
		}
	}
}
