using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace RCALibrary
{
    [ServiceContract]
    public interface IReadFile
    {
        [OperationContract]
        FileBlock ReadBlock(string user, string project, string version, string filename);
    }
}
