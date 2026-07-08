using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockIntel.Application.Common;
using StockIntel.Application.Filings.InsiderActivity;

namespace StockIntel.Api.Controllers;

[ApiController]
[Route("api/tickers")]
[Authorize]
public class TickersController : ControllerBase
{
  private readonly IQueryHandler<GetTickerInsiderActivityQuery, InsiderActivityPage> _getActivity;

  public TickersController(IQueryHandler<GetTickerInsiderActivityQuery, InsiderActivityPage> getActivity)
    => _getActivity = getActivity;

  [HttpGet("{symbol}/insider-activity")]
  public async Task<IActionResult> GetInsiderActivity(
    string symbol,
    [FromQuery] string? cursor,
    [FromQuery] int? pageSize,
    CancellationToken cancellationToken)
  {
    try
    {
      var page = await _getActivity.HandleAsync(new GetTickerInsiderActivityQuery(symbol, cursor, pageSize), cancellationToken);
      return Ok(page);
    }
    catch (ArgumentException ex)
    {
      return BadRequest(new { error = ex.Message });
    }
  }
}