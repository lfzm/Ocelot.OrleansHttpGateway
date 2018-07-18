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
        public string ServiceInterfaceDllAbsolutePath { get; set; }
        public string ServiceName { get; set; }

        private Assembly assembly;
        internal Assembly Assembly
        {
            get
            {
                if (assembly == null)
                {
                    //获取应用程序所在目录（绝对，不受工作目录影响，建议采用此方法获取路径）
                    var basePath = System.IO.Directory.GetCurrentDirectory();
                    assembly = Assembly.LoadFile(basePath+this.ServiceInterfaceDllAbsolutePath);
                }

                return assembly;
            }
            set
            {
                assembly = value;
            }
        }
    }
}
