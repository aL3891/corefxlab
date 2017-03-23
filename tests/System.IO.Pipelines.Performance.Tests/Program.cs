﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using BenchmarkDotNet.Running;

namespace System.IO.Pipelines.Performance.Tests
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.FirstOrDefault() == "Profile")
            {
                var pipeThroughput = new PipeThroughput();
                pipeThroughput.Setup();
                for (int i = 0; i < 100000; i++)
                {
                    pipeThroughput.ParseLiveAspNetInlineWithWBW();
                }
                return;
            }
            var options = (uint[])Enum.GetValues(typeof(BenchmarkType));
            BenchmarkType type;
            if (args.Length != 1 || !Enum.TryParse(args[0], out type))
            {
                Console.WriteLine($"Please add benchmark to run as parameter:");
                for (var i = 0; i < options.Length; i++)
                {
                    Console.WriteLine($"  {((BenchmarkType)options[i]).ToString()}");
                }

                return;
            }

            RunSelectedBenchmarks(type);
        }

        private static void RunSelectedBenchmarks(BenchmarkType type)
        {
            if (type.HasFlag(BenchmarkType.Enumerators))
            {
                BenchmarkRunner.Run<Enumerators>();
            }
            if (type.HasFlag(BenchmarkType.Throughput))
            {
                BenchmarkRunner.Run<PipeThroughput>();
            }
            if (type.HasFlag(BenchmarkType.ReadCursorOperations))
            {
                BenchmarkRunner.Run<ReadCursorOperationsThroughput>();
            }
        }
    }

    [Flags]
    public enum BenchmarkType : uint
    {
        Enumerators = 1,
        Throughput = 2,
        ReadCursorOperations = 4,
        All = uint.MaxValue
    }
}

