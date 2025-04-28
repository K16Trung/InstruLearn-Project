using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InstruLearn_Application.Model.Models
{
    public class LevelAssigned
    {
        [Key]
        public int LevelId { get; set; }
        public int MajorId { get; set; }
        public Major Major { get; set; }
        public string LevelName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal LevelPrice { get; set; }
        public string? SyllabusLink { get; set; }
        public ICollection<Learning_Registration> Learning_Registration { get; set; }
    }
}
