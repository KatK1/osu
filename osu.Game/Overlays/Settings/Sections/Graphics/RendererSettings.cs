﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public partial class RendererSettings : SettingsSubsection
    {
        protected override LocalisableString Header => GraphicsSettingsStrings.RendererHeader;

        private bool automaticRendererInUse;
        private SettingsEnumDropdown<FrameSync>? frameSyncDropdown;
        private bool usingVSync;
        private bool usingMetalRenderer;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config, OsuConfigManager osuConfig, IDialogOverlay? dialogOverlay, OsuGame? game, GameHost host)
        {
            var renderer = config.GetBindable<RendererType>(FrameworkSetting.Renderer);
            automaticRendererInUse = renderer.Value == RendererType.Automatic;
            usingMetalRenderer = host.ResolvedRenderer == RendererType.Metal || host.ResolvedRenderer == RendererType.Deferred_Metal;

            var frameSync = config.GetBindable<FrameSync>(FrameworkSetting.FrameSync);
            usingVSync = frameSync.Value == FrameSync.VSync;

            Children = new Drawable[]
            {
                new RendererSettingsDropdown
                {
                    LabelText = GraphicsSettingsStrings.Renderer,
                    Current = renderer,
                    Items = host.GetPreferredRenderersForCurrentPlatform().Order()
#pragma warning disable CS0612 // Type or member is obsolete
                                .Where(t => t != RendererType.Vulkan && t != RendererType.OpenGLLegacy),
#pragma warning restore CS0612 // Type or member is obsolete
                    Keywords = new[] { @"compatibility", @"directx" },
                },
                // TODO: this needs to be a custom dropdown at some point
                frameSyncDropdown = new SettingsEnumDropdown<FrameSync>
                {
                    LabelText = GraphicsSettingsStrings.FrameLimiter,
                    Current = frameSync,
                    Keywords = new[] { @"fps" },
                },
                new SettingsEnumDropdown<ExecutionMode>
                {
                    LabelText = GraphicsSettingsStrings.ThreadingMode,
                    Current = config.GetBindable<ExecutionMode>(FrameworkSetting.ExecutionMode)
                },
                new SettingsCheckbox
                {
                    LabelText = GraphicsSettingsStrings.ShowFPS,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.ShowFpsDisplay)
                },
            };

            renderer.BindValueChanged(r =>
            {
                if (r.NewValue == host.ResolvedRenderer)
                    return;

                // Need to check startup renderer for the "automatic" case, as ResolvedRenderer above will track the final resolved renderer instead.
                if (r.NewValue == RendererType.Automatic && automaticRendererInUse)
                    return;

                if (game?.RestartAppWhenExited() == true)
                {
                    game.AttemptExit();
                }
                else
                {
                    dialogOverlay?.Push(new ConfirmDialog(GraphicsSettingsStrings.ChangeRendererConfirmation, () => game?.AttemptExit(), () =>
                    {
                        renderer.Value = automaticRendererInUse ? RendererType.Automatic : host.ResolvedRenderer;
                    }));
                }
            });

            frameSync.BindValueChanged(sync =>
            {
                if (sync.NewValue == FrameSync.VSync)
                    usingVSync = true;
                else
                    usingVSync = false;

                handleMetalVSync();
            });

            // run once to ensure warning appears
            handleMetalVSync();
        }

        private void handleMetalVSync()
        {
            if (usingMetalRenderer && !usingVSync)
                frameSyncDropdown?.SetNoticeText(GraphicsSettingsStrings.MetalVSync, true);
            else
                frameSyncDropdown?.ClearNoticeText();
        }

        private partial class RendererSettingsDropdown : SettingsEnumDropdown<RendererType>
        {
            protected override OsuDropdown<RendererType> CreateDropdown() => new RendererDropdown();

            protected partial class RendererDropdown : DropdownControl
            {
                private RendererType hostResolvedRenderer;
                private bool automaticRendererInUse;

                [BackgroundDependencyLoader]
                private void load(FrameworkConfigManager config, GameHost host)
                {
                    var renderer = config.GetBindable<RendererType>(FrameworkSetting.Renderer);
                    automaticRendererInUse = renderer.Value == RendererType.Automatic;
                    hostResolvedRenderer = host.ResolvedRenderer;
                }

                protected override LocalisableString GenerateItemText(RendererType item)
                {
                    if (item == RendererType.Automatic && automaticRendererInUse)
                        return LocalisableString.Interpolate($"{base.GenerateItemText(item)} ({hostResolvedRenderer.GetDescription()})");

                    return base.GenerateItemText(item);
                }
            }
        }
    }
}
