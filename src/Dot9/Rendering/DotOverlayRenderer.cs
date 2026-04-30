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
        DrawVignette(dc, bounds, settings.Vignette);
        DrawDots(dc, bounds, settings.Dots, scale);
        DrawHorizon(dc, bounds, settings.Horizon, scale);
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

    private static void DrawHorizon(DrawingContext dc, Rect bounds, HorizonSettings horizon, double scale)
    {
        if (!horizon.Enabled || horizon.Opacity <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var color = ParseColor(horizon.Color, Colors.LightCyan);
        color.A = (byte)Math.Clamp(horizon.Opacity * 255, 0, 255);
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        var pen = new MediaPen(brush, Math.Max(0.75, horizon.Thickness * scale));
        pen.Freeze();

        var y = bounds.Top + bounds.Height * Math.Clamp(horizon.VerticalPosition, 0, 100) / 100;
        var halfWidth = bounds.Width * Math.Clamp(horizon.Width, 8, 100) / 200;
        var gap = bounds.Width * Math.Clamp(horizon.CentreGap, 0, 60) / 200;
        var centerX = bounds.Left + bounds.Width / 2;
        var left = centerX - halfWidth;
        var right = centerX + halfWidth;

        switch (horizon.Style)
        {
            case HorizonStyle.FullLine:
                DrawLineWithGap(dc, pen, left, right, centerX, gap, y);
                break;
            case HorizonStyle.Segmented:
                var segments = 4;
                var segmentWidth = (halfWidth - gap) / (segments * 1.7);
                for (var i = 0; i < segments; i++)
                {
                    var offset = gap + i * segmentWidth * 1.7;
                    dc.DrawLine(pen, new WpfPoint(centerX - offset - segmentWidth, y), new WpfPoint(centerX - offset, y));
                    dc.DrawLine(pen, new WpfPoint(centerX + offset, y), new WpfPoint(centerX + offset + segmentWidth, y));
                }
                break;
            default:
                var tickWidth = Math.Max(24 * scale, halfWidth * 0.22);
                dc.DrawLine(pen, new WpfPoint(left, y), new WpfPoint(left + tickWidth, y));
                dc.DrawLine(pen, new WpfPoint(right - tickWidth, y), new WpfPoint(right, y));
                break;
        }
    }

    private static void DrawVignette(DrawingContext dc, Rect bounds, VignetteSettings vignette)
    {
        if (!vignette.Enabled || vignette.Opacity <= 0 || vignette.Strength <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
        {
            return;
        }

        var color = ParseColor(vignette.Color, Colors.Black);
        color.A = (byte)Math.Clamp(vignette.Opacity * 255, 0, 255);
        var clear = color;
        clear.A = 0;

        var radius = Math.Clamp(vignette.Radius, 20, 95) / 100;
        var strengthStop = Math.Clamp(vignette.Strength, 0, 100) / 100;
        var brush = new RadialGradientBrush
        {
            Center = new WpfPoint(0.5, 0.5),
            GradientOrigin = new WpfPoint(0.5, 0.5),
            RadiusX = Math.Max(0.2, radius),
            RadiusY = Math.Max(0.2, radius)
        };
        brush.GradientStops.Add(new GradientStop(clear, Math.Clamp(0.42 + radius * 0.32, 0, 0.9)));
        brush.GradientStops.Add(new GradientStop(color, Math.Clamp(0.78 + strengthStop * 0.18, 0.78, 1)));
        brush.Freeze();
        dc.DrawRectangle(brush, null, bounds);
    }

    private static void DrawLineWithGap(DrawingContext dc, MediaPen pen, double left, double right, double centerX, double gap, double y)
    {
        dc.DrawLine(pen, new WpfPoint(left, y), new WpfPoint(centerX - gap, y));
        dc.DrawLine(pen, new WpfPoint(centerX + gap, y), new WpfPoint(right, y));
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
