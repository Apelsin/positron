using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace Positron
{
    #region ConfiguratorBase
    [DataContract]
    public abstract class ConfiguratorBase<T>
    {
        /// <summary>
        /// Creates an XML dictionary writer for configuration files
        /// </summary>
        /// <remarks>
        /// By default, this enables 4-space indentation (not tabs)
        /// </remarks>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected static XmlDictionaryWriter CreateJsonWriter(Stream stream)
        {
            // TODO: figure out some way to enable Unix-style line endings.
            return JsonReaderWriterFactory.CreateJsonWriter(stream, System.Text.Encoding.UTF8, true, true, "    ");
        }
        /// <summary>
        /// The default serializer for all Configuration objects to use
        /// </summary>
        internal XmlObjectSerializer Serializer;
        protected static IEnumerable<Type> KnownTypes()
        {
            yield return typeof(Configurator<T>);
        }
        protected static IEnumerable<Type> KnownTypesMore(IEnumerable<Type> more_types)
        {
            foreach (Type t in KnownTypes())
                yield return t;
            if (more_types != null)
                foreach (Type t in more_types)
                    yield return t;
        }
        protected static XmlObjectSerializer CreateSerializer(IEnumerable<Type> more_types = null)
        {
            return new DataContractJsonSerializer(
                typeof(T),
                new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true,
                    KnownTypes = KnownTypesMore(more_types),
                });
        }

        public ConfiguratorBase(IEnumerable<Type> known_types = null)
        {
            InstantiateSerializer(known_types);
            Setup();
        }
        internal virtual void InstantiateSerializer(IEnumerable<Type> known_types = null)
        {
            Serializer = CreateSerializer(known_types);
        }
        internal virtual void Setup()
        {

        }
    }
    #endregion
    #region Configurator
    [DataContract]
    public class Configurator<T> : ConfiguratorBase<T>
    {
        /// <summary>
        /// Serialize the object into a JSON stream
        /// </summary>
        /// <param name="stream"></param>
        public virtual void Store(Stream stream, T reference)
        {
            using (var writer = CreateJsonWriter(stream))
                Serializer.WriteObject(writer, reference);
        }
        /// <summary>
        /// Deserialize the object from a JSON stream.
        /// </summary>
        /// <remarks>
        /// If the object is a configuration, automatically call Setup()
        /// </remarks>
        /// <param name="stream"></param>
        public virtual T Load(Stream stream)
        {
            T configuration = (T)Serializer.ReadObject(stream);
            if (configuration is Configurator<T>)
            {
                var config = (Configurator<T>)(object)configuration;
                config.Serializer = Serializer;
                config.Setup();
            }
            return configuration;
        }
        public Configurator(IEnumerable<Type> known_types = null) :
            base(known_types)
        {
        }
    }
    #endregion
}
