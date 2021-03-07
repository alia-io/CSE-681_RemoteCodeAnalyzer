using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace RCALibrary
{
    [ServiceContract]
    public interface IAuthentication
    {
        [OperationContract]
        bool Login(AuthenticationRequest credentials);

        [OperationContract]
        bool NewUser(AuthenticationRequest credentials);
    }

    public class AuthenticationRequest
    {
        [DataMember] public string Username { get; set; }
        [DataMember] public string Password { get; set; }
        [DataMember] public string ConfirmPassword { get; set; }
    }
}
