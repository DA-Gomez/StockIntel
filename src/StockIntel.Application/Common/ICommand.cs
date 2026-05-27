
namespace StockIntel.Application.Common;

//TResponse -> generic type placeholder to represent a dynamic response object
public interface ICommand<TResponse> { }

public interface ICommand : ICommand<Unit> { }

//"Unit" -> placeholder for "no meaningful return value" 
public readonly record struct Unit
{
  public static readonly Unit Value = new();
}