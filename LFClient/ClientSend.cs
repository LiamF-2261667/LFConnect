namespace LFClient;

public class ClientSend
{
    public static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.tcp.SendData(_packet);
        
        Console.WriteLine("Package is send!");
    }

    public static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.udp.SendData(_packet);
        
        Console.WriteLine("Package is send!");
    }
}