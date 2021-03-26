///////////////////////////////////////////////////////////////////////////////////////////
///                                                                                     ///
///  IUpload.cs - IUpload service contract, defines requests for creating               ///
///                 a new project, creating a new version, and uploading files          ///
///                                                                                     ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019                         ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10              ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:                        ///
///                 Software Modeling and Analysis, 2021                                ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                    ///
///                                                                                     ///
///////////////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   This module defines the IUpload service contract, which is responsible for handling
 *   user requests to create a new project and to upload new project version files.
 *   Requests sent from a client will be processed by the host server, and the
 *   response (if any) will be sent back to the client.
 * 
 *   Public Interface
 *   ----------------
 *   ChannelFactory<IUpload> uploadFactory = new ChannelFactory<IUpload>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Upload/"));
 *   INavigation uploader = uploadFactory.CreateChannel();
 *   XElement project = uploader.NewProject((string) username, (string) projectName);
 *   bool upload = uploader.NewUpload((string) username, (string) projectName);
 *   uploader.UploadBlock((FileBlock) block);
 *   XElement version = uploader.CompleteUpload();
 */

using System.Xml.Linq;
using System.ServiceModel;

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
}
