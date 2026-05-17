using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedRunners
{
    public static class AdminHelper
    {
        private static HashSet<string> _adminIds;

        private static HashSet<string> AdminIds
        {
            get
            {
                if (_adminIds == null)
                {
                    string raw = AppSettings.GetConfig("AdminPlatformIDs") ?? string.Empty;
                    _adminIds = new HashSet<string>(
                        raw.Split(',', StringSplitOptions.RemoveEmptyEntries)
                           .Select(s => s.Trim())
                           .Where(s => s.Length > 0));
                }
                return _adminIds;
            }
        }

        public static bool IsAdmin(string platformId)
            => !string.IsNullOrEmpty(platformId) && AdminIds.Contains(platformId);
    }
}
