﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System;
using System.Threading.Tasks;

using SiliconStudio.Core;
using SiliconStudio.Core.Diagnostics;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Engine.Graphics;
using SiliconStudio.Paradox.Engine.Graphics.Composers;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Graphics.Regression;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// A base class for rendering tests
    /// </summary>
    public class UnitTestGameBase : GraphicsTestBase
    {
        protected const EntityGroup UIRendereredGroup = EntityGroup.Group1;
        protected const EntityGroup CameraRenderedGroup = EntityGroup.Group2;

        protected readonly Logger Logger = GlobalLogger.GetLogger("Test Game");
        
        private Vector2 lastTouchPosition;

        protected readonly CameraRendererModeForward SceneCameraRenderer = new CameraRendererModeForward { Name = "Camera UI" };
        protected readonly SceneUIRenderer SceneUIRenderer = new SceneUIRenderer { Name = "Scene UI", CullingMask = UIRendereredGroup };

        protected Scene UIScene;

        protected Entity CameraUIRoot = new Entity("Root entity of camera UI") { new UIComponent()  };
        protected Entity SceneUIRoot = new Entity("Root entity of scene UI") { new UIComponent() };

        protected UIComponent CameraUIComponent { get {  return CameraUIRoot.Get<UIComponent>(); } }
        protected UIComponent SceneUIComponent { get {  return SceneUIRoot.Get<UIComponent>(); } }

        /// <summary>
        /// Create an instance of the game test
        /// </summary>
        public UnitTestGameBase()
        {
            StopOnFrameCount = -1;

            GraphicsDeviceManager.PreferredGraphicsProfile = new [] { GraphicsProfile.Level_11_0 };
            UIScene = new Scene
            {
                Settings =
                {
                    GraphicsCompositor = new SceneGraphicsCompositorLayers
                    {
                        Cameras = { new CameraComponent() },
                        Master =
                        {
                            Renderers =
                            {
                                new ClearRenderFrameRenderer { Color = Color.Green, Name = "Clear frame" },
    
                                new SceneDelegateRenderer(SpecificDrawBeforeUI) { Name = "Delegate before main UI"},

                                new SceneCameraRenderer
                                {
                                    CullingMask = CameraRenderedGroup,
                                    Mode = SceneCameraRenderer,
                                },
                                
                                SceneUIRenderer
                            }
                        }
                    }
                }
            };

            CameraUIRoot.Group = CameraRenderedGroup;
            SceneUIRoot.Group = UIRendereredGroup;

            UIScene.AddChild(CameraUIRoot);
            UIScene.AddChild(SceneUIRoot);

            SceneUIRenderer.VirtualResolution = new Int3(1000, 500, 500);
            SceneUIRenderer.VirtualResolutionMode = VirtualResolutionMode.WidthHeightDepth;
        }

        protected virtual void SpecificDrawBeforeUI(RenderContext context, RenderFrame renderFrame)
        {
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // Set dependency properties test values.
            TextBlock.TextColorPropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(Color.LightGray);
            EditText.TextColorPropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(Color.LightGray);
            EditText.SelectionColorPropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(Color.FromAbgr(0x623574FF));
            EditText.CaretColorPropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(Color.FromAbgr(0xF0F0F0FF));
            var buttonPressedTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ButtonPressed);
            var buttonNotPressedTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ButtonNotPressed);
            var buttonOverredTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ButtonOverred);
            Button.PressedImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test button pressed design", buttonPressedTexture) { Borders = 8 * Vector4.One });
            Button.NotPressedImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test button not pressed design", buttonNotPressedTexture) { Borders = 8 * Vector4.One });
            Button.MouseOverImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test button overred design", buttonOverredTexture) { Borders = 8 * Vector4.One });
            var editActiveTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.EditTextActive);
            var editInactiveTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.EditTextInactive);
            var editOverredTexture = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.EditTextOverred);
            EditText.ActiveImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test edit active design", editActiveTexture) { Borders = 12 * Vector4.One });
            EditText.InactiveImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test edit inactive design", editInactiveTexture) { Borders = 12 * Vector4.One });
            EditText.MouseOverImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test edit overred design", editOverredTexture) { Borders = 12 * Vector4.One });
            var toggleButtonChecked = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ToggleButtonChecked);
            var toggleButtonUnchecked = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ToggleButtonUnchecked);
            var toggleButtonIndeterminate = TextureExtensions.FromFileData(GraphicsDevice, ElementTestDesigns.ToggleButtonIndeterminate);
            ToggleButton.CheckedImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test toggle button checked design", toggleButtonChecked) { Borders = 8 * Vector4.One });
            ToggleButton.UncheckedImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test toggle button unchecked design", toggleButtonUnchecked) { Borders = 8 * Vector4.One });
            ToggleButton.IndeterminateImagePropertyKey.DefaultValueMetadata = DefaultValueMetadata.Static(new UIImage("Test toggle button indeterminate design", toggleButtonIndeterminate) { Borders = 8 * Vector4.One });

            Window.IsMouseVisible = true;

            SceneSystem.SceneInstance = new SceneInstance(Services, UIScene);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (gameTime.FrameCount == StopOnFrameCount || Input.IsKeyDown(Keys.Escape))
                Exit();
        }

        protected PointerEvent CreatePointerEvent(PointerState state, Vector2 position)
        {
            if (state == PointerState.Down)
                lastTouchPosition = position;

            var pointerEvent = new PointerEvent(0, position, position - lastTouchPosition, new TimeSpan(), state, PointerType.Touch, true);

            lastTouchPosition = position;

            return pointerEvent;
        }
    }
}