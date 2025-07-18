using Branchecker.Models;
using Branchecker.Models.Core;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class GeminiApiClient {
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _modelPath;
    private readonly string _baseUrl;

    public GeminiApiClient(HttpClient httpClient, IConfiguration config) {
        _httpClient = httpClient;
        _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("API Key is missing");
        _modelPath = config["Gemini:ModelPath"] ?? throw new ArgumentNullException("ModelPath is missing");
        _baseUrl = config["Gemini:BaseUrl"] ?? throw new ArgumentNullException("BaseUrl is missing");
    }

    public async Task<string?> GenerateTextAsync(string prompt) {
        var url = $"{_baseUrl}{_modelPath}?key={_apiKey}";
        Console.WriteLine(url);
        var request = new GeminiRequest {
            contents =
            [
                new Content
                {
                    parts =
                    [
                        new Part { text = prompt }
                    ]
                }
            ]
        };

        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<GeminiResponse>(responseBody);

        return result?.candidates?.FirstOrDefault()?.content?.parts?.FirstOrDefault()?.text;
    }
}
