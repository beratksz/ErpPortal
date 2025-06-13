using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ErpPortal.Application.Models
{
    public class LoginDataViewModel
    {
        public List<SelectListItem> WorkCenters { get; set; } = new();
    }
} 