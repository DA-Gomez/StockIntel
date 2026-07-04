using System.Net;
using FluentAssertions;
using StockIntel.Infrastructure.Filings;
using StockIntel.Infrastructure.UnitTests.TestDoubles;

namespace StockIntel.Infrastructure.UnitTests.Filings;

public class EdgarInsiderFilingSourceTests
{
  private const string SubmissionsJson = """
    {"filings":{"recent":{
        "accessionNumber":["0000320193-26-000006","0001214156-26-000043","0001214156-26-000050","000121415626000061"],
        "filingDate":["2026-01-30","2026-01-28","2026-01-20","2026-01-15"],
        "form":["10-Q","4","4/A","4"],
        "primaryDocument":["aapl-q1.htm","xslF345X05/wk-form4_1.xml","xslF345X05/wk-form4_2.xml","wk-form4_3.xml"]
    }}}
    """;

  private static EdgarInsiderFilingSource CreateSut(StubHttpMessageHandler stub) => new(new HttpClient(stub));

  [Fact]
  public async Task Zips_columnar_arrays_and_keeps_only_form_4()
  {
    var sut = CreateSut(StubHttpMessageHandler.RespondingWithJson(SubmissionsJson));

    var refs = await sut.GetRecentForm4ReferencesAsync("320193", CancellationToken.None);

    refs.Should().HaveCount(2);
    refs[0].AccessionNumber.Should().Be("0001214156-26-000043");
    refs[0].FilingDate.Should().Be(new DateOnly(2026, 1, 28));
    refs[1].AccessionNumber.Should().Be("0001214156-26-000061");  // undashed input, normalized out
  }

  [Fact]
  public async Task Unknown_cik_returns_empty_not_an_exception()
  {
    var stub = new StubHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
    var sut = CreateSut(stub);

    var refs = await sut.GetRecentForm4ReferencesAsync("999999999", CancellationToken.None);

    refs.Should().BeEmpty();
  }
}