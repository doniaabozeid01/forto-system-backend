using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Forto.Application.DTOs.Schedule
{
    public class UpsertEmployeeScheduleRequest
    {
        [Required]
        [MinLength(1)]
        public List<DayScheduleRequest> Days { get; set; } = new();
    }
}
