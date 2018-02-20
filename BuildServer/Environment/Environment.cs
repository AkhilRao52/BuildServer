////////////////////////////////////////////////////////////////////////////////////////////
//  Environment.cs - It defines environment properties for Client and Server              //
//  ver 1.0                                                                               //
//  Language:      Visual C#  2017                                                        //
//  Platform:      Windows 10 (used Virtual Box on mac)                                   //
//  IDE used:      Microsoft Visual Studio 2017, Community Edition                        //                                                               
//  Application:   Build Server , FALL 2017                                               //
//  Author:        Butchi Venkata Akhil Rao,                                              //
//                 Syracuse University brao01@syr.edu                                     //
//  Reference:     Professor Jim Fawcett                                                  //
////////////////////////////////////////////////////////////////////////////////////////////

/*
Module Operations:
==================
Defines environment properties for Client and Server

Build Process:
==============
Required files

- None


Maintenance History:
====================
ver 1.0

*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigator
{
  public struct Environment
  {
    public static string root { get; set; }
    public const long blockSize = 1024;
    public static string endPoint { get; set; }
    public static string address { get; set; }
    public static int port { get; set; }
    public static bool verbose { get; set; }
  }

  public struct ClientEnvironment
  {
    public static string root { get; set; } = "../../../RepositoryStorage/";
    public const long blockSize = 1024;
    public static string endPoint { get; set; } = "http://localhost:8090/IMessagePassingComm";
    public static string address { get; set; } = "http://localhost";
    public static int port { get; set; } = 8090;
    public static bool verbose { get; set; } = false;
  }

  public struct ServerEnvironment
  {
    public static string root { get; set; } = "../../../BuildStorage/";
    public const long blockSize = 1024;
    public static string endPoint { get; set; } = "http://localhost:8080/IMessagePassingComm";
    public static string address { get; set; } = "http://localhost";
    public static int port { get; set; } = 8080;
    public static bool verbose { get; set; } = false;
  }
}
