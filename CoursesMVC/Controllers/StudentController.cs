using CoursesMVC.Models;
using CoursesMVC.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;
using System.Net.Http.Headers;

namespace CoursesMVC.Controllers
{
    public class StudentController : Controller
    {
        private readonly IHttpClientFactory httpClientFactory;

        public StudentController(IHttpClientFactory httpClientFactory)
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
            List<StudentDTO> response = new List<StudentDTO>();

            try
            {
                var client = httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                var httpResponseMess = await client.GetAsync("https://localhost:7248/api/Student/get-all-students");
                httpResponseMess.EnsureSuccessStatusCode();
                response = await httpResponseMess.Content.ReadFromJsonAsync<List<StudentDTO>>();
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
        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddStudent(StudentDTO studentDTO)
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                var httpRequestMessage = new HttpRequestMessage()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri("https://localhost:7248/api/Student/add-student"),
                    Content = new StringContent(JsonSerializer.Serialize(studentDTO), Encoding.UTF8, MediaTypeNames.Application.Json)
                };

                var httpResponseMessage = await client.SendAsync(httpRequestMessage);
                httpResponseMessage.EnsureSuccessStatusCode();

                // Quay trở lại trang chủ sau khi thêm thành công
                return RedirectToAction("Index", "Student");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                return View(studentDTO);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
            {
                return BadRequest("Invalid student ID.");
            }

            try
            {
                var client = httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                var httpResponse = await client.DeleteAsync($"https://localhost:7248/api/Student/delete-student-by-id/{id}");
                if (httpResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Student");
                }
                else
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to delete student. Status code: {httpResponse.StatusCode}, Reason: {httpResponse.ReasonPhrase}, Response: {responseContent}");
                    ModelState.AddModelError(string.Empty, "Failed to delete student. Please try again.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                ModelState.AddModelError(string.Empty, "An error occurred while deleting the student.");
            }

            return RedirectToAction("Index", "Student");
        }
        public async Task<IActionResult> ChiTiet(int id)
        {
            StudentDTO response = new StudentDTO();
            try
            {
                // Create the HTTP client
                var client = httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                var httpResponseMess = await client.GetAsync("https://localhost:7248/api/Student/get-student-by-id/" + id);
                httpResponseMess.EnsureSuccessStatusCode();
                var stringResponseBody = await httpResponseMess.Content.ReadAsStringAsync();
                response = await httpResponseMess.Content.ReadFromJsonAsync<StudentDTO>();
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
            }
            return View(response);
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            StudentDTO responseStudent = null;
            var client = httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            try
            {
                var httpResponseMess = await client.GetAsync($"https://localhost:7248/api/Student/get-student-by-id/{id}");

                if (httpResponseMess.IsSuccessStatusCode)
                {
                    responseStudent = await httpResponseMess.Content.ReadFromJsonAsync<StudentDTO>();
                }
                else
                {
                    ViewBag.Error = "Student does not exist or there was an error while retrieving data.";
                    return View("Error");
                }
            }
            catch (HttpRequestException httpRequestException)
            {
                ViewBag.Error = $"Error sending request: {httpRequestException.Message}";
                return View("Error");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"An error occurred: {ex.Message}";
                return View("Error");
            }

            return View(responseStudent);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromForm] StudentDTO student)
        {
            if (!ModelState.IsValid)
            {
                return View(student);
            }

            try
            {
                var client = httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                var request = new HttpRequestMessage(HttpMethod.Put, $"https://localhost:7248/api/Student/update-student-by-id/{id}")
                {
                    Content = new StringContent(JsonSerializer.Serialize(student), Encoding.UTF8, MediaTypeNames.Application.Json)
                };

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction("Index", "Student");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Update failed. Error details: {errorContent}");
                    return View(student);
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"An error occurred: {ex.Message}";
                return View("Error");
            }
        }
    }
}
