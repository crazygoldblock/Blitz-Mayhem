using System;
using System.Net;

/// <summary>
/// <para>
/// všechny třídy které se posílají po síti rozšiřují tuto třídu 
/// </para>
/// 
/// <para>
/// pokud je potřeba poslat data po síti vytvoří se třída která rozšiřuje tuto třídu,
/// zavolá se Serialize() z této třídy,
/// i přez to že se serializace zavolala na tuto nižší třídu tak c# zahrne do serializace i data rozšiřující třídy,
/// na druhé straně se data deserializují pomocí Deserialize() na tuto třídu,
/// opět i přes to že se deserializuje na tuto nižší třídu tak c# data rozšiřující třídy zachová,
/// podle hodnoty dataId se tato třída přetypuje na správnou rozšiřující třídu.
/// </para>
/// 
/// <code>
/// PositionData pos = new PositionData(new Vector2(2f, 4f));
/// byte[] data = pos.Serialize();
/// // poslat data po síti
/// 
/// // na druhé straně
/// byte[] data;
/// NetworkData packet = NetworkData.Deserialize(data);
/// 
/// if (packet.GetId() == Constants.POSITION_ID)
///     PositionData pos = (PositionData)packet;
/// </code>
/// </summary>
[Serializable]
public class NetworkData
{
    // id podle kterého se pozná na kterou třídu se má tento packet přetypovat po deserializaci
    private int dataId;
    // slouží s detekci duplikátních packetů
    private int uid;
    // ip adresa počítače který tento paket poslal  
    private IPAddress senderAddr;

    public NetworkData(int dataId)
    {
        this.dataId = dataId;

        uid = UnityEngine.Random.Range(0, int.MaxValue);
    }
    public NetworkData(int dataId, int uid)
    {
        this.dataId = dataId;
        this.uid = uid;
    }

    public int GetId()
    {
        return dataId;
    }
    public int GetUid()
    {
        return uid;
    }
    public void SetUid(int uid)
    {
        this.uid = uid;
    }
    public IPAddress GetSenderAddr()
    {
        return senderAddr;
    }
    public void SetSenderAddr(IPAddress senderAddr)
    {
        this.senderAddr = senderAddr;
    }

    public byte[] Serialize()
    {
        return Serialization.Serialize(this);
    }
    public static NetworkData Deserialize(byte[] data)
    {
        return Serialization.Deserialize<NetworkData>(data);
    }
}
