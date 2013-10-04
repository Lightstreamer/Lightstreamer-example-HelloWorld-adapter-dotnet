# Lightstreamer "Hello World" Adapter for .NET #

This project will focus on a .NET port of the Java Data Adapter illustrated in [Lightstreamer "Hello World" Adapter for Java](https://github.com/Weswit/Lightstreamer-example-HelloWorld-adapter-java). In particular, both a <b>C#</b> version and a <b>Visual Basic</b> version of the Data Adapter will be shown.

## Let's get started ##

First, please take a look at the [previous installment](https://github.com/Weswit/Lightstreamer-example-HelloWorld-client-javascript), which provides some background and the general description of the application. Notice that the front-end will be exactly the same. We created a very simple HTML page that subscribes to the "greetings" item, using the "HELLOWORLD" Adapter. Now, we will replace the "HELLOWORLD" Adapter implementation based on Java with C# and Visual Basic equivalents. On the client side, nothing will change, as server-side Adapters can be transparently switched and changed, as long as they respect the same interfaces. Thanks to this decoupling provided by Lightstreamer Server, we could even do something different. For example, we could keep the Java Adapter on the server side and use Flex, instead of HTML, on the client side. Or we could use the C# Adapter on the server side and use Java, instead of HMTL or Flex, on the client side. Basically, all the combinations of languages and technologies on the client side and on the server side are supported.

## Give me some .NET interfaces! ##

Lightstreamer Server exposes native Java Adapter interfaces. The .NET interfaces are added through the <b>Lightstreamer Adapter Remoting Infrastructure</b> (ARI). Let's have a look at it.<br>
![General architecture](general_architecture.png)

ARI is simply made up of two Proxy Adapters and a <b>Network Protocol</b>. The two Proxy Adapters implement the Java interfaces and are meant to be plugged into Lightstreamer Kernel, exactly as we did for our original "HELLOWORLD" Java Adapter. There are two Proxy Adapters because one implements the Data Adapter interface and the other implements the Metadata Adapter interface. Our "Hello World" example uses a default Metadata Adapter, so we only need the <b>Proxy Data Adapter</b>.

What does the Proxy Data Adapter do? Basically, it exposes the Data Adapter interface through TCP sockets. In other words, it offers a Network Protocol, which any remote counterpart can implement to behave as a Lightstreamer Data Adapter. This means you can write a remote Data Adapter in C, in PHP, or in COBOL (?!?), provided that you have access to plain TCP sockets.

But - here is some magic - if your remote Data Adapter is based on .NET, you can forget about direct socket programming, and leverage a ready-made library that exposes a higher level <b>.NET interface</b>. So, you will simply have to implement this .NET interface.<br>
Ok, let's recap... The Proxy Data Adapter converts from a Java interface to TCP sockets. The .NET library converts from TCP sockets to a .NET interface. Clear enough?

## Creating the C# Data Adapter ##

Let's start with C#. Feel free to skip this section if you are only interested in the Visual Basic example (we will replicate the same comments on the code).<br>

We should implement two classes. One (which we will call <b>DataAdapterLauncher</b>) contains the application's <b>Main</b> and initializes the DataProviderServer (the provided piece of code that implements the Network Protocol). The other (which we will call <b>HelloWorldAdapter</b>) implements the actual Adapter interface.

- Create a new C# project (we used Microsoft's [Visual C# Express Edition](http://www.microsoft.com/express/)).
- From the "New Project..." wizard, choose the "Console Application" template. Let's use "adapter_csharp" as the project name.
- From the "Solution Explorer", delete the default Program.cs, then add a reference to the Lightstreamer .NET library: go to the "Browse" tab of the "Add Reference" dialog and point to the DotNetAdapter_N2.dll file, which you can find in the "Lightstreamer\DOCS-SDKs\sdk_adapter_dotnet\lib\" folder of your Lightstreamer installation. 

### DataAdapterLauncher ###

Add a new class (let's call its file <b>DataAdapterLauncher.cs</b>). Replace the source code with this:

```csharp
using System;
using System.Net.Sockets;
 
using Lightstreamer.DotNet.Server;
 
public class DataAdapterLauncher
{
  public static void Main(string [] args)
  {
    string host= "localhost";
    int reqrepPort = 6661;
    int notifPort = 6662;
 
    try
    {
      DataProviderServer server = new DataProviderServer();
      server.Adapter = new HelloWorldAdapter();
 
      TcpClient reqrepSocket = new TcpClient(host, reqrepPort);
      server.RequestStream = reqrepSocket.GetStream();
      server.ReplyStream = reqrepSocket.GetStream();
 
      TcpClient notifSocket = new TcpClient(host, notifPort);
      server.NotifyStream = notifSocket.GetStream();
 
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
```

This code creates a DataProviderServer instance and assigns a HelloWorldAdapter instance (which we will define below) to it. Then, it creates two TCP client sockets, because the Proxy Data Adapter, to which our remote .NET Adapter will connect, needs two connections (but as we said, after creating these sockets, you don't have to bother with reading and writing, as these operations are automatically handled by the DataProviderServer). Let's use TCP ports <b>6661</b> and <b>6662</b>. Assign the stream of the first socket to the RequestStream and ReplyStream properties of the DataProviderServer. Assign the stream of the second socket to the NotifyStream property of the DataProviderServer. Finally, you start the DataProviderServer.

### HelloWorldAdapter ###

Now add another class (call its file <b>HelloWorld.cs</b>). Replace the source code with this:

```csharp
using System;
using System.Collections;
using System.Threading;
 
using Lightstreamer.Interfaces.Data;
 
public class HelloWorldAdapter : IDataProvider
{
  private IItemEventListener _listener;
  private volatile bool go;
 
  public void Init(IDictionary parameters, string configFile)
  {
  }
 
  public bool IsSnapshotAvailable(string itemName)
  {
    return false;
  }
 
  public void SetListener(IItemEventListener eventListener)
  {
    _listener = eventListener;
  }
 
  public void Subscribe(string itemName)
  {
    if (itemName.Equals("greetings"))
    {
      Thread t = new Thread(new ThreadStart(Run));
      t.Start();
    }
  }
 
  public void Unsubscribe(string itemName)
  {
    if (itemName.Equals("greetings"))
    {
      go = false;
    }
  }
 
  public void Run()
  {
    go = true;
    int c = 0;
    Random rand = new Random();
 
      while (go)
      {
        IDictionary eventData = new Hashtable();
        eventData["message"] = c % 2 == 0 ? "Hello" : "World";
        eventData["timestamp"] = DateTime.Now.ToString("s");
        _listener.Update("greetings", eventData, false);
        c++;
        Thread.Sleep(1000 + rand.Next(2000));
      }
  }
}
```

The HelloWorldAdapter class implements the <b>IDataProvider</b> interface (which is the .NET remote equivalent of the Java DataProvider interface).

Implement the <b>SetListener</b> method to receive a reference to the server's listener that you will use to inject events.

Then, implement the <b>Subscribe</b> method. When the "greetings" item is subscribed to by the first user, the Adapter receives that method call and starts a thread that will generate the real-time data. If more users subscribe to the "greetings" item, the Subscribe method is not called any more. When the last user unsubscribes from this item, the Adapter is notified through the <b>Unsubscribe</b> call. In this case we stop the publisher thread for that item. If a new user re-subscribes to "greetings", the Subscribe method is called again. As already mentioned in the previous installment, this approach avoids consuming processing power for items nobody is currently interested in.

The <b>Run</b> method is executed within the thread started by Subscribe. Its code is very simple. We create a Hashtable containing a message (alternating "Hello" and "World") and the current timestamp. Then we inject the Hashtable into the server through the listener. We wait for a random time between 1 and 3 seconds, and we are ready to generate a new event.

### Save and build ###

From the File menu, choose "<b>Save All</b>". Type a location, for example: "c:\".

From the Build menu, choose "<b>Build Solution</b>". Your C# Data Adapter is now compiled. If you used the path above, you will find your executable file under "C:\adapter_csharp\adapter_csharp\bin\Release". It is called "<b>adapter_csharp.exe</b>". But be patient and don't start it now, because you have to configure and start the Lightstreamer Server first. You can skip the section below, dedicated to VB, and go straight to the "Deploy the Proxy Adapter" section.

## Creating the Visual Basic Data Adapter ##

Now let's work with Visual Basic instead of C#.

We will create a module (<b>DataAdapterLauncher</b>) which contains the application's <b>Main</b> and initializes the DataProviderServer (the provided piece of code that implements the Network Protocol). Then we will create a class (<b>HelloWorldAdapter</b>) that implements the actual Adapter interface.

- Create a new VB project (we used Microsoft's [Visual Basic Express Edition](http://www.microsoft.com/express/)).
- From the "New Project..." wizard, choose the "Console Application" template. Let's use "adapter_vb" as the project name.
- From the "Solution Explorer", delete the default Module1.vb, then add a reference to the Lightstreamer .NET library: go to the "Browse" tab of the "Add Reference" dialog and point to the DotNetAdapter_N2.dll file, which you can find in the "Lightstreamer\DOCS-SDKs\sdk_adapter_dotnet\lib\" folder of your Lightstreamer installation.

### DataAdapterLauncher ###

Add a new module (let's call its file <b>DataAdapterLauncher.vb</b>). Replace the source code with this:

```vb
Imports System.Net.Sockets
 
Imports Lightstreamer.DotNet.Server
 
Module Module1
  Sub Main()
    Dim host As String = "localhost"
    Dim reqrepPort As Integer = 6661
    Dim notifPort As Integer = 6662
 
    Try
      Dim server As New DataProviderServer()
      server.Adapter = New HelloWorldAdapter()
 
      Dim reqrepSocket As New TcpClient(host, reqrepPort)
      server.RequestStream = reqrepSocket.GetStream()
      server.ReplyStream = reqrepSocket.GetStream()
 
      Dim notifSocket As New TcpClient(host, notifPort)
      server.NotifyStream = notifSocket.GetStream()
 
      server.Start()
      System.Console.WriteLine("Remote Adapter connected to Lightstreamer Server.")
      System.Console.WriteLine("Ready to publish data...")
    Catch e As Exception
      System.Console.WriteLine("Could not connect to Lightstreamer Server.")
      System.Console.WriteLine("Make sure Lightstreamer Server is started before this Adapter.")
      System.Console.WriteLine(e)
    End Try
  End Sub
End Module
```

This code creates a DataProviderServer instance and assigns a HelloWorldAdapter instance (which we will define below) to it. Then, it creates two TCP client sockets, because the Proxy Data Adapter, to which our remote .NET Adapter will connect, needs two connections (but as we said, after creating these sockets, you don't have to bother with reading and writing, as these operations are automatically handled by the DataProviderServer). Let's use TCP ports <b>6661</b> and <b>6662</b>. Assign the stream of the first socket to the RequestStream and ReplyStream properties of the DataProviderServer. Assign the stream of the second socket to the NotifyStream property of the DataProviderServer. Finally, start the DataProviderServer.

### HelloWorldAdapter ###

Now add a class (call its file <b>HelloWorld.vb</b>). Replace the source code with this:

```vb
Imports System.Collections
Imports System.Threading
 
Imports Lightstreamer.Interfaces.Data
 
Public Class HelloWorldAdapter
  Implements IDataProvider
  Private _listener As IItemEventListener
  Private go As Boolean
 
  Public Sub Init(ByVal parameters As IDictionary, ByVal configFile As String) Implements IDataProvider.Init
  End Sub
 
  Function IsSnapshotAvailable(ByVal itemName As String) As Boolean Implements IDataProvider.IsSnapshotAvailable
    Return False
  End Function
 
  Public Sub SetListener(ByVal eventListener As IItemEventListener) Implements IDataProvider.SetListener
    _listener = eventListener
  End Sub
 
  Public Sub Subscribe(ByVal itemName As String) Implements IDataProvider.Subscribe
    If itemName.Equals("greetings") Then
      Dim t As New Thread(AddressOf Run)
      t.Start()
    End If
  End Sub
 
  Public Sub Unsubscribe(ByVal itemName As String) Implements IDataProvider.Unsubscribe
    If itemName.Equals("greetings") Then
      go = False
    End If
  End Sub
 
  Public Sub Run()
    go = True
    Dim c As Integer = 0
    Dim rand As New Random()
 
    While go
      Dim eventData As IDictionary = New Hashtable()
      eventData("message") = IIf(c Mod 2 = 0, "Hello", "World")
      eventData("timestamp") = DateTime.Now.ToString("s")
      _listener.Update("greetings", eventData, False)
      c += 1
      Thread.Sleep(1000 + rand.Next(2000))
    End While
  End Sub
End Class
```

The HelloWorldAdapter class implements the <b>IDataProvider</b> interface (which is the .NET remote equivalent of the Java DataProvider interface).

Implement the <b>SetListener</b> subroutine to receive a reference to the server's listener that you will use to inject events.

Then, implement the <b>Subscribe</b> subroutine. When the "greetings" item is subscribed to by the first user, the Adapter receives that subroutine call and starts a thread that will generate the real-time data. If more users subscribe to the "greetings" item, the Subscribe subroutine is not called any more. When the last user unsubscribes from this item, the Adapter is notified through the <b>Unsubscribe</B> call. In this case we stop the publisher thread for that item. If a new user re-subscribes to "greetings", the Subscribe subroutine is called again.

The <b>Run</b> subroutine is executed within the thread started by Subscribe. Its code is very simple. We create a Hashtable containing a message (alternating "Hello" and "World") and the current timestamp. Then we inject the Hashtable into the server through the listener. We wait for a random time between 1 and 3 seconds, and we are ready to generate a new event.

### Save and build ###

From the File menu, choose "<b>Save All</b>". Type a location, for example: "c:\".

From the Build menu, choose "<b>Build adapter_vb</b>". Your VB Data Adapter is now compiled. If you used the path above, you will find your executable file under "C:\adapter_vb\adapter_vb\bin\Release". It is called "<b>adapter_vb.exe</b>". As for the C# Adapter, don't start it now, because you have to configure and start the Lightstreamer Server first.

## Deploying the Proxy Adapter ##

Now that our remote Data Adapter is ready, we need to deploy and configure the provided Proxy Adapter within Lightstreamer Server.

Go to the "adapters" folder of your Lightstreamer Server and create a "<b>ProxyHelloWorld</b>" folder inside "adapters", and a "<b>lib</b>" folder inside "ProxyHelloWorld".

Copy the "ls-proxy-adapters.jar" file from "Lightstreamer/DOCS-SDKs/sdk_adapter_remoting_infrastructure/lib" to "Lightstreamer/adapters/ProxyHelloWorld/lib".

Create a new file in "Lightstreamer/adapters/ProxyHelloWorld", call it "adapters.xml", and use the following contents:

```xml
<?xml version="1.0"?>
 
<adapters_conf id="PROXY_HELLOWORLD">
 
  <metadata_provider>
    <adapter_class>com.lightstreamer.adapters.metadata.LiteralBasedProvider</adapter_class>
  </metadata_provider>
 
  <data_provider>
    <adapter_class>com.lightstreamer.adapters.remote.data.RobustNetworkedDataProvider</adapter_class>
    <param name="request_reply_port">6661</param>
    <param name="notify_port">6662</param>
  </data_provider>
 
</adapters_conf>
```

You have just deployed a new Java Adapter pair, where the Metadata Adapter is a default one (called "LiteralBasedProvider") and the Data Adapter is the Proxy Adapter (called "RobustNetworkedDataProvider"). This Adapter pair will be referenced by the clients as "<b>PROXY_HELLOWORLD</b>".

As a final configuration, let's tell our Web client to use this new Adapter pair, rather than those we developed in ["Hello World" for Lightstreamer](https://github.com/Weswit/Lightstreamer-example-HelloWorld-client-javascript). So just edit the "<b>index.htm</b>" page of the Hello World front-end (we deployed it under "Lightstreamer/pages/HelloWorld) and replace:

```js
  var client = new LightstreamerClient(null, "HELLOWORLD");
```

with:

```js
  var client = new LightstreamerClient(null, "PROXY_HELLOWORLD");
  ```

## Running the application ##

Now we have all the pieces ready. Let's enjoy the results.

Start your Lightstreamer Server. When it's up, run either the C# or the VB Remote Adapter.

Open a browser window and go to: http://localhost:8080/HelloWorld/

## Final notes ##

The full API references for the languages covered in this tutorial are available from the links below:

- [.NET API reference for Adapters](http://www.lightstreamer.com/docs/adapter_dotnet_api/index.html)

All the source code described in this article is available in this project.

# See Also #

## Clients using this Adapter ##

* ["Hello World" with Lightstreamer Colosseo](https://github.com/Weswit/Lightstreamer-example-HelloWorld-client-javascript)

## Related projects ##

* [Lightstreamer "Hello World" Adapter for Java](https://github.com/Weswit/Lightstreamer-example-HelloWorld-adapter-java)
* [Lightstreamer "Hello World" Adapter for Sockets](https://github.com/Weswit/Lightstreamer-example-HelloWorld-adapter-socket)

# Lightstreamer Compatibility Notes #

- Compatible with Lightstreamer Java Adapter .NET version 1.7 or newer.