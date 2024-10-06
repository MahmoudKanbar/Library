using System;
using Library.Domain.Users.Entities;

namespace Library.Domain.Sessions;

public interface ISessionsManager
{
    int? GetCurrentUserId();
    Task<User?> GetCurrentUser();
}