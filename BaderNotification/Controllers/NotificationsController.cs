using BaderNotification.Helpers;
using BaderNotification.Models;
using BaderNotification.Models.Data;
using BaderNotification.ServerHubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BaderNotification.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationsController(ApplicationDbContext dbContext, IHubContext<NotificationHub> hubContext)
        {
            _dbContext = dbContext;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userNotifications = await _dbContext.Notifications.Where(n => n.UserId == userId).ToListAsync();
            return Ok(userNotifications);
        }

        [HttpPost]
        public async Task<IActionResult> SendNotificationToUser(AddNotificationDto notificationDto)
        {
            try
            {

                notificationDto.Users.ForEach(async (u) =>
                {
                    var notificationModel = new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = u,
                        Header = notificationDto.Header,
                        Content = notificationDto.Content,
                        DateTime = DateTime.Now,
                        Seen = false
                    };
                    //Database
                    await _dbContext.Notifications.AddAsync(notificationModel);
                    //SignalR
                    await _hubContext.Clients.Group(u).SendAsync("ReceiveNotification",notificationModel);

                });
                await _dbContext.SaveChangesAsync();
                return Ok(new Response
                {
                    Message = "Added New Notification Success",
                    Status = "201"
                });
            }
            catch (Exception ex)
            {

                return BadRequest(new Response
                {
                    Message = ex.Message,
                    Status = "400"
                });
            }

        }
        [HttpPut]
        public async Task<IActionResult> SeenNotification(DeleteNotificationDto notification)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var notificationIsExist = await _dbContext.Notifications.AnyAsync(n => n.Id == notification.NotificationId && n.UserId.ToString() == userId);
                if (!notificationIsExist)
                {
                    return NotFound(new Response
                    {
                        Message = "Invalid Notification Identifier",
                        Status = "404"
                    });
                }
                var notificationInDb = _dbContext.Notifications.FirstOrDefault(n => n.Id == notification.NotificationId);
                notificationInDb.Seen = true;
                await _dbContext.SaveChangesAsync();
                return Ok(new Response
                {
                    Message = "Updated Notification's status successfully",
                    Status = "204"
                });
            }
            catch (Exception ex)
            {
                return Ok(new Response
                {
                    Message = ex.Message,
                    Status = "400"
                });
            }
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteNotification(DeleteNotificationDto notification)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var notificationIsExist = await _dbContext.Notifications.AnyAsync(n => n.Id == notification.NotificationId && n.UserId.ToString() == userId);
                if (!notificationIsExist)
                {
                    return BadRequest(new Response
                    {
                        Message = "Invalid Notification Identifier",
                        Status = "404"
                    });
                }
                var notificationInDb = _dbContext.Notifications.FirstOrDefault(n => n.Id == notification.NotificationId);
                _dbContext.Remove(notificationInDb);

                await _dbContext.SaveChangesAsync();
                return Ok(new Response
                {
                    Message = "Notification Deleted successfully",
                    Status = "204"
                });

            }
            catch (Exception ex)
            {
                return Ok(new Response
                {
                    Message = ex.Message,
                    Status = "400"
                });
            }
        }

    }

    public class AddNotificationDto
    {
        public List<string> Users { get; set; }
        public string Header { get; set; }
        public string Content { get; set; }
        public string RedirectUrl { get; set; }
    }
    public class UpdateNotificationDto
    {
        public Guid NotificationId { get; set; }
    }
    public class DeleteNotificationDto
    {
        public Guid NotificationId { get; set; }
    }
}
