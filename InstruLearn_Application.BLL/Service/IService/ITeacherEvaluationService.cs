﻿using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluation;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluationOption;
using InstruLearn_Application.Model.Models.DTO.TeacherEvaluationQuestion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ITeacherEvaluationService
    {
        Task<ResponseDTO> GetAllEvaluationsAsync();
        Task<ResponseDTO> GetEvaluationByRegistrationIdAsync(int learningRegistrationId);
        Task<ResponseDTO> GetEvaluationsByTeacherIdAsync(int teacherId);
        Task<ResponseDTO> GetEvaluationsByLearnerIdAsync(int learnerId);
        Task<ResponseDTO> GetActiveQuestionsAsync();
        Task<ResponseDTO> GetQuestionByIdAsync(int questionId);
        Task<ResponseDTO> CreateQuestionWithOptionsAsync(CreateTeacherEvaluationQuestionDTO questionDTO);
        Task<ResponseDTO> UpdateQuestionAsync(int questionId, UpdateTeacherEvaluationQuestionDTO questionDTO);
        Task<ResponseDTO> DeleteQuestionAsync(int questionId);
        Task<ResponseDTO> ActivateQuestionAsync(int questionId);
        Task<ResponseDTO> DeactivateQuestionAsync(int questionId);
        Task<ResponseDTO> SubmitEvaluationFeedbackAsync(SubmitTeacherEvaluationDTO submitDTO);
        Task<ResponseDTO> CheckForLastDayEvaluationsAsync();
    }
}
