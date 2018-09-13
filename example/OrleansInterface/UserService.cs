using Orleans;
using OrleansInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrleansInterface
{
    public class UserService : Grain, IUserService
    {


        public Task<string> AddUser(User user)
        {
            return Task.FromResult($"Hello World {user.Name} Sex：{user.Sex}");
        }

        public Task<User> AddUser2(string name, string sex = "女")
        {
            var user = new User()
            {
                Name = name,
                Sex = sex
            };
            return Task.FromResult(user);
        }

        public Task<string> GetUser(string name)
        {
            return Task.FromResult("Hello World " + name + "   id=" + this.GetPrimaryKeyLong());
        }

        public Task<string> GetUser2(string name, string sex = "女")
        {
            return Task.FromResult($"Hello World {name} Sex：{sex}");
        }

        public Task<string> GetUserName()
        {
            return Task.FromResult("Hello World");
        }
    }
}
