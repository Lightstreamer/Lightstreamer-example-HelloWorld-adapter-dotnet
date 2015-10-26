'
' Copyright (c) Lightstreamer Srl
'
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
'
' http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
'

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
