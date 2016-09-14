using UnityEngine;
using System.Collections.Generic;
using PickleTools.Extensions.TypeExtensions;

namespace PickleTools.Criterion {
	public class DrawValue : MonoBehaviour {

		public static int ValueType = -1;

		static List<System.Type> derivedTypes = new List<System.Type>();
		static bool refresh = false;

		public static object DrawValueField(ref string lowerValue, ref string upperValue, int valueType,
											GUIContent titleContent, GUISkin skin, int[] controlIDs,
											params GUILayoutOption[] options) {
			object[] arguments = {
			lowerValue, upperValue, valueType, titleContent, skin, controlIDs, options
		};
			if (derivedTypes.Count == 0 || refresh) {
				derivedTypes = typeof(DrawValue).GetAllDerivedTypes();
			}
			foreach (System.Type type in derivedTypes) {
				int classValueType = (int)type.GetProperty("ValueType").GetValue(null, null);
				if (classValueType == valueType) {
					if (skin == null) {
						skin = ScriptableObject.CreateInstance<GUISkin>();
					}
					if (titleContent == null) {
						titleContent = new GUIContent();
					}
					// call the respective type's draw function
					type.GetMethod("DrawValueField").Invoke(null, arguments);
				}
			}

			return arguments[0];
		}

		public static object DrawValueField(object currentValue, int valueType, int[] controlIDs,
											GUIContent passedContent = null,
											GUISkin skin = null, params GUILayoutOption[] options) {

			object[] arguments = {
			currentValue, valueType, controlIDs, passedContent, skin, options
		};

			if (derivedTypes.Count == 0 || refresh) {
				derivedTypes = typeof(DrawValue).GetAllDerivedTypes();
			}
			foreach (System.Type type in derivedTypes) {
				int classValueType = (int)type.GetProperty("ValueType").GetValue(null, null);
				if (classValueType == valueType) {
					if (skin == null) {
						skin = ScriptableObject.CreateInstance<GUISkin>();
					}
					if (passedContent == null) {
						passedContent = new GUIContent();
					}
					// call the respective type's draw function
					type.GetMethod("DrawValueField").Invoke(null, arguments);
				}
			}

			return arguments[0];
		}
	}
}
