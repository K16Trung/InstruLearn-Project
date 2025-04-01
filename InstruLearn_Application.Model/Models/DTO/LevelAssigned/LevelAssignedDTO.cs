using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models.DTO.LevelAssigned
{
    public class LevelAssignedDTO
    {
        public int LevelAssignedId { get; set; }
        public string? LevelName { get; set; }
        public int MajorId { get; set; }
        public decimal LevelPrice { get; set; }
    }
}
