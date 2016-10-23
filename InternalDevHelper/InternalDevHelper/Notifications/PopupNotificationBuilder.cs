using System;
using System.Windows.Forms;

namespace InternalDevHelper.Notifications
{
    public class PopupNotificationBuilder
    {
        private static string m_TitleSuffix = " - Internal Monitor";

        private string m_Title;
        private string m_Message;
        private string[] m_MessageArguments;
        private bool m_Topmost;
        private MessageBoxIcon m_Icon;

        private PopupNotificationBuilder() { }

        public static PopupNotificationBuilder New()
        {
            return new PopupNotificationBuilder();
        }

        public PopupNotificationBuilder WithTitle(string title)
        {
            m_Title = title;
            return this;
        }

        public PopupNotificationBuilder WithMessage(string message, params string[] messageArguments)
        {
            m_Message = message;
            m_MessageArguments = messageArguments;
            return this;
        }

        public PopupNotificationBuilder WithIcon(MessageBoxIcon icon)
        {
            m_Icon = icon;
            return this;
        }

        public PopupNotificationBuilder Topmost()
        {
            m_Topmost = true;
            return this;
        }

        private IWin32Window GetTopmostForm()
        {
            return new Form { TopMost = true, ShowInTaskbar = true };
        }

        public void Show()
        {
            if (m_Message == null)
            {
                throw new NotImplementedException("Message text should be specified");
            }

            MessageBox.Show(
                m_Topmost ? GetTopmostForm() : null,
                string.Format(m_Message, m_MessageArguments),
                m_Title + m_TitleSuffix,
                MessageBoxButtons.OK,
                m_Icon);
        }

        public void ShowInfo()
        {
            WithIcon(MessageBoxIcon.Information);
            Show();
        }
    }
}
