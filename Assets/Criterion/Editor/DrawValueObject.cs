using UnityEngine;
using UnityEditor;

namespace PickleTools.Criterion {
	public class DrawValueObject : DrawValue {

		new public static int ValueType = 4;

		new public static object DrawValueField(object currentValue, int valueType, int[] controlIDs,
											GUIContent title = null,
											GUISkin skin = null, params GUILayoutOption[] options) {
			if (currentValue == null) {
				currentValue = "";
			}
			string boundsObject = currentValue.ToString();
			GUILayout.BeginHorizontal(options);
			if (title.tooltip == "") {
				title.tooltip = "Type in the value you want to match with.";
			}
			boundsObject = EditorGUILayout.TextField(title, boundsObject, skin.textArea, options);
			GUILayout.EndHorizontal();
			return boundsObject;
		}

	}
}
