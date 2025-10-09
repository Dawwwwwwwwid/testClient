namespace HIVE.Commons.Flatbuffers
{
    using Google.FlatBuffers;
    using HIVE.Commons;
    using HIVE.Commons.Flatbuffers.Generated;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public static class FlatbufferSerializer
    {
      public static Offset<Generated.Payload> ConvertAntilatencyStateToPayload(FlatBufferBuilder builder, ulong senderID, Generated.SubscriptionType subscriptionType, Antilatency.Alt.Tracking.State state)
      {
        // build the node from the arguments
        FlatBufferBuilder nestedBuilder = new FlatBufferBuilder(1);

        Generated.Vec3T position = Math.Vec.Vec3.ConvertToVec3T(state.pose.position);
        Generated.Vec4T rotation = Math.Vec.Vec4.ConvertToVec4T(state.pose.rotation);
        Generated.Vec3T velocity = Math.Vec.Vec3.ConvertToVec3T(state.velocity);

        Offset<Generated.Node> node = Generated.Node.CreateNode(nestedBuilder, senderID, position, rotation, velocity, state.stability.value);
        Offset<Generated.Entity> nodeEntity = Generated.Entity.CreateEntity(nestedBuilder, Generated.EntityUnion.Node, node.Value);
        nestedBuilder.Finish(nodeEntity.Value);

        // build the payload using the node and return it
        VectorOffset nodeVector = Generated.Payload.CreateDataVector(builder, nestedBuilder.SizedByteArray());
        return Generated.Payload.CreatePayload(builder, nodeVector);
      }

      public static byte[] CreatePresenterStaticState(ulong presenterId, string connectionName)
      {
        FlatBufferBuilder nestedBuilder = new FlatBufferBuilder(1);
        ushort subscription = EncodePresenterTypes();

        Offset<Presenter> presenter = Presenter.CreatePresenter(nestedBuilder, presenterId, nestedBuilder.CreateString(connectionName), subscription, SubscriptionRate.Full);
        Offset<HIVE.Commons.Flatbuffers.Generated.Entity> presenterEntity = HIVE.Commons.Flatbuffers.Generated.Entity.CreateEntity(nestedBuilder, EntityUnion.Presenter, presenter.Value);
        nestedBuilder.Finish(presenterEntity.Value);

        FlatBufferBuilder builder = new FlatBufferBuilder(1);
        VectorOffset presenterVector = Payload.CreateDataVector(builder, nestedBuilder.SizedByteArray());
        Offset<Payload> payload = Payload.CreatePayload(builder, presenterVector);
        VectorOffset payloadVector = State.CreatePayloadVector(builder, new Offset<Payload>[] { payload });
        builder.FinishSizePrefixed(State.CreateState(builder, payloadVector).Value);
        return builder.SizedByteArray();
      }

        public static Offset<Generated.Payload> ConvertAntilatencyStateToStaticPayload(FlatBufferBuilder builder, ulong id, Generated.SubscriptionType subscriptionType, Antilatency.Alt.Tracking.State state, string headsetName)
      {
        FlatBufferBuilder nestedBuilder = new FlatBufferBuilder(1);
        // switch over the subscription type to add in sample static data
        switch (subscriptionType)
        {
          case Generated.SubscriptionType.Headset:

            // use y tracking position to calculate height for bounding box
            float distanceFromFloor = state.pose.position.y;
            float height = 0.8f + distanceFromFloor / 2;
            float centre = -(height / 3);

            // construct bounding box
            Generated.BoundingBox.StartBoundingBox(nestedBuilder);
            Generated.BoundingBox.AddCentre(nestedBuilder, Generated.Vec3.CreateVec3(nestedBuilder, 0, centre, 0));
            Generated.BoundingBox.AddDimensions(nestedBuilder, Generated.Vec3.CreateVec3(nestedBuilder, 0.25f, height, 1));
            Generated.BoundingBox.AddRotation(nestedBuilder, Generated.Vec4.CreateVec4(nestedBuilder, 0, 0, 0, 0));
            Generated.BoundingBox.AddEllipsoid(nestedBuilder, true);
            Offset<Generated.BoundingBox> headsetBoundingBox = Generated.BoundingBox.EndBoundingBox(nestedBuilder);

            // get headset subscription ushort
            ushort headsetSubscription = EncodeHeadsetTypes();

            // create headset and headset entity union
            Offset<Generated.Headset> headset = Generated.Headset.CreateHeadset(nestedBuilder, id, nestedBuilder.CreateString(headsetName), headsetSubscription, Generated.SubscriptionRate.Full, headsetBoundingBox);
            Offset<Generated.Entity> headsetEntity = Generated.Entity.CreateEntity(nestedBuilder, Generated.EntityUnion.Headset, headset.Value);
            nestedBuilder.Finish(headsetEntity.Value);

            // pack headset into payload and return
            VectorOffset headsetVector = Generated.Payload.CreateDataVector(builder, nestedBuilder.SizedByteArray());
            return Generated.Payload.CreatePayload(builder, headsetVector);

          case Generated.SubscriptionType.Robot:
            // construct bounding box
            Generated.BoundingBox.StartBoundingBox(nestedBuilder);
            Generated.BoundingBox.AddCentre(nestedBuilder, Generated.Vec3.CreateVec3(nestedBuilder, 0, 0.4f, 0));
            Generated.BoundingBox.AddDimensions(nestedBuilder, Generated.Vec3.CreateVec3(nestedBuilder, 0.25f, 1f, 1));
            Generated.BoundingBox.AddRotation(nestedBuilder, Generated.Vec4.CreateVec4(nestedBuilder, 0, 0, 0, 0));
            Generated.BoundingBox.AddEllipsoid(nestedBuilder, true);
            Offset<Generated.BoundingBox> robotBoundingBox = Generated.BoundingBox.EndBoundingBox(nestedBuilder);

            // get robot subscription ushort
            ushort robotSubscription = EncodeSubscriptionType(Generated.SubscriptionType.Own, 0);

            // create robot and robot entity union
            Offset<Generated.Robot> robot = Generated.Robot.CreateRobot(nestedBuilder, id, nestedBuilder.CreateString(headsetName), robotSubscription, Generated.SubscriptionRate.Full, robotBoundingBox, 0);
            Offset<Generated.Entity> robotEntity = Generated.Entity.CreateEntity(nestedBuilder, Generated.EntityUnion.Robot, robot.Value);
            nestedBuilder.Finish(robotEntity.Value);

            // pack robot into payload and return
            VectorOffset robotVector = Generated.Payload.CreateDataVector(builder, nestedBuilder.SizedByteArray());
            return Generated.Payload.CreatePayload(builder, robotVector);

          default:
            // create an return empty payload for now
            return Generated.Payload.CreatePayload(builder);
        }
      }

      //! Unity specific code, cant be used in its current form
      //! User would need to impliment this on their own
      // public static Offset<Payload> ConvertGeometryToStaticPayload(FlatBufferBuilder builder, Geometry geometry, ulong ownerID)
      // {
      //   FlatBufferBuilder nestedBuilder = new FlatBufferBuilder(1);
      //   nestedBuilder.ForceDefaults = true;

      //   Vector3 centre = geometry.GetBoundingBoxCentre();
      //   Vector3 dimensions = geometry.GetGeometryDimensions();
      //   Quaternion rotation = geometry.GetBoundingBoxRotation();

      //   BoundingBox.StartBoundingBox(nestedBuilder);
      //   BoundingBox.AddCentre(nestedBuilder, Vec3.CreateVec3(nestedBuilder, centre.x, centre.y, centre.z));
      //   BoundingBox.AddDimensions(nestedBuilder, Vec3.CreateVec3(nestedBuilder, dimensions.x, dimensions.y, dimensions.z));
      //   BoundingBox.AddRotation(nestedBuilder, Vec4.CreateVec4(nestedBuilder, rotation.x, rotation.y, rotation.z, rotation.w));
      //   BoundingBox.AddEllipsoid(nestedBuilder, geometry.IsBoundingBoxEllipsoid());
      //   Offset<BoundingBox> geoBox = BoundingBox.EndBoundingBox(nestedBuilder);

      //   Offset<Generated.Geometry> geoOffset = Generated.Geometry.CreateGeometry(nestedBuilder, geometry.EntityID, nestedBuilder.CreateString(geometry.EntityName), ownerID, false, geoBox);
      //   Offset<Generated.Entity> geoEntity = Generated.Entity.CreateEntity(nestedBuilder, EntityUnion.Geometry, geoOffset.Value);
      //   nestedBuilder.Finish(geoEntity.Value);

      //   // build the payload using the node and return it
      //   VectorOffset nodeVector = Payload.CreateDataVector(builder, nestedBuilder.SizedByteArray());
      //   return Payload.CreatePayload(builder, nodeVector);
      // }

      //! Unity specific code, cant be used in its current form
      //! User would need to impliment this on their own
      // public static Offset<Payload> ConvertGeometryToPayload(FlatBufferBuilder builder, Geometry geometry, ulong senderID)
      // {
      //   // build the node from the arguments
      //   FlatBufferBuilder nestedBuilder = new FlatBufferBuilder(1);
      //   Vector3 position = geometry.GetCurrentPosition();
      //   Quaternion rotation = geometry.GetBoundingBoxRotation();
      //   Vector3 velocity = geometry.GetCurrentVelocity();

      //   Vec3T nodePosition = new()
      //   {
      //     X = position.x,
      //     Y = position.y,
      //     Z = position.z
      //   };


      //   Vec4T nodeRotation = new()
      //   {
      //     X = rotation.x,
      //     Y = rotation.y,
      //     Z = rotation.z,
      //     W = rotation.w
      //   };

      //   Vec3T nodeVelocity = new()
      //   {
      //     X = velocity.x,
      //     Y = velocity.y,
      //     Z = velocity.z,
      //   };

      //   Offset<Node> node = Node.CreateNode(nestedBuilder, senderID, nodePosition, nodeRotation, nodeVelocity, 0);
      //   Offset<Generated.Entity> nodeEntity = Generated.Entity.CreateEntity(nestedBuilder, EntityUnion.Node, node.Value);
      //   nestedBuilder.Finish(nodeEntity.Value);

      //   // build the payload using the node and return it
      //   VectorOffset nodeVector = Payload.CreateDataVector(builder, nestedBuilder.SizedByteArray());
      //   return Payload.CreatePayload(builder, nodeVector);
      // }

      public static byte[] CreateOwnerCommandMessage(ulong geometryId, ushort familyId, ulong ownerId, bool exclusive)
      {
        FlatBufferBuilder nestedBuilder = new FlatBufferBuilder(1);

        Offset<Generated.Owner> owner = Generated.Owner.CreateOwner(nestedBuilder, geometryId, familyId, ownerId, exclusive == true ? Generated.OwnershipStatus.Exclusive : Generated.OwnershipStatus.Open);
        Offset<Generated.Command> command = Generated.Command.CreateCommand(nestedBuilder, Generated.CommandUnion.Owner, owner.Value);
        Offset<Generated.Entity> entity = Generated.Entity.CreateEntity(nestedBuilder, Generated.EntityUnion.Command, command.Value);
        nestedBuilder.Finish(entity.Value);

        FlatBufferBuilder builder = new FlatBufferBuilder(1);
        VectorOffset ownerVector = Generated.Payload.CreateDataVector(builder, nestedBuilder.SizedByteArray());
        Offset<Generated.Payload> payload = Generated.Payload.CreatePayload(builder, ownerVector);

        VectorOffset payloadVector = Generated.State.CreatePayloadVector(builder, new Offset<Generated.Payload>[] { payload });

        builder.FinishSizePrefixed(Generated.State.CreateState(builder, payloadVector).Value);
        return builder.SizedByteArray();
      }

      public static byte[] CreateDirectRobotCommandMessage(ulong robotId, Math.Vec.Vec3 destination)
      {
        FlatBufferBuilder nestedBuilder = new(1);

        Generated.Vec3T destinationPosition = new();
        destinationPosition = destination.Serialize(ref destinationPosition);

        Offset<Generated.MoveTo> moveTo = Generated.MoveTo.CreateMoveTo(nestedBuilder, robotId, default, destinationPosition);
        Offset<Generated.Command> command = Generated.Command.CreateCommand(nestedBuilder, Generated.CommandUnion.MoveTo, moveTo.Value);
        Offset<Generated.Entity> entity = Generated.Entity.CreateEntity(nestedBuilder, Generated.EntityUnion.Command, command.Value);
        nestedBuilder.Finish(entity.Value);

        FlatBufferBuilder builder = new(1);
        VectorOffset moveToVector = Generated.Payload.CreateDataVector(builder, nestedBuilder.SizedByteArray());
        Offset<Generated.Payload> payload = Generated.Payload.CreatePayload(builder, moveToVector);

        VectorOffset payloadVector = Generated.State.CreatePayloadVector(builder, new Offset<Generated.Payload>[] { payload });

        builder.FinishSizePrefixed(Generated.State.CreateState(builder, payloadVector).Value);
        return builder.SizedByteArray();
      }

      public static bool MatchSubscriptionType(Generated.SubscriptionType type, ushort subscription)
      {
        return ((ushort)(1 << (int)type) & subscription) == 0;
      }

      public static ushort EncodeSubscriptionType(Generated.SubscriptionType type, ushort subscription)
      {
        return (ushort)((1 << (int)type) | subscription);
      }

      private static ushort EncodeHeadsetTypes()
      {
        ushort subscription = 0;
        subscription = EncodeSubscriptionType(Generated.SubscriptionType.Generic, subscription);
        subscription = EncodeSubscriptionType(Generated.SubscriptionType.Robot, subscription);
        subscription = EncodeSubscriptionType(Generated.SubscriptionType.Headset, subscription);
        subscription = EncodeSubscriptionType(Generated.SubscriptionType.Geometry, subscription);
        return subscription;
      }

      public static ushort EncodePresenterTypes()
      {
        ushort subscription = 0;
        subscription = EncodeSubscriptionType(SubscriptionType.Generic, subscription);
        subscription = EncodeSubscriptionType(SubscriptionType.Robot, subscription);
        subscription = EncodeSubscriptionType(SubscriptionType.Headset, subscription);
        subscription = EncodeSubscriptionType(SubscriptionType.Geometry, subscription);
        subscription = EncodeSubscriptionType(SubscriptionType.Presenter, subscription);
        return subscription;
      }
    }
}