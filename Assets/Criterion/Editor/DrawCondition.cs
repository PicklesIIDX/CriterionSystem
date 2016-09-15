using UnityEngine;
using UnityEditor;

namespace PickleTools.Criterion {
	public class DrawCondition : MonoBehaviour {

		const string GUI_SKIN_PATH = "Assets/queryer/Skin.guiskin";

		public static bool Draw(GUIContent titleContent, ref TriggerConditionModel triggerConditionModel,
								ConditionSelectMenu conditionSelectMenu, CriterionDataLoader<ConditionModel> conditionLoader,
									float screenWidth, GUIStyle factStyle = null, GUISkin valueSkin = null) {


			bool deleted = false;
			if (valueSkin == null) {
				valueSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(GUI_SKIN_PATH);
			}
			if (factStyle != null) {
				GUILayout.BeginHorizontal(factStyle);
			} else {
				GUILayout.BeginHorizontal();
			}
			GUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();

			if (conditionLoader == null || conditionLoader.GetData(triggerConditionModel.UID) == null) {
				conditionLoader = new CriterionDataLoader<ConditionModel>();
				conditionLoader.Load();
			}
			ConditionModel selectedCondition = conditionLoader.GetData(triggerConditionModel.UID);
			// condition
			triggerConditionModel.UID = conditionSelectMenu.DrawSelectMenu(selectedCondition.Name + ":\n" +
				selectedCondition.Description, Event.current.mousePosition, screenWidth, valueSkin);

			if (GUILayout.Button("Delete", GUILayout.Width(60))) {
				deleted = true;
			}
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel++;

			string lowerBound = triggerConditionModel.LowerBound.ToString();
			string upperBound = triggerConditionModel.UpperBound.ToString();
			object newValue = DrawValue.DrawValueField(ref lowerBound, ref upperBound, selectedCondition.ValueUID,
													   titleContent, valueSkin, new int[1] { 0 });
			if (selectedCondition.ValueUID != (int)ValueTypeLoader.ValueType.NUMBER_DECIMAL) {
				triggerConditionModel.LowerBound = newValue.ToString();
				triggerConditionModel.UpperBound = newValue.ToString();
			} else {
				triggerConditionModel.LowerBound = lowerBound;
				triggerConditionModel.UpperBound = upperBound;
			}

			EditorGUI.indentLevel--;
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			return deleted;
		}


		public static bool DrawCompressed(ref TriggerConditionModel triggerConditionModel,
												   ConditionSelectMenu conditionSelectMenu,
													  CriterionDataLoader<ConditionModel> conditionLoader,
												   float screenWidth, int controlID1 = 0, int controlID2 = 1,
												   GUIStyle factStyle = null, GUISkin valueSkin = null) {
			bool deleted = false;
			if (valueSkin == null) {
				valueSkin = ScriptableObject.CreateInstance<GUISkin>();
			}
			if (factStyle != null) {
				GUILayout.BeginHorizontal(factStyle);
			} else {
				GUILayout.BeginHorizontal();
			}

			if (conditionLoader == null || conditionLoader.GetData(triggerConditionModel.UID) == null) {
				conditionLoader = new CriterionDataLoader<ConditionModel>();
				conditionLoader.Load();
			}

			ConditionModel selectedCondition = conditionLoader.GetData(triggerConditionModel.UID);
			if (selectedCondition == null) {
				GUILayout.EndHorizontal();
				return deleted;
			}
			triggerConditionModel.UID = conditionSelectMenu.DrawSelectMenu(selectedCondition.Name + ":\n" +
																	selectedCondition.Description,
																  Event.current.mousePosition, (screenWidth - 30) * 0.7f,
																  valueSkin,
																  GUILayout.Width((screenWidth - 30) * 0.7f));
			string lowerBound = "";
			if (triggerConditionModel.LowerBound != null) {
				lowerBound = triggerConditionModel.LowerBound.ToString();
			}
			string upperBound = "";
			if (triggerConditionModel.UpperBound != null) {
				upperBound = triggerConditionModel.UpperBound.ToString();
			}
			object newValue = DrawValue.DrawValueField(ref lowerBound, ref upperBound,
																  selectedCondition.ValueUID, null,
													   valueSkin, new int[2] { controlID1, controlID2 },
																  GUILayout.Width((screenWidth - 30) * 0.3f),
																  GUILayout.Height(21));
			if (selectedCondition.ValueUID != (int)ValueTypeLoader.ValueType.NUMBER_DECIMAL) {
				triggerConditionModel.LowerBound = newValue.ToString();
				triggerConditionModel.UpperBound = newValue.ToString();
			} else {
				triggerConditionModel.LowerBound = lowerBound;
				triggerConditionModel.UpperBound = upperBound;
			}


			if (GUILayout.Button(new GUIContent("x", "Click this button to remove this condition."), valueSkin.button, GUILayout.Width(23))) {
				deleted = true;
			}
			GUILayout.EndHorizontal();

			return deleted;
		}
	}
}
