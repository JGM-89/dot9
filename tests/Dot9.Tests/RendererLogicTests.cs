using System.Linq;
using System.Windows.Media;
using Dot9.Models;
using Dot9.Rendering;
using Xunit;

namespace Dot9.Tests;

public class RendererLogicTests
{
    // ── Positions: even distribution of dots along an edge ──

    [Fact]
    public void Positions_SingleCount_ReturnsMidpoint()
    {
        var result = DotOverlayRenderer.Positions(0, 100, 1).ToList();
        Assert.Single(result);
        Assert.Equal(50, result[0]);
    }

    [Fact]
    public void Positions_DistributesEvenlyIncludingEndpoints()
    {
        var result = DotOverlayRenderer.Positions(0, 100, 5).ToList();
        Assert.Equal(new double[] { 0, 25, 50, 75, 100 }, result);
    }

    [Fact]
    public void Positions_TwoCount_ReturnsStartAndEnd()
    {
        var result = DotOverlayRenderer.Positions(10, 30, 2).ToList();
        Assert.Equal(new double[] { 10, 30 }, result);
    }

    [Fact]
    public void Positions_NegativeSpan_CollapsesToStart()
    {
        // end < start → span clamps to 0, so every point sits at start (no NaN/negative walk)
        var result = DotOverlayRenderer.Positions(80, 20, 4).ToList();
        Assert.Equal(4, result.Count);
        Assert.All(result, p => Assert.Equal(80, p));
    }

    // ── Edge selection mapping ──

    [Theory]
    [InlineData(EdgeSelection.LeftOnly, true)]
    [InlineData(EdgeSelection.LeftRight, true)]
    [InlineData(EdgeSelection.AllEdges, true)]
    [InlineData(EdgeSelection.RightOnly, false)]
    [InlineData(EdgeSelection.TopBottom, false)]
    [InlineData(EdgeSelection.BottomOnly, false)]
    public void IncludesLeft_MapsCorrectly(EdgeSelection selection, bool expected)
        => Assert.Equal(expected, DotOverlayRenderer.IncludesLeft(selection));

    [Theory]
    [InlineData(EdgeSelection.RightOnly, true)]
    [InlineData(EdgeSelection.LeftRight, true)]
    [InlineData(EdgeSelection.AllEdges, true)]
    [InlineData(EdgeSelection.LeftOnly, false)]
    [InlineData(EdgeSelection.TopBottom, false)]
    public void IncludesRight_MapsCorrectly(EdgeSelection selection, bool expected)
        => Assert.Equal(expected, DotOverlayRenderer.IncludesRight(selection));

    [Theory]
    [InlineData(EdgeSelection.TopOnly, true)]
    [InlineData(EdgeSelection.TopBottom, true)]
    [InlineData(EdgeSelection.AllEdges, true)]
    [InlineData(EdgeSelection.LeftRight, false)]
    [InlineData(EdgeSelection.BottomOnly, false)]
    public void IncludesTop_MapsCorrectly(EdgeSelection selection, bool expected)
        => Assert.Equal(expected, DotOverlayRenderer.IncludesTop(selection));

    [Theory]
    [InlineData(EdgeSelection.BottomOnly, true)]
    [InlineData(EdgeSelection.TopBottom, true)]
    [InlineData(EdgeSelection.AllEdges, true)]
    [InlineData(EdgeSelection.LeftRight, false)]
    [InlineData(EdgeSelection.TopOnly, false)]
    public void IncludesBottom_MapsCorrectly(EdgeSelection selection, bool expected)
        => Assert.Equal(expected, DotOverlayRenderer.IncludesBottom(selection));

    [Fact]
    public void LeftRight_IncludesOnlyLeftAndRight()
    {
        Assert.True(DotOverlayRenderer.IncludesLeft(EdgeSelection.LeftRight));
        Assert.True(DotOverlayRenderer.IncludesRight(EdgeSelection.LeftRight));
        Assert.False(DotOverlayRenderer.IncludesTop(EdgeSelection.LeftRight));
        Assert.False(DotOverlayRenderer.IncludesBottom(EdgeSelection.LeftRight));
    }

    [Fact]
    public void AllEdges_IncludesEveryEdge()
    {
        Assert.True(DotOverlayRenderer.IncludesLeft(EdgeSelection.AllEdges));
        Assert.True(DotOverlayRenderer.IncludesRight(EdgeSelection.AllEdges));
        Assert.True(DotOverlayRenderer.IncludesTop(EdgeSelection.AllEdges));
        Assert.True(DotOverlayRenderer.IncludesBottom(EdgeSelection.AllEdges));
    }

    // ── Colour parsing ──

    [Fact]
    public void ParseColor_ValidHex_Parses()
    {
        var c = DotOverlayRenderer.ParseColor("#87D8E8", Colors.Black);
        Assert.Equal(0x87, c.R);
        Assert.Equal(0xD8, c.G);
        Assert.Equal(0xE8, c.B);
    }

    [Fact]
    public void ParseColor_NamedColor_Parses()
        => Assert.Equal(Colors.Red, DotOverlayRenderer.ParseColor("Red", Colors.Black));

    [Theory]
    [InlineData("not-a-color")]
    [InlineData("")]
    [InlineData("#GGGGGG")]
    public void ParseColor_Invalid_ReturnsFallback(string value)
        => Assert.Equal(Colors.Black, DotOverlayRenderer.ParseColor(value, Colors.Black));
}
