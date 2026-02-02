using System.Text.Json;

namespace Notification_Application.Services;

public interface IUnsplashService
{
    Task<UnsplashSearchResult> SearchPhotosAsync(string query, int page = 1, int perPage = 20);
    Task<UnsplashPhoto?> GetPhotoAsync(string photoId);
    Task<UnsplashPhoto?> GetRandomPhotoAsync(string? query = null);
    Task<IEnumerable<UnsplashCollection>> GetCollectionsAsync(int page = 1, int perPage = 10);
    Task<UnsplashSearchResult> GetCollectionPhotosAsync(string collectionId, int page = 1, int perPage = 20);
    string GetDownloadUrl(string photoId);
}

public class UnsplashService : IUnsplashService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UnsplashService> _logger;
    private readonly string _accessKey;
    private const string BaseUrl = "https://api.unsplash.com";

    public UnsplashService(IConfiguration configuration, ILogger<UnsplashService> logger)
    {
        _logger = logger;
        _accessKey = configuration["Unsplash:AccessKey"] ?? "PsVDfvlqDdqxizfkvv0kMZPcIas1Xu7H7EG7Xwe8Xvk";
        
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Client-ID {_accessKey}");
        _httpClient.DefaultRequestHeaders.Add("Accept-Version", "v1");
    }

    public async Task<UnsplashSearchResult> SearchPhotosAsync(string query, int page = 1, int perPage = 20)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/search/photos?query={Uri.EscapeDataString(query)}&page={page}&per_page={perPage}&orientation=landscape");
            
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UnsplashApiSearchResponse>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            return new UnsplashSearchResult
            {
                Total = result?.Total ?? 0,
                TotalPages = result?.TotalPages ?? 0,
                Photos = result?.Results?.Select(MapToPhoto).ToList() ?? new List<UnsplashPhoto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Unsplash photos for query: {Query}", query);
            return new UnsplashSearchResult { Photos = new List<UnsplashPhoto>() };
        }
    }

    public async Task<UnsplashPhoto?> GetPhotoAsync(string photoId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/photos/{photoId}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var apiPhoto = JsonSerializer.Deserialize<UnsplashApiPhoto>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            return apiPhoto != null ? MapToPhoto(apiPhoto) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Unsplash photo: {PhotoId}", photoId);
            return null;
        }
    }

    public async Task<UnsplashPhoto?> GetRandomPhotoAsync(string? query = null)
    {
        try
        {
            var url = $"{BaseUrl}/photos/random?orientation=landscape";
            if (!string.IsNullOrEmpty(query))
            {
                url += $"&query={Uri.EscapeDataString(query)}";
            }
            
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var apiPhoto = JsonSerializer.Deserialize<UnsplashApiPhoto>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            return apiPhoto != null ? MapToPhoto(apiPhoto) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting random Unsplash photo");
            return null;
        }
    }

    public async Task<IEnumerable<UnsplashCollection>> GetCollectionsAsync(int page = 1, int perPage = 10)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{BaseUrl}/collections?page={page}&per_page={perPage}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var collections = JsonSerializer.Deserialize<List<UnsplashApiCollection>>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            return collections?.Select(c => new UnsplashCollection
            {
                Id = c.Id ?? "",
                Title = c.Title ?? "",
                Description = c.Description,
                TotalPhotos = c.TotalPhotos,
                CoverPhotoUrl = c.CoverPhoto?.Urls?.Small
            }).ToList() ?? new List<UnsplashCollection>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Unsplash collections");
            return new List<UnsplashCollection>();
        }
    }

    public async Task<UnsplashSearchResult> GetCollectionPhotosAsync(string collectionId, int page = 1, int perPage = 20)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{BaseUrl}/collections/{collectionId}/photos?page={page}&per_page={perPage}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var photos = JsonSerializer.Deserialize<List<UnsplashApiPhoto>>(json, new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
            
            return new UnsplashSearchResult
            {
                Photos = photos?.Select(MapToPhoto).ToList() ?? new List<UnsplashPhoto>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Unsplash collection photos: {CollectionId}", collectionId);
            return new UnsplashSearchResult { Photos = new List<UnsplashPhoto>() };
        }
    }

    public string GetDownloadUrl(string photoId)
    {
        return $"{BaseUrl}/photos/{photoId}/download?client_id={_accessKey}";
    }

    private static UnsplashPhoto MapToPhoto(UnsplashApiPhoto apiPhoto)
    {
        return new UnsplashPhoto
        {
            Id = apiPhoto.Id ?? "",
            Description = apiPhoto.Description ?? apiPhoto.AltDescription,
            Width = apiPhoto.Width,
            Height = apiPhoto.Height,
            Color = apiPhoto.Color,
            Urls = new UnsplashUrls
            {
                Raw = apiPhoto.Urls?.Raw ?? "",
                Full = apiPhoto.Urls?.Full ?? "",
                Regular = apiPhoto.Urls?.Regular ?? "",
                Small = apiPhoto.Urls?.Small ?? "",
                Thumb = apiPhoto.Urls?.Thumb ?? ""
            },
            User = new UnsplashUser
            {
                Name = apiPhoto.User?.Name ?? "",
                Username = apiPhoto.User?.Username ?? "",
                ProfileUrl = apiPhoto.User?.Links?.Html ?? ""
            },
            Links = new UnsplashLinks
            {
                Html = apiPhoto.Links?.Html ?? "",
                Download = apiPhoto.Links?.Download ?? "",
                DownloadLocation = apiPhoto.Links?.DownloadLocation ?? ""
            }
        };
    }
}

