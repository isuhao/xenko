﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Xenko.Input.Mapping
{
    /// <summary>
    /// An action that generates a floating point value in the range -1 to 1
    /// </summary>
    public class DirectionAction : InputAction
    {
        /// <summary>
        /// Raised when the axis is not 0
        /// </summary>
        event EventHandler<ChangedEventArgs> OnNotZero;

        /// <summary>
        /// Raised when the axis changes value
        /// </summary>
        event EventHandler<ChangedEventArgs> OnChanged;

        /// <summary>
        /// The last value of this action
        /// </summary>
        public Vector2 Value => lastValue;

        private Vector2 lastValue;

        public override void Update()
        {
            Vector2 target = Vector2.Zero;
            float largest = 0.0f;
            foreach (var gesture in Gestures.OfType<IDirectionGesture>())
            {
                float length = gesture.Direction.Length();
                if (length > largest)
                {
                    target = gesture.Direction;
                    largest = length;
                }
            }

            if (lastValue != target)
            {
                lastValue = target;
                OnChanged?.Invoke(this, new ChangedEventArgs { Value = Value });
            }
            if (largest > 0)
            {
                OnNotZero?.Invoke(this, new ChangedEventArgs { Value = Value });
            }
        }

        public override string ToString()
        {
            return $"Direction Action \"{MappingName}\", {nameof(Value)}: {Value}";
        }

        public class ChangedEventArgs : EventArgs
        {
            public Vector2 Value;
        }
    }
}