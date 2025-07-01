using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;

namespace StaffHabit.Api.Services;

public class DataShapingService
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertiesCache = new();

    public ExpandoObject ShapeData<T>(T entity, string? fields)
    {
        HashSet<string> fieldsSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        // Get the properties of the type T using reflection and cache them
        PropertyInfo[] propertyInfos = PropertiesCache
            .GetOrAdd(typeof(T), t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        // Filter the properties based on the provided fields
        if (fieldsSet.Any())
        {
            propertyInfos = [.. propertyInfos
                .Where(p => fieldsSet
                .Contains(p.Name, StringComparer.OrdinalIgnoreCase))];
        }

        IDictionary<string, object?> shapedObject = new ExpandoObject();
        foreach (var propertyInfo in propertyInfos)
        {
            shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
        }
        return (ExpandoObject)shapedObject;
    }

    public List<ExpandoObject> ShapeCollectionData<T>(IEnumerable<T> entities, string? fields)
    {
        HashSet<string> fieldsSet = fields?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];

        // Get the properties of the type T using reflection and cache them
        PropertyInfo[] propertyInfos = PropertiesCache
            .GetOrAdd(typeof(T), t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        // Filter the properties based on the provided fields
        if (fieldsSet.Any())
        {
            propertyInfos = [.. propertyInfos.Where(p => fieldsSet.Contains(p.Name, StringComparer.OrdinalIgnoreCase)) ];
        }
        List<ExpandoObject> shapedObjects = [];
        foreach (var entity in entities)
        { 
            IDictionary<string, object?> shapedObject = new ExpandoObject();
            foreach (var propertyInfo in propertyInfos)
            {
                shapedObject[propertyInfo.Name] = propertyInfo.GetValue(entity);
            }
            shapedObjects.Add((ExpandoObject)shapedObject);
        }
        return shapedObjects;
    }

    public bool Validate<T>(string? fields)
    {
        if (string.IsNullOrWhiteSpace(fields))
        {
            return true;
        }
        var fieldsSet = fields
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Get the properties of the type T using reflection and cache them
        PropertyInfo[] propertyInfos = PropertiesCache
            .GetOrAdd(typeof(T), t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));

        return fieldsSet.All(f => propertyInfos.Any(propa => propa.Name.Equals(f, StringComparison.OrdinalIgnoreCase)));
    }
}
