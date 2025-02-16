﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps.Drawables.Cards;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Toolbar;
using osuTK;

namespace PpTournamentRefTools.Components
{
    public partial class SettingsButton : ToolbarButton, IHasPopover
    {
        protected override Anchor TooltipAnchor => Anchor.TopRight;

        public SettingsButton()
        {
            Hotkey = GlobalAction.ToggleSettings;
            TooltipMain = "Settings";
            Depth = float.MaxValue;

            SetIcon(new SettingsButtonIcon());
        }

        public Popover? GetPopover() => new SettingsPopover();

        protected override bool OnClick(ClickEvent e)
        {
            this.ShowPopover();
            return base.OnClick(e);
        }

        public partial class SettingsButtonIcon : IconPill
        {
            public SettingsButtonIcon()
                : base(FontAwesome.Solid.Cog)
            {
                IconSize = new Vector2(70);
            }

            public override LocalisableString TooltipText => string.Empty;
        }
    }
}
