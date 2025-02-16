// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace PpTournamentRefTools.Components
{
    public partial class MatchTitleCard : Container
    {
        private readonly APIMatch match;

        public MatchTitleCard(APIMatch match)
        {
            this.match = match;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 15;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colour)
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                new Container()
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding() { Horizontal = 50, Vertical = 10 },
                    Children = new Drawable[]
                    {
                        new OsuSpriteText()
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Colour = colour.Blue,
                            Text = match.Match.BlueTeam ?? "Blue",
                            Font = OsuFont.Default.With(size: 20)
                        },
                        new OsuSpriteText()
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "VS",
                            Font = OsuFont.Default.With(size: 20)
                        },
                        new OsuSpriteText()
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Colour = colour.Red,
                            Text = match.Match.RedTeam ?? "Red",
                            Font = OsuFont.Default.With(size: 20)
                        }
                    }
                }
            };
        }
    }
}
