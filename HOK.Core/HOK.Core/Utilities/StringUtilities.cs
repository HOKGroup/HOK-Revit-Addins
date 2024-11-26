namespace HOK.Core.Utilities
{
    public static class StringUtilities
    {
        /// <summary>
        /// Converts byte size to human readable size ex. kB, MB etc.
        ///  - used by HOK.MissionControl.FamilyPublish
        ///  - used by HOK.MissionControl
        /// </summary>
        /// <param name="byteCount">Size of the file in bytes.</param>
        /// <returns></returns>
        public static string BytesToString(long byteCount)
        {
            string[] suf = { "b", "Kb", "Mb", "Gb", "Tb", "Pb", "Eb" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 0);
            return Math.Sign(byteCount) * num + suf[place];
        }
    }
}
