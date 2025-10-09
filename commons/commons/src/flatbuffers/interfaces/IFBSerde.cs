using System;

namespace HIVE.Commons.Flatbuffers.Interfaces
{
    public interface IFBSerde<S, T, D> where S: struct
    {
        public abstract T Serialize(ref T obj);

        public static D Deserialize(S obj) {
            throw new NotImplementedException();
        }

    }
}