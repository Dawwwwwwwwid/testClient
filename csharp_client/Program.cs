using Google.FlatBuffers;
using HIVE.Commons.Flatbuffers.Generated;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

class VirtualRobotSender
{
    static async Task<int> Main(string[] args)
    {
        string serverIp = "127.0.0.1";
        int port = 6000;

        // Configuration - CHANGED TO 2FPS
        int robotCount = 1;
        int updateIntervalMs = 500; // ~2fps (was 16ms for 60fps)

        ulong baseEntityId = 3781082890840362155;

        using CancellationTokenSource cts = new CancellationTokenSource();

        try
        {
            using TcpClient client = new TcpClient();
            await client.ConnectAsync(serverIp, port);
            Console.WriteLine($"Connected to {serverIp}:{port}");

            using NetworkStream stream = client.GetStream();

            // Send magic number
            uint magic = 0x23476945;
            byte[] magicBytes = BitConverter.GetBytes(magic);
            await stream.WriteAsync(magicBytes, 0, magicBytes.Length, cts.Token);
            await stream.FlushAsync(cts.Token);
            Console.WriteLine("Magic number sent");

            // Start background listener
            Task listener = Task.Run(() => ListenerLoop(stream, cts.Token), cts.Token);

            // Create virtual robot descriptors
            var robots = new List<VirtualRobot>();
            for (int i = 0; i < robotCount; i++)
            {
                ulong id = baseEntityId + (ulong)i;
                string name = $"VirtualRobot_{i + 1}";
                float startX = (float)(-1.5 + i * 1.5);
                float startY = 1.0f + i * 0.2f;
                float startZ = -2.0f;

                float radius = 0.5f + (i * 0.3f);
                float angularSpeed = 0.5f + (i * 0.2f);

                robots.Add(new VirtualRobot
                {
                    Id = id,
                    Name = name,
                    CentreX = startX,
                    CentreY = startY,
                    CentreZ = startZ,
                    Radius = radius,
                    AngularSpeed = angularSpeed,
                    Phase = (float)(i * Math.PI / 4.0)
                });
            }

            Console.WriteLine("=== CREATING VIRTUAL HEADSET ===");

            // STEP 1: Create Robot entities once using their initial centres
            Console.WriteLine("=== CREATING ROBOT ENTITIES ===");
            foreach (var r in robots)
            {
                SendRobotEntity(stream, r.Id, r.Name, SubscriptionRate.Half, r.CentreX, r.CentreY, r.CentreZ);
            }

            await Task.Delay(500, cts.Token);

            // STEP 2: Continuously stream Node updates at 2fps
            Console.WriteLine("=== STARTING NODE POSITION STREAM ===");
            Stopwatch sw = Stopwatch.StartNew();
            long lastUpdateTime = 0;
            long updateCount = 0;

            while (!cts.IsCancellationRequested)
            {
                long now = sw.ElapsedMilliseconds;
                if (now - lastUpdateTime >= updateIntervalMs)
                {
                    float tSeconds = (float)sw.Elapsed.TotalSeconds;

                    foreach (var vr in robots)
                    {
                        float angle = vr.Phase + vr.AngularSpeed * tSeconds;
                        float x = vr.CentreX + vr.Radius * (float)Math.Cos(angle);
                        float z = vr.CentreZ + vr.Radius * (float)Math.Sin(angle);
                        float y = vr.CentreY + 0.05f * (float)Math.Sin(2.0f * angle);

                        //Send a node update
                        SendNodeUpdate(stream, vr.Id, x, y, z, 0.0f);
                    }

                    // Log every update since we're only doing 2fps
                    Console.WriteLine($"Sent {updateCount} batches of Node updates ({robots.Count} robots) at 2fps");

                    lastUpdateTime = now;
                    updateCount++;
                }

                await Task.Delay(10, cts.Token); // Small delay to prevent CPU spinning
            }

            cts.Cancel();
            try { await listener; } catch { }

            return 0; // Success
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fatal error: {ex.Message}");
            return 1; // Error
        }
    }

    class VirtualRobot
    {
        public ulong Id;
        public string Name;
        public float CentreX, CentreY, CentreZ;
        public float Radius;
        public float AngularSpeed;
        public float Phase;
    }

