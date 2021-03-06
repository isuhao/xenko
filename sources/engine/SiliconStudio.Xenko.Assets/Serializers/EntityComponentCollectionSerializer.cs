// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Assets.Serializers;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Core.Yaml;
using SiliconStudio.Core.Yaml.Events;
using SiliconStudio.Core.Yaml.Serialization;
using SiliconStudio.Core.Yaml.Serialization.Serializers;
using SiliconStudio.Xenko.Engine;

namespace SiliconStudio.Xenko.Assets.Serializers
{
    /// <summary>
    /// Error resistant Script loading. It should work even if there is missing properties, members or types.
    /// If main script type is missing (usually due to broken assemblies), it will keep the Yaml representation so that it can be properly saved alter.
    /// </summary>
    [YamlSerializerFactory(YamlAssetProfile.Name)]
    internal class EntityComponentCollectionSerializer : CollectionSerializer
    {
        public override IYamlSerializable TryCreate(SerializerContext context, ITypeDescriptor typeDescriptor)
        {
            var type = typeDescriptor.Type;
            return type == typeof(EntityComponentCollection) ? this : null;
        }

        protected override void ReadAddCollectionItem(ref ObjectContext objectContext, Type elementType, CollectionDescriptor collectionDescriptor, object thisObject, int index)
        {
            var scriptCollection = (EntityComponentCollection)objectContext.Instance;

            EntityComponent value = null;
            bool needAdd = true; // If we could get existing value, no need add to collection
            if (index < scriptCollection.Count)
            {
                value = scriptCollection[index];
                needAdd = false;
            }

            value = (EntityComponent)ReadCollectionItem(ref objectContext, value, elementType, index);
            if (needAdd)
            {
                scriptCollection.Add(value);
            }
        }

        /// <inheritdoc/>
        protected override object ReadCollectionItem(ref ObjectContext objectContext, object value, Type itemType, int index)
        {
            // Save the Yaml stream, in case loading fails we can keep this representation
            var parsingEvents = new List<ParsingEvent>();
            var reader = objectContext.Reader;
            var startDepth = reader.CurrentDepth;
            do
            {
                parsingEvents.Add(reader.Expect<ParsingEvent>());
            } while (reader.CurrentDepth > startDepth);

            // Save states
            var previousReader = objectContext.SerializerContext.Reader;
            var previousAllowErrors = objectContext.SerializerContext.AllowErrors;

            objectContext.SerializerContext.Reader = new EventReader(new MemoryParser(parsingEvents));
            objectContext.SerializerContext.AllowErrors = true;

            try
            {
                return objectContext.ObjectSerializerBackend.ReadCollectionItem(ref objectContext, value, itemType, index);
            }
            catch (YamlException ex)
            {
                // There was a failure, let's keep this object so that it can be serialized back later
                var startEvent = parsingEvents.FirstOrDefault() as MappingStart;
                string typeName = startEvent != null && !string.IsNullOrEmpty(startEvent.Tag) ? startEvent.Tag.Substring(1) : null;

                var log = objectContext.SerializerContext.Logger;
                log?.Warning($"Could not deserialize script {typeName}", ex);

                return new UnloadableComponent(parsingEvents, typeName);
            }
            finally
            {
                // Restore states
                objectContext.SerializerContext.Reader = previousReader;
                objectContext.SerializerContext.AllowErrors = previousAllowErrors;
            }
        }

        /// <inheritdoc/>
        protected override void WriteCollectionItem(ref ObjectContext objectContext, object item, Type itemType, int index)
        {
            // Check if we have a Yaml representation (in case loading failed)
            var unloadableScript = item as UnloadableComponent;
            if (unloadableScript != null)
            {
                var writer = objectContext.Writer;
                foreach (var parsingEvent in unloadableScript.ParsingEvents)
                {
                    writer.Emit(parsingEvent);
                }
                return;
            }

            base.WriteCollectionItem(ref objectContext, item, itemType, index);
        }
    }
}
