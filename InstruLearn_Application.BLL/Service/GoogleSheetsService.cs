using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Models.DTO.Certification;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class GoogleSheetsService : IGoogleSheetsService
    {
        private readonly IConfiguration _configuration;
        private readonly string _spreadsheetId;
        private readonly string _credentialsPath;
        private readonly ILogger<GoogleSheetsService> _logger;

        public GoogleSheetsService(IConfiguration configuration, ILogger<GoogleSheetsService> logger = null)
        {
            _configuration = configuration;
            _logger = logger;
            _spreadsheetId = _configuration["GoogleSheets:SpreadsheetId"];

            string configPath = _configuration["GoogleSheets:CredentialsPath"];

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetCurrentDirectory();

            var possiblePaths = new List<string>
            {
                configPath,
                Path.Combine(baseDirectory, configPath),
                Path.Combine(projectDirectory, configPath),
                Path.Combine(baseDirectory, "Credentials", "credentials.json"),
                Path.Combine(projectDirectory, "Credentials", "credentials.json"),
                @"E:/credentials.json",
                Path.Combine(Directory.GetParent(baseDirectory).FullName, "InstruLearn_Application.Model", "Credentials", "credentials.json")
            };

            string foundPath = null;
            foreach (var path in possiblePaths)
            {
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    foundPath = path;
                    break;
                }
            }

            if (foundPath != null)
            {
                _credentialsPath = foundPath;
                LogInfo($"Using credentials path: {_credentialsPath}");
            }
            else
            {
                var errorMessage = $"Google credentials file not found. Tried paths: {string.Join(", ", possiblePaths.Where(p => !string.IsNullOrEmpty(p)))}";
                LogError(errorMessage);
                throw new FileNotFoundException(errorMessage);
            }
        }

        private void LogInfo(string message)
        {
            if (_logger != null)
            {
                _logger.LogInformation(message);
            }
            else
            {
                Console.WriteLine($"INFO: {message}");
            }
        }

        private void LogError(string message, Exception ex = null)
        {
            if (_logger != null)
            {
                if (ex != null)
                {
                    _logger.LogError(ex, message);
                }
                else
                {
                    _logger.LogError(message);
                }
            }
            else
            {
                Console.WriteLine($"ERROR: {message}");
                if (ex != null)
                {
                    Console.WriteLine($"Exception: {ex}");
                }
            }
        }

        public async Task<bool> SaveCertificationDataAsync(CertificationDataDTO certificationData)
        {
            LogInfo($"Starting to save certification data for ID {certificationData.CertificationId}");
            try
            {
                GoogleCredential credential;
                try
                {
                    LogInfo($"Loading credentials from: {_credentialsPath}");
                    using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
                    {
                        credential = GoogleCredential.FromStream(stream)
                            .CreateScoped(SheetsService.Scope.Spreadsheets);
                    }
                    LogInfo("Credentials loaded successfully");
                }
                catch (Exception ex)
                {
                    LogError($"Error loading credentials file from {_credentialsPath}", ex);
                    throw new Exception($"Failed to load Google credentials: {ex.Message}", ex);
                }

                try
                {
                    LogInfo("Creating Google Sheets service...");
                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = "InstruLearn Certificate Tracking"
                    });

                    var range = "Certificates!A:H";
                    var valueRange = new ValueRange
                    {
                        Values = new List<IList<object>>
                        {
                            new List<object>
                            {
                                certificationData.CertificationId,
                                certificationData.LearnerName,
                                certificationData.LearnerEmail ?? "N/A",
                                certificationData.CertificationType,
                                certificationData.CertificationName,
                                certificationData.IssueDate.ToString("yyyy-MM-dd HH:mm:ss"),
                                certificationData.TeacherName ?? "N/A",
                                certificationData.Subject ?? "N/A"
                            }
                        }
                    };

                    LogInfo($"Checking if spreadsheet with ID {_spreadsheetId} exists...");

                    try
                    {
                        var spreadsheet = await service.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();
                        LogInfo($"Successfully connected to spreadsheet: {spreadsheet.Properties.Title}");

                        bool sheetExists = spreadsheet.Sheets.Any(s => s.Properties.Title == "Certificates");

                        if (!sheetExists)
                        {
                            LogInfo("Certificates sheet not found, creating it...");
                            var addSheetRequest = new Request
                            {
                                AddSheet = new AddSheetRequest
                                {
                                    Properties = new SheetProperties
                                    {
                                        Title = "Certificates"
                                    }
                                }
                            };

                            var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
                            {
                                Requests = new List<Request> { addSheetRequest }
                            };

                            await service.Spreadsheets.BatchUpdate(batchUpdateRequest, _spreadsheetId).ExecuteAsync();
                            LogInfo("Created 'Certificates' sheet");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error accessing spreadsheet or creating sheet", ex);
                        throw new Exception($"Error accessing Google Sheets: {ex.Message}", ex);
                    }

                    try
                    {
                        LogInfo("Checking for headers...");
                        var getRequest = service.Spreadsheets.Values.Get(_spreadsheetId, "Certificates!A1:H1");
                        ValueRange getResponse = await getRequest.ExecuteAsync();

                        if (getResponse.Values == null || getResponse.Values.Count == 0)
                        {
                            LogInfo("Headers don't exist, creating them...");
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
                            var headerResponse = await headerUpdateRequest.ExecuteAsync();
                            LogInfo($"Headers created successfully, updated {headerResponse.UpdatedCells} cells");
                        }
                        else
                        {
                            LogInfo("Headers already exist");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error checking/setting headers", ex);
                    }

                    try
                    {
                        LogInfo("Appending certification data...");
                        var appendRequest = service.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
                        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
                        var appendResponse = await appendRequest.ExecuteAsync();

                        if (appendResponse != null && appendResponse.Updates != null)
                        {
                            LogInfo($"Successfully wrote data to Google Sheets. Updated range: {appendResponse.Updates.UpdatedRange}, Updated cells: {appendResponse.Updates.UpdatedCells}");
                            return true;
                        }
                        else
                        {
                            LogError("Append operation returned null response or null updates");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error appending data to Google Sheets", ex);
                        throw new Exception($"Failed to append data to Google Sheets: {ex.Message}", ex);
                    }
                }
                catch (Exception ex)
                {
                    LogError("Error initializing Google Sheets service", ex);
                    throw new Exception($"Error initializing Google Sheets service: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                LogError("Error saving to Google Sheets", ex);

                if (ex.InnerException != null)
                {
                    LogError($"Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }
    }
}