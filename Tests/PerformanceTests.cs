using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using NUnit.Framework;
using TB.ComponentModel;
using TypeGuesser;
using TypeGuesser.Deciders;

namespace Tests;

public class PerformanceTests
{
    [Test]
    public void Performance_Decimals()
    {
        var culture = new CultureInfo("en-gb");

        var inputs = new List<string>();

        var r = new Random(500);
        for (var i = 0; i < 1_000_000; i++)
        {
            inputs.Add((r.NextDouble() * 1000.0).ToString("F"));
        }

        var decider = new DecimalTypeDecider(new CultureInfo("en-GB"));

        var req = new DatabaseTypeRequest(null);

        var sw = new Stopwatch();

        sw.Start();

        foreach (var s in inputs)
        {
            decider.IsAcceptableAsType(s, req);
        }

        sw.Stop();

        Console.WriteLine($"DatabaseTypeRequest.IsAcceptableAsType:{sw.ElapsedMilliseconds} ms");

        sw.Restart();

        var g = new Guesser();

        foreach (var s in inputs)
        {
            g.AdjustToCompensateForValue(s);
        }

        sw.Stop();

        Console.WriteLine($"Guesser.AdjustToCompensateForValue:{sw.ElapsedMilliseconds} ms");


        sw.Restart();

        foreach (var s in inputs)
        {
            s.To<decimal>(culture);
        }

        sw.Stop();

        Console.WriteLine($"To<decimal>:{sw.ElapsedMilliseconds} ms");


        sw.Restart();


        foreach (var s in inputs)
        {
            decimal.TryParse(s,NumberStyles.Any,culture,out _ );
        }

        sw.Stop();

        Console.WriteLine($"Parse:{sw.ElapsedMilliseconds} ms");

    }
}