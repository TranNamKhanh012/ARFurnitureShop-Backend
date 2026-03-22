using Microsoft.AspNetCore.Mvc;
using ARFurniture.AdminWeb.Models;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace ARFurniture.AdminWeb.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProductController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- HIỂN THỊ DANH SÁCH ---
        public async Task<IActionResult> Index()
        {
            var model = new List<AdminProductViewModel>();
            try
            {
                // Chỉ cần gọi tên API, phần link gốc đã được cấu hình tự động ở Program.cs
                var response = await _httpClient.GetAsync("Products/admin-list");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    model = JsonSerializer.Deserialize<List<AdminProductViewModel>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                else
                {
                    ViewBag.Error = $"API trả về lỗi: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Không thể kết nối đến API. Chi tiết: " + ex.Message;
            }

            return View(model);
        }

        // --- HÀM PHỤ: LẤY DANH MỤC CHO DROPDOWN ---
        private async Task<List<CategoryViewModel>> GetCategories()
        {
            try
            {
                var response = await _httpClient.GetAsync("Categories");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<CategoryViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
            }
            catch { }
            return new List<CategoryViewModel>();
        }

        // --- GIAO DIỆN THÊM MỚI ---
        public async Task<IActionResult> Create()
        {
            var model = new ProductFormViewModel { Categories = await GetCategories() };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductFormViewModel model)
        {
            var content = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Products/admin-create", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            var errorDetail = await response.Content.ReadAsStringAsync();
            model.Categories = await GetCategories();
            ViewBag.Error = $"API báo lỗi ({response.StatusCode}): {errorDetail}";

            return View(model);
        }

        // --- GIAO DIỆN SỬA ---
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"Products/admin-get/{id}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var model = JsonSerializer.Deserialize<ProductFormViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                model.Categories = await GetCategories();
                return View(model);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ProductFormViewModel model)
        {
            var content = new StringContent(JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"Products/admin-update/{model.Id}", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            // Ép nó hiện chi tiết lỗi nếu cập nhật thất bại
            var errorDetail = await response.Content.ReadAsStringAsync();
            model.Categories = await GetCategories();
            ViewBag.Error = $"Lỗi khi cập nhật ({response.StatusCode}): {errorDetail}";

            return View(model);
        }

        // --- XỬ LÝ XÓA ---
        public async Task<IActionResult> Delete(int id)
        {
            await _httpClient.DeleteAsync($"Products/admin-delete/{id}");
            return RedirectToAction("Index");
        }
    }
}