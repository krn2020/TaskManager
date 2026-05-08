using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace TaskManager.Services
{
    public class GiphyService
    {
        private readonly HttpClient _httpClient = new();
        private readonly string _apiKey;

        public GiphyService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> GetRandomGifUrlAsync()
        {
            var url = $"https://api.giphy.com/v1/gifs/random?api_key={_apiKey}&rating=g";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var gifUrl = root.GetProperty("data").GetProperty("images").GetProperty("original").GetProperty("url").GetString();
            return gifUrl ?? "https://media.giphy.com/media/3o7abB06u9bNzA8LC8/giphy.gif";
        }
    }
}