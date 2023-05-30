#region License
/*
* Copyright (c) Lightstreamer Srl
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
#endregion License

using System;
using System.Net.Sockets;

using Lightstreamer.DotNet.Server;

public class DataAdapterLauncher
{
    public static void Main(string[] args)
    {
        string host = "localhost";
        int port = 6661;

        try
        {
            DataProviderServer server = new DataProviderServer();
            server.Adapter = new HelloWorldAdapter();

            TcpClient socket = new TcpClient(host, port);
            server.RequestStream = socket.GetStream();
            server.ReplyStream = socket.GetStream();

            server.Start();
            System.Console.WriteLine("Remote Adapter connected to Lightstreamer Server.");
            System.Console.WriteLine("Ready to publish data...");
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Could not connect to Lightstreamer Server.");
            System.Console.WriteLine("Make sure Lightstreamer Server is started before this Adapter.");
            System.Console.WriteLine(e);
        }
    }
}