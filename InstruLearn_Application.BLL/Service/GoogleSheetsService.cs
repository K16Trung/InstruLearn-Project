using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Certification;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class GoogleSheetsService : IGoogleSheetsService
    {
        private readonly IConfiguration _configuration;
        private readonly string _spreadsheetId;
        private readonly string _credentialsPath;

        public GoogleSheetsService(IConfiguration configuration)
        {
            _configuration = configuration;
            _spreadsheetId = _configuration["GoogleSheets:SpreadsheetId"];
            _credentialsPath = _configuration["GoogleSheets:CredentialsPath"];
        }

        public async Task<bool> SaveCertificationDataAsync(CertificationDataDTO certificationData)
        {
            try
            {
                // Load the credentials from the JSON file
                GoogleCredential credential;
                using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(SheetsService.Scope.Spreadsheets);
                }

                // Create the Sheets API service
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "InstruLearn Certificate Tracking"
                });

                // Prepare the data to write
                var range = "Certificates!A:H"; // Update with your sheet name and range
                var valueRange = new ValueRange
                {
                    Values = new List<IList<object>>
                    {
                        new List<object>
                        {
                            certificationData.CertificationId,
                            certificationData.LearnerName,
                            certificationData.LearnerEmail,
                            certificationData.CertificationType,
                            certificationData.CertificationName,
                            certificationData.IssueDate.ToString("yyyy-MM-dd HH:mm:ss"),
                            certificationData.TeacherName ?? "N/A",
                            certificationData.Subject ?? "N/A"
                        }
                    }
                };

                // Check if headers exist or need to be created
                var getRequest = service.Spreadsheets.Values.Get(_spreadsheetId, "Certificates!A1:H1");
                ValueRange getResponse = await getRequest.ExecuteAsync();

                // If headers don't exist, add them
                if (getResponse.Values == null || getResponse.Values.Count == 0)
                {
                    var headerRange = new ValueRange
                    {
                        Values = new List<IList<object>>
                        {
                            new List<object>
                            {
                                "Certificate ID",
                                "Learner Name",
                                "Learner Email",
                                "Certificate Type",
                                "Certificate Name",
                                "Issue Date",
                                "Teacher Name",
                                "Subject"
                            }
                        }
                    };

                    var headerUpdateRequest = service.Spreadsheets.Values.Update(headerRange, _spreadsheetId, "Certificates!A1:H1");
                    headerUpdateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                    await headerUpdateRequest.ExecuteAsync();
                }

                // Append the data at the end
                var appendRequest = service.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
                // Fix - use the correct enum value from ValueInputOption
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                var appendResponse = await appendRequest.ExecuteAsync();

                return appendResponse != null;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error saving to Google Sheets: {ex.Message}");
                return false;
            }
        }
    }
}