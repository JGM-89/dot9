using System.Windows;
using System.Windows.Media;
using Dot9.Models;
using MediaBrush = System.Windows.Media.Brush;
using MediaColor = System.Windows.Media.Color;
using MediaPen = System.Windows.Media.Pen;
using WpfPoint = System.Windows.Point;

namespace Dot9.Rendering;

public static class DotOverlayRenderer
{
    public static void Draw(DrawingContext dc, Rect bounds, Dot9Settings settings, double scale = 1)
    {
        DrawDots(dc, bounds, settings.Dots, scale);
        DrawCentreAnchor(dc, bounds, settings.CentreAnchor, scale);
    }

    public static void DrawDots(DrawingContext dc, Rect bounds, DotSettings dots, double scale = 1)
    {
        if (!dots.Enabled || dots.Opacity <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var color = ParseColor(dots.Color, Colors.LightCyan);
        color.A = (byte)Math.Clamp(dots.Opacity * 255, 0, 255);

        var brush = new SolidColorBrush(color);
        brush.Freeze();

        var pen = dots.Shape == DotShape.Ring ? new MediaPen(brush, Math.Max(1.25 * scale, dots.Size * 0.16 * scale)) : null;
        pen?.Freeze();

        var size = dots.Size * scale;
        var edgeDistance = dots.EdgeDistance * scale;
        var cornerExclusion = dots.CornerExclusion * scale;
        var count = Math.Clamp(dots.DotsPerEdge, 1, 64);

        if (IncludesLeft(dots.Edges))
        {
            foreach (var y in Positions(bounds.Top + cornerExclusion, bounds.Bottom - cornerExclusion, count))
            {
                DrawDot(dc, dots.Shape, brush, pen, new WpfPoint(bounds.Left + edgeDistance, y), size);
            }
        }

        if (IncludesRight(dots.Edges))
        {
            foreach (var y in Positions(bounds.Top + cornerExclusion, bounds.Bottom - cornerExclusion, count))
            {
                DrawDot(dc, dots.Shape, brush, pen, new WpfPoint(bounds.Right - edgeDistance, y), size);
            }
        }

        if (IncludesTop(dots.Edges))
        {
            foreach (var x in Positions(bounds.Left + cornerExclusion, bounds.Right - cornerExclusion, count))
            {
                DrawDot(dc, dots.Shape, brush, pen, new WpfPoint(x, bounds.Top + edgeDistance), size);
            }
        }

        if (IncludesBottom(dots.Edges))
        {
            foreach (var x in Positions(bounds.Left + cornerExclusion, bounds.Right - cornerExclusion, count))
            {
                DrawDot(dc, dots.Shape, brush, pen, new WpfPoint(x, bounds.Bottom - edgeDistance), size);
            }
        }
    }

    private static void DrawCentreAnchor(DrawingContext dc, Rect bounds, CentreAnchorSettings anchor, double scale)
    {
        if (!anchor.Enabled || anchor.Opacity <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var color = ParseColor(anchor.Color, Colors.White);
        color.A = (byte)Math.Clamp(anchor.Opacity * 255, 0, 255);
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        var pen = new MediaPen(brush, Math.Max(1, anchor.StrokeWidth * scale));
        pen.Freeze();

        var center = new WpfPoint(bounds.Left + bounds.Width / 2, bounds.Top + bounds.Height / 2);
        var size = Math.Max(4, anchor.Size * scale);
        switch (anchor.Shape)
        {
            case CentreAnchorShape.Dot:
                dc.DrawEllipse(brush, null, center, size * 0.5, size * 0.5);
                break;
            case CentreAnchorShape.Cross:
                dc.DrawLine(pen, new WpfPoint(center.X - size, center.Y), new WpfPoint(center.X + size, center.Y));
                dc.DrawLine(pen, new WpfPoint(center.X, center.Y - size), new WpfPoint(center.X, center.Y + size));
                break;
            default:
                dc.DrawEllipse(null, pen, center, size * 0.55, size * 0.55);
                break;
        }
    }

    private static IEnumerable<double> Positions(double start, double end, int count)
    {
        if (count == 1)
        {
            yield return (start + end) / 2;
            yield break;
        }

        var span = Math.Max(0, end - start);
        for (var i = 0; i < count; i++)
        {
            yield return start + (span * i / (count - 1));
        }
    }

    private static void DrawDot(DrawingContext dc, DotShape shape, MediaBrush brush, MediaPen? ringPen, WpfPoint center, double size)
    {
        switch (shape)
        {
            case DotShape.Pill:
                var pill = new Rect(center.X - size * 0.72, center.Y - size * 0.36, size * 1.44, size * 0.72);
                dc.DrawRoundedRectangle(brush, null, pill, size * 0.36, size * 0.36);
                break;
            case DotShape.Ring:
                dc.DrawEllipse(null, ringPen, center, size * 0.52, size * 0.52);
                break;
            case DotShape.SoftGlow:
                var glow = new RadialGradientBrush
                {
                    Center = new WpfPoint(0.5, 0.5),
                    GradientOrigin = new WpfPoint(0.5, 0.5),
                    RadiusX = 0.55,
                    RadiusY = 0.55
                };
                if (brush is SolidColorBrush solid)
                {
                    var outer = solid.Color;
                    outer.A = 0;
                    glow.GradientStops.Add(new GradientStop(solid.Color, 0));
                    glow.GradientStops.Add(new GradientStop(outer, 1));
                }
                dc.DrawEllipse(glow, null, center, size * 0.95, size * 0.95);
                break;
            default:
                dc.DrawEllipse(brush, null, center, size * 0.5, size * 0.5);
                break;
        }
    }

    private static MediaColor ParseColor(string value, MediaColor fallback)
    {
        try
        {
            return (MediaColor)System.Windows.Media.ColorConverter.ConvertFromString(value)!;
        }
        catch
        {
            return fallback;
        }
    }

    private static bool IncludesLeft(EdgeSelection selection) =>
        selection is EdgeSelection.LeftOnly or EdgeSelection.LeftRight or EdgeSelection.AllEdges;

    private static bool IncludesRight(EdgeSelection selection) =>
        selection is EdgeSelection.RightOnly or EdgeSelection.LeftRight or EdgeSelection.AllEdges;

    private static bool IncludesTop(EdgeSelection selection) =>
        selection is EdgeSelection.TopOnly or EdgeSelection.TopBottom or EdgeSelection.AllEdges;

    private static bool IncludesBottom(EdgeSelection selection) =>
        selection is EdgeSelection.BottomOnly or EdgeSelection.TopBottom or EdgeSelection.AllEdges;
}
