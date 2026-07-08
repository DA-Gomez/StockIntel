using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockIntel.Application.Common;
using StockIntel.Application.Filings.InsiderActivity;
using StockIntel.Application.Watchlists.AddTicker;
using StockIntel.Application.Watchlists.Create;
using StockIntel.Application.Watchlists.List;

namespace StockIntel.Api.Controllers;

[ApiController]
[Route("api/watchlists")]
[Authorize]//every endpoint in this controller needs a valid jwt
public class WatchlistController : ControllerBase
{
  private readonly ICommandHandler<CreateWatchlistCommand, CreateWatchlistResponse> _createHandler;
  private readonly ICommandHandler<AddTickerCommand, Unit> _addTickerHandler;
  private readonly IQueryHandler<ListWatchlistsQuery, ListWatchlistsResponse> _listHandler;
  private readonly IQueryHandler<GetWatchlistInsiderActivityQuery, InsiderActivityPage> _getInsiderActivity;


  public WatchlistController(
    ICommandHandler<CreateWatchlistCommand, CreateWatchlistResponse> createHandler,
    ICommandHandler<AddTickerCommand, Unit> addTickerHandler,
    IQueryHandler<ListWatchlistsQuery, ListWatchlistsResponse> listHandler,
    IQueryHandler<GetWatchlistInsiderActivityQuery, InsiderActivityPage> getInsiderActivity)
  {
    _createHandler = createHandler;
    _addTickerHandler = addTickerHandler;
    _listHandler = listHandler;
    _getInsiderActivity = getInsiderActivity;
  }

  [HttpPost]
  [ProducesResponseType(StatusCodes.Status201Created)]
  public async Task<IActionResult> Create([FromBody] CreateWatchlistRequest request, CancellationToken cancellationToken)
  {
    var res = await _createHandler.HandleAsync(new CreateWatchlistCommand(request.Name), cancellationToken);

    return CreatedAtAction(nameof(List), null, res);
  }

  [HttpGet]
  [ProducesResponseType(StatusCodes.Status200OK)]
  public async Task<IActionResult> List(CancellationToken cancellationToken)
  {
    var res = await _listHandler.HandleAsync(new ListWatchlistsQuery(), cancellationToken);

    return Ok(res);
  }

  [HttpPost("{watchlistId:guid}/tickers")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status403Forbidden)]
  public async Task<IActionResult> AddTicker(Guid watchlistId, [FromBody] AddTickerRequest request, CancellationToken cancellationToken)
  {
    try
    {
      await _addTickerHandler.HandleAsync(new AddTickerCommand(watchlistId, request.Symbol), cancellationToken);
      return NoContent();
    }
    catch (KeyNotFoundException)
    {
      return NotFound();
    }
    catch (UnauthorizedAccessException)
    {
      return Forbid();
    }
    catch (ArgumentException e)
    {
      return BadRequest(new { error = e.Message });
    }
  }

  [HttpGet("{id:guid}/insider-activity")]
public async Task<IActionResult> GetInsiderActivity(
    Guid id,
    [FromQuery] string? cursor,
    [FromQuery] int? pageSize,
    CancellationToken cancellationToken)
{
    try
    {
      var page = await _getInsiderActivity.HandleAsync(new GetWatchlistInsiderActivityQuery(id, cursor, pageSize), cancellationToken);
      return Ok(page);
    }
    catch (ArgumentException ex) { return BadRequest(new { error = ex.Message }); }
    catch (KeyNotFoundException ex) { return NotFound(new { error = ex.Message }); }
    catch (UnauthorizedAccessException ex) { return StatusCode(403, new { error = ex.Message }); }
}
}

public record CreateWatchlistRequest(string Name);
public record AddTickerRequest(string Symbol);