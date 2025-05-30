namespace HOK.ProjectSheetManager.Classes
{
    public class UtilitySQL
    {
        public string LiteralOrNull(string input)
        {
            if (input == "")
            {
                return "NULL";
            }
            else
            {
                return "'" + input.Replace("'", "''") + "'";
            }
        }
        public string ReplaceQuote(string input)
        {
            return input.Replace("'", "''");
        }
    }
}
