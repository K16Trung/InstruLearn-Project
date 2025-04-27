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
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class GoogleSheetsService : IGoogleSheetsService
    {
        private readonly IConfiguration _configuration;
        private readonly string _spreadsheetId;
        private readonly string _credentialsPath;
        private readonly string _applicationName;
        private readonly string _sheetName;
        private readonly ILogger<GoogleSheetsService> _logger;
        private readonly bool _ignoreErrors;

        public GoogleSheetsService(IConfiguration configuration, ILogger<GoogleSheetsService> logger = null)
        {
            _configuration = configuration;
            _logger = logger;

            var configSpreadsheetId = _configuration["GoogleSheets:SpreadsheetId"];
            if (configSpreadsheetId != null && configSpreadsheetId.Contains("spreadsheets/d/"))
            {
                var parts = configSpreadsheetId.Split(new[] { "spreadsheets/d/" }, StringSplitOptions.None);
                if (parts.Length > 1)
                {
                    var idPart = parts[1];
                    _spreadsheetId = idPart.Split('/')[0];
                }
                else
                {
                    _spreadsheetId = configSpreadsheetId;
                }
            }
            else
            {
                _spreadsheetId = configSpreadsheetId;
            }

            LogInfo($"Using spreadsheet ID: {_spreadsheetId}");

            _ignoreErrors = bool.TryParse(_configuration["GoogleSheets:IgnoreErrors"], out bool ignoreErrors) && ignoreErrors;
            _applicationName = _configuration["GoogleSheets:ApplicationName"] ?? "InstruLearn Certificate Tracking";
            _sheetName = _configuration["GoogleSheets:SheetName"] ?? "Certificates";

            string configPath = _configuration["GoogleSheets:CredentialsPath"];

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string projectDirectory = Directory.GetCurrentDirectory();

            LogInfo($"Base Directory: {baseDirectory}");
            LogInfo($"Project Directory: {projectDirectory}");
            LogInfo($"Config Path: {configPath}");

            var possiblePaths = new List<string>
            {
                configPath,
                Path.Combine(baseDirectory, configPath),
                Path.Combine(projectDirectory, configPath),
                Path.Combine(baseDirectory, "Credentials", "credentials.json"),
                Path.Combine(projectDirectory, "Credentials", "credentials.json"),
                Path.Combine(Directory.GetParent(baseDirectory)?.FullName ?? "", "Credentials", "credentials.json"),
                Path.Combine(Directory.GetParent(baseDirectory)?.FullName ?? "", "InstruLearn_Application.Model", "Credentials", "credentials.json")
            };

            LogInfo($"Searching for credentials file in the following locations: {string.Join(", ", possiblePaths)}");

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
            else if (_ignoreErrors)
            {
                _credentialsPath = configPath;
                LogWarning($"Credentials file not found at any path, but errors are ignored. Will use: {_credentialsPath}");
            }
            else
            {
                var errorMessage = $"Google credentials file not found. Tried paths: {string.Join(", ", possiblePaths.Where(p => !string.IsNullOrEmpty(p)))}";
                LogError(errorMessage);
                throw new FileNotFoundException(errorMessage);
            }

            LogInfo($"GoogleSheetsService initialized with SpreadsheetId: {_spreadsheetId}, ApplicationName: {_applicationName}, IgnoreErrors: {_ignoreErrors}");
        }

        private void LogInfo(string message)
        {
            _logger?.LogInformation(message);
            Console.WriteLine($"INFO: {message}");
        }

        private void LogWarning(string message)
        {
            _logger?.LogWarning(message);
            Console.WriteLine($"WARNING: {message}");
        }

        private void LogError(string message, Exception ex = null)
        {
            if (ex != null)
            {
                _logger?.LogError(ex, message);
                Console.WriteLine($"ERROR: {message}\nException: {ex}");

                if (ex is Google.GoogleApiException apiEx)
                {
                    Console.WriteLine($"Google API Error: Code={apiEx.Error?.Code}, Message={apiEx.Error?.Message}");
                }

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
            else
            {
                _logger?.LogError(message);
                Console.WriteLine($"ERROR: {message}");
            }
        }

        public async Task<bool> SaveCertificationDataAsync(CertificationDataDTO certificationData)
        {
            LogInfo($"Starting to save certification data for ID {certificationData.CertificationId}");
            LogInfo($"Using spreadsheet ID: {_spreadsheetId}");
            LogInfo($"Credentials path: {_credentialsPath}, Exists: {File.Exists(_credentialsPath)}");

            try
            {
                GoogleCredential credential;
                try
                {
                    if (!File.Exists(_credentialsPath) && _ignoreErrors)
                    {
                        LogWarning($"Credentials file not found at {_credentialsPath}, but errors are ignored. Skipping Google Sheets update.");
                        return true;
                    }

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
                    if (_ignoreErrors)
                    {
                        LogWarning("Ignoring credentials error as configured");
                        return true;
                    }
                    throw new Exception($"Failed to load Google credentials: {ex.Message}", ex);
                }

                try
                {
                    LogInfo("Creating Google Sheets service...");
                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = _applicationName
                    });

                    var range = "Certificates!A:H";
                    var valueRange = new ValueRange
                    {
                        Values = new List<IList<object>>
                        {
                            new List<object>
                            {
                                certificationData.CertificationId,
                                certificationData.LearnerName ?? "Unknown",
                                certificationData.LearnerEmail ?? "Unknown",
                                certificationData.CertificationType ?? "Unknown",
                                certificationData.CertificationName ?? "Unknown",
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
                        if (_ignoreErrors)
                        {
                            LogWarning("Ignoring spreadsheet access error as configured");
                            return true;
                        }
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
                        if (_ignoreErrors)
                        {
                            LogWarning("Ignoring headers error as configured");
                        }
                        else
                        {
                            throw;
                        }
                    }

                    try
                    {
                        LogInfo("Appending certification data...");

                        LogInfo($"DEBUG: About to execute append operation for spreadsheetId={_spreadsheetId}, range={range}");
                        LogInfo($"DEBUG: Data being written: ID={certificationData.CertificationId}, Name={certificationData.LearnerName}");

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
                            return !_ignoreErrors;
                        }
                    }
                    catch (Google.GoogleApiException gex)
                    {
                        LogError($"Google API Error during append: Code={gex.Error?.Code}, Message={gex.Error?.Message}", gex);
                        if (_ignoreErrors)
                        {
                            LogWarning("Ignoring Google API error as configured");
                            return true;
                        }
                        throw;
                    }
                    catch (Exception ex)
                    {
                        LogError("Error appending data to Google Sheets", ex);
                        if (_ignoreErrors)
                        {
                            LogWarning("Ignoring append error as configured");
                            return true;
                        }
                        throw new Exception($"Failed to append data to Google Sheets: {ex.Message}", ex);
                    }
                }
                catch (Exception ex)
                {
                    LogError("Error initializing Google Sheets service", ex);
                    if (_ignoreErrors)
                    {
                        LogWarning("Ignoring service initialization error as configured");
                        return true;
                    }
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

                if (_ignoreErrors)
                {
                    LogWarning("Ignoring Google Sheets error as configured");
                    return true;
                }
                return false;
            }
        }

        public async Task<Dictionary<string, object>> TestGoogleSheetsConnectionAsync()
        {
            var result = new Dictionary<string, object>
            {
                ["SpreadsheetId"] = _spreadsheetId,
                ["CredentialsPath"] = _credentialsPath,
                ["CredentialsFileExists"] = File.Exists(_credentialsPath),
                ["Success"] = false
            };

            try
            {
                GoogleCredential credential;
                using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
                {
                    credential = GoogleCredential.FromStream(stream)
                        .CreateScoped(SheetsService.Scope.Spreadsheets);
                }
                result["CredentialsLoaded"] = true;

                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = _applicationName
                });
                result["ServiceCreated"] = true;

                var spreadsheet = await service.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();
                result["SpreadsheetTitle"] = spreadsheet.Properties.Title;
                result["SheetCount"] = spreadsheet.Sheets.Count;
                result["Sheets"] = string.Join(", ", spreadsheet.Sheets.Select(s => s.Properties.Title));
                result["Success"] = true;

                return result;
            }
            catch (Exception ex)
            {
                result["Error"] = ex.Message;
                result["ErrorType"] = ex.GetType().Name;

                if (ex is Google.GoogleApiException apiEx)
                {
                    result["ApiErrorCode"] = apiEx.Error?.Code;
                    result["ApiErrorMessage"] = apiEx.Error?.Message;
                }

                return result;
            }
        }
    }
}