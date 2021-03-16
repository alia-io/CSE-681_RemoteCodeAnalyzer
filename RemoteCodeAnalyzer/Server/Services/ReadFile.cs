using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using RCALibrary;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults = true)]
    public class ReadFile : IReadFile
    {
        public FileBlock ReadBlock(string user, string project, string version, string filename)
        {
            throw new NotImplementedException();
        }
    }
}
