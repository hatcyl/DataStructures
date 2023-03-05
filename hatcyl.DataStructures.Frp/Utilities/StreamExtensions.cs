﻿using Sodium.Frp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace hatcyl.DataStructures.Frp.Utilities
{
    public static class StreamExtensions
    {
        public static Stream<IEnumerable<T>> MergeToEnumerable<T>(this IEnumerable<Stream<T>> streams) =>
            streams.Select(x => x.ConvertToEnumerable()).MergeToEnumerable();

        public static Stream<IEnumerable<T>> MergeToEnumerable<T>(this IEnumerable<Stream<IEnumerable<T>>> streams) =>
            streams.Count() == 1 ? streams.Single() : MergeToEnumerable(streams.Skip(2).Prepend(streams.First().Merge(streams.Skip(1).First(), (x, y) => x.Concat(y))));

        public static Stream<IEnumerable<T>> ConvertToEnumerable<T>(this Stream<T> stream) =>
            stream.Map(x => Enumerable.Empty<T>().Append(x));
    }
}