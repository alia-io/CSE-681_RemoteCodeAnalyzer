///////////////////////////////////////////////////////////////////////////////////////////
///                                                                                     ///
///  INavigation.cs - INavigation service contract, defines requests for navigation     ///
///                 and maintenance of navigators and directory trees                   ///
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
 *   This module defines the INavigation service contract, which is responsible for
 *   handling user requests to navigate through the directory tree, as well as
 *   maintaining the state of the directory tree and the list of logged in navigators.
 *   Requests sent from a client will be processed by the host server, and the
 *   response (if any) will be sent back to the client.
 * 
 *   Public Interface
 *   ----------------
 *   ChannelFactory<INavigation> navigationFactory = new ChannelFactory<INavigation>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Navigation/"));
 *   INavigation navigator = navigationFactory.CreateChannel();
 *   DirectoryData data = navigator.Initialize((string) username);
 *   DirectoryData data = navigator.NavigateInto((string) identifier);
 *   DirectoryData data = navigator.NavigateBack();
 *   navigator.RemoveNavigator();
 */

using System.ServiceModel;

namespace RCALibrary
{
    [ServiceContract]
    public interface INavigation
    {
        [OperationContract]
        DirectoryData Initialize(string username);

        [OperationContract]
        DirectoryData NavigateInto(string identifier);

        [OperationContract]
        DirectoryData NavigateBack();

        [OperationContract]
        void RemoveNavigator();
    }
}
