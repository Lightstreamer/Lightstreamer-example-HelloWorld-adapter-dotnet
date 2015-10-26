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