using Microsoft.AspNetCore.Mvc;
using RetailappPOE.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Headers;

namespace RetailappPOE.Controllers
{
    public class FilesController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseFunctionUrl = "http://localhost:7274/api/";

        public FilesController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync(_baseFunctionUrl + "uploads");
            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Failed to load files.";
                return View(new List<FilesModel>());
            }

            var content = await response.Content.ReadAsStringAsync();
            var files = JsonSerializer.Deserialize<List<FilesModel>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<FilesModel>();

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
                ModelState.AddModelError("", "Please select a file.");
                return View();
            }

            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.FileName); 

            var response = await _httpClient.PostAsync(_baseFunctionUrl + "uploads", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = $"File '{file.FileName}' uploaded successfully!";
                return RedirectToAction("Index");
            }

            var error = await response.Content.ReadAsStringAsync();
            ModelState.AddModelError("", $"Upload failed: {error}");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string fileName)
        {
            var response = await _httpClient.DeleteAsync($"{_baseFunctionUrl}uploads/{fileName}");
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "File deleted.";
            }
            else
            {
                TempData["Error"] = "Failed to delete file.";
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Download(string fileName)
        {
            var response = await _httpClient.GetAsync($"{_baseFunctionUrl}uploads/download/{fileName}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "File not found.";
                return RedirectToAction("Index");
            }

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return File(bytes, "application/octet-stream", fileName);
        }
    }
}