using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Server
{
    [ServiceContract]
    public interface IUpload
    {
        [OperationContract]
        bool NewProject(string username, string projectName);
    }

    [DataContract]
    public class FileBlock
    {

    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class Upload : IUpload
    {
        private XElement Home;
        public bool NewProject(string username, string projectName)
        {


            return false;
        }
    }
}
