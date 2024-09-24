using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ProjectManager.Api.User.Services
{
    public class ImgurService
    {
        private readonly HttpClient _httpClient;
        private readonly string _imgurClientId;

        public ImgurService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _imgurClientId = configuration["Imgur:ClientId"]; // Asegúrate de tener esta configuración en appsettings.json
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            using (var fileStream = file.OpenReadStream())
            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();
                string base64Image = Convert.ToBase64String(fileBytes);

                var formData = new MultipartFormDataContent();
                formData.Add(new StringContent(base64Image), "image");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", _imgurClientId);
                var response = await _httpClient.PostAsync("https://api.imgur.com/3/image", formData);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(jsonString);

                    // Obtener la URL de la imagen subida
                    var imageUrl = json["data"]?["link"]?.ToString();

                    if (string.IsNullOrEmpty(imageUrl))
                    {
                        throw new Exception("Error: No se pudo obtener la URL de la imagen.");
                    }

                    return imageUrl;  // Retorna solo la URL de la imagen
                }

                // Manejo del error
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al subir la imagen: {errorContent}");
            }
        }

        public Task DeleteImageAsync(string publicId)
        {
            // Imgur no permite eliminar imágenes a través de un ID público sin autenticación adicional
            return Task.CompletedTask;
        }
    }
}
