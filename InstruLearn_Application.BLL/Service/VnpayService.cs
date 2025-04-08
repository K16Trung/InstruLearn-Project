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
            DateTime vietnamTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, "SE Asia Standard Time");

            var pay = new VnPayLibrary();

            pay.AddRequestData("vnp_Version", _vnpaySettings.Version);
            pay.AddRequestData("vnp_Command", _vnpaySettings.Command);
            pay.AddRequestData("vnp_TmnCode", _vnpaySettings.TmnCode);
            pay.AddRequestData("vnp_Amount", ((int)request.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", vietnamTime.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _vnpaySettings.CurrCode);
            pay.AddRequestData("vnp_IpAddr", ipAddress.Replace("::1", "127.0.0.1")); 
            pay.AddRequestData("vnp_Locale", _vnpaySettings.Locale);
            pay.AddRequestData("vnp_OrderInfo", request.OrderDescription);
            pay.AddRequestData("vnp_OrderType", request.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", _vnpaySettings.PaymentBackReturnUrl);
            pay.AddRequestData("vnp_TxnRef", request.TransactionId); 

            var paymentUrl = pay.CreateRequestUrl(_vnpaySettings.BaseUrl, _vnpaySettings.HashSecret);

            return paymentUrl;
        }

        public VnpayPaymentResponse ProcessPaymentReturn(IQueryCollection collection)
        {
            var vnPayLibrary = new VnPayLibrary();
            var paymentResponse = vnPayLibrary.GetFullResponseData(collection, _vnpaySettings.HashSecret);

            if (collection.ContainsKey("vnp_Amount") && decimal.TryParse(collection["vnp_Amount"], out decimal amount))
            {
                paymentResponse.Amount = amount / 100; 
            }

            paymentResponse.Message = GetResponseMessage(paymentResponse.ResponseCode);
            paymentResponse.PaymentMethod = "VnPay";

            return paymentResponse;
        }

        public bool ValidateSignature(string inputHash, Dictionary<string, string> requestData)
        {

            if (requestData.ContainsKey("vnp_SecureHashType"))
            {
                requestData.Remove("vnp_SecureHashType");
            }
            if (requestData.ContainsKey("vnp_SecureHash"))
            {
                requestData.Remove("vnp_SecureHash");
            }


            var sortedData = new SortedDictionary<string, string>(requestData, new VnPayLibrary.VnPayCompare());
            
            var queryString = string.Join("&", sortedData
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{WebUtility.UrlEncode(kv.Key)}={WebUtility.UrlEncode(kv.Value)}"));

            var hashValue = HmacSha512(_vnpaySettings.HashSecret, queryString);

            return string.Equals(inputHash, hashValue, StringComparison.OrdinalIgnoreCase);
        }

        private string HmacSha512(string key, string inputData)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }

        private string GetResponseMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Successful transaction",
                "01" => "Order not found",
                "02" => "Order already confirmed",
                "03" => "Invalid amount",
                "04" => "Invalid signature",
                "05" => "Transaction rejected",
                "06" => "Error occurred during processing",
                "07" => "Transaction declined by bank",
                "08" => "Transaction timeout",
                "09" => "Duplicate transaction",
                "10" => "Amount exceeds limit",
                "11" => "Transaction canceled",
                "12" => "Invalid card or account",
                "13" => "Invalid OTP",
                "24" => "Connection timeout",
                "51" => "Account does not have enough funds",
                "65" => "Account exceeded transaction limit",
                "75" => "Maximum number of incorrect OTP entries exceeded",
                "79" => "Account exceeds the number of transactions allowed per day",
                "99" => "Other errors",
                _ => "Unknown error"
            };
        }
    }
}