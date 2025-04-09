using AutoMapper;
using InstruLearn_Application.BLL.Service.IService;
using InstruLearn_Application.DAL.UoW.IUoW;
using InstruLearn_Application.Model.Models;
using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.QnA;
using InstruLearn_Application.Model.Models.DTO.Syllabus;
using InstruLearn_Application.Model.Models.DTO.SyllbusContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service
{
    public class SyllabusContentService : ISyllabusContentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public SyllabusContentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ResponseDTO>> GetAllSyllabusContentsAsync()
        {
            var syllabusContentList = await _unitOfWork.SyllabusContentRepository.GetAllAsync();

            var groupedBySyllabus = syllabusContentList.GroupBy(sc => sc.SyllabusId);

            var consolidatedResult = new List<SyllabusContentDTO>();

            foreach (var group in groupedBySyllabus)
            {
                var syllabusId = group.Key;
                var firstItem = group.First();

                var syllabusContentDto = new SyllabusContentDTO
                {
                    SyllabusId = syllabusId,
                    SyllabusName = firstItem.Syllabus?.SyllabusName ?? "Unknown Syllabus",
                    SyllabusContents = new List<SyllabusContentDetailDTO>()
                };

                foreach (var content in group)
                {
                    syllabusContentDto.SyllabusContents.Add(new SyllabusContentDetailDTO
                    {
                        SyllabusContentId = content.SyllabusContentId,
                        ContentName = content.ContentName
                    });
                }

                consolidatedResult.Add(syllabusContentDto);
            }

            return new List<ResponseDTO>
    {
        new ResponseDTO
        {
            IsSucceed = true,
            Message = "Syllabus Content retrieved successfully.",
            Data = consolidatedResult
        }
    };
        }

        public async Task<ResponseDTO> GetSyllabusContentByIdAsync(int syllabusContentId)
        {
            var syllabusContent = await _unitOfWork.SyllabusContentRepository.GetByIdAsync(syllabusContentId);
            if (syllabusContent == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Syllabus Content not found."
                };
            }

            // Get all content items for this syllabus
            var allContentsForSyllabus = await _unitOfWork.SyllabusContentRepository.GetWithIncludesAsync(
                sc => sc.SyllabusId == syllabusContent.SyllabusId,
                "Syllabus"
            );

            // Create the DTO with the proper structure
            var syllabusContentDto = new SyllabusContentDTO
            {
                SyllabusId = syllabusContent.SyllabusId,
                SyllabusName = syllabusContent.Syllabus?.SyllabusName ?? "Unknown",
                SyllabusContents = _mapper.Map<List<SyllabusContentDetailDTO>>(allContentsForSyllabus)
            };

            return new ResponseDTO
            {
                IsSucceed = true,
                Message = "Syllabus Content retrieved successfully.",
                Data = syllabusContentDto
            };
        }

        public async Task<ResponseDTO> AddSyllabusContentAsync(CreateSyllabusContentDTO createDto)
        {
            var syllabus = await _unitOfWork.SyllabusRepository.GetByIdAsync(createDto.SyllabusId);
            if (syllabus == null)
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Syllabus not found",
                };
            }

            var syllbusContentObj = _mapper.Map<Syllabus_Content>(createDto);
            syllbusContentObj.Syllabus = syllabus;

            await _unitOfWork.SyllabusContentRepository.AddAsync(syllbusContentObj);
            await _unitOfWork.SaveChangeAsync();

            var response = new ResponseDTO
            {
                IsSucceed = true,
                Message = "Add syllabus content successfully",
            };

            return response;
        }

        public async Task<ResponseDTO> UpdateSyllabusContentAsync(int syallbusContentId, UpdateSyllabusContentDTO updateDto)
        {
            var syllabusContentUpdate = await _unitOfWork.SyllabusContentRepository.GetByIdAsync(syallbusContentId);
            if (syllabusContentUpdate != null)
            {
                syllabusContentUpdate = _mapper.Map(updateDto, syllabusContentUpdate);
                await _unitOfWork.SyllabusContentRepository.UpdateAsync(syllabusContentUpdate);
                var result = await _unitOfWork.SaveChangeAsync();
                if (result > 0)
                {
                    return new ResponseDTO
                    {
                        IsSucceed = true,
                        Message = "Syllabus content update successfully!"
                    };
                }
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = "Syllabus content update failed!"
                };
            }
            return new ResponseDTO
            {
                IsSucceed = false,
                Message = "Syllabus content not found!"
            };
        }

        public async Task<ResponseDTO> DeleteSyllabusContentAsync(int syallbusContentId)
        {
            var deleteSyllabusContent = await _unitOfWork.SyllabusContentRepository.GetByIdAsync(syallbusContentId);
            if (deleteSyllabusContent != null)
            {
                await _unitOfWork.SyllabusContentRepository.DeleteAsync(syallbusContentId);
                await _unitOfWork.SaveChangeAsync();

                return new ResponseDTO
                {
                    IsSucceed = true,
                    Message = "Syllabus Content deleted successfully"
                };
            }
            else
            {
                return new ResponseDTO
                {
                    IsSucceed = false,
                    Message = $"Syllabus Content with ID {syallbusContentId} not found"
                };
            }
        }
    }
}