using InstruLearn_Application.Model.Models.DTO.ResponseType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.Response
{
    public class ResponseForLearningRegisDTO
    {
        public int ResponseId { get; set; }
        public List<ReponseTypeDTO> ResponseTypes { get; set; } = new List<ReponseTypeDTO>();
        public string ResponseName { get; set; }
    }
}
