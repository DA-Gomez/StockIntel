using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace StockIntel.Api.IntegrationTests.Users;

[Collection("Postgres")]//tells xUnit this test class belongs to the Postgres collection, share the fixture.
public class RegisterEndpointTests
{
  private readonly StockIntelApiFactory _factory;
  private readonly HttpClient _client;

  public RegisterEndpointTests(PostgresFixture postgres)
  {
    _factory = new StockIntelApiFactory(postgres.ConnectionString);
    _client = _factory.CreateClient();
  }

  [Fact]
  public async Task Register_WithValidInput_Returns201()
  {
    var email = $"test-{Guid.NewGuid()}@example.com";
    var request = new { email, password = "password123" };

    var res = await _client.PostAsJsonAsync("/api/users/register", request);
    // var body1 = await res.Content.ReadAsStringAsync();
    // res.StatusCode.Should().Be(HttpStatusCode.Created, body1); // body prints on failure

    res.StatusCode.Should().Be(HttpStatusCode.Created);
    var body = await res.Content.ReadFromJsonAsync<RegisterResponseDto>();
    body!.Email.Should().Be(email);
    body.UserId.Should().NotBeEmpty();
  }

  [Fact]
  public async Task Register_WithDuplicateEmail_Returns409()
  {
    var email = $"duplicate-{Guid.NewGuid()}@example.com";
    var request = new { email, password = "password123" };

    await _client.PostAsJsonAsync("/api/users/register", request);
    var res = await _client.PostAsJsonAsync("/api/users/register", request);
    // var body = await res.Content.ReadAsStringAsync();
    // res.StatusCode.Should().Be(HttpStatusCode.Created, body); // body prints on failure

    res.StatusCode.Should().Be(HttpStatusCode.Conflict);
  }

  [Fact]
  public async Task Register_WithInvalidEmail_Returns400()
  {
    var request = new { email = "invalid-email", password = "password123" };

    var res = await _client.PostAsJsonAsync("/api/users/register", request);

    res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
  }

  private record RegisterResponseDto(Guid UserId, string Email);
}

//unique email per request so there is no interference