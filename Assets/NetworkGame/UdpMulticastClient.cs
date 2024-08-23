using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// třída přes kterou probíhá všechna UDP komunikace po síti
/// 
/// nikde nejsou použité locky nebo thread-safe datové struktury protože všechny funkce se vždy volají buď 
/// z Update funkcí které se všechny volají ze stejného vlákna 
/// nebo z Coroutine které unity také volá ze stejného vlákna
/// na čtení i psaní se nikdy nepoužívají jiná vlákna než to hlavní
/// 
/// unity stejně nepovoluje upravování objektů ve scéně z jiného než hlavního vlákna
/// </summary>
public class UdpMulticastClient
{
    private UdpClient client;

    private readonly int multicastPort;
    private readonly IPAddress multicastAddress;

    private LinkedList<int> uids = new();

    // statistiky pro debugování
    public int packetsRecieved = 0;
    public int packetsSent = 0;
    public int positionsSent = 0;
    public int positionsRecieved = 0;

    public UdpMulticastClient(IPAddress addr, int port)
    {
        multicastPort = port;
        multicastAddress = addr;

        UdpClient c = new();

        c.ExclusiveAddressUse = false;
        c.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        c.Client.Bind(new IPEndPoint(IPAddress.Any, multicastPort));

        c.JoinMulticastGroup(multicastAddress);

        client = c;

        // automaticky uzavřít při zavření aplikace nebo změnění scény
        Application.quitting += Close;
        SceneManager.activeSceneChanged += CloseChange;
    }
    public void SendPacket(NetworkData packet)
    {
        if (packet.GetId() == Constants.POSITION_ID)
            positionsSent++;
        else
            packetsSent++;

        // přidat uid packetu do listu
        // aby si tato třída nemyslela že je to nový packet když ho přijme
        uids.AddLast(packet.GetUid());

        byte[] data = packet.Serialize();

        client.Send(data, data.Length, new IPEndPoint(multicastAddress, multicastPort));
    }
    /// <summary>
    /// zjistit jestli je packet duplikátní 
    /// pokud je většinou se zahodí
    /// </summary>
    private bool IsDuplicate(int uid) {
        foreach (int i in uids) 
        {
            if (uid == i) 
                return true;
        }

        // pokud není duplikání přidat do listu uid přijmutých packetů
        uids.AddLast(uid);

        // ukládat jen posledních 10 uid packetů
        if (uids.Count > 10)
            uids.RemoveFirst();

        return false;
    }
    /// <summary>
    /// vrátí jeden validní nový packet ze sítě nebo null
    /// 
    /// pokud přečte duplicitní packet čte dál
    /// pokud přečte packet který nejde deserializovat čte dál
    /// pokud v UdpClientovi už žádný další packet není vrátí null
    /// </summary>
    public NetworkData GetPacket()
    {
        try
        {
            while (client.Available > 0)
            {
                IPEndPoint senderEndpoint = new(IPAddress.Any, 0);

                byte[] receivedData = client.Receive(ref senderEndpoint);

                NetworkData data = NetworkData.Deserialize(receivedData);

                if (data == null)
                {
                    Debug.LogError("invalid packet");
                    continue;
                }

                data.SetSenderAddr(senderEndpoint.Address);

                if (IsDuplicate(data.GetUid()))
                    continue;

                if (data.GetId() == Constants.POSITION_ID)
                    positionsRecieved++;
                else
                    packetsRecieved++;

                return data;
            }
        }
        catch
        {
            // může se stát že někdo chce číst packet ale jiná funkce již tento multiCast zavřela
            // toto se ale hlavně děje jen při změně scény takže tento tryCatch je zde spíše aby v konzoli nepřekážely zbytečné errory
        }

        return null;
    }
    public void Close()
    {
        try
        {
            client.DropMulticastGroup(multicastAddress);
            client.Close();
        }
        catch (Exception)
        {

        }

        Application.quitting -= Close;
        SceneManager.activeSceneChanged -= CloseChange;
    }
    private void CloseChange(Scene current, Scene next)
    {
        if (current.name == null)
            return;

        Close();
    }
}
