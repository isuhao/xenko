// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using System.Linq;
using SiliconStudio.Xenko.Shaders;

namespace SiliconStudio.Xenko.Graphics
{
    /// <summary>
    /// Describes how <see cref="DescriptorSet"/> will be bound together.
    /// </summary>
    public class RootSignature : GraphicsResourceBase
    {
        internal readonly EffectDescriptorSetReflection EffectDescriptorSetReflection;

        public static RootSignature New(GraphicsDevice graphicsDevice, EffectDescriptorSetReflection effectDescriptorSetReflection)
        {
            return new RootSignature(graphicsDevice, effectDescriptorSetReflection);
        }

        private RootSignature(GraphicsDevice graphicsDevice, EffectDescriptorSetReflection effectDescriptorSetReflection)
            : base(graphicsDevice)
        {
            this.EffectDescriptorSetReflection = effectDescriptorSetReflection;
        }

        protected internal override bool OnRecreate()
        {
            return true;
        }
    }
}
