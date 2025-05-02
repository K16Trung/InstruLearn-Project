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
                    if (!File.Exists(_credentialsPath))
                    {
                        LogError($"Credentials file not found at {_credentialsPath}");
                        return false;
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
                    return false;
                }

                try
                {
                    LogInfo("Creating Google Sheets service...");
                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = _applicationName
                    });

                    var range = $"{_sheetName}!A:J";
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
                        certificationData.Subject ?? "N/A",
                        certificationData.FileStatus ?? string.Empty,
                        certificationData.FileLink ?? string.Empty
                    }
                }
                    };

                    LogInfo($"Checking if spreadsheet with ID {_spreadsheetId} exists...");

                    try
                    {
                        var spreadsheet = await service.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();
                        LogInfo($"Successfully connected to spreadsheet: {spreadsheet.Properties.Title}");

                        bool sheetExists = spreadsheet.Sheets.Any(s => s.Properties.Title == _sheetName);

                        if (!sheetExists)
                        {
                            LogInfo($"{_sheetName} sheet not found, creating it...");
                            var addSheetRequest = new Request
                            {
                                AddSheet = new AddSheetRequest
                                {
                                    Properties = new SheetProperties
                                    {
                                        Title = _sheetName
                                    }
                                }
                            };

                            var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
                            {
                                Requests = new List<Request> { addSheetRequest }
                            };

                            await service.Spreadsheets.BatchUpdate(batchUpdateRequest, _spreadsheetId).ExecuteAsync();
                            LogInfo($"Created '{_sheetName}' sheet");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error accessing spreadsheet or creating sheet", ex);
                        if (ex is Google.GoogleApiException gex && gex.Error?.Code == 404)
                        {
                            LogError($"Spreadsheet with ID {_spreadsheetId} not found. Please check your configuration.");
                        }
                        return false;
                    }

                    try
                    {
                        LogInfo("Checking for headers...");
                        var getRequest = service.Spreadsheets.Values.Get(_spreadsheetId, $"{_sheetName}!A1:J1");
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
                                "Subject",
                                "File Status",
                                "File Link"
                            }
                        }
                    };

                            var headerUpdateRequest = service.Spreadsheets.Values.Update(headerRange, _spreadsheetId, $"{_sheetName}!A1:J1");
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
                        return false;
                    }

                    try
                    {
                        LogInfo("Appending certification data...");
                        LogInfo($"DEBUG: About to execute append operation for spreadsheetId={_spreadsheetId}, range={range}");
                        LogInfo($"DEBUG: Data being written: ID={certificationData.CertificationId}, Name={certificationData.LearnerName}");

                        var appendRequest = service.Spreadsheets.Values.Append(valueRange, _spreadsheetId, range);
                        appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
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
                    catch (Google.GoogleApiException gex)
                    {
                        LogError($"Google API Error during append: Code={gex.Error?.Code}, Message={gex.Error?.Message}", gex);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        LogError("Error appending data to Google Sheets", ex);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    LogError("Error initializing Google Sheets service", ex);
                    return false;
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

        public async Task<Dictionary<string, object>> TestGoogleSheetsConnectionAsync()
        {
            LogInfo("Starting Google Sheets connection test");

            var result = new Dictionary<string, object>
            {
                ["SpreadsheetId"] = _spreadsheetId,
                ["CredentialsPath"] = _credentialsPath,
                ["CredentialsFileExists"] = File.Exists(_credentialsPath),
                ["SheetName"] = _sheetName,
                ["AppName"] = _applicationName,
                ["Success"] = false,
                ["BaseDirectory"] = AppDomain.CurrentDomain.BaseDirectory,
                ["CurrentDirectory"] = Directory.GetCurrentDirectory()
            };

            try
            {
                if (!File.Exists(_credentialsPath))
                {
                    result["Error"] = $"Credentials file not found at {_credentialsPath}";
                    LogError((string)result["Error"]);
                    return result;
                }

                GoogleCredential credential;
                try
                {
                    LogInfo($"Loading credentials from {_credentialsPath}");
                    using (var stream = new FileStream(_credentialsPath, FileMode.Open, FileAccess.Read))
                    {
                        credential = GoogleCredential.FromStream(stream)
                            .CreateScoped(SheetsService.Scope.Spreadsheets);
                    }
                    result["CredentialsLoaded"] = true;
                    LogInfo("Credentials loaded successfully");
                }
                catch (Exception ex)
                {
                    result["Error"] = $"Failed to load credentials: {ex.Message}";
                    LogError((string)result["Error"], ex);
                    return result;
                }

                try
                {
                    LogInfo("Creating Google Sheets service");
                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = _applicationName
                    });
                    result["ServiceCreated"] = true;
                    LogInfo("Google Sheets service created successfully");

                    LogInfo($"Attempting to access spreadsheet with ID: {_spreadsheetId}");
                    var spreadsheet = await service.Spreadsheets.Get(_spreadsheetId).ExecuteAsync();

                    result["SpreadsheetTitle"] = spreadsheet.Properties.Title;
                    result["SheetCount"] = spreadsheet.Sheets.Count;
                    result["Sheets"] = string.Join(", ", spreadsheet.Sheets.Select(s => s.Properties.Title));
                    result["Success"] = true;

                    LogInfo($"Successfully connected to spreadsheet: {spreadsheet.Properties.Title}");
                    LogInfo($"Sheets found: {result["Sheets"]}");

                    try
                    {
                        LogInfo("Testing write permission...");
                        var testRange = $"{_sheetName}!A1:A1";

                        try
                        {
                            var readRequest = service.Spreadsheets.Values.Get(_spreadsheetId, testRange);
                            await readRequest.ExecuteAsync();
                        }
                        catch (Google.GoogleApiException gex) when (gex.Error?.Message?.Contains("Unable to parse range") == true)
                        {
                            LogInfo($"Sheet '{_sheetName}' not found, creating it");
                            var addSheetRequest = new Request
                            {
                                AddSheet = new AddSheetRequest
                                {
                                    Properties = new SheetProperties
                                    {
                                        Title = _sheetName
                                    }
                                }
                            };

                            var batchUpdateRequest = new BatchUpdateSpreadsheetRequest
                            {
                                Requests = new List<Request> { addSheetRequest }
                            };

                            await service.Spreadsheets.BatchUpdate(batchUpdateRequest, _spreadsheetId).ExecuteAsync();
                        }

                        var testValue = new ValueRange
                        {
                            Values = new List<IList<object>>
                    {
                        new List<object> { "Test Cell - " + DateTime.Now.ToString() }
                    }
                        };

                        var updateRequest = service.Spreadsheets.Values.Update(testValue, _spreadsheetId, testRange);
                        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                        await updateRequest.ExecuteAsync();

                        result["WritePermissionTest"] = "Success";
                        LogInfo("Write permission test successful");
                    }
                    catch (Exception ex)
                    {
                        result["WritePermissionTest"] = $"Failed: {ex.Message}";
                        LogError("Write permission test failed", ex);
                    }

                    return result;
                }
                catch (Google.GoogleApiException gex)
                {
                    result["Error"] = gex.Message;
                    result["ErrorType"] = "GoogleApiException";
                    result["ApiErrorCode"] = gex.Error?.Code;
                    result["ApiErrorMessage"] = gex.Error?.Message;

                    LogError($"Google API Error: Code={gex.Error?.Code}, Message={gex.Error?.Message}", gex);

                    return result;
                }
                catch (Exception ex)
                {
                    result["Error"] = ex.Message;
                    result["ErrorType"] = ex.GetType().Name;
                    LogError("Error testing Google Sheets connection", ex);
                    return result;
                }
            }
            catch (Exception ex)
            {
                result["Error"] = ex.Message;
                result["ErrorType"] = ex.GetType().Name;
                result["StackTrace"] = ex.StackTrace;
                LogError("Unexpected error during Google Sheets test", ex);
                return result;
            }
        }

        public async Task<List<CertificationDataDTO>> GetAllCertificatesAsync()
        {
            LogInfo("Starting to fetch all certificates from Google Sheets");
            var certificates = new List<CertificationDataDTO>();

            try
            {
                GoogleCredential credential;
                try
                {
                    if (!File.Exists(_credentialsPath))
                    {
                        LogError($"Credentials file not found at {_credentialsPath}");
                        return certificates;
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
                    return certificates;
                }

                try
                {
                    LogInfo("Creating Google Sheets service...");
                    var service = new SheetsService(new BaseClientService.Initializer()
                    {
                        HttpClientInitializer = credential,
                        ApplicationName = _applicationName
                    });

                    var range = $"{_sheetName}!A2:J"; // Start from row 2 to skip headers
                    LogInfo($"Fetching data from range: {range}");

                    var request = service.Spreadsheets.Values.Get(_spreadsheetId, range);
                    var response = await request.ExecuteAsync();

                    if (response == null || response.Values == null || response.Values.Count == 0)
                    {
                        LogInfo("No certificate data found in the spreadsheet");
                        return certificates;
                    }

                    LogInfo($"Found {response.Values.Count} certificates in the spreadsheet");

                    foreach (var row in response.Values)
                    {
                        try
                        {
                            // Ensure the row has enough columns
                            if (row.Count < 8)
                            {
                                LogWarning($"Row has insufficient columns: {row.Count}. Expected at least 8 columns.");
                                continue;
                            }

                            // Parse certificate ID
                            if (!int.TryParse(row[0]?.ToString(), out int certId))
                            {
                                LogWarning($"Could not parse Certificate ID: {row[0]}");
                                continue;
                            }

                            // Parse issue date
                            DateTime issueDate;
                            if (!DateTime.TryParse(row[5]?.ToString(), out issueDate))
                            {
                                LogWarning($"Could not parse Issue Date: {row[5]}");
                                issueDate = DateTime.MinValue; // Default value
                            }

                            var certificate = new CertificationDataDTO
                            {
                                CertificationId = certId,
                                LearnerName = row[1]?.ToString() ?? "Unknown",
                                LearnerEmail = row[2]?.ToString() ?? "Unknown",
                                CertificationType = row[3]?.ToString() ?? "Unknown",
                                CertificationName = row[4]?.ToString() ?? "Unknown",
                                IssueDate = issueDate,
                                TeacherName = row[6]?.ToString() ?? "N/A",
                                Subject = row[7]?.ToString() ?? "N/A",
                                FileStatus = row.Count > 8 ? row[8]?.ToString() ?? string.Empty : string.Empty,
                                FileLink = row.Count > 9 ? row[9]?.ToString() ?? string.Empty : string.Empty
                            };

                            certificates.Add(certificate);
                        }
                        catch (Exception ex)
                        {
                            LogError($"Error parsing certificate row: {ex.Message}", ex);
                            // Continue to next row
                        }
                    }

                    LogInfo($"Successfully parsed {certificates.Count} certificates");
                    return certificates;
                }
                catch (Google.GoogleApiException gex)
                {
                    LogError($"Google API Error during fetch: Code={gex.Error?.Code}, Message={gex.Error?.Message}", gex);
                    return certificates;
                }
                catch (Exception ex)
                {
                    LogError("Error fetching data from Google Sheets", ex);
                    return certificates;
                }
            }
            catch (Exception ex)
            {
                LogError("Error getting certificates from Google Sheets", ex);
                if (ex.InnerException != null)
                {
                    LogError($"Inner Exception: {ex.InnerException.Message}");
                }
                return certificates;
            }
        }

    }
}