# LFConnect
A library for TCP and UDP connections between a server and client.

The project is finished but not release ready!

I've made the github repository a year later, so the documentation isn't the nicest.
Originally I planned on uploading the code to nuget, so that you could import my library in any .NET project.
However, after it was finished, I didn't see the use for anyone as there are way better options.
So I ended up just using it on my own.

# Documentation
I'll be seperating the server and client, but in order to work they both have to be setup correctly.
However, I haven't tested what happens when both LFServer and LFClient are imported in the same project.
(They were supposed to be used in seperate projects.)

Something important to note:
In this library I let the client and server communicate in custom packets.
How these packets are handled should be made in the opposite program.

### Importing via nuget (outdated)
In the server program:
```cs
using LFServer;
```
In the client program:
```cs
using LFClient;
```

## Server
### Starting a server
```cs
Server.Start(int MaxClients, int Port);
```
### Creating and sending a packet
```cs
using (Packet packet = new Packet(int packetId)) {
    packet.Write(DATA);
    
    ServerSend.SENDINGMETHOD(...);
}
```
DATA can be these datatypes: `byte, byte[], short, int, long, float, bool, string, Vector3, Quaternion`.

SENDINGMETHOD can be the following:
```cs
SendTCPData(int clientId, Packet packet);
SendTCPDataToAll(Packet packet);
SendUDPData(int clientId, Packet packet);
SendUDPDataToAll(Packet packet);
```
The names are self-explanitory.

It is very importing to keep this packetId, since you'll have to make a handler in the client program using this.
The id's for clientPackets and serverPackets are seperate.

I recommend using an enum to store these id's.

### Handling a packet
Handling a packet is really easy, you have to know the packet id send from the client and have a handle function.
```cs
Server.AddClientPacketHandler(int packetId, PacketHandler handleFunction);
```
The packetId has to be the id set in the client Program.
handleFunction has to be in the form of:
```cs
public delegate void PacketHandler(int fromClientId, Packet packet);
```

The handleFunction would like something like:
```cs
public static void handleFunction(int fromClientId, Packet Packet) {
    int data = packet.ReadInt();
    string nextData = packet.readString();
    
    // You can use this data for whatever you like...
}
```
The datatypes can be read with: `ReadType();` with Type being all the datatypes that can be written in a packet.

## Client
### Starting a client
```cs
Client.Start(string ip, int port);
```

### Creating and sending a packet
```cs
using (Packet packet = new Packet(int id)) {
    packet.Write(DATA);
    
    ClientSend.SENDINGMETHOD(...);
}
```
DATA can be these datatypes: `byte, byte[], short, int, long, float, bool, string, Vector3, Quaternion`.
 
SENDINGMETHOD can be the following:
```cs
SendTCPData(Packet packet);
SendUDPData(Packet packet);
```
The names are self-explanitory.

It is very importing to keep this packetId, since you'll have to make a handler in the client program using this.
The id's for clientPackets and serverPackets are seperate.

I recommend using an enum to store these id's.

### Handling a packet
Handling a packet is really easy, you have to know the packet id send from the server and have a handle function.
```cs
Client.AddServerPacketHandler(int packetId, PacketHandler handleFunction);
```
The packetId has to be the id set in the server Program.
handleFunction has to be in the form of:
```cs
public delegate void PacketHandler(Packet packet);
```

The handleFunction would look something like:
```cs
public static void handleFunction(Packet Packet) {
    int data = packet.ReadInt();
    string nextData = packet.readString();
    
    // You can use this data for whatever you like...
}
```
The datatypes can be read with: `ReadType();` with Type being all the datatypes that can be written in a packet.

# Example
All of this documentation put together in a two programs could look something like:

### Server Program:
Main.cs:
```cs
// Importing the library (downloaded from nuget, outdated)
using LFServer;

class MainClass
{
    public static void Main()
    {
        // Initializing client packets to their corresponding handler functions
        Server.AddClientPacketHandler(1, ClientReceived);
        // Starting the server
        Server.Start(20, 25565);
    }

    // The handler function for the client packet of id 1 (client-side id)
    public static void ClientReceived(int fromClientId, Packet packet)
    {
        Console.WriteLine("received client packet");
        
        // Read a string from the packet
        string msg = packet.ReadString();
        Console.WriteLine(msg);
        
        // Send a response packet to all the clients
        sendTestMsgToClients();
    }
    
    // Send a test msg to all clients
    public static void sendTestMsgToClients() {
        // Create a Packet with the id of 1 (server-side id)
        using (Packet packet = new Packet(1))
        {
            // Write the number 10
            packet.Write(10);
            // Then write the string "test"
            packet.Write("test");
            
            // Send over TCP (so we don't less chance of packet-loss) to all clients
            ServerSend.SendTCPDataToAll(packet);
        }
    }
}
```

### Client Program:
Main.cs:
```cs
// Importing the library (downloaded from nuget, outdated)
using LFClient;

class MainClass
{
    public static void Main()
    {
        // Connecting to the server (from localhost)
        Client.Start("0.0.0.0", 25565);
        // Initializing server packets to their corresponding handler functions
        Client.AddServerPacketHandler(1, testReceived);
        
        // Create a Packet with the id of 1 (client-side id)
        using (Packet packet = new Packet(1))
        {
            // Write a string to the packet
            packet.Write("I have connected to the server!");

            // Send over TCP (so we don't less chance of packet-loss)
            ClientSend.SendTCPData(packet);
        }
    }
    
    // The handler function for the server packet of id 1 (server-side id)
    public static void testReceived(Packet packet)
    {
        // Read the int from the packet
        int number = packet.ReadInt();
        // Read the following string from the packet
        string text = packet.ReadString();
        
        // Print this number and text
        Console.WriteLine(number + text);
    }
}
```

### Result:
1. Start server program
2. Start client program
3. received client packet
4. In server console: "I have connected to the server!" will be printed
5. In client console: "10test" will be printed

For each client program started after, step 3 and 4 will be repeated.
