using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace ErpPortal.Web.Models
{
    public class LoginDataViewModel
    {
        public List<SelectListItem> WorkCenters { get; set; } = new();
    }
} 