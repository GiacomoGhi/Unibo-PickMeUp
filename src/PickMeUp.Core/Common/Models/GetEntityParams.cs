namespace PickMeUp.Core.Common.Models;

public class GetEntityParams<T> : ParamsBase where T : struct
{
    /// <summary>
    /// Entity to get identifier.
    /// </summary>
    public T EntityId { get; set; }

    /// <summary>
    /// Mask used to specify additional data to include with the entity.
    /// </summary>
    public int? AdditionalEntityDataMask { get; set; }
}