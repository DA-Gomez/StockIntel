using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using StockIntel.Application.Abstractions.Filings;
using StockIntel.Infrastructure.Filings;

namespace Microsoft.Extensions.DependencyInjection;

public static class EdgarServiceCollectionExtensions
{
  public static IServiceCollection AddEdgar(this IServiceCollection services)
  {
    services.AddOptions<EdgarOptions>()
      .BindConfiguration(EdgarOptions.SectionName)
      .ValidateDataAnnotations()
      .ValidateOnStart();

    services.AddSingleton<EdgarRateLimiter>();
    services.AddTransient<RateLimitingHandler>();

    //named client. used by the singleton CompanyDirectory via IHttpClientFactory
    var directory = services.AddHttpClient(EdgarHttp.DirectoryClientName)
      .ConfigureHttpClient(ApplyEdgarDefualts)
      .ConfigurePrimaryHttpMessageHandler(CreatePrimaryHandler);
    directory.AddStandardResilienceHandler();
    directory.AddHttpMessageHandler<RateLimitingHandler>();

    //typed client. stateless (safe as a transient)
    var filings = services.AddHttpClient<IInsiderFilingSource, EdgarInsiderFilingSource>()
      .ConfigureHttpClient(ApplyEdgarDefualts)
      .ConfigurePrimaryHttpMessageHandler(CreatePrimaryHandler);
    filings.AddStandardResilienceHandler();
    filings.AddHttpMessageHandler<RateLimitingHandler>();

    services.AddSingleton<ICompanyDirectory, CompanyDirectory>();
    return services;
  }

  private static void ApplyEdgarDefualts(IServiceProvider sp, HttpClient client)
  {
    var options = sp.GetRequiredService<IOptions<EdgarOptions>>().Value;

    //TryAddWithoutValidation: the SEC's "Name (email)" shape trips .NETs strict User-Agent grammar validation. we want the exact string sent
    client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", options.UserAgent);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
  }

  private static HttpMessageHandler CreatePrimaryHandler() => new SocketsHttpHandler
  {
    //edgar gzips its own json when asked. submissions are around 1mb
    AutomaticDecompression = DecompressionMethods.All
  };
}

internal static class EdgarHttp
{
  public const string DirectoryClientName = "edgar-directory";
  public const string CompanyTickersUrl = "https://www.sec.gov/files/company_tickers.json";
  public static string SubmissionsUrl(string cik) => $"https://data.sec.gov/submissions/CIK{cik}.json";
}