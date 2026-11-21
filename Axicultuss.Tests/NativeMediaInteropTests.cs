using System;
using System.Runtime.InteropServices;
using DirectShowLib;
using DirectShowLib.DMO;
using Xunit;

namespace Axicultus.Tests;

public sealed class NativeMediaInteropTests
{
    [Fact]
    public void MediaTypeCopyRetainsMetadataAndOwnsAnIndependentFormatBuffer()
    {
        var source = new AMMediaType();
        var copy = new AMMediaType();
        try
        {
            Assert.Equal(0, DMOUtils.MoInitMediaType(source, 8));
            Assert.Equal(8, source.formatSize);
            source.majorType = MediaType.Video;
            source.fixedSizeSamples = true;
            Marshal.WriteByte(source.formatPtr, 0, 17);
            Assert.Equal(0, DMOUtils.MoCopyMediaType(copy, source));
            Assert.Equal(source.majorType, copy.majorType);
            Assert.True(copy.fixedSizeSamples);
            Assert.Equal(8, copy.formatSize);
            Assert.NotEqual(source.formatPtr, copy.formatPtr);
            Marshal.WriteByte(source.formatPtr, 0, 31);
            Assert.Equal(17, Marshal.ReadByte(copy.formatPtr));
        }
        finally
        {
            DsUtils.FreeAMMediaType(copy);
            DsUtils.FreeAMMediaType(source);
        }
    }

    [Fact]
    public void FilterGraphCanEnterAndLeaveTheRunningObjectTable()
    {
        var graph = (IFilterGraph)new FilterGraph();
        try
        {
            using var registration = new DsROTEntry(graph);
        }
        finally { Marshal.ReleaseComObject(graph); }
    }

    [Fact]
    public void EnumeratingWithEmptyTypeFiltersReturnsAUsableComEnumerator()
    {
        Assert.Equal(0, DMOUtils.DMOEnum(DMOCategory.AudioEffect, 0, 0, null, 0, null, out var enumerator));
        try { Assert.Equal(0, enumerator.Reset()); }
        finally { Marshal.ReleaseComObject(enumerator); }
    }
}
