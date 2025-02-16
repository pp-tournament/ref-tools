// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Tablet;
using osu.Framework.Platform;
using osu.Game;
using osu.Game.Graphics.Cursor;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using PpTournamentRefTools.Components;
using PpTournamentRefTools.Configuration;

namespace PpTournamentRefTools
{
    public partial class Game : OsuGameBase
    {
        private Bindable<WindowMode> windowMode = null!;
        private DependencyContainer dependencies = null!;

        [Resolved]
        private FrameworkConfigManager frameworkConfig { get; set; } = null!;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        protected override IDictionary<FrameworkSetting, object> GetFrameworkConfigDefaults() => new Dictionary<FrameworkSetting, object>
        {
            { FrameworkSetting.VolumeUniversal, 0.0d },
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            var apiConfig = new SettingsManager(Storage);
            dependencies.CacheAs(apiConfig);
            dependencies.CacheAs(new APIManager(apiConfig));

            Ruleset.Value = new OsuRuleset().RulesetInfo;

            var dialogOverlay = new DialogOverlay();
            dependencies.CacheAs(dialogOverlay);

            var notificationDisplay = new NotificationDisplay();
            dependencies.CacheAs(notificationDisplay);

            AddRange(new Drawable[]
            {
                new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new MainScreen()
                },
                dialogOverlay,
                notificationDisplay
            });
        }

        public override void SetHost(GameHost host)
        {
            base.SetHost(host);

            host.Window.CursorState |= CursorState.Hidden;

            var tabletInputHandler = host.AvailableInputHandlers.FirstOrDefault(x => x is OpenTabletDriverHandler && x.IsActive);

            if (tabletInputHandler != null)
            {
                tabletInputHandler.Enabled.Value = false;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            windowMode = frameworkConfig.GetBindable<WindowMode>(FrameworkSetting.WindowMode);
            windowMode.BindValueChanged(mode => windowMode.Value = WindowMode.Windowed, true);
        }
    }
}
