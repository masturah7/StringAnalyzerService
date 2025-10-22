# String Analyzer API

A simple **ASP.NET Core Web API** that analyzes strings dynamically — checking if they’re palindromes, counting characters, computing SHA-256 hashes, and more.  
Built as part of the **Backend Wizards Stage 0 Challenge**.

## 🚀 Features
- Analyze any string and get:
  - Length  
  - Word count  
  - Palindrome status  
  - Unique character count  
  - SHA-256 hash  
  - Character frequency map  
- Retrieve, filter, or delete analyzed strings  
- In-memory data storage (no external database)  
- Swagger UI for easy testing

## Tech Stack
- **Language:** C#  
- **Framework:** ASP.NET Core Web API  
- **Database:** In-Memory storage  
- **Documentation:** Swagger (OpenAPI)

## Setup Instructions
1️⃣ Clone the Repository
```bash
git clone https://github.com/masturah7/StringAnalyzerService.git
cd StringAnalyzerService
2️⃣ Restore Dependencies
dotnet restore
3️⃣ Run the Application
dotnet run
4️⃣ Open in Browser
Navigate to:
https://localhost:5001/swagger
API Endpoints
Method	Endpoint	Description
POST	/api/strings	Analyze a new string
GET	/api/strings	Get all analyzed strings (supports filters)
GET	/api/strings/{string_value}	Fetch a single analyzed string
DELETE	/api/strings/{string_value}
🧠 Example Request
POST /api/strings
{
  "value": "racecar"
}
{
  "id": "a1b2c3...",
  "value": "racecar",
  "createdAt": "2025-10-18T12:00:00Z",
  "properties": {
    "length": 7,
    "isPalindrome": true,
    "uniqueCharacters": 4,
    "wordCount": 1,
    "sha256Hash": "9c8f4d...",
    "characterFrequencyMap": {
      "r": 2,
      "a": 2,
      "c": 2,
      "e": 1
    }
  }
}


🧩 Example Filters
Filter	Usage Example
is_palindrome	/api/strings?is_palindrome=true
min_length	/api/strings?min_length=3
max_length	/api/strings?max_length=10
contains_character	/api/strings?contains_character=a
