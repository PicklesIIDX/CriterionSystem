using UnityEngine;
using UnityEditor;

namespace PickleTools.Criterion {
	public class DrawValueFloatRange : DrawValue {

		new public static int ValueType = 1;

		new public static object DrawValueField(ref string lowerValue, ref string upperValue, int valueType,
											GUIContent titleContent, GUISkin skin, int[] controlIDs,
											params GUILayoutOption[] options) {
			if (valueType == ValueType) {
				float lower = 0;
				float.TryParse(lowerValue, out lower);
				float upper = 0;
				float.TryParse(upperValue, out upper);
				// draw
				EditorGUILayout.BeginHorizontal(options);
				lower = EditorGUILayout.FloatField(lower, skin.textField, GUILayout.Width(50));
				EditorGUILayout.LabelField(new GUIContent(">=<", "The left value is the minimum and the right value is the maximum value" +
														  " allowed for this to be true."), skin.label, GUILayout.Width(30));
				upper = EditorGUILayout.FloatField(upper, skin.textField, GUILayout.Width(50));
				EditorGUILayout.EndHorizontal();
				// assign
				lowerValue = lower.ToString();
				upperValue = upper.ToString();
			}
			return lowerValue;
		}
	}
}
