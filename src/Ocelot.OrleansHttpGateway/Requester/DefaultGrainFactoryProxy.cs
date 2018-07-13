using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocelot.OrleansHttpGateway.Configuration;
using Orleans;
using Orleans.Runtime;

namespace Ocelot.OrleansHttpGateway.Requester
{
    public class DefaultGrainFactoryProxy : IGrainFactoryProxy
    {
        private readonly IClusterClientBuilder _clusterClientBuilder;
        public DefaultGrainFactoryProxy(IClusterClientBuilder clusterClientBuilder)
        {
            this._clusterClientBuilder = clusterClientBuilder;
        }
        public void BindGrainReference(IAddressable grain)
        {
            grain.BindGrainReference(this);
        }

        public Task<TGrainObserverInterface> CreateObjectReference<TGrainObserverInterface>(IGrainObserver obj) where TGrainObserverInterface : IGrainObserver
        {
            throw new NotImplementedException();
        }

        public Task DeleteObjectReference<TGrainObserverInterface>(IGrainObserver obj) where TGrainObserverInterface : IGrainObserver
        {
            throw new NotImplementedException();
        }

        public TGrainInterface GetGrain<TGrainInterface>(Guid primaryKey, string grainClassNamePrefix = null) where TGrainInterface : IGrainWithGuidKey
        {
          return  this._clusterClientBuilder.GetClusterClient<TGrainInterface>()
                .GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(long primaryKey, string grainClassNamePrefix = null) where TGrainInterface : IGrainWithIntegerKey
        {
            return this._clusterClientBuilder.GetClusterClient<TGrainInterface>()
                .GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(string primaryKey, string grainClassNamePrefix = null) where TGrainInterface : IGrainWithStringKey
        {
            return this._clusterClientBuilder.GetClusterClient<TGrainInterface>()
                .GetGrain<TGrainInterface>(primaryKey, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(Guid primaryKey, string keyExtension, string grainClassNamePrefix = null) where TGrainInterface : IGrainWithGuidCompoundKey
        {
            return this._clusterClientBuilder.GetClusterClient<TGrainInterface>()
                .GetGrain<TGrainInterface>(primaryKey, keyExtension, grainClassNamePrefix);
        }

        public TGrainInterface GetGrain<TGrainInterface>(long primaryKey, string keyExtension, string grainClassNamePrefix = null) where TGrainInterface : IGrainWithIntegerCompoundKey
        {
            return this._clusterClientBuilder.GetClusterClient<TGrainInterface>()
                .GetGrain<TGrainInterface>(primaryKey, keyExtension,grainClassNamePrefix);
        }
    
      
  
    }
}
