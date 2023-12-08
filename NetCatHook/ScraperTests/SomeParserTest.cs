﻿using NetCatHook.Scraper.App;
using Xunit;

namespace NetCatHook.ScraperTests;

public class SomeParserTest
{

    [Fact]
    public void Test()
    {
        SomeParser parser = new();
        var result = parser.DoSome(100);

        Assert.Equal("str-100", result);
    }


}