    static void SendRobotEntity(NetworkStream stream, ulong id, string name, SubscriptionRate rate,
        float centreX = 0f, float centreY = 1.0f, float centreZ = -2f)
    {
        try
        {
            var entityBuilder = new FlatBufferBuilder(512);
            var nameOffset = entityBuilder.CreateString(name);

            ushort subscription = EncodeSubscriptionTypes(
                SubscriptionType.Environment,
                SubscriptionType.Headset,
                SubscriptionType.Robot
            );

            // Create bounding box
            var bbOffset = BoundingBox.CreateBoundingBox(entityBuilder,
                centre: new Vec3T { X = centreX, Y = centreY, Z = centreZ },
                dimensions: new Vec3T { X = 0.5f, Y = 0.5f, Z = 0.5f },
                rotation: new Vec4T { W = 1, X = 0, Y = 0, Z = 0 },
                ellipsoid: false);

            // Create Robot with proper subscription
            var robotOffset = Robot.CreateRobot(entityBuilder,
                id: id,
                nameOffset: nameOffset,
                subscription: subscription,
                rate: rate,
                bounding_boxOffset: bbOffset,
                colour: 0xFF00FF00);

            SendEntityPayload(stream, entityBuilder, robotOffset, EntityUnion.Robot);

            Console.WriteLine($"Robot entity created: {name} (ID: {id:X})");
            Console.WriteLine($"Centre: ({centreX:F2}, {centreY:F2}, {centreZ:F2})");
            Console.WriteLine($"Subscriptions: 0x{subscription:X4} ({GetSubscriptionTypes(subscription)})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending robot entity: {ex.Message}");
        }
    }

    static ushort EncodeSubscriptionTypes(params SubscriptionType[] types)
    {
        ushort subscription = 0;
        foreach (var type in types)
        {
            subscription |= (ushort)(1 << (int)type);
        }
        return subscription;
    }

    static string GetSubscriptionTypes(ushort subscription)
    {
        var types = new List<string>();
        foreach (SubscriptionType type in Enum.GetValues(typeof(SubscriptionType)))
        {
            if ((subscription & (1 << (int)type)) != 0)
            {
                types.Add(type.ToString());
            }
        }
        return string.Join(", ", types);
    }

    static void SendNodeUpdate(NetworkStream stream, ulong id, float x, float y, float z, float error = 0.0f)
    {
        try
        {
            var entityBuilder = new FlatBufferBuilder(256);

            var position = new Vec3T { X = x, Y = y, Z = z };
            var rotation = new Vec4T { W = 1, X = 0, Y = 0, Z = 0 };
            var velocity = new Vec3T { X = 0f, Y = 0f, Z = 0f };

            var nodeOffset = Node.CreateNode(entityBuilder,
                id: id,
                position: position,
                rotation: rotation,
                velocity: velocity,
                error: error);

            SendEntityPayload(stream, entityBuilder, nodeOffset, EntityUnion.Node);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending node update: {ex.Message}");
        }
    }

    static void SendEntityPayload(NetworkStream stream, FlatBufferBuilder entityBuilder,
        Offset<Robot> entityOffset, EntityUnion entityType)
    {
        var entityUnionOffset = Entity.CreateEntity(entityBuilder, entityType, entityOffset.Value);
        entityBuilder.Finish(entityUnionOffset.Value);
        byte[] entityBytes = entityBuilder.SizedByteArray();

        SendPayload(stream, entityBytes, "Robot Entity");
    }

    static void SendEntityPayload(NetworkStream stream, FlatBufferBuilder entityBuilder,
        Offset<Node> entityOffset, EntityUnion entityType)
    {
        var entityUnionOffset = Entity.CreateEntity(entityBuilder, entityType, entityOffset.Value);
        entityBuilder.Finish(entityUnionOffset.Value);
        byte[] entityBytes = entityBuilder.SizedByteArray();

        SendPayload(stream, entityBytes, "Node Update");
    }

