using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System;
using UnityEngine;
using System.Threading;

/// <summary>
/// třída přes kterou probíhá všechna TCP komunikace po síti
/// </summary>
public class TcpMulticastClient
{
    private TcpClient client;
    private List<TcpClient> clients = new();

    private TcpListener listener;

    private bool host;

    // zastavení vlákna pro připojování klientů
    private bool end = false;

    private LinkedList<int> uids = new();

    private int lastPacketId = -1;
    private int lastPacketUid;
    private int lastCode;

    /// <summary>
    /// vytvoření tcp klenta který je připojený k hostovi 
    /// </summary>
    public TcpMulticastClient(IPAddress addr, int port)
    {
        client = new();
        client.Connect(addr, port);
        
        host = false;
    }
    /// <summary>
    /// Vytvoření tcp klienta který se připojuje ke všem ostaním klentům
    /// </summary>
    public TcpMulticastClient()
    {
        listener = new(IPAddress.Any, 0);
        host = true;

        listener.Start();

        Thread thread = new Thread(new ThreadStart(AcceptConnections));
        thread.Start();
    }
    /// <summary>
    /// připojení nových klientů na separátním vlákně
    /// </summary>
    private void AcceptConnections()
    {
        while (!end)
        {
            TcpClient t = listener.AcceptTcpClient();

            lock (clients)
            {
                clients.Add(t);
            }
        }
    }
    /// <summary>
    /// poslání jednoho paketu hostovi
    /// pokud je tento klient host tak místo toho tento paket pošle všem klientům
    /// </summary>
    public void SendPacket(NetworkData packet)
    {   
        if (host)
        {
            if (packet.GetId() == lastPacketId)
            {
                if (lastPacketId == Constants.PING_ID)
                {
                    if (((PingData)packet).GetCode() == lastCode)
                        packet.SetUid(lastPacketUid);
                }
                else
                    packet.SetUid(lastPacketUid);
            }
            
            lastPacketId = -1;

            for (int i = 0; i < clients.Count; i++)
            {
                try
                {
                    SendPacketInner(clients[i], packet);
                }
                catch
                {
                    lock (clients)
                    {
                        clients[i].Close();
                        clients.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        else
        {
            try
            {
                SendPacketInner(client, packet);
            }
            catch { }
        }
    }
    /// <summary>
    /// vrátí jeden packet od hosta
    /// pokud je tento klient host, vrátí paket od jakéhokoliv klienta
    /// </summary>
    /// <returns></returns>
    public NetworkData GetPacket()
    {
        if (host)
            return GetPacketHost();

        try
        {
            return GetPacketInner(client);
        }
        catch { }

        return null;
    }
    /// <summary>
    /// vrátí paket od jakéhokoliv klienta
    /// </summary>
    private NetworkData GetPacketHost()
    {
        for (int i = 0; i < clients.Count; i++)
        {
            try
            {
                NetworkData p = GetPacketInner(clients[i]);

                if (p == null)
                    continue;

                lastPacketId = p.GetId();
                lastPacketUid = p.GetUid();

                if (lastPacketId == Constants.PING_ID)
                    lastCode = ((PingData)p).GetCode();

                return p;
            }
            catch
            {
                lock (clients)
                {
                    clients[i].Close();
                    clients.RemoveAt(i);
                    i--;
                }
                
            }
        }

        return null;
    }
    private bool IsDuplicate(int uid)
    {
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
    private void SendPacketInner(TcpClient c, NetworkData p)
    {
        uids.AddLast(p.GetUid());

        if (DebugMode.DEBUG_NETWORK)
            Debug.Log("TCP-: " + p.ToString());

        byte[] data = p.Serialize();
        byte[] lenByte = BitConverter.GetBytes(data.Length);
        
        c.GetStream().Write(lenByte, 0, lenByte.Length);
        c.GetStream().Write(data, 0, data.Length);
    }
    private NetworkData GetPacketInner(TcpClient c)
    {
        while (c.GetStream().DataAvailable)
        {
            byte[] lenByte = new byte[sizeof(int)];

            c.GetStream().Read(lenByte, 0, lenByte.Length);

            int len = BitConverter.ToInt32(lenByte, 0);

            byte[] data = new byte[len];

            c.GetStream().Read(data, 0, len);

            NetworkData netData = NetworkData.Deserialize(data);

            if (netData == null)
                continue;

            if (IsDuplicate(netData.GetUid()))
                continue;

            if (DebugMode.DEBUG_NETWORK)
                Debug.Log("TCP+: " + netData.ToString());
            return netData;
        }

        return null;
    }
    /// <summary>
    /// vrátí port na kterém běží tcpListener hosta
    /// </summary>
    public int GetPort()
    {
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    public void Close()
    {
        if (host)
        {
            foreach(TcpClient c in clients)
            {
                c.Close();
            }
        }
        else
        {
            client.Close();
        }

        end = true;
    }
}
