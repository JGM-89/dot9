using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Dot9.Services;

public static class GithubLinkInjector
{
    private const string GithubUrl = "https://github.com/JGM-89/dot9";
    private const string LinkTag = "Dot9GithubLink";

    public static void Attach(Dot9.MainWindow window)
    {
        window.Loaded += (_, _) => Apply(window);
    }

    private static void Apply(Dot9.MainWindow window)
    {
        UpdateDisplayedVersion(window);
        AddGithubButton(window);
    }

    private static void UpdateDisplayedVersion(DependencyObject root)
    {
        foreach (var textBlock in Descendants<TextBlock>(root))
        {
            if (textBlock.Text == "1.0.0")
            {
                textBlock.Text = "1.0.1";
            }
        }
    }

    private static void AddGithubButton(Dot9.MainWindow window)
    {
        if (window.FindName("AboutView") is not ScrollViewer { Content: StackPanel aboutStack })
        {
            return;
        }

        if (aboutStack.Children.OfType<FrameworkElement>().Any(child => Equals(child.Tag, LinkTag)))
        {
            return;
        }

        var linkButton = new Button
        {
            Content = $"GitHub: {GithubUrl}",
            Tag = LinkTag,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(0, 0, 0, 14),
            Padding = new Thickness(12, 8, 12, 8)
        };
        linkButton.Click += (_, _) => OpenGithub();

        aboutStack.Children.Insert(Math.Min(1, aboutStack.Children.Count), linkButton);
    }

    private static void OpenGithub()
    {
        try
        {
            Process.Start(new ProcessStartInfo(GithubUrl)
            {
                UseShellExecute = true
            });
        }
        catch
        {
            // A failed browser launch should not interrupt the settings UI.
        }
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        var visited = new HashSet<DependencyObject>();
        var stack = new Stack<DependencyObject>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!visited.Add(current))
            {
                continue;
            }

            if (current is T match)
            {
                yield return match;
            }

            try
            {
                var visualCount = VisualTreeHelper.GetChildrenCount(current);
                for (var i = 0; i < visualCount; i++)
                {
                    stack.Push(VisualTreeHelper.GetChild(current, i));
                }
            }
            catch (InvalidOperationException)
            {
                // Some logical children are not visual tree nodes.
            }

            foreach (var logicalChild in LogicalTreeHelper.GetChildren(current).OfType<DependencyObject>())
            {
                stack.Push(logicalChild);
            }
        }
    }
}
