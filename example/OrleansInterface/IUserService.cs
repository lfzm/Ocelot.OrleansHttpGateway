using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;
using Microsoft.AspNetCore.Authorization;

namespace OrleansInterface
{
    public interface  IUserService:IGrainWithGuidKey
    {
        Task<string> GetUserName();

        [Authorize(Roles = "Admin")]
        Task<string> GetUser(string name);

        Task<string> GetUser(string name,string sex);

        Task<string> AddUser(User user);

        Task<string> AddUser(int id, User user);
    }
}
