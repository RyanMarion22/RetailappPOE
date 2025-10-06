using Microsoft.AspNetCore.Mvc;
using RetailappPOE.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace RetailappPOE.Controllers
{
    public class FilesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseFunctionUrl = "https://ryanst10440289part2-a3avddhxerhyhke4.southafricanorth-01.azurewebsites.net/api/";

        public FilesController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(_baseFunctionUrl + "uploads");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to load files from Azure Function.";
                return View(new List<FilesModel>());
            }

            var content = await response.Content.ReadAsStringAsync();
            var files = JsonSerializer.Deserialize<List<FilesModel>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(files);
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                return View();
            }

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();
            var base64 = Convert.ToBase64String(bytes);

            var payload = new
            {
                FileName = file.FileName,
                Base64 = base64
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_baseFunctionUrl + "blobs/upload", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "File uploaded successfully.";
                return RedirectToAction("Index");
            }

            ModelState.AddModelError("", "Failed to upload file via Azure Function.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string fileName)
        {
            var deleteUrl = $"{_baseFunctionUrl}DeleteFile?fileName={fileName}";
            var response = await _httpClient.DeleteAsync(deleteUrl);

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to delete file via Azure Function.";
            }
            else
            {
                TempData["Success"] = "File deleted successfully.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Download(string fileName)
        {
            var response = await _httpClient.GetAsync($"{_baseFunctionUrl}DownloadFile?fileName={fileName}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to download file via Azure Function.";
                return RedirectToAction("Index");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return File(bytes, "application/octet-stream", fileName);
        }
    }
}
