using UnityEngine;
using UnityEditor;

namespace PickleTools.Criterion {
	public class DrawAction : MonoBehaviour {

		// this should be configured to be the UID of your UpdateWorldState action type, if you have one
		const int ACTION_TYPE_UPDATE_WORLD_STATE = -1;

		public static bool Draw(ref SequenceActionModel sequenceActionModel, WorldStateData worldStateData,
		                        CriterionDataLoader<ConditionModel> conditionLoader, CriterionDataLoader<ActionModel> actionLoader,
										ActionSelectMenu actionSelectMenu, ConditionSelectMenu conditionSelectMenu,
								int controlID, float screenWidth, int thenActionLevel = 0,
								GUIStyle actionStyle = null, GUISkin skin = null) {
			bool delete = false;


			int actionIndex = sequenceActionModel.UID;
			if (actionLoader == null || actionLoader.GetData(actionIndex) == null) {
				actionLoader = new CriterionDataLoader<ActionModel>();
				actionLoader.Load();
			}
			ActionModel selectedAction = actionLoader.GetData(actionIndex);

			GUILayout.BeginVertical(actionStyle, GUILayout.Width(screenWidth));
			EditorGUILayout.BeginHorizontal();
			GUIContent typeTooltip = new GUIContent("(" + thenActionLevel + ")" + "Action Type", "Select an action type");
			if (selectedAction != null) {
				typeTooltip = new GUIContent("(" + thenActionLevel + ")" + "Action UID", selectedAction.UID + ":\n" + selectedAction.Description);
			}
			EditorGUILayout.PrefixLabel(typeTooltip);
			EditorGUI.BeginChangeCheck();
			actionIndex = actionSelectMenu.DrawSelectMenu(selectedAction.Description, Event.current.mousePosition,
				screenWidth, skin, GUILayout.Width(screenWidth));
			if (EditorGUI.EndChangeCheck()) {
				ActionModel defaultAction = actionLoader.GetData(actionIndex);
				SequenceActionModel[] thenAction = sequenceActionModel.Then;
				sequenceActionModel = new SequenceActionModel() {
					UID = actionIndex,
					Then = thenAction
				};
				System.Array.Copy(defaultAction.Parameters, sequenceActionModel.Parameters, defaultAction.Parameters.Length);
			}
			if (GUILayout.Button("Delete", GUILayout.MaxWidth(60))) {
				delete = true;
			}
			EditorGUILayout.EndHorizontal();

			if (sequenceActionModel.UID == ACTION_TYPE_UPDATE_WORLD_STATE && worldStateData != null) {
				worldStateData = DrawMemory.Draw(worldStateData, conditionLoader, skin, conditionSelectMenu, screenWidth);
				if (sequenceActionModel.Parameters.Length < 3) {
					System.Array.Resize<object>(ref sequenceActionModel.Parameters, 3);
				}
				sequenceActionModel.SetParameter(0, worldStateData.ConditionUID);
				sequenceActionModel.SetParameter(1, worldStateData.Value);
				sequenceActionModel.SetParameter(2, worldStateData.Expiration);
			} else {
				for (int p = 0; p < sequenceActionModel.Parameters.Length; p++) {
					EditorGUILayout.BeginHorizontal();

					if (selectedAction != null && selectedAction.Parameters.Length > p) {
						string infoString = selectedAction.Parameters[p].Name + ":\n" + selectedAction.Parameters[p].Description;
						GUIContent parameterTooltip = new GUIContent(selectedAction.Parameters[p].Name, infoString);
						int valueType = selectedAction.Parameters[p].ValueType;
						if (sequenceActionModel.GetParameter(p) == null) {
							sequenceActionModel.SetParameter(p, "0");
							EditorGUILayout.HelpBox("Not drawing parameter for " + sequenceActionModel.UID + " because its current value is null", MessageType.Warning);
						}

						DrawValue.DrawValueField(sequenceActionModel.GetParameter(p), valueType, new int[1] { 3000 + p },
												 parameterTooltip, skin);

					} else {
						EditorGUILayout.PrefixLabel("PARAMETER NOT DEFINED");
						sequenceActionModel.SetParameter(p, GUILayout.TextField(sequenceActionModel.GetParameter(p).ToString()));
					}

					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndVertical();
			return delete;
		}
	}
}
