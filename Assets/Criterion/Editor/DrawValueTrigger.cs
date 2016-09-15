using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PickleTools.UnityEditor;
using UnityEngine.Assertions;

namespace PickleTools.Criterion {
	public class DrawValueTrigger : DrawValue {

		new public static int ValueType = 5;

		private static SearchableSelectList triggerSearchList;
		static Dictionary<int, object> triggerUIDFromSelection = new Dictionary<int, object>();
		static CriterionDataLoader<TriggerModel> triggerLoader;

		static string SELECTED_TRIGGER_PREFS = "Criterion.SelectedTrigger";

		new public static object DrawValueField(object currentValue, int valueType, int[] controlIDs,
											GUIContent title = null,
											GUISkin skin = null, params GUILayoutOption[] options) {

			Assert.IsTrue(controlIDs.Length > 0);

			if (triggerUIDFromSelection.ContainsKey(controlIDs[0])) {
				currentValue = triggerUIDFromSelection[controlIDs[0]];
				triggerUIDFromSelection.Remove(controlIDs[0]);
				GUI.changed = true;
			}
			int triggerUID = 0;
			int.TryParse(currentValue.ToString(), out triggerUID);

			if (triggerLoader == null || triggerLoader.GetData(triggerUID) == null) {
				triggerLoader = new CriterionDataLoader<TriggerModel>();
				triggerLoader.Load();
			}
			if (triggerSearchList == null) {
				triggerSearchList = new SearchableSelectList(new List<string>(triggerLoader.Names),
															 delegate (int selection) {
																 triggerUIDFromSelection.Add(controlIDs[0], selection);
																 PopupWindow.focusedWindow.Close();
															 }, triggerUID, skin);
			}

			GUILayout.BeginHorizontal(options);
			TriggerModel model = triggerLoader.GetData(triggerUID);
			if (model != null) {
				title.text = model.Name;
			}
			if (GUILayout.Button(title, skin.button)) {
				Rect rect = new Rect(Event.current.mousePosition, new Vector2(400, 800));
				PopupWindow.Show(rect, triggerSearchList);
			}

			if (GUILayout.Button("->", skin.button, GUILayout.Width(32))) {
				if (triggerUID > -1) {
					EditorPrefs.SetInt(SELECTED_TRIGGER_PREFS, triggerUID);
				}
			}
			GUILayout.EndHorizontal();
			return triggerUID;
		}
	}
}