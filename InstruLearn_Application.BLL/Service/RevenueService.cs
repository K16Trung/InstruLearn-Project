using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Enum;
using InstruLearn_Application.Model.Models.DTO;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class RevenueService : IRevenueService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RevenueService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseDTO> GetTotalRevenueAsync()
        {
            try
            {
                // Query completed transactions with Payment type only
                var successfulTransactions = await _unitOfWork.WalletTransactionRepository
                    .GetQuery()
                    .Where(t => t.Status == TransactionStatus.Complete &&
                                t.TransactionType == TransactionType.Payment)
                    .ToListAsync();

                decimal totalRevenue = successfulTransactions.Sum(t => t.Amount);

                // Calculate revenue by payment type
                var courseRevenue = await GetRevenueByPaymentForTypeAsync(PaymentFor.Online_Course);
                var learningRegistrationRevenue = await GetRevenueByPaymentForTypeAsync(PaymentFor.LearningRegistration);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Tổng doanh thu được tính toán thành công.",
                    Data = new
                    {
                        TotalRevenue = totalRevenue,
                        CourseRevenue = courseRevenue,
                        LearningRegistrationRevenue = learningRegistrationRevenue,
                        TransactionCount = successfulTransactions.Count,
                        LastUpdated = DateTime.Now
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi tính toán tổng doanh thu: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetRevenueByTimeRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                // Ensure end date includes the entire day
                endDate = endDate.Date.AddDays(1).AddMilliseconds(-1);

                // Query completed transactions within date range
                var successfulTransactions = await _unitOfWork.WalletTransactionRepository
                    .GetQuery()
                    .Where(t => t.Status == TransactionStatus.Complete &&
                                t.TransactionType == TransactionType.Payment &&
                                t.TransactionDate >= startDate &&
                                t.TransactionDate <= endDate)
                    .ToListAsync();

                decimal totalRevenue = successfulTransactions.Sum(t => t.Amount);

                // Get revenue for payments
                var courseRevenue = await GetRevenueByPaymentForTypeAsync(PaymentFor.Online_Course, startDate, endDate);
                var learningRegistrationRevenue = await GetRevenueByPaymentForTypeAsync(PaymentFor.LearningRegistration, startDate, endDate);

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Doanh thu trong khoảng thời gian được tính toán thành công.",
                    Data = new
                    {
                        StartDate = startDate.ToString("dd/MM/yyyy"),
                        EndDate = endDate.ToString("dd/MM/yyyy"),
                        TotalRevenue = totalRevenue,
                        CourseRevenue = courseRevenue,
                        LearningRegistrationRevenue = learningRegistrationRevenue,
                        TransactionCount = successfulTransactions.Count
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi tính toán doanh thu theo khoảng thời gian: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetRevenueByTypeAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                // Base query for completed payment transactions
                var baseQuery = _unitOfWork.PaymentsRepository
                    .GetQuery()
                    .Where(p => p.Status == PaymentStatus.Completed);

                // Apply date filters if provided
                if (startDate.HasValue)
                {
                    baseQuery = baseQuery.Where(p => p.WalletTransaction.TransactionDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    // Ensure end date includes the entire day
                    var endDateValue = endDate.Value.Date.AddDays(1).AddMilliseconds(-1);
                    baseQuery = baseQuery.Where(p => p.WalletTransaction.TransactionDate <= endDateValue);
                }

                // Get all completed payments with their payment type
                var payments = await baseQuery
                    .Include(p => p.WalletTransaction)
                    .ToListAsync();

                // Group revenue by payment type
                var revenueByType = payments
                    .GroupBy(p => p.PaymentFor)
                    .Select(g => new
                    {
                        PaymentType = g.Key.ToString(),
                        Revenue = g.Sum(p => p.AmountPaid),
                        Count = g.Count()
                    })
                    .ToList();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Doanh thu theo loại được tính toán thành công.",
                    Data = new
                    {
                        RevenueByType = revenueByType,
                        TotalRevenue = payments.Sum(p => p.AmountPaid),
                        TotalCount = payments.Count,
                        StartDate = startDate?.ToString("dd/MM/yyyy") ?? "All time",
                        EndDate = endDate?.ToString("dd/MM/yyyy") ?? "Current"
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi tính toán doanh thu theo loại: {ex.Message}"
                };
            }
        }

        public async Task<ResponseDTO> GetMonthlyRevenueAsync(int year)
        {
            try
            {
                var startDate = new DateTime(year, 1, 1);
                var endDate = new DateTime(year, 12, 31, 23, 59, 59);

                // Get all completed transactions for the year
                var transactions = await _unitOfWork.WalletTransactionRepository
                    .GetQuery()
                    .Where(t => t.Status == TransactionStatus.Complete &&
                                t.TransactionType == TransactionType.Payment &&
                                t.TransactionDate >= startDate &&
                                t.TransactionDate <= endDate)
                    .ToListAsync();

                // Group transactions by month
                var monthlyRevenue = transactions
                    .GroupBy(t => t.TransactionDate.Month)
                    .Select(g => new
                    {
                        Month = g.Key,
                        MonthName = new DateTime(year, g.Key, 1).ToString("MMMM"),
                        Revenue = g.Sum(t => t.Amount),
                        TransactionCount = g.Count()
                    })
                    .OrderBy(r => r.Month)
                    .ToList();

                // Fill in missing months with zero revenue
                var completeMonthlyRevenue = Enumerable.Range(1, 12)
                    .Select(month => monthlyRevenue.FirstOrDefault(r => r.Month == month) ?? new
                    {
                        Month = month,
                        MonthName = new DateTime(year, month, 1).ToString("MMMM"),
                        Revenue = 0m,
                        TransactionCount = 0
                    })
                    .ToList();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = $"Doanh thu theo tháng cho năm {year} được tính toán thành công.",
                    Data = new
                    {
                        Year = year,
                        TotalRevenue = transactions.Sum(t => t.Amount),
                        TotalTransactions = transactions.Count,
                        MonthlyRevenue = completeMonthlyRevenue
                    }
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Lỗi khi tính toán doanh thu theo tháng: {ex.Message}"
                };
            }
        }

        private async Task<decimal> GetRevenueByPaymentForTypeAsync(PaymentFor paymentType, DateTime? startDate = null, DateTime? endDate = null)
        {
            try
            {
                var query = _unitOfWork.PaymentsRepository.GetQuery()
                    .Where(p => p.Status == PaymentStatus.Completed && p.PaymentFor == paymentType);

                if (startDate.HasValue)
                {
                    query = query.Where(p => p.WalletTransaction.TransactionDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(p => p.WalletTransaction.TransactionDate <= endDate.Value);
                }

                var payments = await query.ToListAsync();
                return payments.Sum(p => p.AmountPaid);
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}