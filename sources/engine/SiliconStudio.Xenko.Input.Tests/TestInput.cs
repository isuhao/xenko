﻿// Copyright (c) 2016 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Linq;
using NUnit.Framework;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Games;
using SiliconStudio.Xenko.Graphics.Regression;
using SiliconStudio.Xenko.Input.Gestures;

namespace SiliconStudio.Xenko.Input.Tests
{
    public class TestInput : GameTestBase
    {
        public TestInput()
        {
            InputSourceSimulated.Enabled = true;
        }
        
        void TestPressRelease()
        {
            var events = Input.InputEvents;
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);

            var keyboard = InputSourceSimulated.Instance.Keyboard;

            // Test press
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0] is KeyEvent);
            var keyEvent = (KeyEvent)events[0];
            Assert.IsTrue(keyEvent.Key == Keys.A);
            Assert.IsTrue(keyEvent.State == ButtonState.Down);
            Assert.IsTrue(keyEvent.RepeatCount == 0);
            Assert.IsTrue(keyEvent.Device == keyboard);

            Input.SimulateKeyUp(Keys.A);
            Input.Update(DrawTime);

            // Test release
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0] is KeyEvent);
            keyEvent = (KeyEvent)events[0];
            Assert.IsTrue(keyEvent.Key == Keys.A);
            Assert.IsTrue(keyEvent.State == ButtonState.Up);
        }

        void TestRepeat()
        {
            var events = Input.InputEvents;
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);
            Input.SimulateKeyDown(Keys.A);
            Input.Update(DrawTime);

            var keyboard = InputSourceSimulated.Instance.Keyboard;

            // Test press with release
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0] is KeyEvent);
            var keyEvent = (KeyEvent)events[0];
            Assert.IsTrue(keyEvent.Key == Keys.A);
            Assert.IsTrue(keyEvent.State == ButtonState.Down);
            Assert.IsTrue(keyEvent.RepeatCount == 3);
            Assert.IsTrue(keyEvent.Device == keyboard);

            Input.SimulateKeyUp(Keys.A);
            Input.Update(DrawTime);

            // Test release
            Assert.AreEqual(1, events.Count);
            Assert.IsTrue(events[0] is KeyEvent);
            keyEvent = (KeyEvent)events[0];
            Assert.IsTrue(keyEvent.Key == Keys.A);
            Assert.IsTrue(keyEvent.State == ButtonState.Up);
        }

        void TestMouse()
        {
            var mouse = InputSourceSimulated.Instance.Mouse;

            Vector2 targetPosition;
            mouse.SetPosition(targetPosition = new Vector2(0.5f, 0.5f));
            Input.Update(DrawTime);

            Assert.AreEqual(targetPosition, Input.MousePosition);

            mouse.SetPosition(targetPosition = new Vector2(0.6f, 0.5f));
            mouse.HandleButtonDown(MouseButton.Left);
            Input.Update(DrawTime);

            // Check for pointer events (2, 1 move, 1 down)
            Assert.AreEqual(2, Input.PointerEvents.Count);
            Assert.AreEqual(PointerEventType.Moved, Input.PointerEvents[0].EventType);
            Assert.IsFalse(Input.PointerEvents[0].IsDown);

            // Check down
            Assert.AreEqual(PointerEventType.Pressed, Input.PointerEvents[1].EventType);
            Assert.IsTrue(Input.PointerEvents[1].IsDown);

            // Check delta
            Assert.AreEqual(new Vector2(0.1f, 0.0f), Input.PointerEvents[0].DeltaPosition);
            Assert.AreEqual(new Vector2(0.0f, 0.0f), Input.PointerEvents[1].DeltaPosition);

            // And the position after that, when the pointer goes down
            Assert.AreEqual(targetPosition, Input.PointerEvents[1].Position);

            // Check if new absolute delta matches the one reported in the input manager
            Assert.AreEqual(Input.PointerEvents[0].AbsoluteDeltaPosition, Input.AbsoluteMouseDelta);

            mouse.HandleButtonUp(MouseButton.Left);
            Input.Update(DrawTime);

            // Check up
            Assert.AreEqual(1, Input.PointerEvents.Count);
            Assert.AreEqual(PointerEventType.Released, Input.PointerEvents[0].EventType);
            Assert.IsFalse(Input.PointerEvents[0].IsDown);
        }

        void TestSingleFrameStates()
        {
            var keyboard = InputSourceSimulated.Instance.Keyboard;

            keyboard.SimulateDown(Keys.Space);
            keyboard.SimulateUp(Keys.Space);
            Input.Update(DrawTime);

            Assert.IsTrue(Input.IsKeyPressed(Keys.Space));
            Assert.IsTrue(Input.IsKeyReleased(Keys.Space));
            Assert.IsFalse(Input.IsKeyDown(Keys.Space));


            var mouse = InputSourceSimulated.Instance.Mouse;

            mouse.SimulateMouseDown(MouseButton.Extended2);
            mouse.SimulateMouseUp(MouseButton.Extended2);
            Input.Update(DrawTime);

            Assert.IsTrue(Input.IsMouseButtonPressed(MouseButton.Extended2));
            Assert.IsTrue(Input.IsMouseButtonReleased(MouseButton.Extended2));
            Assert.IsFalse(Input.IsMouseButtonDown(MouseButton.Extended2));
            
            mouse.SimulateMouseDown(MouseButton.Left);
            mouse.SimulateMouseUp(MouseButton.Left);
            mouse.SimulateMouseDown(MouseButton.Left);
            Input.Update(DrawTime);

            Assert.IsTrue(Input.IsMouseButtonPressed(MouseButton.Left));
            Assert.IsTrue(Input.IsMouseButtonReleased(MouseButton.Left));
            Assert.IsTrue(Input.IsMouseButtonDown(MouseButton.Left));

            mouse.SimulateMouseUp(MouseButton.Left);
            Input.Update(DrawTime);
        }

        void TestTapGesture()
        {
            var mouse = InputSourceSimulated.Instance.Mouse;
            
            TapGesture tap = new TapGesture(2, 1);

            Input.ActivatedGestures.Add(tap);

            mouse.HandleButtonDown(MouseButton.Left);
            Input.Update(DrawTime);
            mouse.HandleButtonUp(MouseButton.Left);
            Input.Update(DrawTime);
            mouse.HandleButtonDown(MouseButton.Left);
            Input.Update(DrawTime);
            mouse.HandleButtonUp(MouseButton.Left);
            Input.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(2)));

            // Test tap occured
            Assert.AreEqual(1, tap.Events.Count);
            var tapEvent = (TapEventArgs)tap.Events[0];
            Assert.AreEqual(mouse.Position, tapEvent.TapPosition);
            Assert.AreEqual(2, tapEvent.DeltaTime.Seconds);
            Assert.AreEqual(PointerGestureEventType.Occurred, tapEvent.EventType);

            mouse.HandleButtonDown(MouseButton.Left);
            Input.Update(DrawTime);
            mouse.HandleButtonUp(MouseButton.Left);
            Input.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(2))); // Wait too long
            mouse.HandleButtonDown(MouseButton.Left);
            Input.Update(DrawTime);
            mouse.HandleButtonUp(MouseButton.Left);
            Input.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(2)));

            // Test no tap occured
            Assert.AreEqual(0, tap.Events.Count);

            Input.ActivatedGestures.Remove(tap);
        }

        void TestCompositeGesture()
        {
            var mouse = InputSourceSimulated.Instance.Mouse;

            CompositeGesture composite = new CompositeGesture();

            Input.ActivatedGestures.Add(composite);

            // Starting position
            //
            //    (1)     (2)
            //
            mouse.SimulatePointer(PointerEventType.Moved, new Vector2(0.6f, 0.5f), 1);
            mouse.SimulatePointer(PointerEventType.Moved, new Vector2(0.4f, 0.5f), 2);
            Input.Update(DrawTime);
            mouse.SimulatePointer(PointerEventType.Pressed, new Vector2(0.6f, 0.5f), 1);
            mouse.SimulatePointer(PointerEventType.Pressed, new Vector2(0.4f, 0.5f), 2);
            Input.Update(DrawTime);

            // Check pointer state
            Assert.AreEqual(3, mouse.PointerPoints.Count);
            Assert.AreEqual(new Vector2(0.6f, 0.5f), mouse.PointerPoints[1].Position);
            Assert.IsTrue(mouse.PointerPoints[1].IsDown);

            // Ending position
            //        (1)
            //       /   /
            //        (2)
            mouse.SimulatePointer(PointerEventType.Moved, new Vector2(0.5f, 0.4f), 1);
            mouse.SimulatePointer(PointerEventType.Moved, new Vector2(0.5f, 0.6f), 2);
            Input.Update(DrawTime);

            Assert.IsNotEmpty(composite.Events);
            var evt = (CompositeEventArgs)composite.Events.Last();

            // Check reported rotation
            Assert.AreEqual(PointerGestureEventType.Changed, evt.EventType);
            Assert.AreEqual(-Math.PI * 0.5f, evt.TotalRotation, 0.01f); // 1/4 clockwise rotation
            
            mouse.SimulatePointer(PointerEventType.Released, new Vector2(0.5f, 0.4f), 1);
            mouse.SimulatePointer(PointerEventType.Released, new Vector2(0.5f, 0.6f), 2);
            Input.Update(DrawTime);

            // Check ended
            Assert.IsNotEmpty(composite.Events);
            evt = (CompositeEventArgs)composite.Events.Last();
            Assert.AreEqual(PointerGestureEventType.Ended, evt.EventType);

            Input.Update(DrawTime);

            // Check empty event list
            Assert.IsEmpty(composite.Events);

            Input.ActivatedGestures.Remove(composite);
        }

        void TestConnectedDevices()
        {
            Assert.IsTrue(Input.HasMouse);
            Assert.NotNull(Input.Mouse);
            Assert.IsTrue(Input.HasPointer);
            Assert.NotNull(Input.Pointer);
            Assert.IsTrue(Input.HasKeyboard);
            Assert.NotNull(Input.Keyboard);
            Assert.IsFalse(Input.HasGamePad);
            Assert.IsFalse(Input.HasGameController);

            bool keyboardAdded = false;
            bool keyboardRemoved = false;

            Input.DeviceRemoved += (sender, args) =>
            {
                if (args.Device == InputSourceSimulated.Instance.Keyboard)
                    keyboardRemoved = true;
            };
            Input.DeviceAdded += (sender, args) =>
            {
                if (args.Device == InputSourceSimulated.Instance.Keyboard)
                    keyboardAdded = true;
            };

            // Check keyboard removal
            InputSourceSimulated.Instance.SetKeyboardConnected(false);
            Assert.IsTrue(keyboardRemoved);
            Assert.IsFalse(keyboardAdded);
            Assert.IsNull(Input.Keyboard);
            Assert.IsFalse(Input.HasKeyboard);

            // Check keyboard addition
            InputSourceSimulated.Instance.SetKeyboardConnected(true);
            Assert.IsTrue(keyboardAdded);
            Assert.IsNotNull(Input.Keyboard);
            Assert.IsTrue(Input.HasKeyboard);

            // Test not crashing with no keyboard/mouse
            InputSourceSimulated.Instance.SetKeyboardConnected(false);
            InputSourceSimulated.Instance.SetMouseConnected(false);

            Input.Update(DrawTime);
            Input.Update(DrawTime);
            Input.Update(DrawTime);

            InputSourceSimulated.Instance.SetKeyboardConnected(true);
            InputSourceSimulated.Instance.SetMouseConnected(true);
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.Update(TestConnectedDevices);
            FrameGameSystem.Update(TestPressRelease);
            FrameGameSystem.Update(TestRepeat);
            FrameGameSystem.Update(TestMouse);
            FrameGameSystem.Update(TestSingleFrameStates);
            FrameGameSystem.Update(TestTapGesture);
            FrameGameSystem.Update(TestCompositeGesture);
        }

        [Test]
        public static void RunInputTest()
        {
            RunGameTest(new TestInput());
        }

        public static void Main()
        {
            RunInputTest();
        }
    }
}