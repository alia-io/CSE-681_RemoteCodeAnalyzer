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
        void Remove();

        [OperationContract]
        DirectoryData Initialize(string username);

        [OperationContract]
        DirectoryData NavigateInto(string name);

        [OperationContract]
        DirectoryData NavigateBack();

        [OperationContract]
        void UpdateRoot(XElement newRoot);
    }

    [DataContract]
    public class DirectoryData
    {
        [DataMember] public XElement CurrentDirectory { get; set; }
        [DataMember] public List<XElement> Children { get; set; }

        public DirectoryData(XElement current)
        {
            CurrentDirectory = current;
            Children = new List<XElement>();

            CurrentDirectory.RemoveNodes();
        }
    }
}
