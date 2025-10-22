using Microsoft.AspNetCore.Mvc;
using StringAnalyzerAPI.Helpers;
using StringAnalyzerAPI.Model;
using StringAnalyzerAPI.Repository;
using StringAnalyzerService.API.DTO;
using StringAnalyzerService.API.Services;


namespace StringAnalyzerAPI.Controllers
{
    [ApiController]
    [Route("strings")]
    public class StringsController : ControllerBase
    {
        private readonly StringAnalysisService _analyzer;

        public StringsController(StringAnalysisService analyzer)
        {
            _analyzer = analyzer;
        }

        // 1) Create / Analyze
        [HttpPost]
        public IActionResult Create([FromBody] CreateStringRequestDto? request)
        {
            if (request is null)
                return BadRequest(new { error = "Invalid request body or missing 'value' field." });

            if (request.Value is null)
                return BadRequest(new { error = "Missing 'value' field." });

            if (request.Value.GetType() != typeof(string))
                return UnprocessableEntity(new { error = "Invalid data type for 'value' (must be string)." });

            var model = _analyzer.Analyze(request.Value);

            if (!InMemoryStringRepository.Add(model))
                return Conflict(new { error = "String already exists in the system." });

            // Return 201 with body matching spec
            return CreatedAtAction(nameof(GetByValue), new { string_value = model.Value }, ShapeResponse(model));
        }

        // 2) Get Specific String by string_value
        [HttpGet("{string_value}")]
        public IActionResult GetByValue(string string_value)
        {
            var model = InMemoryStringRepository.GetByValue(string_value);
            if (model == null)
                return NotFound(new { error = "String does not exist in the system." });

            return Ok(ShapeResponse(model));
        }

        // 3) Get All with filters
        [HttpGet]
        public IActionResult GetAll(
            [FromQuery] bool? is_palindrome,
            [FromQuery] int? min_length,
            [FromQuery] int? max_length,
            [FromQuery] int? word_count,
            [FromQuery] string? contains_character)
        {
            // Validate query params (basic)
            if (min_length.HasValue && min_length < 0) return BadRequest(new { error = "min_length must be >= 0." });
            if (max_length.HasValue && max_length < 0) return BadRequest(new { error = "max_length must be >= 0." });
            if (word_count.HasValue && word_count < 0) return BadRequest(new { error = "word_count must be >= 0." });
            if (!string.IsNullOrEmpty(contains_character) && contains_character.Length != 1)
                return BadRequest(new { error = "contains_character must be a single character." });

            var data = InMemoryStringRepository.GetAll().AsEnumerable();

            if (is_palindrome.HasValue) data = data.Where(x => x.Properties.IsPalindrome == is_palindrome.Value);
            if (min_length.HasValue) data = data.Where(x => x.Properties.Length >= min_length.Value);
            if (max_length.HasValue) data = data.Where(x => x.Properties.Length <= max_length.Value);
            if (word_count.HasValue) data = data.Where(x => x.Properties.WordCount == word_count.Value);
            if (!string.IsNullOrEmpty(contains_character))
                data = data.Where(x => x.Value.Contains(contains_character, StringComparison.OrdinalIgnoreCase));

            var result = data.Select(ShapeResponse).ToList();

            return Ok(new
            {
                data = result,
                count = result.Count,
                filters_applied = new
                {
                    is_palindrome,
                    min_length,
                    max_length,
                    word_count,
                    contains_character
                }
            });
        }
        [HttpGet("filter-by-natural-language")]
        public IActionResult FilterByNaturalLanguage([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { message = "Query cannot be empty" });

            var parsed = QueryParser.Parse(query);

            var data = StringRepository.GetAll();

            if (parsed.IsPalindrome.HasValue)
                data = data.Where(x => x.Properties.IsPalindrome == parsed.IsPalindrome.Value);

            if (parsed.WordCount.HasValue)
                data = data.Where(x => x.Properties.WordCount == parsed.WordCount.Value);

            if (parsed.MinLength.HasValue)
                data = data.Where(x => x.Properties.Length >= parsed.MinLength.Value);

            if (!string.IsNullOrEmpty(parsed.ContainsCharacter))
                data = data.Where(x => x.Value.Contains(parsed.ContainsCharacter, StringComparison.OrdinalIgnoreCase));

            var shaped = data.Select(ShapeResponse).ToList();

            return Ok(new
            {
                data = shaped,
                count = shaped.Count,
                interpreted_query = new
                {
                    original = query,
                    parsed_filters = new
                    {
                        is_palindrome = parsed.IsPalindrome,
                        word_count = parsed.WordCount,
                        min_length = parsed.MinLength,
                        contains_character = parsed.ContainsCharacter
                    }
                }
            });
        }


