using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProposalService.Data;
using ProposalService.Models;
using ProposalService.Models.DTOs;
using ProposalService.Models.Entities;

namespace ProposalService.Services
{
    public class ServiceProposal
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notifyService;
        private readonly HttpClient _httpClient;

        public ServiceProposal(
            AppDbContext context,
            NotificationService notifyService,
            HttpClient httpClient)
        {
            _context = context;
            _notifyService = notifyService;
            _httpClient = httpClient;
        }

        

        public async Task<ProposalDto> Create(CreateProposalDto dto)
        {
            var proposal = new Proposal
            {
                TaskId = dto.TaskId,
                ExecutorId = dto.ExecutorId,
                Status = "pending"
            };

            var task = await _httpClient
                 .GetFromJsonAsync<TaskDto>($"api/tasks/{dto.TaskId}");

            await _notifyService.SendNotificationAsync(new NotificationDto
            {
                Type = "RESPOND",
                Recipient = new Recipient { UserId = task.CreatedByUserId }
            });

            _context.Proposals.Add(proposal);
            await _context.SaveChangesAsync();
            return new ProposalDto
            {
                Id = proposal.Id,
                TaskId = proposal.TaskId,
                ExecutorId = proposal.ExecutorId,
                Status = proposal.Status
            };
        }

        public async Task<PagedResultDto<ProposalDto>> GetAll(int page, int limit)
        {
            var query = _context.Proposals.AsQueryable();

            var total = await query.CountAsync();

            var proposals = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(p => new ProposalDto
                {
                    Id = p.Id,
                    TaskId = p.TaskId,
                    ExecutorId = p.ExecutorId,
                    Status = p.Status
                })
                .ToListAsync();

            return new PagedResultDto<ProposalDto>
            {
                Data = proposals,
                Total = total,
                Page = page,
                Limit = limit
            };
        }

        

        public async Task<PagedResultDto<ProposalDto>> GetByTaskId(Guid taskId, int page, int limit)
        {

            var query = _context.Proposals
               .Where(p => p.TaskId == taskId);
               

            var total = await query.CountAsync();

            var proposals = await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(p => new ProposalDto
                {
                    Id = p.Id,
                    TaskId = p.TaskId,
                    ExecutorId = p.ExecutorId,
                    Status = p.Status
                })
                .ToListAsync();

            return new PagedResultDto<ProposalDto>
            {
                Data = proposals,
                Total = total,
                Page = page,
                Limit = limit
            };
        }

        public async Task<List<ProposalDto>> GetByUserId(Guid userId)
        {
            return await _context.Proposals
            .Where(p => p.ExecutorId == userId)
            .Select(p => new ProposalDto
            {
                Id = p.Id,
                TaskId = p.TaskId,
                ExecutorId = p.ExecutorId,
                Status = p.Status
            })
            .ToListAsync();
        }
        public async Task<ProposalDto?> UpdateStatus(Guid id, string status)
        {
            var proposal = await _context.Proposals.FindAsync(id);
            if (proposal == null) return null;

            proposal.Status = status;
            await _context.SaveChangesAsync();

            return new ProposalDto
            {
                Id = proposal.Id,
                TaskId = proposal.TaskId,
                ExecutorId = proposal.ExecutorId,
                Status = proposal.Status
            };
        }

        public async Task<bool> Accept(Guid proposalId)
        {
            var proposal = await _context.Proposals
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null) return false;

            //если уже подтверждено
            var alreadyAccepted = await _context.Proposals
                .AnyAsync(p=>p.TaskId == proposal.TaskId && p.Status == "accepted");
            if (alreadyAccepted) return false;

            
            proposal.Status = "accepted";

            //Отклонить остальные запросы
            var others = await _context.Proposals
                .Where(p => p.TaskId == proposal.TaskId && p.Id != proposalId)
                .ToListAsync();

            foreach (var p in others)       
                p.Status = "rejected";

            await _context.SaveChangesAsync();


            //отправка уведомление исполнителю о подтверждении
            await _notifyService.SendNotificationAsync(new NotificationDto
            {
                Type = "APPLICATION_APPROVED",
                Recipient = new Recipient { UserId = proposal.ExecutorId }
            });

            //отправка события
            try
            {
                using var http = new HttpClient();
                await http.PostAsJsonAsync(
                    "http://localhost:5017/api/tasks/proposal-accepted",
                    new ProposalAcceptedDto { TaskId = proposal.TaskId}
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to notify WorkService: {ex.Message}");
            }
            return true;
        }

    }
}
