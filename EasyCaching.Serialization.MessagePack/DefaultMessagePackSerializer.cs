﻿namespace EasyCaching.Serialization.MessagePack
{
    using System;
    using EasyCaching.Core.Serialization;
    using global::MessagePack;
    using global::MessagePack.Resolvers;

    /// <summary>
    /// Default messagepack serializer.
    /// </summary>
    public class DefaultMessagePackSerializer : IEasyCachingSerializer
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="T:EasyCaching.Serialization.MessagePack.DefaultMessagePackSerializer"/> class.
        /// </summary>
        public DefaultMessagePackSerializer()
        {
            MessagePackSerializer.SetDefaultResolver(CompositeResolver.Instance);
            CompositeResolver.RegisterAndSetAsDefault(
       // Resolve DateTime first
       NativeDateTimeResolver.Instance
       , ContractlessStandardResolver.Instance
   );
            
        }

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public T Deserialize<T>(byte[] bytes)
        {
            return MessagePackSerializer.Deserialize<T>(bytes);
        }

        /// <summary>
        /// Deserialize the specified bytes.
        /// </summary>
        /// <returns>The deserialize.</returns>
        /// <param name="bytes">Bytes.</param>
        /// <param name="type">Type.</param>
        public object Deserialize(byte[] bytes, Type type)
        {
            return MessagePackSerializer.NonGeneric.Deserialize(type, bytes);
        }

        /// <summary>
        /// Serialize the specified value.
        /// </summary>
        /// <returns>The serialize.</returns>
        /// <param name="value">Value.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public byte[] Serialize<T>(T value)
        {
            return MessagePackSerializer.Serialize(value);
        }

        /// <summary>
        /// Serializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        public ArraySegment<byte> SerializeObject(object value)
        {
            return MessagePackSerializer.SerializeUnsafe<object>(value, TypelessContractlessStandardResolver.Instance);
        }

        /// <summary>
        /// Deserializes the object.
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="value">Value.</param>
        public object DeserializeObject(ArraySegment<byte> value)
        {
            return MessagePackSerializer.Deserialize<object>(value, TypelessContractlessStandardResolver.Instance);
        }

    }
}
