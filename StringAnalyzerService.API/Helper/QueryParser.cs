using StringAnalyzerAPI.Model;

namespace StringAnalyzerAPI.Helpers;


public static class QueryParser
{
    public static ParsedQuery Parse(string query)
    {
        query = query.ToLowerInvariant();
        var parsed = new ParsedQuery();

        if (query.Contains("palindromic"))
            parsed.IsPalindrome = true;

        if (query.Contains("single word"))
            parsed.WordCount = 1;

        if (query.Contains("longer than"))
        {
            // Extract number (e.g., "longer than 10 characters")
            var match = System.Text.RegularExpressions.Regex.Match(query, @"longer than (\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int num))
                parsed.MinLength = num + 1;
        }

        if (query.Contains("containing the letter"))
        {
            // e.g., "strings containing the letter z"
            var match = System.Text.RegularExpressions.Regex.Match(query, @"letter (\w)");
            if (match.Success)
                parsed.ContainsCharacter = match.Groups[1].Value;
        }

        return parsed;
    }
}

