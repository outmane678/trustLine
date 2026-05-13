using AnonymousComplaintsAPI.Configurations;
using AnonymousComplaintsAPI.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AnonymousComplaintsAPI.Hubs
{
    public class SolutionsHub : Hub
    {
        // Store user connections per complaint
        private static readonly ConcurrentDictionary<string, HashSet<string>> ComplaintUsers 
            = new ConcurrentDictionary<string, HashSet<string>>();
        
        // Store unread message counts: ComplaintId_UserId -> Count
        private static readonly ConcurrentDictionary<string, int> UnreadCounts 
            = new ConcurrentDictionary<string, int>();
        
        // Store last read timestamp: ComplaintId_UserId -> DateTime
        private static readonly ConcurrentDictionary<string, DateTime> LastReadTimestamps 
            = new ConcurrentDictionary<string, DateTime>();

        private readonly ILogger<SolutionsHub> _logger;
        private readonly AnonymousComplaintsV002Context _context;
        private readonly IOptions<ChatSettings> _chatSettings; 

        public SolutionsHub(ILogger<SolutionsHub> logger, AnonymousComplaintsV002Context context, IOptions<ChatSettings> chatSettings)
        {
            _logger = logger;
            _context = context;
            _chatSettings = chatSettings;
        }

        public override async Task OnConnectedAsync()
        {
            if (!_chatSettings.Value.IsEnabled)
            {
                _logger.LogWarning("Tentative de connexion au chat alors qu'il est désactivé");
                Context.Abort();
                return;
            }
            await base.OnConnectedAsync();
        }
        public async Task JoinComplaintChat(int complaintId, int userId)
        {
            // NOUVEAU: Vérification
            if (!_chatSettings.Value.IsEnabled)
            {
                throw new HubException("Le chat est actuellement désactivé");
            }

            var groupName = $"Complaint_{complaintId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var userSet = ComplaintUsers.GetOrAdd(groupName, _ => new HashSet<string>());
            lock (userSet)
            {
                userSet.Add(userId.ToString());
            }

            _logger.LogInformation($"User {userId} joined complaint {complaintId}");
        }

        public async Task LeaveComplaintChat(int complaintId, int userId)
        {
            var groupName = $"Complaint_{complaintId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            if (ComplaintUsers.TryGetValue(groupName, out var userSet))
            {
                lock (userSet)
                {
                    userSet.Remove(userId.ToString());
                }
            }
        }

        public async Task CheckUserOnlineStatus(int complaintId)
        {
            var groupName = $"Complaint_{complaintId}";
            int onlineCount = 0;

            if (ComplaintUsers.TryGetValue(groupName, out var userSet))
            {
                lock (userSet)
                {
                    onlineCount = userSet.Count;
                }
            }
        }

        public async Task SendSolution(int complaintId, object solution)
        {
            // NOUVEAU: Vérification
            if (!_chatSettings.Value.IsEnabled)
            {
                throw new HubException("Le chat est actuellement désactivé");
            }

            var groupName = $"Complaint_{complaintId}";

            await Clients.Group(groupName).SendAsync("ReceiveSolution", solution);

            _logger.LogInformation($"Solution sent to complaint {complaintId}");

            await UpdateUnreadCountsForComplaint(complaintId, solution);
        }

        public async Task MarkMessagesAsRead(int complaintId)
        {
            // NOUVEAU: Vérification
            if (!_chatSettings.Value.IsEnabled)
            {
                throw new HubException("Le chat est actuellement désactivé");
            }

            var userId = GetCurrentUserId();
            if (userId == null) return;

            var key = $"{complaintId}_{userId}";

            UnreadCounts.AddOrUpdate(key, 0, (k, v) => 0);
            LastReadTimestamps.AddOrUpdate(key, DateTime.UtcNow, (k, v) => DateTime.UtcNow);

            _logger.LogInformation($"User {userId} marked messages as read for complaint {complaintId}");

            await Clients.Caller.SendAsync("MessagesMarkedAsRead", complaintId);
        }

        public async Task<int> GetUnreadCount(int complaintId, int userId)
        {
            var key = $"{complaintId}_{userId}";
            
            // First, try to get from in-memory cache
            if (UnreadCounts.TryGetValue(key, out var count))
            {
                return count;
            }

            // If not in cache, calculate from database
            var unreadCount = await CalculateUnreadCountFromDatabase(complaintId, userId);
            UnreadCounts.TryAdd(key, unreadCount);

            return unreadCount;
        }

        public async Task NotifyMessageRead(int solutionId, int userId)
        {
            var complaintId = await GetComplaintIdForSolution(solutionId);
            if (complaintId.HasValue)
            {
                var groupName = $"Complaint_{complaintId.Value}";
                await Clients.Group(groupName).SendAsync("MessageRead", solutionId, userId);
            }
        }

        private async Task UpdateUnreadCountsForComplaint(int complaintId, object solution)
        {
            try
            {
                // Get the sender ID from solution object
                var solutionDict = solution as Dictionary<string, object>;
                if (solutionDict == null || !solutionDict.ContainsKey("createdBy"))
                    return;

                var senderId = Convert.ToInt32(solutionDict["createdBy"]);

                // Get all users who should receive notifications (all users in this complaint except sender)
                var usersToNotify = await GetComplaintParticipants(complaintId);
                usersToNotify = usersToNotify.Where(u => u != senderId).ToList();

                foreach (var userId in usersToNotify)
                {
                    var key = $"{complaintId}_{userId}";
                    var lastRead = LastReadTimestamps.GetOrAdd(key, DateTime.MinValue);
                    
                    // Only increment if user hasn't read messages recently
                    var now = DateTime.UtcNow;
                    if ((now - lastRead).TotalSeconds > 5) // Buffer of 5 seconds
                    {
                        var newCount = UnreadCounts.AddOrUpdate(key, 1, (k, v) => v + 1);
                        
                        // Notify the specific user about their updated unread count
                        await Clients.User(userId.ToString()).SendAsync("UnreadCountUpdate", newCount);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating unread counts");
            }
        }

        private async Task<List<int>> GetComplaintParticipants(int complaintId)
        {
            // Query your database to get all users who are part of this complaint
            // This could be the creator, assigned managers, etc.
            var participants = await _context.Solutions
                .Where(s => s.AnonymousComplaintId == complaintId)
                .Select(s => s.CreatedBy)
                .Distinct()
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToListAsync();

            // Also add the complaint creator
            var complaint = await _context.AnonymousComplaints
                .FirstOrDefaultAsync(c => c.AnonymousComplaintId == complaintId);
            
            if (complaint != null && complaint.CreatedBy.HasValue)
            {
                if (!participants.Contains(complaint.CreatedBy.Value))
                {
                    participants.Add(complaint.CreatedBy.Value);
                }
            }

            return participants;
        }

        private async Task<int> CalculateUnreadCountFromDatabase(int complaintId, int userId)
        {
            // Get the last read timestamp from database
            var lastRead = GetLastReadFromMemory(complaintId, userId);

            // Count messages created after last read that aren't from this user
            var unreadCount = await _context.Solutions
                .Where(s => s.AnonymousComplaintId == complaintId
                       && s.CreatedAt.HasValue
                       && s.CreatedAt.Value > lastRead
                       && s.CreatedBy.HasValue
                       && s.CreatedBy.Value != userId)
                .CountAsync();

            return unreadCount;
        }

        private static DateTime GetLastReadFromMemory(int complaintId, int userId)
        {
            var key = $"{complaintId}_{userId}";
            return LastReadTimestamps.TryGetValue(key, out var ts) ? ts : DateTime.MinValue;
        }

        private async Task<int?> GetComplaintIdForSolution(int solutionId)
        {
            var solution = await _context.Solutions.FindAsync(solutionId);
            return solution?.AnonymousComplaintId;
        }

        private int? GetCurrentUserId()
        {
            // Get user ID from the claims
            var userIdClaim = Context.User?.FindFirst("userId") 
                           ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier);
            
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            // Clean up user from all groups they were in
            _logger.LogInformation($"Connection {Context.ConnectionId} disconnected");
            await base.OnDisconnectedAsync(exception);
        }
    }
}