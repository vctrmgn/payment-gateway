namespace PaymentGateway.SharedKernel.Util;

public class PaginatedList<T>
{
    public int Limit { get; }
    public int Skip { get;  }
    public int TotalItems { get; }
    public IReadOnlyList<T> PageItems { get; }
    
    public PaginatedList(IReadOnlyList<T> pageItems, int limit, int skip, int totalItems)
    {
        PageItems = pageItems;
        Skip = skip;
        Limit = limit;
        TotalItems = totalItems;
    }
}