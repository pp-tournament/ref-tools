// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Screens.Play.HUD;
using osuTK;
using static PpTournamentRefTools.APIMatch.MatchEvent.MatchGame.MatchScore;

namespace PpTournamentRefTools.Components
{
    public partial class MatchScore : Container
    {
        private readonly APIMatch.MatchEvent.MatchGame.MatchScore score;
        public readonly double Pp;

        [Resolved]
        private Bindable<RulesetInfo> ruleset { get; set; } = null!;

        public MatchScore(APIMatch.MatchEvent.MatchGame.MatchScore score, double pp)
        {
            this.score = score;
            Pp = pp;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Masking = true;
            CornerRadius = 10;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colour)
        {
            var rulesetInstance = ruleset.Value.CreateInstance();
            var mods = rulesetInstance.AllMods.Where(x => score.Mods.Contains(x.Acronym)).Select(x => x.CreateInstance()).ToList();

            Children = new Drawable[]
            {
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.15f,
                    Colour = score.Match.Team == "red" ? colour.Red : colour.Blue
                },
                new FillFlowContainer()
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Padding = new MarginPadding(7),
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new DrawableRank(score.Rank)
                        {
                            RelativeSizeAxes = Axes.None,
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Width = 30,
                            Height = 15,
                        },
                        new OsuSpriteText()
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Text = UserCache.Get(score.UserID)?.Username ?? score.UserID.ToString(),
                            UseFullGlyphHeight = false
                        },
                        new ModDisplay()
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Scale = new Vector2(0.35f),
                            Current = { Value = mods }
                        },
                        new OsuSpriteText()
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Text = $"{score.Accuracy * 100:N2}%",
                            Font = OsuFont.Default.With(size: 14),
                            Alpha = 0.8f,
                            UseFullGlyphHeight = false
                        },
                        new OsuSpriteText
                        {
                            Text = $"{{ {formatStatistics(score.Statistics)} }}",
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular),
                            Alpha = 0.5f,
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                        },
                    }
                },
                new OsuSpriteText()
                {
                    Origin = Anchor.CentreRight,
                    Anchor = Anchor.CentreRight,
                    Margin = new MarginPadding(7),
                    Text = $"{Pp:N2}pp",
                    Font = OsuFont.Default.With(size: 18, weight: FontWeight.SemiBold),
                    UseFullGlyphHeight = false
                }
            };
        }

        private static string formatStatistics(MatchScoreStatistics statistics)
        {
            return $"{statistics.Count300} / {statistics.Count100} / {statistics.Count50} / {statistics.CountMiss}";
        }
    }
}
