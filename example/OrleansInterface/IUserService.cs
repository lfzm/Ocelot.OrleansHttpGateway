using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace OrleansInterface
{
    public interface  IUserService:IGrainWithGuidKey
    {
        Task<string> GetUserName();

        Task<string> GetUser(string name);

        Task<string> GetUser(string name,string sex);

        Task<string> AddUser(User user);

        Task<string> AddUser(int id, User user);
    }
}
