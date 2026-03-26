using Microsoft.AspNetCore.Mvc;
using ProposalService.Services;
using ProposalService.Models.DTOs;

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
        public async Task<IActionResult> GetAll()
        {
            var proposals = await _service.GetAll();
            return Ok(proposals);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateProposalDto dto)
        {
            var proposal = await _service.Create(dto);
            return Ok(proposal);
        }

        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetByTaskId(Guid taskId)
        {
            var proposals = await _service.GetByTaskId(taskId);
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

    }
}
