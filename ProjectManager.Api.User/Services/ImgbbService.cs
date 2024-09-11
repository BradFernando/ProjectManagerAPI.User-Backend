using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace ProjectManager.Api.User.Services
{
    public class ImgbbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _imgbbApiKey;

        public ImgbbService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _imgbbApiKey = configuration["Imgbb:ApiKey"]; // Asegúrate de tener esta configuración en appsettings.json
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            using (var fileStream = file.OpenReadStream())
            using (var memoryStream = new MemoryStream())
            {
                await fileStream.CopyToAsync(memoryStream);
                byte[] fileBytes = memoryStream.ToArray();
                string base64Image = Convert.ToBase64String(fileBytes);

                var formData = new MultipartFormDataContent
                {
                    { new StringContent(base64Image), "image" }
                };

                var response = await _httpClient.PostAsync($"https://api.imgbb.com/1/upload?key={_imgbbApiKey}", formData);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(jsonString);
                    return json["data"]?["url"]?.ToString(); // Obtén la URL de la imagen subida
                }

                return null; // Manejo de errores en caso de fallo
            }
        }

        public Task DeleteImageAsync(string publicId)
        {
            // Imgbb no tiene una función directa de eliminar imágenes mediante un publicId como Cloudinary.
            // Tendrás que almacenar el "delete_url" proporcionado por imgbb al subir la imagen para usarlo posteriormente.
            return Task.CompletedTask;
        }
    }
}