// API Response Models
public class UnsplashApiSearchResponse
{
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public List<UnsplashApiPhoto>? Results { get; set; }
}

public class UnsplashApiPhoto
{
    public string? Id { get; set; }
    public string? Description { get; set; }
    public string? AltDescription { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? Color { get; set; }
    public UnsplashApiUrls? Urls { get; set; }
    public UnsplashApiUser? User { get; set; }
    public UnsplashApiLinks? Links { get; set; }
}

public class UnsplashApiUrls
{
    public string? Raw { get; set; }
    public string? Full { get; set; }
    public string? Regular { get; set; }
    public string? Small { get; set; }
    public string? Thumb { get; set; }
}

public class UnsplashApiUser
{
    public string? Name { get; set; }
    public string? Username { get; set; }
    public UnsplashApiLinks? Links { get; set; }
}

public class UnsplashApiLinks
{
    public string? Html { get; set; }
    public string? Download { get; set; }
    public string? DownloadLocation { get; set; }
}

public class UnsplashApiCollection
{
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int TotalPhotos { get; set; }
    public UnsplashApiPhoto? CoverPhoto { get; set; }
}

// Public Models
public class UnsplashSearchResult
{
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public List<UnsplashPhoto> Photos { get; set; } = new();
}

public class UnsplashPhoto
{
    public string Id { get; set; } = "";
    public string? Description { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? Color { get; set; }
    public UnsplashUrls Urls { get; set; } = new();
    public UnsplashUser User { get; set; } = new();
    public UnsplashLinks Links { get; set; } = new();
}

public class UnsplashUrls
{
    public string Raw { get; set; } = "";
    public string Full { get; set; } = "";
    public string Regular { get; set; } = "";
    public string Small { get; set; } = "";
    public string Thumb { get; set; } = "";
}

public class UnsplashUser
{
    public string Name { get; set; } = "";
    public string Username { get; set; } = "";
    public string ProfileUrl { get; set; } = "";
}

public class UnsplashLinks
{
    public string Html { get; set; } = "";
    public string Download { get; set; } = "";
    public string DownloadLocation { get; set; } = "";
}

public class UnsplashCollection
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public int TotalPhotos { get; set; }
    public string? CoverPhotoUrl { get; set; }
}
