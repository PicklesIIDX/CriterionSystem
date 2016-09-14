using UnityEngine;
using UnityEditor;

namespace PickleTools.Criterion {
	public class DrawValueBool : DrawValue {

		new public static int ValueType = 2;

		new public static object DrawValueField(object currentValue, int valueType, int[] controlIDs,
											GUIContent title = null,
											GUISkin skin = null, params GUILayoutOption[] options) {
			bool boolValue = false;
			bool.TryParse(currentValue.ToString(), out boolValue);
			GUILayout.BeginHorizontal(options);
			if (title.tooltip == "") {
				title.tooltip = "Select the toggle for true. Deselect for false.";
			}
			boolValue = EditorGUILayout.Toggle(title, boolValue, skin.toggle, options);
			GUILayout.EndHorizontal();
			return boolValue;
		}

	}
}
