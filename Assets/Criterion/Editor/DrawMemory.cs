using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PickleTools.Criterion {
	public class DrawMemory : MonoBehaviour {
		public static WorldStateData Draw(WorldStateData worldStateData,
										  ConditionLoader conditionLoader,
										  GUISkin skin,
										  ConditionSelectMenu conditionSelectMenu, float screenWidth) {

			GUILayout.BeginVertical();

			// Fact
			if (worldStateData == null) {
				worldStateData = new WorldStateData(0, "", 0.0f, false, false);
			}

			if (conditionLoader == null || conditionLoader.GetCondition(worldStateData.ConditionUID) == null) {
				conditionLoader = new ConditionLoader();
				conditionLoader.Load();
			}

			string conditionDescription = "null";
			ConditionModel selectedCondition = conditionLoader.GetCondition(worldStateData.ConditionUID);
			if (selectedCondition == null) {
				worldStateData.ConditionUID = 0;
				conditionSelectMenu.LastEntrySelected = 0;
			} else {
				conditionDescription = selectedCondition.Description;
			}

			EditorGUILayout.BeginHorizontal();
			worldStateData.ConditionUID = conditionSelectMenu.DrawSelectMenu(conditionDescription, Event.current.mousePosition,
																  screenWidth, skin, GUILayout.Width(screenWidth));
			if (selectedCondition == null || worldStateData.ConditionUID != selectedCondition.UID) {
				selectedCondition = conditionLoader.GetCondition(worldStateData.ConditionUID);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel++;
			// value
			object newValue = "";
			newValue = DrawValue.DrawValueField(worldStateData.Value, worldStateData.ConditionUID, new int[1] { 0 },
									 new GUIContent("Value Type"), skin, GUILayout.Width(screenWidth));
			worldStateData.Value = newValue.ToString();
			worldStateData.Expiration = EditorGUILayout.FloatField(new GUIContent("Expiration",
																				  "The amount of time in seconds before" +
																				  " this value expries" +
																				  " and resets to its default value."),
																   worldStateData.Expiration,
																   skin.textArea, GUILayout.Width(screenWidth * 0.5f));

			EditorGUI.indentLevel--;
			GUILayout.EndVertical();

			return worldStateData;
		}

		public static WorldStateData Draw(Rect position, WorldStateData worldStateData,
										  ConditionLoader conditionLoader, TagLoader tagLoader,
										  int dataCount,
												ref List<ConditionSelectMenu> categoryTypeMenus,
												ConditionSelectHandler callback,
												GUISkin skin) {

			if (conditionLoader == null || conditionLoader.ConditionModels.Length == 0 ||
			  tagLoader == null || tagLoader.TagModels.Length == 0) {
				conditionLoader = new ConditionLoader();
				conditionLoader.Load();
				tagLoader = new TagLoader();
				tagLoader.Load();
			}

			ConditionSelectMenu objectTypeMenu = null;
			if (dataCount >= categoryTypeMenus.Count) {
				objectTypeMenu = new ConditionSelectMenu();
				for (int t = 0; t < tagLoader.TagNames.Length; t++) {
					List<ConditionModel> categoryEntries = new List<ConditionModel>();
					for (int f = 0; f < conditionLoader.ConditionModels.Length; f++) {
						for (int fTag = 0; fTag < conditionLoader.ConditionModels[f].Tags.Count; fTag++) {
							if (conditionLoader.ConditionModels[f].Tags[fTag] == t) {
								categoryEntries.Add(conditionLoader.ConditionModels[f]);
							}
						}
					}
					objectTypeMenu.AddCategory(tagLoader.TagNames[t], categoryEntries.ToArray());
				}
				categoryTypeMenus.Add(objectTypeMenu);
				objectTypeMenu.EntrySelected += callback;
			} else {
				objectTypeMenu = categoryTypeMenus[dataCount];
			}

			GUILayout.BeginHorizontal(skin.box);
			GUILayout.BeginVertical();

			// Fact

			EditorGUILayout.BeginHorizontal();

			GUIContent objectTooltip = new GUIContent("Condition Type", conditionLoader.ConditionModels[worldStateData.ConditionUID].Name + ":\n" +
														conditionLoader.ConditionModels[worldStateData.ConditionUID].Description);
			EditorGUILayout.PrefixLabel(objectTooltip);
			// Select object type from a category selector
			if (GUILayout.Button(conditionLoader.ConditionModels[worldStateData.ConditionUID].Name)) {
				float posX = Event.current.mousePosition.x - position.width * 0.2f;
				float posY = Event.current.mousePosition.y;
				objectTypeMenu.DrawSelectMenu("Select a condition",
					new Vector2(posX, posY),
					position.width * 0.4f,
					skin);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUI.indentLevel++;
			worldStateData.Value = DrawValue.DrawValueField(worldStateData.Value,
									 conditionLoader.ConditionModels[worldStateData.ConditionUID].ValueUID,
									new int[1] { 4000 + worldStateData.ConditionUID }).ToString();

			// expiration
			worldStateData.Expiration = EditorGUILayout.FloatField(new GUIContent("Expiration",
																				  "The amount of time in seconds before" +
																				  " this value expries" +
																				  " and resets to its default value."),
																   worldStateData.Expiration,
																   skin.textArea, GUILayout.Width(position.width * 0.5f));

			EditorGUI.indentLevel--;
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();

			return worldStateData;
		}
	}
}
