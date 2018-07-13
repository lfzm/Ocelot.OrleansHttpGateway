using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Model
{
    public class GrainReference
    {
        public GrainReference(Type type,object grain)
        {
            this.GrainType = type;
            this.Grain = grain;
        }
        public Assembly Assembly { get { return this.Assembly; } }
        public Type GrainType { get;  }
        public object Grain { get; }
    }
}
