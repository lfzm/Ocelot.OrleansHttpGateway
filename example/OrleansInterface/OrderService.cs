using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrleansInterface
{
    public class OrderService : Grain, IOrderService
    {
        public Task<string> GetOrderId()
        {
            return Task.FromResult( this.GetPrimaryKey().ToString());
        }
    }
}
