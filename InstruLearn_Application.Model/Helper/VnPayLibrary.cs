using InstruLearn_Application.Model.Models.DTO.Vnpay;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Globalization;

namespace InstruLearn_Application.Model.Helper
{
    public class VnPayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());
        public VnpayPaymentResponse GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            var vnPay = new VnPayLibrary();

            // Add all VNPay parameters to the response data
            foreach (var (key, value) in collection)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnPay.AddResponseData(key, value);
                }
            }

            // Extract the secure hash for validation
            var vnpSecureHash = collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value;

            // Validate the signature
            var checkSignature = vnPay.ValidateSignature(vnpSecureHash, hashSecret);

            if (!checkSignature)
            {
                return new VnpayPaymentResponse()
                {
                    Success = false,
                    Message = "Invalid signature"
                };
            }

            // Map VNPay parameters to the response model
            return new VnpayPaymentResponse()
            {
                Success = true,
                Amount = vnPay.GetResponseData("vnp_Amount"),
                BankCode = vnPay.GetResponseData("vnp_BankCode"),
                BankTranNo = vnPay.GetResponseData("vnp_BankTranNo"),
                CardType = vnPay.GetResponseData("vnp_CardType"),
                OrderDescription = vnPay.GetResponseData("vnp_OrderInfo"),
                PayDate = vnPay.GetResponseData("vnp_PayDate"),
                ResponseCode = vnPay.GetResponseData("vnp_ResponseCode"),
                TmnCode = vnPay.GetResponseData("vnp_TmnCode"),
                TransactionNo = vnPay.GetResponseData("vnp_TransactionNo"),
                TransactionStatus = vnPay.GetResponseData("vnp_TransactionStatus"),
                TxnRef = vnPay.GetResponseData("vnp_TxnRef"),
                SecureHash = vnpSecureHash,
                PaymentMethod = "VNPay",
                OrderId = vnPay.GetResponseData("vnp_TxnRef"),
                PaymentId = vnPay.GetResponseData("vnp_TransactionNo"),
                TransactionId = vnPay.GetResponseData("vnp_TxnRef"),
                Message = GetResponseMessage(vnPay.GetResponseData("vnp_ResponseCode")),
                Token = vnpSecureHash
            };
        }
        public string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;

                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                    }

                    if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();

                    return ipAddress;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "127.0.0.1";
        }
        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData.Add(key, value);
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData.Add(key, value);
            }
        }
        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
        }
        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();

            foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            var querystring = data.ToString();

            baseUrl += "?" + querystring;
            var signData = querystring;
            if (signData.Length > 0)
            {
                signData = signData.Remove(data.Length - 1, 1);
            }

            var vnpSecureHash = HmacSha512(vnpHashSecret, signData);
            baseUrl += "vnp_SecureHash=" + vnpSecureHash;

            return baseUrl;
        }
        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var rspRaw = GetResponseData();
            var myChecksum = HmacSha512(secretKey, rspRaw);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
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
        private string GetResponseData()
        {
            var data = new StringBuilder();
            if (_responseData.ContainsKey("vnp_SecureHashType"))
            {
                _responseData.Remove("vnp_SecureHashType");
            }

            if (_responseData.ContainsKey("vnp_SecureHash"))
            {
                _responseData.Remove("vnp_SecureHash");
            }

            foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
            {
                data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
            }

            //remove last '&'
            if (data.Length > 0)
            {
                data.Remove(data.Length - 1, 1);
            }

            return data.ToString();
        }
        public class VnPayCompare : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                if (x == y) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                var vnpCompare = CompareInfo.GetCompareInfo("en-US");
                return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
            }
        }
        private string GetResponseMessage(string responseCode)
        {
            return responseCode switch
            {
                "00" => "Successful transaction",
                "01" => "Order not found",
                "02" => "Transaction declined by bank",
                "03" => "Invalid card/account information",
                "04" => "Invalid card/account",
                "05" => "Insufficient funds",
                "06" => "Error from payment gateway",
                "07" => "Card/account has reached credit limit",
                "09" => "Expired card/account",
                "10" => "Card/account is restricted",
                "11" => "Card/account is inactive",
                "12" => "Invalid card authentication",
                "13" => "Transaction is already processed",
                "24" => "Transaction cancelled",
                "51" => "Insufficient balance",
                "65" => "Exceeds withdrawal limit",
                "75" => "Exceeded number of attempts",
                "79" => "Card is suspected of fraud",
                "99" => "Connection timed out",
                _ => "Unknown error"
            };
        }
    }
}
