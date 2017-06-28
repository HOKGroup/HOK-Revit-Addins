using System;
using System.Linq;
using HOK.MissionControl.Core.Schemas;

namespace HOK.MissionControl.Utils
{
    public static class MonitorUtilities
    {
        /// <summary>
        /// Checks if given updater is present and is turned on.
        /// </summary>
        /// <param name="project">Project to get updater from.</param>
        /// <param name="config">Configuration associated with the Project.</param>
        /// <param name="updaterId">Controlling updater id to match.</param>
        /// <returns>True if updater exists and is on.</returns>
        public static bool IsUpdaterOn(Project project, Configuration config, Guid updaterId)
        {
            var updaterFound = config.updaters
                .FirstOrDefault(x => string.Equals(x.updaterId.ToLower(), updaterId.ToString().ToLower(),
                    StringComparison.Ordinal));

            return updaterFound?.isUpdaterOn ?? false;
        }
    }
}