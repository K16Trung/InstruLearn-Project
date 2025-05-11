// Fixed code for InstruLearn_Application.BLL/Service/RevenueService.cs

using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class RevenueService : IRevenueService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RevenueService> _logger;

        public RevenueService(IUnitOfWork unitOfWork, ILogger<RevenueService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ResponseDTO> GetTotalRevenueAsync()
        {
            try
            {
                _logger.LogInformation("Calculating total revenue across all types");

                var successfulTransactions = await _unitOfWork.WalletTransactionRepository
                    .GetQuery()
                    .Where(t => t.Status == TransactionStatus.Complete &&
                               t.TransactionType == TransactionType.Payment)
                    .ToListAsync();

                decimal totalRevenue = successfulTransactions.Sum(t => t.Amount);

                var (oneOnOneRegistrations, oneOnOneCount) = await GetOneOnOneRegistrationRevenueAsync();
                var (centerClassRegistrations, centerClassCount) = await GetCenterClassRegistrationRevenueAsync();
                var (courseRevenue, courseCount) = await GetCourseRevenueAsync();

                decimal reservationFees = await GetReservationFeesAsync();

                var (phase40Payments, phase60Payments) = await GetOneOnOnePaymentPhasesAsync();

                var centerClassInitialPayments = await GetCenterClassInitialPaymentsAsync();

                var dailyRevenue = await GetDailyRevenueAsync(DateTime.Now.AddDays(-30), DateTime.Now);
                var weeklyRevenue = await GetWeeklyRevenueAsync(DateTime.Now.AddDays(-90), DateTime.Now);
                var monthlyRevenue = await GetMonthlyRevenueForYearAsync(DateTime.Now.Year);
                var yearlyRevenue = await GetYearlyRevenueAsync(5); 

                dynamic phase40PaymentsObj = phase40Payments;
                dynamic phase60PaymentsObj = phase60Payments;
                dynamic centerClassInitialPaymentsObj = centerClassInitialPayments;

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Tổng doanh thu được tính toán thành công.",
                    Data = new
                    {
                        Overview = new
                        {
                            TotalRevenue = totalRevenue,
                            TransactionCount = successfulTransactions.Count,
                            LastUpdated = DateTime.Now
                        },
                        DetailedRevenue = new
                        {
                            OneOnOneRegistrationRevenue = ((dynamic)oneOnOneRegistrations).TotalRevenue,
                            OneOnOneRegistrationCount = oneOnOneCount,
                            CenterClassRevenue = ((dynamic)centerClassRegistrations).TotalRevenue,
                            CenterClassCount = centerClassCount,
                            CourseRevenue = ((dynamic)courseRevenue).TotalRevenue,
                            CourseCount = courseCount
                        },
                        ReservationFees = new
                        {
                            TotalAmount = reservationFees,
                            Count = (int)(reservationFees / 50000)
                        },
                        OneOnOnePaymentPhases = new
                        {
                            Phase40Payments = phase40PaymentsObj,
                            Phase60Payments = phase60PaymentsObj,
                            TotalAmount = phase40PaymentsObj.TotalAmount + phase60PaymentsObj.TotalAmount
                        },
                        CenterClassPayments = new
                        {
                            InitialPayments = centerClassInitialPaymentsObj,
                            TotalAmount = centerClassInitialPaymentsObj.TotalAmount
                        },
                        TimePeriods = new
                        {
                            Daily = dailyRevenue.Take(7),
                            Weekly = weeklyRevenue.Take(8),
                            Monthly = monthlyRevenue,
                            Yearly = yearlyRevenue
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating total revenue");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi tính toán tổng doanh thu: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetMonthlyRevenueAsync(int year)
        {
            try
            {
                _logger.LogInformation($"Getting monthly revenue for year {year}");

                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31, 23, 59, 59);

                var transactions = await _unitOfWork.WalletTransactionRepository
                    .GetQuery()
                    .Where(t => t.Status == TransactionStatus.Complete &&
                               t.TransactionType == TransactionType.Payment &&
                               t.TransactionDate.Year == year)
                    .ToListAsync();

                var monthlyData = await GetMonthlyRevenueForYearAsync(year);

                var yearRevenueByType = await GetRevenueByTypeForTimeRangeAsync(startDate, endDate);

                var weeklyData = await GetWeeklyRevenueAsync(startDate, endDate);

                var quarterlyData = ((IEnumerable<dynamic>)monthlyData)
                    .GroupBy(m => ((int)m.Month - 1) / 3 + 1)
                    .Select(g => new
                    {
                        Quarter = g.Key,
                        Revenue = g.Sum(m => (decimal)m.Revenue),
                        TransactionCount = g.Sum(m => (int)m.TransactionCount)
                    })
                    .ToList();

                var oneOnOneByMonth = await GetOneOnOneRegistrationRevenueByMonthAsync(year);

                var centerClassByMonth = await GetCenterClassRegistrationRevenueByMonthAsync(year);

                var courseByMonth = await GetCourseRevenueByMonthAsync(year);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Doanh thu theo tháng cho năm {year} được tính toán thành công.",
                    Data = new
                    {
                        Year = year,
                        TotalRevenue = transactions.Sum(t => t.Amount),
                        TotalTransactions = transactions.Count,
                        ByTimeUnit = new
                        {
                            Monthly = monthlyData,
                            Quarterly = quarterlyData,
                            Weekly = weeklyData
                        },
                        ByRevenueType = new
                        {
                            FullYear = yearRevenueByType,
                            MonthlyBreakdown = new
                            {
                                OneOnOneRegistrations = oneOnOneByMonth,
                                CenterClassRegistrations = centerClassByMonth,
                                CoursePurchases = courseByMonth
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating monthly revenue for year {year}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi tính toán doanh thu theo tháng: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetWeeklyRevenueAsync(int year, int weekNumber)
        {
            try
            {
                _logger.LogInformation($"Getting weekly revenue for Year {year}, Week {weekNumber}");

                DateTime jan1 = new DateTime(year, 1, 1);
                int daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
                DateTime firstMonday = jan1.AddDays(daysOffset > 0 ? daysOffset : daysOffset + 7);

                DateTime weekStart = firstMonday.AddDays((weekNumber - 1) * 7);
                DateTime weekEnd = weekStart.AddDays(6);

                var transactions = await _unitOfWork.WalletTransactionRepository
                    .GetQuery()
                    .Where(t => t.Status == TransactionStatus.Complete &&
                               t.TransactionType == TransactionType.Payment &&
                               t.TransactionDate >= weekStart &&
                               t.TransactionDate <= weekEnd)
                    .ToListAsync();

                var dailyData = transactions
                    .GroupBy(t => t.TransactionDate.DayOfWeek)
                    .Select(g => new
                    {
                        DayOfWeek = g.Key.ToString(),
                        Revenue = g.Sum(t => t.Amount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(x => (int)Enum.Parse<DayOfWeek>(x.DayOfWeek))
                    .ToList();

                var completeDailyData = Enumerable.Range(0, 7)
                    .Select(i =>
                    {
                        var day = (DayOfWeek)i;
                        var existingDay = dailyData.FirstOrDefault(d => d.DayOfWeek == day.ToString());
                        return existingDay ?? new
                        {
                            DayOfWeek = day.ToString(),
                            Revenue = 0m,
                            TransactionCount = 0
                        };
                    })
                    .ToList();

                var (oneOnOneDetails, oneOnOneCount) = await GetOneOnOneRegistrationRevenueAsync(weekStart, weekEnd);
                var (centerClassDetails, centerClassCount) = await GetCenterClassRegistrationRevenueAsync(weekStart, weekEnd);
                var (courseDetails, courseCount) = await GetCourseRevenueAsync(weekStart, weekEnd);

                decimal reservationFees = await GetReservationFeesAsync(weekStart, weekEnd);

                var (phase40Payments, phase60Payments) = await GetOneOnOnePaymentPhasesAsync(weekStart, weekEnd);
                var centerClassInitialPayments = await GetCenterClassInitialPaymentsAsync(weekStart, weekEnd);

                dynamic phase40PaymentsObj = phase40Payments;
                dynamic phase60PaymentsObj = phase60Payments;
                dynamic centerClassInitialPaymentsObj = centerClassInitialPayments;

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Doanh thu cho tuần {weekNumber} năm {year} được tính toán thành công.",
                    Data = new
                    {
                        WeekInfo = new
                        {
                            Year = year,
                            WeekNumber = weekNumber,
                            StartDate = weekStart.ToString("dd/MM/yyyy"),
                            EndDate = weekEnd.ToString("dd/MM/yyyy")
                        },
                        Summary = new
                        {
                            TotalRevenue = transactions.Sum(t => t.Amount),
                            TransactionCount = transactions.Count,
                            DailyAverage = transactions.Any() ? transactions.Sum(t => t.Amount) / 7 : 0
                        },
                        DailyBreakdown = completeDailyData,
                        DetailedRevenue = new
                        {
                            OneOnOneRegistrations = oneOnOneDetails,
                            CenterClassRegistrations = centerClassDetails,
                            CoursePurchases = courseDetails,
                            ReservationFees = new
                            {
                                TotalAmount = reservationFees,
                                Count = (int)(reservationFees / 50000)
                            },
                            OneOnOnePaymentPhases = new
                            {
                                Phase40Payments = phase40PaymentsObj,
                                Phase60Payments = phase60PaymentsObj,
                                TotalAmount = phase40PaymentsObj.TotalAmount + phase60PaymentsObj.TotalAmount
                            },
                            CenterClassPayments = new
                            {
                                InitialPayments = centerClassInitialPaymentsObj,
                                TotalAmount = centerClassInitialPaymentsObj.TotalAmount
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating weekly revenue for Year {year}, Week {weekNumber}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi tính toán doanh thu theo tuần: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetDailyRevenueAsync(DateTime date)
        {
            try
            {
                _logger.LogInformation($"Getting daily revenue for {date:yyyy-MM-dd}");

                DateTime startTime = date.Date;
                DateTime endTime = date.Date.AddDays(1).AddMilliseconds(-1);

                var transactions = await _unitOfWork.WalletTransactionRepository
                    .GetQuery()
                    .Where(t => t.Status == TransactionStatus.Complete &&
                               t.TransactionType == TransactionType.Payment &&
                               t.TransactionDate >= startTime &&
                               t.TransactionDate <= endTime)
                    .ToListAsync();

                var hourlyData = transactions
                    .GroupBy(t => t.TransactionDate.Hour)
                    .Select(g => new
                    {
                        Hour = g.Key,
                        TimeDisplay = $"{g.Key:D2}:00 - {g.Key:D2}:59",
                        Revenue = g.Sum(t => t.Amount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(h => h.Hour)
                    .ToList();

                var completeHourlyData = Enumerable.Range(0, 24)
                    .Select(hour =>
                    {
                        var existingHour = hourlyData.FirstOrDefault(h => h.Hour == hour);
                        return existingHour ?? new
                        {
                            Hour = hour,
                            TimeDisplay = $"{hour:D2}:00 - {hour:D2}:59",
                            Revenue = 0m,
                            TransactionCount = 0
                        };
                    })
                    .ToList();

                var (oneOnOneDetails, oneOnOneCount) = await GetOneOnOneRegistrationRevenueAsync(startTime, endTime);
                var (centerClassDetails, centerClassCount) = await GetCenterClassRegistrationRevenueAsync(startTime, endTime);
                var (courseDetails, courseCount) = await GetCourseRevenueAsync(startTime, endTime);

                decimal reservationFees = await GetReservationFeesAsync(startTime, endTime);

                var (phase40Payments, phase60Payments) = await GetOneOnOnePaymentPhasesAsync(startTime, endTime);
                var centerClassInitialPayments = await GetCenterClassInitialPaymentsAsync(startTime, endTime);

                dynamic phase40PaymentsObj = phase40Payments;
                dynamic phase60PaymentsObj = phase60Payments;
                dynamic centerClassInitialPaymentsObj = centerClassInitialPayments;

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Doanh thu cho ngày {date:dd/MM/yyyy} được tính toán thành công.",
                    Data = new
                    {
                        Date = date.ToString("dd/MM/yyyy"),
                        DayOfWeek = date.DayOfWeek.ToString(),
                        Summary = new
                        {
                            TotalRevenue = transactions.Sum(t => t.Amount),
                            TransactionCount = transactions.Count,
                            HourlyAverage = transactions.Any() ? transactions.Sum(t => t.Amount) / 24 : 0,
                            PeakHour = completeHourlyData
                                .OrderByDescending(h => h.Revenue)
                                .ThenByDescending(h => h.TransactionCount)
                                .FirstOrDefault()?.TimeDisplay
                        },
                        HourlyBreakdown = completeHourlyData,
                        DetailedRevenue = new
                        {
                            OneOnOneRegistrations = oneOnOneDetails,
                            CenterClassRegistrations = centerClassDetails,
                            CoursePurchases = courseDetails,
                            ReservationFees = new
                            {
                                TotalAmount = reservationFees,
                                Count = (int)Math.Round(reservationFees / 50000, 0)
                            },
                            OneOnOnePaymentPhases = new
                            {
                                Phase40Payments = phase40PaymentsObj,
                                Phase60Payments = phase60PaymentsObj,
                                TotalAmount = phase40PaymentsObj.TotalAmount + phase60PaymentsObj.TotalAmount
                            },
                            CenterClassPayments = new
                            {
                                InitialPayments = centerClassInitialPaymentsObj,
                                TotalAmount = centerClassInitialPaymentsObj.TotalAmount
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating daily revenue for {date:yyyy-MM-dd}");
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi tính toán doanh thu theo ngày: {ex.Message}"
                };
            }
        }

        #region Helper Methods

        private async Task<(object TotalRevenue, int Count)> GetOneOnOneRegistrationRevenueAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.PaymentsRepository
                .GetQuery()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.PaymentFor == PaymentFor.LearningRegistration);

            if (startDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate <= endDate.Value);
            }

            var payments = await query
                .Include(p => p.WalletTransaction)
                .ToListAsync();

            var oneOnOnePayments = payments;

            decimal totalRevenue = oneOnOnePayments.Sum(p => p.AmountPaid);
            int count = oneOnOnePayments.Count;

            return (new { TotalRevenue = totalRevenue, Count = count }, count);
        }



        private async Task<(object TotalRevenue, int Count)> GetCenterClassRegistrationRevenueAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.PaymentsRepository
                .GetQuery()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.PaymentFor == PaymentFor.LearningRegistration);

            if (startDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate <= endDate.Value);
            }

            var payments = await query
                .Include(p => p.WalletTransaction)
                .ToListAsync();

            var centerClassRegistrations = await _unitOfWork.LearningRegisRepository
                .GetQuery()
                .Where(lr => lr.ClassId != null)
                .Include(lr => lr.Learning_Registration_Type)
                .Include(lr => lr.Classes)
                .ToListAsync();

            decimal totalRevenue = payments.Sum(p => p.AmountPaid);
            int count = centerClassRegistrations.Count;

            return (new { TotalRevenue = totalRevenue, Count = count }, count);
        }

        private async Task<(object TotalRevenue, int Count)> GetCourseRevenueAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.PaymentsRepository
                .GetQuery()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.PaymentFor == PaymentFor.Online_Course);

            if (startDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate <= endDate.Value);
            }

            var coursePayments = await query
                .Include(p => p.WalletTransaction)
                .ToListAsync();

            decimal totalRevenue = coursePayments.Sum(p => p.AmountPaid);
            int count = coursePayments.Count;

            return (new { TotalRevenue = totalRevenue, Count = count }, count);
        }

        private async Task<decimal> GetReservationFeesAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.LearningRegisRepository
                .GetQuery()
                .Where(lr => lr.Status != LearningRegis.Rejected);

            if (startDate.HasValue)
            {
                query = query.Where(lr => lr.RequestDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(lr => lr.RequestDate.Date <= endDate.Value.Date);
            }

            var count = await query
                .Where(lr => lr.ClassId == null)
                .CountAsync();

            return count * 50000;
        }

        private async Task<(object Phase40, object Phase60)> GetOneOnOnePaymentPhasesAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.PaymentsRepository
                .GetQuery()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.PaymentFor == PaymentFor.LearningRegistration);

            if (startDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate <= endDate.Value);
            }

            var payments = await query
                .Include(p => p.WalletTransaction)
                .ToListAsync();

            var learningRegistrations = await _unitOfWork.LearningRegisRepository
                .GetQuery()
                .Where(lr => lr.ClassId == null)
                .ToListAsync();

            var averagePrice = learningRegistrations.Where(lr => lr.Price.HasValue).Average(lr => lr.Price ?? 0);
            var threshold = averagePrice / 2;

            var smallerPayments = payments
                .Where(p => p.AmountPaid < threshold)
                .Select(p => new
                {
                    AmountPaid = p.AmountPaid,
                    TransactionDate = p.WalletTransaction.TransactionDate
                })
                .ToList();

            var largerPayments = payments
                .Where(p => p.AmountPaid >= threshold)
                .Select(p => new
                {
                    AmountPaid = p.AmountPaid,
                    TransactionDate = p.WalletTransaction.TransactionDate
                })
                .ToList();

            return (
                new
                {
                    Count = smallerPayments.Count,
                    TotalAmount = smallerPayments.Sum(p => p.AmountPaid),
                    Payments = smallerPayments
                },
                new
                {
                    Count = largerPayments.Count,
                    TotalAmount = largerPayments.Sum(p => p.AmountPaid),
                    Payments = largerPayments
                }
            );
        }

        private async Task<object> GetCenterClassInitialPaymentsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.PaymentsRepository
                .GetQuery()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.PaymentFor == PaymentFor.LearningRegistration);

            if (startDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate <= endDate.Value);
            }

            var payments = await query
                .Include(p => p.WalletTransaction)
                .ToListAsync();

            var centerClassRegistrations = await _unitOfWork.LearningRegisRepository
                .GetQuery()
                .Where(lr => lr.ClassId != null)
                .Include(lr => lr.Classes)
                .ToListAsync();

            var initialPayments = payments
                .Where(p => p.AmountPaid <= 200000)
                .Select(p => new
                {
                    PaymentAmount = p.AmountPaid,
                    TransactionDate = p.WalletTransaction.TransactionDate
                })
                .ToList();

            return new
            {
                Count = initialPayments.Count,
                TotalAmount = initialPayments.Sum(p => p.PaymentAmount),
                Payments = initialPayments
            };
        }

        private async Task<List<object>> GetDailyRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = await _unitOfWork.WalletTransactionRepository
                .GetQuery()
                .Where(t => t.Status == TransactionStatus.Complete &&
                           t.TransactionType == TransactionType.Payment &&
                           t.TransactionDate >= startDate &&
                           t.TransactionDate <= endDate)
                .ToListAsync();

            var dailyRevenue = transactions
                .GroupBy(t => t.TransactionDate.Date)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    DayOfWeek = g.Key.DayOfWeek.ToString(),
                    Revenue = g.Sum(t => t.Amount),
                    TransactionCount = g.Count()
                })
                .OrderBy(d => DateTime.Parse(d.Date))
                .ToList<object>();

            return dailyRevenue;
        }

        private async Task<List<object>> GetWeeklyRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = await _unitOfWork.WalletTransactionRepository
                .GetQuery()
                .Where(t => t.Status == TransactionStatus.Complete &&
                           t.TransactionType == TransactionType.Payment &&
                           t.TransactionDate >= startDate &&
                           t.TransactionDate <= endDate)
                .ToListAsync();

            var weeklyRevenue = transactions
                .GroupBy(t =>
                {
                    DateTime firstDayOfYear = new DateTime(t.TransactionDate.Year, 1, 1);
                    int daysOffset = DayOfWeek.Monday - firstDayOfYear.DayOfWeek;
                    if (daysOffset > 0) daysOffset -= 7;
                    DateTime firstMonday = firstDayOfYear.AddDays(daysOffset);
                    int weekNumber = (int)Math.Ceiling((t.TransactionDate.Date - firstMonday).TotalDays / 7) + 1;
                    return new { Year = t.TransactionDate.Year, Week = weekNumber };
                })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    WeekNumber = g.Key.Week,
                    WeekStart = GetWeekStartDate(g.Key.Year, g.Key.Week).ToString("yyyy-MM-dd"),
                    WeekEnd = GetWeekStartDate(g.Key.Year, g.Key.Week).AddDays(6).ToString("yyyy-MM-dd"),
                    Revenue = g.Sum(t => t.Amount),
                    TransactionCount = g.Count()
                })
                .OrderBy(w => w.Year)
                .ThenBy(w => w.WeekNumber)
                .ToList<object>();

            return weeklyRevenue;
        }

        private async Task<List<object>> GetMonthlyRevenueAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = await _unitOfWork.WalletTransactionRepository
                .GetQuery()
                .Where(t => t.Status == TransactionStatus.Complete &&
                           t.TransactionType == TransactionType.Payment &&
                           t.TransactionDate >= startDate &&
                           t.TransactionDate <= endDate)
                .ToListAsync();

            var monthlyRevenue = transactions
                .GroupBy(t => new { Year = t.TransactionDate.Year, Month = t.TransactionDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM"),
                    Revenue = g.Sum(t => t.Amount),
                    TransactionCount = g.Count()
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList<object>();

            return monthlyRevenue;
        }

        private async Task<List<object>> GetMonthlyRevenueForYearAsync(int year)
        {
            var transactions = await _unitOfWork.WalletTransactionRepository
                .GetQuery()
                .Where(t => t.Status == TransactionStatus.Complete &&
                           t.TransactionType == TransactionType.Payment &&
                           t.TransactionDate.Year == year)
                .ToListAsync();

            var monthlyRevenueData = transactions
                .GroupBy(t => t.TransactionDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    MonthName = new DateTime(year, g.Key, 1).ToString("MMMM"),
                    Revenue = g.Sum(t => t.Amount),
                    TransactionCount = g.Count()
                })
                .OrderBy(m => m.Month)
                .ToList();

            var completeMonthlyRevenue = Enumerable.Range(1, 12)
                .Select(month =>
                {
                    var existingData = monthlyRevenueData.FirstOrDefault(m => m.Month == month);
                    return existingData ?? new
                    {
                        Month = month,
                        MonthName = new DateTime(year, month, 1).ToString("MMMM"),
                        Revenue = 0m,
                        TransactionCount = 0
                    };
                })
                .ToList<object>();

            return completeMonthlyRevenue;
        }

        private async Task<List<object>> GetYearlyRevenueAsync(int numberOfYears)
        {
            int currentYear = DateTime.Now.Year;
            int startYear = currentYear - numberOfYears + 1;

            var transactions = await _unitOfWork.WalletTransactionRepository
                .GetQuery()
                .Where(t => t.Status == TransactionStatus.Complete &&
                           t.TransactionType == TransactionType.Payment &&
                           t.TransactionDate.Year >= startYear &&
                           t.TransactionDate.Year <= currentYear)
                .ToListAsync();

            var yearlyRevenueData = transactions
                .GroupBy(t => t.TransactionDate.Year)
                .Select(g => new
                {
                    Year = g.Key,
                    Revenue = g.Sum(t => t.Amount),
                    TransactionCount = g.Count()
                })
                .OrderBy(y => y.Year)
                .ToList();

            var completeYearlyRevenue = Enumerable.Range(startYear, numberOfYears)
                .Select(year =>
                {
                    var existingData = yearlyRevenueData.FirstOrDefault(y => y.Year == year);
                    return existingData ?? new
                    {
                        Year = year,
                        Revenue = 0m,
                        TransactionCount = 0
                    };
                })
                .ToList<object>();

            return completeYearlyRevenue;
        }

        private async Task<List<object>> GetRevenueByTypeForTimeRangeAsync(DateTime startDate, DateTime endDate)
        {
            var payments = await _unitOfWork.PaymentsRepository
                .GetQuery()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.WalletTransaction.TransactionDate >= startDate &&
                           p.WalletTransaction.TransactionDate <= endDate)
                .Include(p => p.WalletTransaction)
                .ToListAsync();

            var revenueByType = payments
                .GroupBy(p => p.PaymentFor)
                .Select(g => new
                {
                    PaymentType = g.Key.ToString(),
                    PaymentTypeName = GetPaymentTypeDisplayName(g.Key),
                    Revenue = g.Sum(p => p.AmountPaid),
                    TransactionCount = g.Count()
                })
                .OrderByDescending(t => t.Revenue)
                .ToList<object>();

            return revenueByType;
        }

        private async Task<(List<object> Phase40, List<object> Phase60)> GetDetailedPaymentPhaseBreakdownAsync(
    DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.PaymentsRepository
                .GetQuery()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.PaymentFor == PaymentFor.LearningRegistration);

            if (startDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate <= endDate.Value);
            }

            var payments = await query
                .Include(p => p.WalletTransaction)
                .Include(p => p.Wallet)
                .ThenInclude(w => w.Learner)
                .ToListAsync();

            var oneOnOneRegistrations = await _unitOfWork.LearningRegisRepository
                .GetQuery()
                .Where(lr => lr.ClassId == null)
                .Include(lr => lr.Learner)
                .Include(lr => lr.Teacher)
                .Include(lr => lr.Major)
                .ToListAsync();


            var averagePrice = oneOnOneRegistrations.Where(lr => lr.Price.HasValue).Average(lr => lr.Price ?? 0);
            var phase40Threshold = averagePrice * 0.4m * 1.2m;

            var phase40Details = payments
                .Where(p => p.AmountPaid <= phase40Threshold)
                .Select(p => new
                {
                    TransactionId = p.TransactionId,
                    Amount = p.AmountPaid,
                    LearnerId = p.Wallet.LearnerId,
                    LearnerName = p.Wallet.Learner?.FullName ?? "Unknown",
                    PaymentDate = p.WalletTransaction.TransactionDate,
                    Phase = "40%"
                })
                .OrderByDescending(p => p.PaymentDate)
                .ToList<object>();

            var phase60Details = payments
                .Where(p => p.AmountPaid > phase40Threshold)
                .Select(p => new
                {
                    TransactionId = p.TransactionId,
                    Amount = p.AmountPaid,
                    LearnerId = p.Wallet.LearnerId,
                    LearnerName = p.Wallet.Learner?.FullName ?? "Unknown",
                    PaymentDate = p.WalletTransaction.TransactionDate,
                    Phase = "60%"
                })
                .OrderByDescending(p => p.PaymentDate)
                .ToList<object>();

            return (phase40Details, phase60Details);
        }

        private async Task<List<object>> GetDetailedCenterClassPaymentsAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.PaymentsRepository
                .GetQuery()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.PaymentFor == PaymentFor.LearningRegistration);

            if (startDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate <= endDate.Value);
            }

            var payments = await query
                .Include(p => p.WalletTransaction)
                .Include(p => p.Wallet)
                .ThenInclude(w => w.Learner)
                .ToListAsync();

            var centerClassRegistrations = await _unitOfWork.LearningRegisRepository
                .GetQuery()
                .Where(lr => lr.ClassId != null)
                .Include(lr => lr.Learner)
                .Include(lr => lr.Classes)
                .ThenInclude(c => c.Teacher)
                .Include(lr => lr.Major)
                .ToListAsync();

            var centerClassDetails = payments.Select(p => new
            {
                TransactionId = p.TransactionId,
                LearnerId = p.Wallet.LearnerId,
                LearnerName = p.Wallet.Learner?.FullName ?? "Unknown",
                Amount = p.AmountPaid,
                PaymentDate = p.WalletTransaction.TransactionDate,
                Phase = p.AmountPaid < 200000 ? "10% Initial" : "Other"
            })
            .OrderByDescending(p => p.PaymentDate)
            .ToList<object>();

            return centerClassDetails;
        }

        private async Task<List<object>> GetDetailedCoursePaymentsAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _unitOfWork.PaymentsRepository
                .GetQuery()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.PaymentFor == PaymentFor.Online_Course);

            if (startDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(p => p.WalletTransaction.TransactionDate <= endDate.Value);
            }

            var payments = await query
                .Include(p => p.WalletTransaction)
                .ThenInclude(wt => wt.Wallet)
                .ThenInclude(w => w.Learner)
                .ToListAsync();

            var courseDetails = payments
                .Select(p => new
                {
                    TransactionId = p.TransactionId,
                    LearnerId = p.WalletTransaction.Wallet.LearnerId,
                    LearnerName = p.WalletTransaction.Wallet.Learner?.FullName ?? "Unknown",
                    Amount = p.AmountPaid,
                    PaymentDate = p.WalletTransaction.TransactionDate
                })
                .OrderByDescending(p => p.PaymentDate)
                .ToList<object>();

            return courseDetails;
        }

        private async Task<List<object>> GetOneOnOneRegistrationRevenueByMonthAsync(int year)
        {
            var payments = await _unitOfWork.PaymentsRepository
                .GetQuery()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.PaymentFor == PaymentFor.LearningRegistration &&
                           p.WalletTransaction.TransactionDate.Year == year)
                .Include(p => p.WalletTransaction)
                .ToListAsync();

            var oneOnOneRegistrations = await _unitOfWork.LearningRegisRepository
                .GetQuery()
                .Where(lr => lr.ClassId == null)
                .ToListAsync();

            var averagePrice = oneOnOneRegistrations.Where(lr => lr.Price.HasValue).Average(lr => lr.Price ?? 0);
            var phase40Threshold = averagePrice * 0.4m * 1.2m;

            var monthlyData = payments
                .GroupBy(p => p.WalletTransaction.TransactionDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    MonthName = new DateTime(year, g.Key, 1).ToString("MMMM"),
                    Revenue = g.Sum(p => p.AmountPaid),
                    TransactionCount = g.Count(),
                    RegistrationCount = g.Count(),
                    Phase40Amount = g.Where(p => p.AmountPaid <= phase40Threshold).Sum(p => p.AmountPaid),
                    Phase60Amount = g.Where(p => p.AmountPaid > phase40Threshold).Sum(p => p.AmountPaid)
                })
                .OrderBy(m => m.Month)
                .ToList();

            var completeMonthlyData = Enumerable.Range(1, 12)
                .Select(month =>
                {
                    var existingData = monthlyData.FirstOrDefault(m => m.Month == month);
                    return existingData ?? new
                    {
                        Month = month,
                        MonthName = new DateTime(year, month, 1).ToString("MMMM"),
                        Revenue = 0m,
                        TransactionCount = 0,
                        RegistrationCount = 0,
                        Phase40Amount = 0m,
                        Phase60Amount = 0m
                    };
                })
                .ToList<object>();

            return completeMonthlyData;
        }

        private async Task<List<object>> GetCenterClassRegistrationRevenueByMonthAsync(int year)
        {
            var payments = await _unitOfWork.PaymentsRepository
                .GetQuery()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.PaymentFor == PaymentFor.LearningRegistration &&
                           p.WalletTransaction.TransactionDate.Year == year)
                .Include(p => p.WalletTransaction)
                .ToListAsync();

            var centerClassRegistrations = await _unitOfWork.LearningRegisRepository
                .GetQuery()
                .Where(lr => lr.ClassId != null)
                .ToListAsync();

            var monthlyData = payments
                .GroupBy(p => p.WalletTransaction.TransactionDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    MonthName = new DateTime(year, g.Key, 1).ToString("MMMM"),
                    Revenue = g.Sum(p => p.AmountPaid),
                    TransactionCount = g.Count(),
                    RegistrationCount = g.Count(),
                    Initial10PercentAmount = g.Where(p => p.AmountPaid < 200000).Sum(p => p.AmountPaid)
                })
                .OrderBy(m => m.Month)
                .ToList();

            var completeMonthlyData = Enumerable.Range(1, 12)
                .Select(month =>
                {
                    var existingData = monthlyData.FirstOrDefault(m => m.Month == month);
                    return existingData ?? new
                    {
                        Month = month,
                        MonthName = new DateTime(year, month, 1).ToString("MMMM"),
                        Revenue = 0m,
                        TransactionCount = 0,
                        RegistrationCount = 0,
                        Initial10PercentAmount = 0m
                    };
                })
                .ToList<object>();

            return completeMonthlyData;
        }

        private async Task<List<object>> GetCourseRevenueByMonthAsync(int year)
        {
            var payments = await _unitOfWork.PaymentsRepository
                .GetQuery()
                .Where(p => p.Status == PaymentStatus.Completed &&
                           p.PaymentFor == PaymentFor.Online_Course &&
                           p.WalletTransaction.TransactionDate.Year == year)
                .Include(p => p.WalletTransaction)
                .ToListAsync();

            var monthlyData = payments
                .GroupBy(p => p.WalletTransaction.TransactionDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    MonthName = new DateTime(year, g.Key, 1).ToString("MMMM"),
                    Revenue = g.Sum(p => p.AmountPaid),
                    TransactionCount = g.Count(),
                    PurchaseCount = g.Select(p => p.TransactionId).Distinct().Count()
                })
                .OrderBy(m => m.Month)
                .ToList();

            var completeMonthlyData = Enumerable.Range(1, 12)
                .Select(month =>
                {
                    var existingData = monthlyData.FirstOrDefault(m => m.Month == month);
                    return existingData ?? new
                    {
                        Month = month,
                        MonthName = new DateTime(year, month, 1).ToString("MMMM"),
                        Revenue = 0m,
                        TransactionCount = 0,
                        PurchaseCount = 0
                    };
                })
                .ToList<object>();

            return completeMonthlyData;
        }

        private DateTime GetWeekStartDate(int year, int weekNumber)
        {
            DateTime jan1 = new DateTime(year, 1, 1);

            int daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;

            if (daysOffset > 0) daysOffset -= 7;

            DateTime firstMonday = jan1.AddDays(daysOffset);

            return firstMonday.AddDays((weekNumber - 1) * 7);
        }

        private string GetPaymentTypeDisplayName(PaymentFor paymentType)
        {
            return paymentType switch
            {
                PaymentFor.LearningRegistration => "Đăng ký học 1-1",
                PaymentFor.Online_Course => "Khóa học trực tuyến",
                PaymentFor.AddFuns => "Nạp tiền",
                PaymentFor.ApplicationFee => "Phí đăng ký",
                _ => "Khác"
            };
        }
        #endregion
    }
}