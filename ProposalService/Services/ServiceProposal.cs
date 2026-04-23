using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProposalService.Data;
using ProposalService.Models;
using ProposalService.Models.DTOs;
using ProposalService.Models.Entities;
using static WorkService.Exceptions.MyCustomExceptions;

namespace ProposalService.Services
{
    public class ServiceProposal
    {
        private readonly AppDbContext _context;
        private readonly NotificationServiceClient _notifyService;
        private readonly WorkServiceClient _workService;

        public ServiceProposal(
            AppDbContext context,
            NotificationServiceClient notifyService,
            WorkServiceClient workService)
        {
            _context = context;
            _notifyService = notifyService;
            _workService = workService;
        }



        public async Task<ProposalDto> Create(Guid executorId, CreateProposalDto dto)
        {
            var task = await _workService.GetTask(dto.TaskId);

            if (task == null) throw new NotFoundException("Задача не найдена");

            if (task.CreatedByUserId == executorId) throw new ForbiddenException("Нельзя откликнуться на свою задачу");

            var exists = await _context.Proposals
                .AnyAsync(p => p.TaskId == dto.TaskId && p.ExecutorId == executorId);

            if (exists) throw new ConflictException("Вы уже откликнулись на эту задачу");

            var proposal = new Proposal
            {
                TaskId = dto.TaskId,
                ExecutorId = executorId,
                Status = "pending"
            };

            

            _context.Proposals.Add(proposal);
            await _context.SaveChangesAsync();

            await _notifyService.SendNotificationAsync(new NotificationDto
            {
                Title = task.Title,
                Type = "RESPOND",
                Recipient = new Recipient { UserId = task.CreatedByUserId }
            });

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

        public async Task<PagedResultDto<ProposalDto>> GetMyProposals(Guid userId, int page, int limit)
        {
            var query = _context.Proposals
                .Where(p => p.ExecutorId == userId)
                .OrderByDescending(p => p.Id);

            var total = await query.CountAsync();
            var proposals = await query
            .Skip((page-1) * limit)
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

            var task = await _workService.GetTask(proposal.TaskId);

            if (task == null) throw new NotFoundException("Задача не найдена");

            //если уже подтверждено
            var alreadyAccepted = await _context.Proposals
                .AnyAsync(p=>p.TaskId == proposal.TaskId && p.Status == "accepted");
            if (alreadyAccepted) throw new BadRequestException("Задача уже утверждена");

            
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
                Title = task.Title,
                Type = "APPLICATION_APPROVED",
                Recipient = new Recipient { UserId = proposal.ExecutorId }
            });

            //отправка события
            try
            {
                await _workService.NotifyProposalAccepted(proposal.TaskId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to notify WorkService: {ex.Message}");
            }
            return true;
        }

        public async Task<Dictionary<Guid, List<Guid>>> GetExecutorsByTaskIdsAsync(List<Guid> taskIds)
        {
            var result = await _context.Proposals
            .Where(p => taskIds.Contains(p.TaskId))
            .GroupBy(p => p.TaskId)
            .Select(g => new
            {
                TaskId = g.Key,
                Executors = g.Select(x => x.ExecutorId).Distinct().ToList()
            })
            .ToListAsync();

            return result.ToDictionary(x => x.TaskId, x => x.Executors);
        }

    }
}
