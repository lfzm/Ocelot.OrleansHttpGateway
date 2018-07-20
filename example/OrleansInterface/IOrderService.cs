using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrleansInterface
{
  public  interface IOrderService : IGrainWithGuidKey
    {
        Task<string> GetOrderId();
    }
}
