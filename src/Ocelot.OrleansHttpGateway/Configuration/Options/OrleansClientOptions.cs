using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ocelot.OrleansHttpGateway.Configuration
{
    public class OrleansClientOptions
    {
        public string ServiceId { get; set; }
        public string ClusterId { get; set; }
        /// <summary>
        /// Orleans Grain Interface path name
        /// </summary>
        public string InterfaceDllPathName { get; set; }
        public string InterfaceNameTemplate { get; set; }

        private Assembly assembly;
        internal Assembly Assembly
        {
            get
            {
                if (assembly == null)
                    assembly = Assembly.LoadFile(this.InterfaceDllPathName);

                return assembly;
            }
            set
            {
                assembly = value;
            }
        }
    }
}
