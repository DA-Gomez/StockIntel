using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using StockIntel.Application.Abstractions.Filings;

namespace StockIntel.Infrastructure.UnitTests.Filings;

public class EdgarHttpTests
{
  [Fact]
  public void Builds_archives_url_with_all_three_transformations()
  {
    var reference = new FilingReference("0000320193", "0001214156-26-000043", new DateOnly(2026, 4, 3), "xslF345X05/wk-form4_1.xml");

    EdgarHttp.FilingDocumentUrl(reference).Should().Be("https://www.sec.gov/Archives/edgar/data/320193/000121415626000043/wk-form4_1.xml");
  }

  [Theory]
  [InlineData("xslF345X05/wk-form4_1.xml", "wk-form4_1.xml")]
  [InlineData("wk-form4_1.xml", "wk-form4_1.xml")]// no prefix
  [InlineData("xslF345X03/primary_doc.xml", "primary_doc.xml")]
  public void Strips_only_the_stylesheet_prefix(string input, string expected) =>
    EdgarHttp.StripRenderedPrefix(input).Should().Be(expected);
}