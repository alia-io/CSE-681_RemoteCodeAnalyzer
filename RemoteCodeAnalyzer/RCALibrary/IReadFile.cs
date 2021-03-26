///////////////////////////////////////////////////////////////////////////////////
///                                                                             ///
///  IReadFile.cs - IReadFile service contract, defines requests for reading    ///
///                 files and obtaining file metadata                           ///
///                                                                             ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019                 ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10      ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:                ///
///                 Software Modeling and Analysis, 2021                        ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu            ///
///                                                                             ///
///////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   This module defines the IReadFile service contract, which is responsible for
 *   handling user requests to read uploaded files and analysis files, as well as
 *   metadata which provides areas of interest in the files.
 * 
 *   Public Interface
 *   ----------------
 *   ChannelFactory<IReadFile> readFileFactory = new ChannelFactory<IReadFile>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/ReadFile/"));
 *   INavigation filereader = readFileFactory.CreateChannel();
 *   FileBlock block = filereader.ReadBlock((string) user, (string) project, (string) version, (string) filename);
 *   XElement metadata = filereader.FileMetadata((string) user, (string) project, (string) version, (string) filename);
 */

using System.Xml.Linq;
using System.ServiceModel;

namespace RCALibrary
{
    [ServiceContract]
    public interface IReadFile
    {
        [OperationContract]
        FileBlock ReadBlock(string user, string project, string version, string filename);

        [OperationContract]
        XElement FileMetadata(string user, string project, string version, string filename);
    }
}
