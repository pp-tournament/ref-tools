// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.API.Requests.Responses;

namespace PpTournamentRefTools
{
    /// <summary>
    /// Match API does not provide user data so we want to get it as soon as possible and reuse from a cache
    /// </summary>
    public static class UserCache
    {
        private static readonly Dictionary<int, APIUser> cache = new Dictionary<int, APIUser>();

        public static APIUser? Get(int id) => cache.TryGetValue(id, out var value) ? value : null;

        public static void Add(APIUser user) => cache.Add(user.OnlineID, user);

        public static bool Contains(int id) => cache.ContainsKey(id);
    }
}
