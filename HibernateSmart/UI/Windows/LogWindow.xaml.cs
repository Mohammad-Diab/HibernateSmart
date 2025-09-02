using HibernateSmart.Core;
using HibernateSmart.Utils;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace HibernateSmart
{
    public partial class LogWindow : Window
    {
        private Run _cursorRun;
        private bool _allowScrollToEnd = true;

        public ObservableCollection<LogEntry> Logs { get; } = new ObservableCollection<LogEntry>();
        private const int MaxLines = 1000;

        public LogWindow(BackgroundHost host)
        {
            InitializeComponent();
            OnRoleChanged(host.IsServer);
            Logger.Logged += OnLogged;
            host.RoleChanged += OnRoleChanged;
            Closed += (s, e) =>
            {
                Logger.Logged -= OnLogged;
                host.RoleChanged -= OnRoleChanged;
            };
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            const double Margin = 50;
            _allowScrollToEnd = e.VerticalOffset + e.ViewportHeight >= e.ExtentHeight - Margin;
        }

        private void OnLogged(AppMode mode, LogLevel level, string line)
        {
            if (Logs.Count >= MaxLines)
                Logs.RemoveAt(0);

            Logs.Add(new LogEntry { Source = mode.ToString(), Message = line, Level = level.ToString() });
            Dispatcher.Invoke(() => Append(mode.ToString(), level, line));
        }

        private void OnRoleChanged(bool isServer)
        {
            Title = $"HibernateSmart {(isServer ? "(Server)" : "(Client)")} - Live Log";
            logTitle.Text = Title;
        }

        private void Append(string mode, LogLevel level, string line)
        {
            Brush sourceColor = mode.Equals("server", StringComparison.OrdinalIgnoreCase)
                ? Brushes.RoyalBlue
                : Brushes.Green;

            Brush msgColor;
            if (level == LogLevel.Warn)
                msgColor = Brushes.Gold;
            else if (level == LogLevel.Error)
                msgColor = Brushes.Red;
            else
                msgColor = Brushes.White;

            var para = new Paragraph { Margin = new Thickness(0) };
            para.Inlines.Add(new Run($"[{mode}] ") { Foreground = sourceColor });
            para.Inlines.Add(new Run(line) { Foreground = msgColor });
            para.Inlines.Add(_cursorRun);
            LogContainer.Document.Blocks.Add(para);

            while (LogContainer.Document.Blocks.Count > MaxLines)
                LogContainer.Document.Blocks.Remove(LogContainer.Document.Blocks.FirstBlock);

            if (_allowScrollToEnd) LogContainer.ScrollToEnd();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _cursorRun = cursorRun;
            LogContainer.Document.Blocks.Remove(cursorParagraph);
            if (VisualTreeHelper.GetChild(LogContainer, 0) is Decorator border && border.Child is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            }
        }
    }

    /// <summary>
    /// Represents a log entry for display in the log window.
    /// </summary>
    public class LogEntry
    {
        public string Source { get; set; }
        public string Message { get; set; }
        public string Level { get; set; }
    }
}

