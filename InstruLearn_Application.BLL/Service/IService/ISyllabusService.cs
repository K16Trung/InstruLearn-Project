using InstruLearn_Application.Model.Models.DTO;
using InstruLearn_Application.Model.Models.DTO.Syllabus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.BLL.Service.IService
{
    public interface ISyllabusService
    {
        Task<List<ResponseDTO>> GetAllSyllabusAsync();
        Task<ResponseDTO> GetSyllabusByIdAsync(int syllabusId);
        Task<ResponseDTO> GetSyllabusByClassIdAsync(int classId);
        Task<ResponseDTO> CreateSyllabusAsync(CreateSyllabusDTO createSyllabusDTO);
        Task<ResponseDTO> UpdateSyllabusAsync(int syllabusId, UpdateSyllabusDTO updateSyllabusDTO);
        Task<ResponseDTO> DeleteSyllabusAsync(int syllabusId);
    }
}
