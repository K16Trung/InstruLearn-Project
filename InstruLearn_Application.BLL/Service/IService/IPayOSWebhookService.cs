﻿using InstruLearn_Application.Model.Models.DTO.PayOSWebhook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IPayOSWebhookService
    {
        Task ProcessWebhookAsync(PayOSWebhookDTO webhookDto);
    }
}
