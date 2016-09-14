using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace PickleTools.Extensions.TypeExtensions {

	/// <summary>
	/// Source: Nate Barbettini
	/// http://stackoverflow.com/questions/2362580/discovering-derived-types-using-reflection
	/// </summary>

	public static class TypeExtensions {

		public static List<Type> GetAllDerivedTypes(this Type type) {
			return Assembly.GetAssembly(type).GetAllDerivedTypes(type);
		}

		public static List<Type> GetAllDerivedTypes(this Assembly assembly, Type type) {
			return assembly
				.GetTypes()
				.Where(t => t != type && type.IsAssignableFrom(t))
				.ToList();
		}
	}
}