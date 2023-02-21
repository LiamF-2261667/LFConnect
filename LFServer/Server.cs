using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace LFServer;

public class Server
{
    public static int MaxUsers { get; private set; }
    public static int Port { get; private set; }
    public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
    public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers = new Dictionary<int, PacketHandler>();

    private static TcpListener tcpListener;
    private static UdpClient udpListener;

    public static bool isRunning = true;
    
    public static void Start(int _maxUsers, int _port)
    {
        Thread mainThread = new Thread(new ThreadStart(MainTread));
        mainThread.Start();
        
        MaxUsers = _maxUsers;
        Port = _port;

        Console.WriteLine("Starting server...");
        InitializeServerData();

        tcpListener = new TcpListener(IPAddress.Any, Port);
        tcpListener.Start();
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);

        udpListener = new UdpClient(Port);
        udpListener.BeginReceive(UDPReceiveCallback, null);

        Console.WriteLine($"Server started on port {Port}.");
    }

    public static void MainTread()
    {
        Console.WriteLine($"Main thread started.");

        while (isRunning)
        {
            ThreadManager.UpdateMain();
        }
    }

    public static void Stop()
    {
        tcpListener.Stop();
        udpListener.Close();
        Console.WriteLine("Closed the server!");
    }
    
    /// <summary>Handles new TCP connections.</summary>
    private static void TCPConnectCallback(IAsyncResult _result)
    {
        TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
        tcpListener.BeginAcceptTcpClient(TCPConnectCallback, null);
        Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= MaxUsers; i++)
        {
            if (clients[i].tcp.socket == null)
            {
                clients[i].tcp.Connect(_client);
                return;
            }
        }

        Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
    }

    /// <summary>Receives incoming UDP data.</summary>
    private static void UDPReceiveCallback(IAsyncResult _result)
    {
        try
        {
            IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (_data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(_data))
            {
                int _clientId = _packet.ReadInt();

                if (_clientId == 0)
                {
                    return;
                }

                if (clients[_clientId].udp.endPoint == null)
                {
                    // If this is a new connection
                    clients[_clientId].udp.Connect(_clientEndPoint);
                    return;
                }

                if (clients[_clientId].udp.endPoint.ToString() == _clientEndPoint.ToString())
                {
                    // Ensures that the client is not being impersonated by another by sending a false clientID
                    clients[_clientId].udp.HandleData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            Console.WriteLine($"Error receiving UDP data: {_ex}");
        }
    }

    /// <summary>Sends a packet to the specified endpoint via UDP.</summary>
    /// <param name="_clientEndPoint">The endpoint to send the packet to.</param>
    /// <param name="_packet">The packet to send.</param>
    public static void SendUDPData(IPEndPoint _clientEndPoint, Packet _packet)
    {
        try
        {
            if (_clientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
        }
    }

    /// <summary>Initializes all necessary server data.</summary>
    private static void InitializeServerData()
    {
        for (int i = 1; i <= MaxUsers; i++)
        {
            clients.Add(i, new Client(i));
        }
        Console.WriteLine("Initialized server data.");
    }

    public static void AddClientPacketHandler(int packetId, PacketHandler handler)
    {
        packetHandlers.Add(packetId, handler);
        Console.WriteLine($"Added package-{packetId} handler.");
    }
}