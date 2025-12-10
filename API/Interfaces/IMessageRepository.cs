using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface IMessageRepository
{   
    void AddMessage(Message message);
    void DeleteMessage(Message message);
    Task<Message?> GetMessage(string smessageId);
    Task<PaginatedResult<MessageDto>> GetMesagesForMember(MessageParams messageParams);
    Task<IReadOnlyList<MessageDto>> GetMessageThread(string currentMemberId, string recipientId);
    Task<bool> SaveAllChanges();
}
