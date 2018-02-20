/////////////////////////////////////////////////////////////////////
//  MainWindow.xaml.cs - This is the .cs file for MainWindow       //
//  ver 1.0                                                        //
//  Language:      Visual C#  2017                                 //
//  Platform:      Windows 10 (used Virtual Box on mac)            //
//  IDE used:      Microsoft Visual Studio 2017, Community Edition //                                                               
//  Application:   Build Server , FALL 2017                        //
//  Author:        Butchi Venkata Akhil Rao,                       //
//                 Syracuse University brao01@syr.edu              //
/////////////////////////////////////////////////////////////////////

/*
Module Operations:
==================
Writes the interaction logic for main window 

Build Process:
==============
Required files

- MainWindow.xaml


Maintenance History:
====================
ver 1.0

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using MessagePassingComm;
using System.Threading;
using System.Xml.Linq;
using System.IO;

namespace Client
{
    
    public partial class MainWindow : Window
    {
        public string from { get; set; } = "http://localhost:8080/IMessagePassingComm";

        Dictionary<string, Action<CommMessage>> message = new Dictionary<string, Action<CommMessage>>();
        Comm client { get; set; } = null;
        Thread ThreadReceiver = null;
        List<String> FilesListRepository { get; set; } = new List<String>();
        public MainWindow()                          // Initializing the components for main window  
        {
            InitializeComponent();
            client = new Comm("http://localhost",8080);
            MessageDispatcherInit();
            ThreadReceiver = new Thread(ReceivingThreadProc);
            ThreadReceiver.Start();
        }

        void MessageDispatcherInit()                // Initiating the message reponse
        {
            Console.WriteLine("Requirement 10: Shall include a Graphical User Interface, built using WPF");
            message["getRepositoryFiles"] = (CommMessage msg) =>
              {
                  RepsoitoryListBox.Items.Clear();
                  foreach(string f in msg.arguments)
                  {
                      RepsoitoryListBox.Items.Add(f);
                  }
              };
            message["getRepositoryXMLFiles"] = (CommMessage msg) =>
            {
              RepositoryXmlListBox.Items.Clear();
                    foreach (string f in msg.arguments)
                {                   
                    RepositoryXmlListBox.Items.Add(f);
                }
            };
        }
       
        private void temporary()
        {
            Console.WriteLine("Requirement 11: The GUI client shall be a separate process, implemented with WPF and using message-passing communication. It shall provide mechanisms to get file lists from the Repository, and select files for packaging into a test library1, e.g., a test element specifying driver and tested files, added to a build request structure. It shall provide the capability of repeating that process to add other test libraries to the build request structure");
            CommMessage RepoFilesRequest = new CommMessage(CommMessage.MessageType.request);
            RepoFilesRequest.author = "client";
            RepoFilesRequest.command = "RequestRepositoryFiles";
            RepoFilesRequest.from = this.from;
            RepoFilesRequest.to = "http://localhost:8081/IMessagePassingComm";
            RepoFilesRequest.errorMsg = null;
            client.postMessage(RepoFilesRequest);
        }

        void ReceivingThreadProc()
        {
            Console.Write("\n  Client's receive thread is starting");
            while (true)
            {
                CommMessage message = client.getMessage();
                message.show();
                if (message.command == null)
                    continue;

                                                // Action value of Dispatcher is passed to the main thread for execution
                if ((message.command == "getRepositoryXMLFiles") || (message.command == "getRepositoryFiles"))
                    Dispatcher.Invoke(this.message[message.command], new object[] { message });
            }
        }

        private void BuildRequest(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Requirement 12: The client shall send build request structures to the repository for storage and transmission to the Build Server");
            CommMessage BuildRequestMsg = new CommMessage(CommMessage.MessageType.request);
            BuildRequestMsg.from = this.from;
            BuildRequestMsg.to = "http://localhost:8081/IMessagePassingComm";
            BuildRequestMsg.errorMsg = null;
            BuildRequestMsg.author = "Client";
            BuildRequestMsg.command = "BuildRequest";
            BuildRequestMsg.arguments = SelectedFiles();

            client.postMessage(BuildRequestMsg);
            RepsoitoryListBox.SelectedIndex = -1;

        }

        private void RepositoryFiles(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Requirement 13:The client shall be able to request the repository to send a build request in its storage to the Build Server for build processing");
            CommMessage RepoFilesRequest = new CommMessage(CommMessage.MessageType.request);
            RepoFilesRequest.author = "client";
            RepoFilesRequest.command = "RequestRepositoryFiles";
            RepoFilesRequest.from = this.from;
            RepoFilesRequest.to= "http://localhost:8081/IMessagePassingComm";
            RepoFilesRequest.errorMsg = null;
            client.postMessage(RepoFilesRequest);

        }

        private void XmlFiles(object sender, RoutedEventArgs e)
        {
            CommMessage RepoXMLFiles = new CommMessage(CommMessage.MessageType.request);
            RepoXMLFiles.author = "client";
            RepoXMLFiles.command = "RequestRepositoryXMLFiles";
            RepoXMLFiles.from = this.from;
            RepoXMLFiles.to = "http://localhost:8081/IMessagePassingComm";
            RepoXMLFiles.errorMsg = null;
            client.postMessage(RepoXMLFiles);
        }

       
        private List<string> SelectedFiles()
        {
            List<string> selectedFiles = new List<string>();
            foreach (string item in RepsoitoryListBox.SelectedItems)
            {
                selectedFiles.Add(item);
               
            }
            return selectedFiles;

        }
      
        private List<String> XMLFiles()
        {
            List<String> XMLFilesSelected = new List<String>();
            foreach (string item in RepositoryXmlListBox.SelectedItems)
            {

                XMLFilesSelected.Add(item);
            }
            return XMLFilesSelected;
        }


        private void BuildReqSend(object sender, RoutedEventArgs e)
        {
            CommMessage BuildRequestsSend = new CommMessage(CommMessage.MessageType.reply);
            BuildRequestsSend.from = this.from;
            BuildRequestsSend.to = "http://localhost:8081/IMessagePassingComm";
            BuildRequestsSend.command = "sendBuildRequest";
            BuildRequestsSend.author = "client";
            BuildRequestsSend.arguments = XMLFiles();
            client.postMessage(BuildRequestsSend);
            RepositoryXmlListBox.SelectedIndex = -1;

        }

        private void KillProcess(object sender, RoutedEventArgs e)
        {
            CommMessage ProcessMessageNum = new CommMessage(CommMessage.MessageType.request);
            ProcessMessageNum.from = "http://localhost:8080/IMessagePassingComm";
            ProcessMessageNum.to = "http://localhost:8083/IMessagePassingComm";
            ProcessMessageNum.command = "KillProcess";
            ProcessMessageNum.author = "client";
            client.postMessage(ProcessMessageNum);
        }


        private void NumProcesses(object sender, RoutedEventArgs e)
        {
            if (numberofprocess.Text != null)
            {
                List<string> Num = new List<string>();
                Num.Add(numberofprocess.Text);
                CommMessage ProcessMesssageNum = new CommMessage(CommMessage.MessageType.request);
                ProcessMesssageNum.from = this.from;
                ProcessMesssageNum.to = "http://localhost:8083/IMessagePassingComm";
                ProcessMesssageNum.command = "startchildbuilder";
                ProcessMesssageNum.author = "client";
                ProcessMesssageNum.arguments = Num;
                client.postMessage(ProcessMesssageNum);

            }
        }
       
    }
}
