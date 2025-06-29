using System.Linq.Dynamic.Core;

namespace StaffHabit.Api.Services.Sorting;

public sealed class SortMappingProvider(IEnumerable<ISortMappingDefinition> sortMappingDefinitions)
{
    public SortMapping[] GetMappings<TSource, TDestination>()
    {
        SortMappingDefinition<TSource, TDestination>? sortMappingDefinition = sortMappingDefinitions
            .OfType<SortMappingDefinition<TSource, TDestination>>().FirstOrDefault();

        if (sortMappingDefinition is null)
        {
            throw new InvalidOperationException($"The mapping from '{typeof(TSource).Name}' into '{typeof(TDestination).Name} isn't defined");
        }
        return sortMappingDefinition.Mappings;
    } 
}


