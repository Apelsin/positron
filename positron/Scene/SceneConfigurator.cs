using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace Positron
{
    public class SceneConfigurator : Configurator<Scene>
    {
        #region Configurator Design Pattern
        new protected static IEnumerable<Type> KnownTypes()
        {
            foreach(Type type in Configurator<Scene>.KnownTypes())
                yield return type;
            yield return typeof(ContractElement);
            yield return typeof(SceneRoot);
            yield return typeof(GameObject);
            yield return typeof(Camera);
            yield return typeof(SpriteBase);
        }
        new protected static IEnumerable<Type> KnownTypesMore(IEnumerable<Type> more_types)
        {
            foreach (Type t in KnownTypes())
                yield return t;
            if (more_types != null)
                foreach (Type t in more_types)
                    yield return t;
        }
        new protected static XmlObjectSerializer CreateSerializer(IEnumerable<Type> more_types)
        {
            return Configurator<Scene>.CreateSerializer(KnownTypesMore(more_types));
        }
        internal override void InstantiateSerializer(IEnumerable<Type> known_types = null)
        {
            Serializer = CreateSerializer(known_types);
        }
        #endregion
        /// <summary>
        /// Deserialize the object from a JSON stream.
        /// </summary>
        /// <remarks>
        /// If the object is a configuration, automatically call Setup()
        /// </remarks>
        /// <param name="stream"></param>
        public Scene Load(System.IO.Stream stream, PositronGame game)
        {
            var scene = base.Load(stream);
            scene.Game = game;
            return scene;
        }
        public SceneConfigurator(IEnumerable<Type> known_types):
            base(known_types)
        {
        }
    }
}
