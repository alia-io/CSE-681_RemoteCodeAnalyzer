using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace RCALibrary
{
    [DataContract]
    public class AuthenticationRequest
    {
        [DataMember] public string Username { get; set; }
        [DataMember] public string Password { get; set; }
        [DataMember] public string ConfirmPassword { get; set; }
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

    [DataContract]
    public class FileBlock
    {
        [DataMember] public string FileName { get; set; }
        [DataMember] public int Number { get; set; }
        [DataMember] public bool LastBlock { get; set; }
        [DataMember] public int Length { get; set; }
        [DataMember] public byte[] Buffer { get; private set; }
        
        public FileBlock()
        {
            Buffer = new byte[16000];
        }

        public FileBlock(string filename, int number)
        {
            FileName = filename;
            Number = number;
            Buffer = new byte[16000];
        }
    }
}
