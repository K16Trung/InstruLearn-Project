using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LevelAssigned
{
    public class CreateLevelAssignedDTO
    {
        public int MajorId { get; set; }
        public string LevelName { get; set; }
        public decimal LevelPrice { get; set; }
    }
}
