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
    public interface INavigation
    {
        [OperationContract]
        DirectoryData Initialize(string username);

        [OperationContract]
        DirectoryData Navigate();

        [OperationContract]
        void UpdateRoot(XElement newRoot);
    }

    [DataContract]
    [KnownType(typeof(UserDirectory))]
    [KnownType(typeof(ProjectDirectory))]
    [KnownType(typeof(VersionDirectory))]
    [KnownType(typeof(CodeFile))]
    [KnownType(typeof(AnalysisFile))]
    public class DirectoryData
    {
        [DataMember] public IFileSystemItem CurrentDirectory { get; set; }
        [DataMember] public List<IFileSystemItem> Children { get; set; }
    }
}
