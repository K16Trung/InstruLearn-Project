using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.SyllbusContent
{
    public class SyllabusContentDTO
    {
        public int SyllabusId { get; set; }
        public string SyllabusName { get; set; }
        public List<SyllabusContentDetailDTO> SyllabusContents { get; set; } = new List<SyllabusContentDetailDTO>();
    }
}
