using System;
using System.Collections.Generic;
using System.Linq;

namespace InternalDevHelper.Utils.ProcessUtils
{
    public class Result
    {
        public readonly IEnumerable<FeedbackLine> Feedback;
        public readonly Exception Error;
        
        public Result(IEnumerable<FeedbackLine> feedback)
        {
            Error = null;
            Feedback = feedback;
        }

        public Result(Exception error, IEnumerable<FeedbackLine> feedback)
        {
            Error = error;
            Feedback = feedback;
        }

        public bool Success(bool countErrorLinesAsFailed)
        {
            if (Error != null)
            {
                return false;
            }

            if (countErrorLinesAsFailed && Feedback.Any(f => f.Level == FeedbackLine.LineLevel.Error))
            {
                return false;
            }

            return true;
        }

        public string GetDisplayError()
        {
            var joinSeparator = "\n";
            var combinedFeedback = string.Join(joinSeparator, Feedback.Select(f => $"{f.Level.ToString()} {f.Text}"));
            var exceptionPart = Error != null ? "EXCEPTION: " + Error.Message : "";
            return string.Join(joinSeparator, combinedFeedback, exceptionPart);
        }
    }
}
