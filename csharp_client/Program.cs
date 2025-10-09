using Google.FlatBuffers;
using System.Net.Sockets;
using HIVE.Commons.Flatbuffers.Generated;

string serverIp = "127.0.0.1";
int port = 6000;

using (TcpClient client = new TcpClient(serverIp, port))
{

    using (NetworkStream stream = client.GetStream())
    {
        //Send 4 byte magic number to identify the protocol.
        //I believe that this is a requiremnt of the HIVE protocol.
        uint magic = 0x23476945;
        byte[] magicBytes = BitConverter.GetBytes(magic);
        stream.Write(magicBytes, 0, 4);

        //Build the inner entity flatbuffer (in this case its a presenter)
        var entityBuilder = new FlatBufferBuilder(256);

        //This creates a presenter entity
        var nameOffset = entityBuilder.CreateString("TestPresenter");
        Presenter.StartPresenter(entityBuilder);
        Presenter.AddId(entityBuilder, 12345);
        Presenter.AddName(entityBuilder, nameOffset);
        Presenter.AddSubscription(entityBuilder, 0);
        Presenter.AddRate(entityBuilder, SubscriptionRate.Full);
        var presenterOffset = Presenter.EndPresenter(entityBuilder);

        //This wraps the presenter in an entity union so that it can be sent. (Dont ask me what an entity union is)
        var entityOffset = Entity.CreateEntity(entityBuilder, EntityUnion.Presenter, presenterOffset.Value);
        entityBuilder.Finish(entityOffset.Value);
        byte[] entityBytes = entityBuilder.SizedByteArray();

        //Then build the outer flatbuffer that contains the nested_flatbuffer field
        var builder = new FlatBufferBuilder(1024);

        //This does the nested flatbuffer part I think.
        var dataVector = Payload.CreateDataVector(builder, entityBytes);

        //Here we create the payload that contains the entity
        Payload.StartPayload(builder);
        Payload.AddData(builder, dataVector);
        var payloadOffset = Payload.EndPayload(builder);

        var payloadsVector = State.CreatePayloadVector(builder, new[] { payloadOffset });


        //Finally create the state that contains the payload
        State.StartState(builder);
        State.AddPayload(builder, payloadsVector);
        var stateOffset = State.EndState(builder);

        builder.Finish(stateOffset.Value);

        //Get the final serialized byte array
        byte[] flatbufferMsg = builder.SizedByteArray();

        //Send the length prefix followed by the actual flatbuffer message
        byte[] lenPrefix = BitConverter.GetBytes(flatbufferMsg.Length);
        stream.Write(lenPrefix, 0, lenPrefix.Length);

        //Send the actual flatbuffer message
        stream.Write(flatbufferMsg, 0, flatbufferMsg.Length);

        //Read the server response (if any) and up to 4096 bytes
        byte[] buffer = new byte[4096];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        Console.WriteLine($"Received {bytesRead} bytes from server.");
    }
}