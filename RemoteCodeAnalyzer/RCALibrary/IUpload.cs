using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace RCALibrary
{
    [ServiceContract]
    public interface IUpload
    {
        [OperationContract]
        XElement NewProject(string username, string projectName);
    }

    [DataContract]
    public class FileBlock
    {

    }
}
