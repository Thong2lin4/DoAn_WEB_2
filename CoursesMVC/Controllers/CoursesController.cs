using CoursesMVC.Models;
using CoursesMVC.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Mime;
using System.Text.Json;
using System.Text;
using static System.Reflection.Metadata.BlobBuilder;
using System.Net.Http.Headers;

namespace CoursesMVC.Controllers
{
    public class CoursesController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;

        public CoursesController(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }
        private void AddAuthorizationHeader(HttpClient client)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        public async Task<IActionResult> Index()
        {
            List<CoursesDTO> response = new List<CoursesDTO>();

            try
            {
                var client = httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                var httpResponseMess = await client.GetAsync("https://localhost:7248/api/Courses/get-all-courses");
                httpResponseMess.EnsureSuccessStatusCode();
                response = await httpResponseMess.Content.ReadFromJsonAsync<List<CoursesDTO>>();
            }
            catch (Exception ex)
            {
                var errorModel = new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
                };
                return View("Error", errorModel);
            }

            return View(response);
        }
        [HttpGet]
        public IActionResult AddCourses()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCourses(addCourses addCoursesDTO)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                var httpRequestMessage = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://localhost:7248/api/Courses/add-course"),
                    Content = new StringContent(JsonSerializer.Serialize(addCoursesDTO), Encoding.UTF8, MediaTypeNames.Application.Json)
                };

                var httpResponseMessage = await client.SendAsync(httpRequestMessage);
                httpResponseMessage.EnsureSuccessStatusCode();

                // Quay trở lại trang chủ sau khi thêm thành công
                return RedirectToAction("Index", "Courses");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(addCoursesDTO);
            }

        }
        public async Task<IActionResult> ChiTiet(int id)
        {
            CoursesDTO response = new CoursesDTO();
            try
            {
                // lấy dữ liệu books from   
                var client = httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                var httpResponseMess = await client.GetAsync("https://localhost:7248/api/Courses/get-course-by-id/" + id);
                httpResponseMess.EnsureSuccessStatusCode();
                var stringResponseBody = await httpResponseMess.Content.ReadAsStringAsync();
                response = await httpResponseMess.Content.ReadFromJsonAsync<CoursesDTO>();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(response);
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteCourses(int id)
        {
            if (id == 0)
            {
                return BadRequest("Invalid course ID.");
            }

            try
            {
                var client = httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                var httpResponse = await client.DeleteAsync($"https://localhost:7248/api/Courses/delete-course-by-id/{id}");
                if (httpResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Courses");
                }
                else
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to delete course. Status code: {httpResponse.StatusCode}, Reason: {httpResponse.ReasonPhrase}, Response: {responseContent}");
                    ModelState.AddModelError(string.Empty, "Failed to delete course. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the course.");
            }

            return RedirectToAction("Index", "Courses");
        }
        /* Xóa Courses */
        [HttpGet]
        public async Task<IActionResult> editCourses(int id)
        {
            addCourses responseBook = null;
            var client = httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            try
            {
                var httpResponseMess = await client.GetAsync($"https://localhost:7248/api/Courses/get-course-by-id/{id}");

                if (httpResponseMess.IsSuccessStatusCode)
                {
                    responseBook = await httpResponseMess.Content.ReadFromJsonAsync<addCourses>();
                }
                else
                {
                    ViewBag.Error = "Sản phẩm không tồn tại hoặc có lỗi trong quá trình lấy dữ liệu.";
                    return View("Error");
                }
            }
            catch (HttpRequestException httpRequestException)
            {
                ViewBag.Error = $"Lỗi khi gửi yêu cầu: {httpRequestException.Message}";
                return View("Error");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Đã xảy ra lỗi: {ex.Message}";
                return View("Error");
            }

            return View(responseBook);
        }

        [HttpPost]
        public async Task<IActionResult> editCourses(int id, [FromForm] addCourses course)
        {
            if (!ModelState.IsValid)
            {
                return View(course);
            }

            try
            {
                var client = httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                var request = new HttpRequestMessage(HttpMethod.Put, $"https://localhost:7248/api/Courses/update-course-by-id/{id}")
                {
                    Content = new StringContent(JsonSerializer.Serialize(course), Encoding.UTF8, MediaTypeNames.Application.Json)
                };

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Courses");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Cập nhật không thành công. Chi tiết lỗi: {errorContent}");
                    return View(course);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Có lỗi xảy ra: {ex.Message}";
                return View("Error");
            }
        }

    }
}

