using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProposalService.Models.DTOs;
using ProposalService.Services;
using WorkService.Models.DTOs;

namespace ProposalService.Controllers
{
    [ApiController]
    [Route("api/proposals")]
    public class ProposalController : Controller
    {
        private readonly ServiceProposal _service;

        public ProposalController(ServiceProposal service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            const int maxLimit = 50;

            if (page < 1)
                page = 1;

            if (limit < 1)
                limit = 10;

            if (limit > maxLimit)
                limit = maxLimit;

            var proposals = await _service.GetAll(page, limit);
            return Ok(proposals);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromHeader(Name = "X-User-Id")] Guid executorId,
            [FromBody] CreateProposalDto dto)
        {
            var proposal = await _service.Create(executorId, dto);
            return Ok(proposal);
        }

        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetByTaskId(Guid taskId, [FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            const int maxLimit = 50;

            if (page < 1)
                page = 1;

            if (limit < 1)
                limit = 10;

            if (limit > maxLimit)
                limit = maxLimit;
            var proposals = await _service.GetByTaskId(taskId, page, limit);
            return Ok(proposals);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetByUserId([FromHeader(Name = "X-User-Id")] Guid userId)
        {
            var proposals = await _service.GetByUserId(userId);
            return Ok(proposals);
        }


        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] string status)
        {
            if (!new[] { "pending", "accepted", "rejected" }.Contains(status))
                return BadRequest("Invalid status");

            var updated = await _service.UpdateStatus(id, status);
            if (updated == null) return NotFound();

            return Ok(updated);
        }

        [HttpPatch("{id}/accept")]
        public async Task<IActionResult> AcceptProposal(Guid id)
        {
            var success = await _service.Accept(id);
            if (!success) return BadRequest("Proposal cannot be accepted");
            return Ok();
        }

        [HttpPost("by-tasks")]
        public async Task<ActionResult<GetExecutorsByTasksResponse>> GetByTasks(
        [FromBody] GetExecutorsByTasksRequest request)
        {

            var data = await _service.GetExecutorsByTaskIdsAsync(request.TaskIds);

            return Ok(new GetExecutorsByTasksResponse
            {
                Data = data
            });
        }
    }
}
