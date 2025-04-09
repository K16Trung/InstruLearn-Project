using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.Model.Configuration;
using InstruLearn_Application.Model.Helper;
using InstruLearn_Application.Model.Models.DTO.Vnpay;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class VnpayService : IVnpayService
    {
        private readonly VnpaySettings _vnpaySettings;

        public VnpayService(VnpaySettings vnpaySettings)
        {
            _vnpaySettings = vnpaySettings;
        }
        public string CreatePaymentUrl(VnpayPaymentRequest request, string ipAddress)
        {
            if (string.IsNullOrEmpty(request.TransactionId))
            {
                throw new ArgumentException("Transaction ID cannot be empty");
            }

            DateTime vietnamTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");
            string createDate = vietnamTime.ToString("yyyyMMddHHmmss");

            string orderCode = string.IsNullOrEmpty(request.OrderId)
                ? DateTime.Now.Ticks.ToString()
                : request.OrderId;

            var pay = new VnPayLibrary();

            pay.AddRequestData("vnp_Version", _vnpaySettings.Version);
            pay.AddRequestData("vnp_Command", _vnpaySettings.Command);
            pay.AddRequestData("vnp_TmnCode", _vnpaySettings.TmnCode);

            pay.AddRequestData("vnp_Amount", ((long)(request.Amount * 100)).ToString());

            pay.AddRequestData("vnp_CreateDate", createDate);
            pay.AddRequestData("vnp_CurrCode", _vnpaySettings.CurrCode);

            ipAddress = ipAddress.Replace("::1", "127.0.0.1");
            pay.AddRequestData("vnp_IpAddr", ipAddress);

            pay.AddRequestData("vnp_Locale", _vnpaySettings.Locale);

            string orderInfo = !string.IsNullOrEmpty(request.OrderDescription)
                ? request.OrderDescription
                : $"Payment for order {orderCode}";
            pay.AddRequestData("vnp_OrderInfo", orderInfo);

            pay.AddRequestData("vnp_OrderType", request.OrderType);

            string returnUrl = _vnpaySettings.PaymentBackReturnUrl;

            pay.AddRequestData("vnp_ReturnUrl", returnUrl);

            pay.AddRequestData("vnp_TxnRef", request.TransactionId);

            var paymentUrl = pay.CreateRequestUrl(_vnpaySettings.BaseUrl, _vnpaySettings.HashSecret);

            return paymentUrl;
        }
    }
}