    static void SendPayload(NetworkStream stream, byte[] entityBytes, string messageType = "Message")
    {
        try
        {
            var builder = new FlatBufferBuilder(1024);
            var dataVector = Payload.CreateDataVector(builder, entityBytes);

            Payload.StartPayload(builder);
            Payload.AddData(builder, dataVector);
            var payloadOffset = Payload.EndPayload(builder);

            var payloadsVector = State.CreatePayloadVector(builder, new[] { payloadOffset });

            State.StartState(builder);
            State.AddPayload(builder, payloadsVector);
            var stateOffset = State.EndState(builder);

            builder.FinishSizePrefixed(stateOffset.Value);

            byte[] flatbufferMsg = builder.SizedByteArray();

            // DISPLAY BYTE INFORMATION
            Console.WriteLine($"=== {messageType} Bytes ===");
            Console.WriteLine($"Total size: {flatbufferMsg.Length} bytes");
            Console.WriteLine($"First 32 bytes (hex): {BytesToHexString(flatbufferMsg, 32)}");
            Console.WriteLine($"First 32 bytes (decimal): {BytesToDecimalString(flatbufferMsg, 32)}");

            if (flatbufferMsg.Length > 32)
            {
                Console.WriteLine($"... and {flatbufferMsg.Length - 32} more bytes");
            }

            Console.WriteLine($"Sending {flatbufferMsg.Length} bytes (WITH SIZE PREFIX)");

            stream.Write(flatbufferMsg, 0, flatbufferMsg.Length);
            stream.Flush();

            Console.WriteLine("Size-prefixed message delivered");
            Console.WriteLine(); // Empty line for readability
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Send failed: {ex.Message}");
            throw;
        }
    }

    // Helper method to display bytes as hex string
    static string BytesToHexString(byte[] bytes, int maxBytes = int.MaxValue)
    {
        int bytesToShow = Math.Min(bytes.Length, maxBytes);
        StringBuilder hex = new StringBuilder(bytesToShow * 3);
        for (int i = 0; i < bytesToShow; i++)
        {
            hex.AppendFormat("{0:X2}", bytes[i]);
            if (i < bytesToShow - 1)
                hex.Append(" ");
        }
        return hex.ToString();
    }

    // Helper method to display bytes as decimal string
    static string BytesToDecimalString(byte[] bytes, int maxBytes = int.MaxValue)
    {
        int bytesToShow = Math.Min(bytes.Length, maxBytes);
        StringBuilder dec = new StringBuilder(bytesToShow * 4);
        for (int i = 0; i < bytesToShow; i++)
        {
            dec.AppendFormat("{0,3}", bytes[i]);
            if (i < bytesToShow - 1)
                dec.Append(" ");
        }
        return dec.ToString();
    }

    static async Task ListenerLoop(NetworkStream stream, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                byte[] sizePrefixBuffer = new byte[4];
                int sizePrefixRead = await stream.ReadAsync(sizePrefixBuffer, 0, 4, ct);

                if (sizePrefixRead == 0) break;
                if (sizePrefixRead < 4) break;

                int messageLength = BitConverter.ToInt32(sizePrefixBuffer, 0) - 4;

                if (messageLength <= 0) continue;

                byte[] messageBuffer = new byte[messageLength];
                int totalBytesRead = await stream.ReadAsync(messageBuffer, 0, messageLength, ct);

                if (totalBytesRead < messageLength) break;

                Console.WriteLine($"Received {messageLength} byte message");
                ProcessIncomingMessage(messageBuffer);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Listener error: {ex.Message}");
        }
    }

    static void ProcessIncomingMessage(byte[] messageBuffer)
    {
        try
        {
            var byteBuffer = new ByteBuffer(messageBuffer);
            var state = State.GetRootAsState(byteBuffer);

            for (int i = 0; i < state.PayloadLength; i++)
            {
                var payload = state.Payload(i);
                var entity = payload?.GetDataAsEntity();
                if (entity.HasValue)
                {
                    switch (entity.Value.EntityType)
                    {
                        case EntityUnion.Robot:
                            var robot = entity.Value.Entity_AsRobot();
                            Console.WriteLine($"Received Robot: {robot.Name} (ID: {robot.Id})");
                            break;
                        case EntityUnion.Node:
                            var node = entity.Value.Entity_AsNode();
                            if (node.Position.HasValue)
                            {
                                var pos = node.Position.Value;
                                Console.WriteLine($"Received Node: ID {node.Id} at ({pos.X:F2}, {pos.Y:F2}, {pos.Z:F2})");
                            }
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing message: {ex.Message}");
        }
    }
}