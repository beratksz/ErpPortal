using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ErpPortal.Application.Interfaces.Services;
using ErpPortal.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace ErpPortal.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkCentersController : ControllerBase
    {
        private readonly IWorkCenterService _workCenterService;

        public WorkCentersController(IWorkCenterService workCenterService)
        {
            _workCenterService = workCenterService;
        }

        [HttpGet]
        public async Task<ActionResult<List<WorkCenter>>> GetWorkCenters()
        {
            return await _workCenterService.GetAllWorkCentersAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkCenter>> GetWorkCenter(int id)
        {
            var workCenter = await _workCenterService.GetWorkCenterByIdAsync(id);
            if (workCenter == null)
            {
                return NotFound();
            }
            return workCenter;
        }

        [HttpGet("code/{code}")]
        public async Task<ActionResult<WorkCenter>> GetWorkCenterByCode(string code)
        {
            var workCenter = await _workCenterService.GetWorkCenterByCodeAsync(code);
            if (workCenter == null)
            {
                return NotFound();
            }
            return workCenter;
        }

        [HttpPost]
        public async Task<ActionResult<WorkCenter>> CreateWorkCenter(WorkCenter workCenter)
        {
            var createdWorkCenter = await _workCenterService.CreateWorkCenterAsync(workCenter);
            return CreatedAtAction(nameof(GetWorkCenter), new { id = workCenter.Id }, createdWorkCenter);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkCenter(int id, WorkCenter workCenter)
        {
            if (id != workCenter.Id)
            {
                return BadRequest();
            }

            await _workCenterService.UpdateWorkCenterAsync(workCenter);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkCenter(int id)
        {
            await _workCenterService.DeleteWorkCenterAsync(id);
            return NoContent();
        }

        [HttpGet("{workCenterId}/users")]
        public async Task<ActionResult<List<User>>> GetWorkCenterUsers(int workCenterId)
        {
            return await _workCenterService.GetWorkCenterUsersAsync(workCenterId);
        }
    }
}