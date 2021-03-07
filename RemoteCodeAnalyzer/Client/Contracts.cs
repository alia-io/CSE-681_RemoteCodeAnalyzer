using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace Client
{
    /* Contracts for Authentication */

    [ServiceContract]
    public interface IAuthentication
    {
        [OperationContract]
        bool Login(AuthenticationRequest credentials);

        [OperationContract]
        bool NewUser(AuthenticationRequest credentials);
    }

    [DataContract(Namespace = "RemoteCodeAnalyzer")]
    public class AuthenticationRequest
    {
        [DataMember] public string Username { get; set; }
        [DataMember] public string Password { get; set; }
        [DataMember] public string ConfirmPassword { get; set; }
    }

    /* Contracts for Navigation */

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

    [DataContract(Namespace = "Navigation")]
    public class DirectoryData
    {
        [DataMember] public IFileSystemItem CurrentDirectory { get; set; }
        [DataMember] public List<IFileSystemItem> Children { get; set; }
    }

    /* Contracts for Upload */

    [ServiceContract]
    public interface IUpload
    {
        [OperationContract]
        bool NewProject(string username, string projectName);
    }
}
