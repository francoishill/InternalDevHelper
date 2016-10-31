using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace InternalDevHelper.Utils.ProcessUtils
{
    public class Runner
    {
        private readonly ProcessStartInfo StartInfo;
        private readonly List<FeedbackLine> Feedback;

        public Runner(ProcessStartInfo startInfo)
        {
            StartInfo = startInfo;
            Feedback = new List<FeedbackLine>();
        }

        private void AppendFeedback(FeedbackLine line)
        {
            lock (Feedback)
            {
                Feedback.Add(line);
            }
        }

        public Result RunAndWait()
        {
            Feedback.Clear();

            try
            {
                var process = new Process() { StartInfo = StartInfo };
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;

                process.OutputDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        AppendFeedback(new FeedbackLine(args.Data, FeedbackLine.LineLevel.Output));
                    }
                };

                process.ErrorDataReceived += (sender, args) =>
                {
                    if (args.Data != null)
                    {
                        AppendFeedback(new FeedbackLine(args.Data, FeedbackLine.LineLevel.Error));
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                process.WaitForExit();

                return new Result(Feedback);
            }
            catch (Exception exception)
            {
                return new Result(exception, Feedback);
            }
        }
    }
}
