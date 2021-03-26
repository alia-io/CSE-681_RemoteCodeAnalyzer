///////////////////////////////////////////////////////////////////////////////////////////////
///                                                                                         ///
///  DataContracts.cs - Defines data contracts used to pass data between client and host    ///
///                                                                                         ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019                             ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10                  ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:                            ///
///                 Software Modeling and Analysis, 2021                                    ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                        ///
///                                                                                         ///
///////////////////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   The AuthenticationRequest data contract is used to sent username and password data to
 *   the IAuthentication service.
 *   
 *   The DirectoryData data contract is used to send XML elements containing the current
 *   directory and its children to a client from the INavigation service.
 *   
 *   The FileBlock data contract is used to send blocks of text (as bytes) from a file,
 *   along with the file name and other associated data. This is used by the IUpload
 *   service to upload file blocks from a client, and by the IReadFile service to
 *   send file blocks to a client.
 * 
 *   Public Interface
 *   ----------------
 *   
 *   AuthenticationRequest
 *   ---------------------
 *   AuthenticationRequest authenticationRequest = new AuthenticationRequest((string) username, (string) password);
 *   AuthenticationRequest authenticationRequest = new AuthenticationRequest((string) username, (string) password, (string) confirmPassword);
 *   string username = authenticationRequest.Username;
 *   string password = authenticationRequest.Password;
 *   string confirmPassword = authenticationRequest.ConfirmPassword;
 *   
 *   DirectoryData
 *   -------------
 *   DirectoryData directoryData = new DirectoryData((XElement) current);
 *   directoryData.AddChild((XElement) child);
 *   XElement currentDirectory = directoryData.CurrentDirectory;
 *   List<XElement> children = directoryData.Children;
 *   
 *   FileBlock
 *   ---------
 *   FileBlock fileBlock = new FileBlock();
 *   FileBlock fileBlock = new FileBlock((string) filename, (int) number);
 *   fileBlock.FileName = (string) filename;
 *   fileBlock.Number = (int) number;
 *   fileBlock.LastBlock = (bool) lastBlock;
 *   fileBlock.Length = (int) length;
 *   string filename = fileBlock.FileName;
 *   int number = fileBlock.Number;
 *   bool lastBlock = fileBlock.LastBlock;
 *   int length = fileBlock.Length;
 *   byte[] buffer = fileBlock.Buffer;
 */

using System.Xml.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RCALibrary
{
    [DataContract]
    public class AuthenticationRequest
    {
        [DataMember] public string Username { get; private set; }
        [DataMember] public string Password { get; private set; }
        [DataMember] public string ConfirmPassword { get; private set; }

        public AuthenticationRequest(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public AuthenticationRequest(string username, string password, string confirmPassword)
        {
            Username = username;
            Password = password;
            ConfirmPassword = confirmPassword;
        }
    }

    [DataContract]
    public class DirectoryData
    {
        [DataMember] public XElement CurrentDirectory { get; private set; }
        [DataMember] public List<XElement> Children { get; private set; }

        public DirectoryData(XElement current)
        {
            CurrentDirectory = current;
            Children = new List<XElement>();

            CurrentDirectory.RemoveNodes();
        }

        public void AddChild(XElement child)
        {
            child.RemoveNodes();
            Children.Add(child);
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
            LastBlock = false;
            Buffer = new byte[16000];
        }

        public FileBlock(string filename, int number)
        {
            FileName = filename;
            Number = number;
            LastBlock = false;
            Buffer = new byte[16000];
        }
    }
}
