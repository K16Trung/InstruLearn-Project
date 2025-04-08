using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Vnpay;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IVnpayService
    {
        string CreatePaymentUrl(VnpayPaymentRequest request, string ipAddress);
    }
}
