using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace Scratch
{
    public class EchoStateSerialiser
    {
        public string SerilaiseEchoState(EchoState obj)
        {
            var memoryStream = new MemoryStream();
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(memoryStream, obj);
            var bytes = memoryStream.ToArray().ToList();

            string echoState = "";
            bytes.ForEach(b => echoState += b.ToString() + "|");
            echoState = echoState.Remove(echoState.Length - 1);

            return echoState;
        }

        public EchoState DeserializeEchoState(string serializedEchoState)
        {
            var bytesIn = serializedEchoState.Split('|');
            var bytes = new byte[bytesIn.Length];
            int index = 0;
            foreach (var byteIn in bytesIn)
            {
                bytes[index] = byte.Parse(byteIn);
                index++;
            }

            var memoryStream = new MemoryStream(bytes);
            var binaryFormatter = new BinaryFormatter();
            memoryStream.Position = 0;

            return (EchoState)binaryFormatter.Deserialize(memoryStream);
        }
    }
}