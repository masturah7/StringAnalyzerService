


using StringAnalyzerAPI.Model;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace StringAnalyzerAPI.Repository;
public static class StringRepository
{
    private static readonly List<StringModel> _storage = new();

    // Add a new analyzed string
    public static void Add(StringModel model)
    {
        _storage.Add(model);
    }

    // Get all stored strings
    public static IEnumerable<StringModel> GetAll()
    {
        return _storage;
    }

    // Get a string by its value (case-insensitive)
    public static StringModel? GetByValue(string value)
    {
        return _storage.FirstOrDefault(x =>
            string.Equals(x.Value, value, StringComparison.OrdinalIgnoreCase));
    }

    // Check if a string already exists (by its hash)
    public static bool Exists(string sha256Hash)
    {
        return _storage.Any(x => x.Properties.Sha256Hash == sha256Hash);
    }

    // Delete a string by its value
    public static bool Delete(string value)
    {
        var item = GetByValue(value);
        if (item == null) return false;

        _storage.Remove(item);
        return true;
    }
}


