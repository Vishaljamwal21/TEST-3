﻿using Newtonsoft.Json;
using SMS_APP.Models;
using SMS_APP.Repository.IRepository;
using System.Net;
using System.Text;

namespace SMS_APP.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public Repository(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<User> Authenticate(string Email, string Password)
        {
            try
            {
                var credentials = new UserVM { Email = Email, Password = Password };
                var url = $"{URL.AuthenticateAPIPath}"; // Ensure the correct endpoint URL
                var response = await _httpClientFactory.CreateClient().PostAsJsonAsync(url, credentials);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<User>(responseData);
                }
                else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Authentication failed: {errorMessage}");
                }
                else
                {
                    throw new Exception($"Authentication failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<bool> CreateAsync(string url, T ObjToCreate)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (ObjToCreate != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(ObjToCreate),
                    Encoding.UTF8, "application/json");
            }
            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.Created)
                return true;
            else
                return false;
        }

        public async Task<bool> DeleteAsync(string url, int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, url + "/" + id.ToString());
            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return true;
            else
                return false;
        }

        public async Task<IEnumerable<T>> GetAllAsync(string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var jsonstring = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<IEnumerable<T>>(jsonstring);
            }
            return null;
        }

        public async Task<T> GetAsync(string url, int id)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url + "/" + id.ToString());
                var client = _httpClientFactory.CreateClient();
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var jsonstring = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(jsonstring);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public async Task<bool> IsUniqueUser(string Email)
        {
            var url = $"{URL.RegisterAPIPath}/IsUniqueUser/{Email}";
            var response = await _httpClientFactory.CreateClient().GetAsync(url);
            return response.IsSuccessStatusCode;
        }

        public async Task<User> Register(string Email, string Password)
        {
            var newUser = new User { Email = Email, Password = Password };
            var url = $"{URL.RegisterAPIPath}"; // No need to append "/Register" as it's already included in the path
            var response = await _httpClientFactory.CreateClient().PostAsJsonAsync(url, newUser);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(responseData))
                {
                    return JsonConvert.DeserializeObject<User>(responseData);
                }
                else
                {
                    throw new Exception("Failed to deserialize response data. Response content is null or empty.");
                }
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"Registration failed: {response.ReasonPhrase}. Error: {errorResponse}");
            }
        }

        public async Task<bool> UpdateAsync(string url, T ObjToUpdate)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, url);
            if (ObjToUpdate != null)
            {
                request.Content = new StringContent(JsonConvert.SerializeObject(ObjToUpdate),
                    Encoding.UTF8, "application/json");
            }
            var client = _httpClientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return true;
            else
                return false;
        }
        public async Task<IEnumerable<Student>> GetStudentByEmailAsync(string email, string url)
        {
            var allData = await GetAllAsync(url);
            if (typeof(T) == typeof(Student))
            {
                var studentData = allData as IEnumerable<Student>;
                if (studentData != null)
                {
                    return studentData.Where(student => student.Email == email).ToList();
                }
            }
            return null;
        }
        public async Task<bool> IsUniqueEmail(string email)
        {
            try
            {
                var url = $"{URL.RegisterAPIPath}/IsUniqueUser/{email}";
                var response = await _httpClientFactory.CreateClient().GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentId(int studentId)
        {
            try
            {
                var url = $"{URL.EnrollmentAPIPath}?studentId={studentId}";
                var response = await _httpClientFactory.CreateClient().GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var allEnrollments = JsonConvert.DeserializeObject<IEnumerable<Enrollment>>(responseData);

                    // Filter enrollments by studentId
                    var studentEnrollments = allEnrollments.Where(enrollment => enrollment.StudentId == studentId);

                    return studentEnrollments;
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return Enumerable.Empty<Enrollment>();
                }
                else
                {
                    throw new Exception($"Failed to retrieve enrollments for student with ID {studentId}. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while retrieving enrollments for student with ID {studentId}.", ex);
            }
        }
        public async Task<Grade> GetEnrollmentIdAsync(string url, int enrollmentId)
         {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, $"{URL.GradeAPIPath}/enrollment/{enrollmentId}");
                var client = _httpClientFactory.CreateClient();
                HttpResponseMessage response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var jsonstring = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Grade>(jsonstring);
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }


    }
}
