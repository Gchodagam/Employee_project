using BackEnd.Core.DbContext;
using BackEnd.Core.Dtos.General;
using BackEnd.Core.Dtos.Message;
using BackEnd.Core.Entities;
using BackEnd.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BackEnd.Core.Services
{
    public class MessageService : IMessageService
    {
        #region Constructor & DI
        private readonly ApplicationDbContext _context;
        private readonly ILogService _logService;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessageService(ApplicationDbContext context, ILogService logService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logService = logService;
            _userManager = userManager;
        }
        #endregion

        #region CreateNewMessageAsync
        public async Task<GeneralServiceResponseDto> CreateNewMessageAsync(ClaimsPrincipal User, CreateMessageDto createMessageDto)
        {
            try
            {
                if (User.Identity.Name == createMessageDto.ReceiverUserName)
                    return new GeneralServiceResponseDto()
                    {
                        IsSucceed = false,
                        StatusCode = 400,
                        Message = "Sender and Receiver can not be same",
                    };

                var isReceiverUserNameValid = _userManager.Users.Any(q => q.UserName == createMessageDto.ReceiverUserName);
                if (!isReceiverUserNameValid)
                    return new GeneralServiceResponseDto()
                    {
                        IsSucceed = false,
                        StatusCode = 400,
                        Message = "Receiver UserName is not valid",
                    };

                Message newMessage = new Message()
                {
                    SenderUserName = User.Identity.Name,
                    ReceiverUserName = createMessageDto.ReceiverUserName,
                    Text = createMessageDto.Text
                };
                await _context.Messages.AddAsync(newMessage);
                await _context.SaveChangesAsync();
                await _logService.SaveNewLog(User.Identity.Name, "Send Message");
            }
            catch (Exception ex) 
            {
                Serilog.Log.Error("Failure : {@RequestName} , {@Error} , {@DateTimeUTC}",
          "CreateNewMessageAsync", ex.Message, DateTime.Today);
                return null;
            }

            return new GeneralServiceResponseDto()
            {
                IsSucceed = true,
                StatusCode = 201,
                Message = "Message saved successfully",
            };
        }
        #endregion

        #region GetMessagesAsync
        public async Task<IEnumerable<GetMessageDto>> GetMessagesAsync()
        {
            var messages = await _context.Messages
                .Select(q => new GetMessageDto()
                {
                    Id = q.Id,
                    SenderUserName = q.SenderUserName,
                    ReceiverUserName = q.ReceiverUserName,
                    Text = q.Text,
                    CreatedAt = q.CreatedAt
                })
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return messages;
        }
        #endregion

        #region GetMyMessagesAsync
        public async Task<IEnumerable<GetMessageDto>> GetMyMessagesAsync(ClaimsPrincipal User)
        {
            try
            {
                var loggedInUser = User.Identity.Name;

                var messages = await _context.Messages
                    .Where(q => q.SenderUserName == loggedInUser || q.ReceiverUserName == loggedInUser)
                 .Select(q => new GetMessageDto()
                 {
                     Id = q.Id,
                     SenderUserName = q.SenderUserName,
                     ReceiverUserName = q.ReceiverUserName,
                     Text = q.Text,
                     CreatedAt = q.CreatedAt
                 })
                 .OrderByDescending(q => q.CreatedAt)
                 .ToListAsync();

                return messages;
            }
            catch (Exception ex) 
            {
                Serilog.Log.Error("Failure : {@RequestName} , {@Error} , {@DateTimeUTC}",
                       "GetMyMessagesAsync", ex.Message, DateTime.Today);
                return null;
            }
        }
        #endregion
    }
}
