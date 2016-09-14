using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PickleTools.FileAccess;

namespace PickleTools.Criterion {
	[CustomEditor(typeof(MemoryView))]
	public class MemoryViewInspector : Editor {

		Memory memory;
		MemoryLoader memoryLoader;

		Vector2 fragmentScrollPosition = Vector2.zero;

		GUISkin skin;

		ConditionSelectMenu conditionSelectMenu;

		ConditionLoader conditionLoader;

		const string GUI_SKIN_PATH = "PickleTools/Editor/GUISkin.guiskin";

		public void OnEnable(){

			MemoryView memoryView = target as MemoryView;
			memory = memoryView.Memory;

			conditionLoader = new ConditionLoader();
			conditionLoader.Load();
			IFileSaver fileSaver = new EditorFileSaver("");

			if(memory == null){
				memoryLoader = new MemoryLoader();
				memoryLoader.Load(fileSaver);
				MemoryModel memoryModel = memoryLoader.GetMemory(0);
				memory = new Memory(memoryModel, null, conditionLoader);
			}
			if(skin == null){
				skin = AssetDatabase.LoadAssetAtPath<GUISkin>(GUI_SKIN_PATH);
			}

			if(conditionSelectMenu == null){
				conditionSelectMenu = new ConditionSelectMenu();
				conditionSelectMenu.EntrySelected += ConditionSelectMenu_EntrySelected;

				ConditionModel[] conditions = conditionLoader.ConditionModels;
				TagLoader tagLoader = new TagLoader();
				tagLoader.Load();

				string[] tagTypes = new string[tagLoader.TagModels.Length];
				for(int t = 0; t < tagTypes.Length; t ++){
					tagTypes[t] = tagLoader.TagModels[t].Name;
				}
				for(int t = 0; t < tagTypes.Length; t ++){
					List<ConditionModel> categoryEntries = new List<ConditionModel>();
					for(int c = 0; c < conditions.Length; c ++){
						if(conditions[c] == null){
							continue;
						}
						for(int fTag = 0; fTag < conditions[c].Tags.Count; fTag ++){
							if(conditions[c].Tags[fTag] == t){
								categoryEntries.Add(conditions[c]);
							}
						}
					}
					conditionSelectMenu.AddCategory(tagTypes[t], categoryEntries.ToArray());
				}
				conditionSelectMenu.LastEntrySelected = 0;
			}
		}

		public override void OnInspectorGUI ()
		{
			if(skin == null){
				OnEnable();
				return;
			}

			if(GUILayout.Button("Refresh", skin.button)){
				OnEnable();
				return;
			}
			fragmentScrollPosition = GUILayout.BeginScrollView(fragmentScrollPosition);
			GUILayout.BeginVertical();
			if(memory != null){
				for(int f = 0; f < memory.Fragments.Length; f ++){
					if(memory.Fragments[f] == null || memory.Fragments[f].UID <= 0){
						continue;
					}

					object value = "";

					if(ValueTypeLoader.IsBoolValue(memory.Fragments[f].ValueID)) {
						bool boolValue = false;
						memory.TryGetValue(memory.Fragments[f].UID, out boolValue);
						EditorGUI.BeginChangeCheck();
						value = DrawValue.DrawValueField(boolValue,
                             memory.Fragments[f].ValueID, new int[1]{6000 + f },
							new GUIContent(memory.Fragments[f].Name),
							skin
						);
						if(EditorGUI.EndChangeCheck()) {
							bool.TryParse(value.ToString(), out boolValue);
							memory.EditMemory(memory.Fragments[f].UID, boolValue, 0.0f);
						}
					} else if(ValueTypeLoader.IsFloatValue(memory.Fragments[f].ValueID)) {
						float floatValue = -1.0f;
						memory.TryGetValue(memory.Fragments[f].UID, out floatValue);
						EditorGUI.BeginChangeCheck();
						value = DrawValue.DrawValueField(floatValue,
							 memory.Fragments[f].ValueID, new int[1] { 7000 + f },
							new GUIContent(memory.Fragments[f].Name),
							skin
						);
						if(EditorGUI.EndChangeCheck()) {
							float.TryParse(value.ToString(), out floatValue);
							memory.EditMemory(memory.Fragments[f].UID, floatValue, 0.0f);
						}
					} else {
						memory.TryGetValue(memory.Fragments[f].UID, out value);
						EditorGUI.BeginChangeCheck();
						value = DrawValue.DrawValueField(value,
                             memory.Fragments[f].ValueID, new int[1]{ 8000 + f },
							new GUIContent(memory.Fragments[f].Name),
							skin
						);
						if(EditorGUI.EndChangeCheck()) {
							memory.EditMemory(memory.Fragments[f].UID, value);
						}
					}
				}
				GUILayout.Label("+ Add Memory", skin.label);
				conditionSelectMenu.DrawSelectMenu("", Event.current.mousePosition, Screen.width, skin);
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
		}

		void ConditionSelectMenu_EntrySelected (ConditionSelectMenu menu, int item)
		{
			int uid = item;
			if(uid > 0 && memory.Fragments[uid].UID <= 0){

				if(ValueTypeLoader.IsBoolValue(conditionLoader.GetCondition(uid).ValueUID)) {
					bool boolValue = false;
					memory.EditMemory(uid, boolValue, 0.0f);
				} else if(ValueTypeLoader.IsFloatValue(conditionLoader.GetCondition(uid).ValueUID)) {
					float floatValue = -1.0f;
					memory.EditMemory(uid, floatValue, 0.0f);
				} else {
					memory.EditMemory(uid, "");
				}
			}
			conditionSelectMenu.LastEntrySelected = 0;
		}
	}

}