namespace PickMeUp.Core.Common.Models;

public class EditEntityParams<T> : ParamsBase where T : class
{
    /// <summary>
    /// Entity to be edited.
    /// </summary>
    public T Entity { get; set; } = default!;
}
