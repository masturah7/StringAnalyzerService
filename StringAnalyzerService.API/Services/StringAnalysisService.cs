using StringAnalyzerAPI.Model;
using System.Security.Cryptography;
using System.Text;

namespace StringAnalyzerService.API.Services
{
    public class StringAnalysisService
    {
        public StringModel Analyze(string value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            // Do not trim away spaces because length & character map should include them as per spec.
            var sha = ComputeSha256(value);
            var properties = new StringProperties
            {
                Length = value.Length,
                IsPalindrome = IsPalindrome(value),
                UniqueCharacters = value.Distinct().Count(),
                WordCount = CountWords(value),
                Sha256Hash = sha,
                CharacterFrequencyMap = BuildCharFrequencyMap(value)
            };

            return new StringModel
            {
                Id = sha,
                Value = value,
                Properties = properties,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static string ComputeSha256(string raw)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }

        private static bool IsPalindrome(string input)
        {
            // Case-insensitive. Keep characters (so "A man a plan" is NOT a palindrome unless you asked to ignore spaces).
            // Spec says case-insensitive; it does NOT explicitly say to ignore non-letters, so we only ignore case.
            var lower = input.ToLowerInvariant();
            return lower.SequenceEqual(lower.Reverse());
        }

        private static int CountWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0;
            var parts = input.Split((char[])null, StringSplitOptions.RemoveEmptyEntries); // whitespace split
            return parts.Length;
        }

        private static Dictionary<string, int> BuildCharFrequencyMap(string input)
        {
            // Use string keys to ensure JSON has characters as keys (including whitespace)
            var dict = new Dictionary<string, int>();
            foreach (var ch in input)
            {
                var key = ch.ToString();
                if (dict.ContainsKey(key)) dict[key] += 1;
                else dict[key] = 1;
            }
            return dict;
        }
    }
}
