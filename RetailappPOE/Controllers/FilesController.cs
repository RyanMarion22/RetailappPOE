using Microsoft.AspNetCore.Mvc;
using RetailappPOE.Models;
using RetailappPOE.Services;

namespace RetailappPOE.Controllers
{
    public class FilesController : Controller
    {
        private readonly FilesService _fileService;
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10 MB limit
        private readonly string[] _allowedExtensions =
     { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".docx", ".xlsx", ".txt" };


        public FilesController(FilesService fileService)
        {
            _fileService = fileService;
        }

        // List files
        public async Task<IActionResult> Index()
        {
            try
            {
                var files = await _fileService.GetFilesAsync();
                return View(files);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error loading files: " + ex.Message;
                return View(new List<FilesModel>());
            }
        }

        // Upload file (GET)
        public IActionResult Upload()
        {
            return View();
        }

        // Upload file (POST)
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                return View();
            }

            // Validate file size
            if (file.Length > _maxFileSize)
            {
                ModelState.AddModelError("", $"File size cannot exceed {_maxFileSize / (1024 * 1024)} MB.");
                return View();
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("", $"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
                return View();
            }

            try
            {
                await _fileService.UploadFileAsync(file);
                TempData["Message"] = "File uploaded successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error uploading file: " + ex.Message);
                return View();
            }
        }

        // Download file
        public async Task<IActionResult> Download(string fileName)
        {
            try
            {
                var stream = await _fileService.DownloadFileAsync(fileName);

                if (stream == null)
                    return NotFound();

                return File(stream, "application/octet-stream", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error downloading file: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // Delete file
        public async Task<IActionResult> Delete(string fileName)
        {
            try
            {
                await _fileService.DeleteFileAsync(fileName);
                TempData["Message"] = "File deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error deleting file: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
