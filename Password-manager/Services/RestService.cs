using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Password_manager.Entities;

namespace Password_manager.Services
{
    public class RestService
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ILogger<RestService> _logger;
        public RestService(ILogger<RestService> logger) 
        {
            _logger = logger;
            _client = new HttpClient();
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task RegisterNewCloudAccount(LoginDTO registerCredentials)
        {
            Uri uri = new Uri(string.Format(Constants.RestUrl, "AccountsItems/register"));

            try
            {
                string json = JsonSerializer.Serialize(registerCredentials, _serializerOptions);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(uri, content);

                var body = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Request to register new account on cloud has been succesfull: {StatusCode} - {content}",
                        response.StatusCode, body);

                    var res = JsonSerializer.Deserialize<JsonElement>(body, _serializerOptions);

                    string token = res.GetProperty("Token").GetString();
                    long cloudId = res.GetProperty("Id").GetInt64();
                    string email = res.GetProperty("Email").GetString();


                }
                else
                {
                    _logger.LogWarning("Request to register wasn't succesfull: {StatusCode} - {content}", response.StatusCode, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Request to register new account on cloud failed: {0}", ex);
            }
        }

        public async Task LoginToCloudAccount(LoginDTO loginCredentials)
        {
            Uri uri = new Uri(string.Format(Constants.RestUrl, "AccountsItems/register"));

            try
            {
                string json = JsonSerializer.Serialize(loginCredentials, _serializerOptions);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _client.PostAsync(uri, content);

                var body = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Request to login new account on cloud has been succesfull: {StatusCode} - {content}",
                        response.StatusCode, body);

                    var res = JsonSerializer.Deserialize<JsonElement>(body, _serializerOptions);

                    string token = res.GetProperty("Token").GetString();
                }
                else
                {
                    _logger.LogWarning("Request to login wasn't succesfull: {StatusCode} - {content}", response.StatusCode, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Request to login new account on cloud failed: {0}", ex);
            }
        }


    }
}
