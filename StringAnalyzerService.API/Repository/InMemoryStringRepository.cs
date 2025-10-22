namespace StringAnalyzerAPI.Repository;
using global::StringAnalyzerAPI.Model;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;


public static class InMemoryStringRepository
{
    // ConcurrentDictionary for thread-safety
    private static readonly ConcurrentDictionary<string, StringModel> _store = new();

    public static bool Add(StringModel model)
    {
        // Add only if not exists
        return _store.TryAdd(model.Id, model);
    }

    public static StringModel? GetByHash(string hash)
        => _store.TryGetValue(hash, out var v) ? v : null;

    public static StringModel? GetByValue(string value)
        => _store.Values.FirstOrDefault(x => x.Value == value);

    public static IEnumerable<StringModel> GetAll() => _store.Values;

    public static bool DeleteByValue(string value)
    {
        var existing = GetByValue(value);
        if (existing == null) return false;
        return _store.TryRemove(existing.Id, out _);
    }

    public static bool ExistsHash(string hash) => _store.ContainsKey(hash);
}
