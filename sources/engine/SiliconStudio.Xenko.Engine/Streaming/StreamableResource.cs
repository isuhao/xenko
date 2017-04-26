﻿// Copyright (c) 2014-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.

using System;

namespace SiliconStudio.Xenko.Streaming
{
    /// <summary>
    /// Base class for all resources that can be dynamicly streamed.
    /// </summary>
    public abstract class StreamableResource : IDisposable
    {
        /// <summary>
        /// Gets the manager.
        /// </summary>
        public StreamingManager Manager { get; private set; }

        /// <summary>
        /// Gets the resource storage.
        /// </summary>
        public ContentStorage Storage { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        public bool IsDisposed => Manager == null;

        /// <summary>
        /// Gets the current residency level.
        /// </summary>
        public abstract int CurrentResidency { get; }

        /// <summary>
        /// Gets the allocated residency level.
        /// </summary>
        public abstract int AllocatedResidency { get; }

        /// <summary>
        /// Gets the target residency level.
        /// </summary>
        public int TargetResidency { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether this resource is allocated.
        /// </summary>
        public bool IsAllocated => AllocatedResidency > 0;
        
        protected StreamableResource(StreamingManager manager, ContentStorage storage)
        {
            Storage = storage;
            Manager = manager;
            Manager.RegisterResource(this);
        }

        void IDisposable.Dispose()
        {
            if (IsDisposed)
                throw new InvalidOperationException();

            Manager.UnregisterResource(this);
            Manager = null;
        }
    }
}
