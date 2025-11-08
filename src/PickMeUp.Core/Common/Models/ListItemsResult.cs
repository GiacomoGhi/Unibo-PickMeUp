namespace PickMeUp.Core.Common.Models;

public class ListItemsResult<T> where T : class
{
    /// <summary>
    /// Collection of items for the current page.
    /// </summary>
    public T[] Items { get; set; } = [];

    /// <summary>
    /// Total count of all items (for calculating if more items exist).
    /// </summary>
    public int TotalCount { get; set; }
}
