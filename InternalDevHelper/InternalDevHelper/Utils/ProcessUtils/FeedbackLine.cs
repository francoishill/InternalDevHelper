namespace InternalDevHelper.Utils.ProcessUtils
{
    public class FeedbackLine
    {
        public enum LineLevel { Output, Error }

        public readonly string Text;
        public readonly LineLevel Level;

        public FeedbackLine(string text, LineLevel level)
        {
            Text = text;
            Level = level;
        }
    }
}