        // 5) Delete
        [HttpDelete("{string_value}")]
        public IActionResult Delete(string string_value)
        {
            var ok = InMemoryStringRepository.DeleteByValue(string_value);
            if (!ok) return NotFound(new { error = "String does not exist in the system." });
            return NoContent();
        }

        // Helper to map internal model to exact response shape (snake_case keys)
        private static object ShapeResponse(StringModel m)
        {
            return new
            {
                id = m.Id,
                value = m.Value,
                properties = new
                {
                    length = m.Properties.Length,
                    is_palindrome = m.Properties.IsPalindrome,
                    unique_characters = m.Properties.UniqueCharacters,
                    word_count = m.Properties.WordCount,
                    sha256_hash = m.Properties.Sha256Hash,
                    character_frequency_map = m.Properties.CharacterFrequencyMap
                },
                created_at = m.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
        }

        // Very small parser for sample NL queries
        private static (bool? IsPalindrome, int? WordCount, int? MinLength, string? ContainsCharacter)? ParseNaturalLanguageQuery(string q)
        {
            // Examples supported:
            // "all single word palindromic strings" -> word_count=1, is_palindrome=true
            // "strings longer than 10 characters" -> min_length=11
            // "palindromic strings that contain the first vowel" -> is_palindrome=true, contains_character=a (heuristic)
            // "strings containing the letter z" -> contains_character=z

            bool? isPalindrome = null;
            int? wordCount = null;
            int? minLength = null;
            string? containsCharacter = null;

            if (q.Contains("palindromic") || q.Contains("palindrome") || q.Contains("palindromic strings"))
                isPalindrome = true;
            if (q.Contains("single word") || q.Contains("one word"))
                wordCount = 1;

            // longer than N
            var idx = q.IndexOf("longer than");
            if (idx >= 0)
            {
                var after = q.Substring(idx + "longer than".Length).Trim();
                var first = after.Split(' ').FirstOrDefault();
                if (int.TryParse(first, out var n)) minLength = n + 1;
            }

            // containing the letter X or containing the letter 'z'
            if (q.Contains("containing the letter"))
            {
                var parts = q.Split("containing the letter");
                var tail = parts.Last().Trim();
                if (!string.IsNullOrEmpty(tail))
                {
                    containsCharacter = tail.Trim().Split(' ')[0].Trim(new char[] { '\'', '"', '.' }).Substring(0, 1);
                }
            }
            else if (q.Contains("containing the"))
            {
                // e.g. "palindromic strings that contain the first vowel"
                if (q.Contains("first vowel"))
                    containsCharacter = "a"; // heuristic: first vowel -> 'a'
            }
            else
            {
                // simpler: "strings containing the letter z" or "containing z"
                var words = q.Split(' ');
                var idx2 = Array.IndexOf(words, "containing");
                if (idx2 >= 0 && idx2 + 1 < words.Length)
                {
                    var candidate = words[idx2 + 1].Trim(new char[] { '.', ',', '\'', '"' });
                    if (candidate.Length == 1) containsCharacter = candidate;
                    else if (candidate == "the" && idx2 + 2 < words.Length)
                    {
                        var cand2 = words[idx2 + 2].Trim(new char[] { '.', ',', '\'', '"' });
                        if (cand2.Length == 1) containsCharacter = cand2;
                    }
                }
            }

            // If nothing parsed, return null
            if (isPalindrome == null && wordCount == null && minLength == null && containsCharacter == null)
                return null;

            return (isPalindrome, wordCount, minLength, containsCharacter);
        }
    }
}
