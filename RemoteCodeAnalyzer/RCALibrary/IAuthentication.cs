using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;


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
}
