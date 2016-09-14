using UnityEngine;
using UnityEditor;

namespace PickleTools.Criterion {
	public class DrawValueInt : DrawValue {

		new public static int ValueType = 3;

		new public static object DrawValueField(object currentValue, int valueType, int[] controlIDs,
											GUIContent title = null,
											GUISkin skin = null, params GUILayoutOption[] options) {
			int intValue = 0;
			int.TryParse(currentValue.ToString(), out intValue);
			return EditorGUILayout.IntField(title, intValue, skin.textField, options);
		}

	}
}
