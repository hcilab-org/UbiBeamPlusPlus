using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UbiDisplays.Model.Native
{
	static class ListExtensions
	{
		public static List<T> Slice<T>(this List<T> source, int start)
		{
			return source.Skip(start).ToList<T>();
		}

		public static List<T> Slice<T>(this List<T> source, int start, int size)
		{
			return source.Skip(start).Take(size).ToList<T>();
		}

		public static List<T> Splice<T>(this List<T> source, int start)
		{
			List<T> ret = source.Skip(start).ToList<T>();
			source.RemoveRange(start, source.Count - start);
			return ret;
		}

		public static List<T> Splice<T>(this List<T> source, int start, int size)
		{
			List<T> ret = source.Skip(start).Take(size).ToList<T>();
			source.RemoveRange(start, size);
			return ret;
		}
	}
}
