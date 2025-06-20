﻿using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Feedback;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface IFeedbackService
    {
        Task<ResponseDTO> CreateFeedbackAsync(CreateFeedbackDTO feedbackDTO);
        Task<ResponseDTO> UpdateFeedbackAsync(int feedbackId, UpdateFeedbackDTO feedbackDTO);
        Task<ResponseDTO> DeleteFeedbackAsync(int feedbackId);
        Task<FeedbackDTO> GetFeedbackByIdAsync(int feedbackId);
        Task<List<FeedbackDTO>> GetAllFeedbackAsync();
    }
}
