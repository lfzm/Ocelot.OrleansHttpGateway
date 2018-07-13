using Orleans;
using OrleansInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrleansInterface
{
    public class UserService :Grain, IUserService
    {
        public Task<string> AddUser(int id, User user)
        {
            return Task.FromResult($"{id} Hello World {user.Name} Sex：{user.Sex}"); 
        }

        public Task<string> AddUser(User user)
        {
            return Task.FromResult($"Hello World {user.Name} Sex：{user.Sex}");
        }

        public Task<string> GetUser(string name)
        {
            return Task.FromResult("Hello World "+ name);
        }

        public Task<string> GetUser(string name, string sex)
        {
            return Task.FromResult($"Hello World {name} Sex：{sex}");
        }

        public Task<string> GetUserName()
        {
            return Task.FromResult( "Hello World");
        }
    }
}
