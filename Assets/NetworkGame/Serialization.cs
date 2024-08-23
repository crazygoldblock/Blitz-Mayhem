using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

/// <summary>
/// používá se na serializaci a deserializaci všech objektů 
/// které se potřeba poslat po síti
/// </summary>
public class Serialization
{
    public static byte[] Serialize<T>(T o)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        byte[] array;

        using (MemoryStream stream = new MemoryStream())
        {
            formatter.Serialize(stream, o);
            array = stream.ToArray();
        }
        return array;
    }
    public static T Deserialize<T>(byte[] data)
    {
        BinaryFormatter formatter = new();

        using MemoryStream stream = new(data);

        try 
        {
            return (T)formatter.Deserialize(stream);
        }
        catch (SerializationException) 
        { 
            return default;
        }
    }
}
