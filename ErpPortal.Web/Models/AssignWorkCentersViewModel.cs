using System.Collections.Generic;

namespace ErpPortal.Web.Models;

public class AssignWorkCentersViewModel
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<WorkCenterCheckbox> WorkCenters { get; set; } = new();
}

public class WorkCenterCheckbox
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool Selected { get; set; }
} 