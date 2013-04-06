using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Letterbox.WebClient.Clients
{
    public class MessageSerializer
    {
        public byte[] SerializeMessage(object message)
        {
            MemoryStream stream = new MemoryStream();

            var serializer = new DataContractSerializer(message.GetType());

            var writer = XmlDictionaryWriter.CreateBinaryWriter(stream);
            serializer.WriteObject(writer, message);
            writer.Flush();

            return stream.ToArray();
        }

        public T DeserializeMessage<T>(Stream stream) where T : class
        {
            var serializer = new DataContractSerializer(typeof(T));
            stream.Position = 0;

            var quotas = new XmlDictionaryReaderQuotas();
            using (var reader = XmlDictionaryReader.CreateBinaryReader(stream, quotas))
            {
                T message = serializer.ReadObject(reader, false) as T;
                return message;
            }
        }
    }
}
