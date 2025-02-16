# Run this script to use a local copy of osu rather than fetching it from nuget.
# It expects the osu directory to be at the same level as the osu-tools directory

$PROJECTS=@("PpTournamentRefTools.csproj")

$SLN="PpTournamentRefTools.sln"

$DEPENDENCIES=@("..\osu\osu.Game.Rulesets.Osu\osu.Game.Rulesets.Osu.csproj"
    "..\osu\osu.Game\osu.Game.csproj"
)

dotnet sln $SLN add $DEPENDENCIES

ForEach ($CSPROJ in $PROJECTS)
{
    dotnet remove $CSPROJ package ppy.osu.Game
    dotnet remove $CSPROJ package ppy.osu.Game.Rulesets.Osu

    dotnet add $CSPROJ reference $DEPENDENCIES
}
