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

        [OperationContract]
        bool NewUpload(string username, string projectName);

        [OperationContract]
        void UploadBlock(FileBlock block);

        [OperationContract]
        XElement CompleteUpload();
    }

    [DataContract]
    public class FileBlock
    {
        [DataMember] public string FileName { get; set; }
        [DataMember] public int Number { get; set; }
        [DataMember] public byte[] Buffer { get; private set; }

        public FileBlock()
        {
            Buffer = new byte[5000];
        }

        public FileBlock(string filename, int number)
        {
            FileName = filename;
            Number = number;
            Buffer = new byte[5000];
        }
    }
}
