using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PickleTools.Criterion.ConditionLookup;
using PickleTools.Extensions.ArrayExtensions;
using PickleTools.FileAccess;
using System.Text.RegularExpressions;

namespace PickleTools.Criterion {
	public class TriggerEditor : EditorWindow {

		int selectedTrigger = -1;
		List<ConditionSelectMenu> conditionSelectMenus = new List<ConditionSelectMenu>();

		Vector2 scrollPosition = Vector2.zero;
		string newTriggerName = "";

		GUISkin skin = null;

		bool[] disabledConditionArray = new bool[0];

		TriggerModel[] triggerModels = new TriggerModel[0];
		TriggerModel editingTrigger;

		bool initialized = false;

		Texture imageBackground;
		bool changesMade = false;

		string searchField = "";

		KeywordSearchOptions searchOptions;

		bool updateTriggersFromSearch = false;

		TriggerLoader triggerLoader;
		ConditionLoader conditionLoader;
		TagLoader tagLoader;

		EditorFileSaver fileSaver;

		Regex regexMatchSpaces;
		Regex regularCharacters;
		string nameError = "";

		public static readonly string IMAGE_PATH = "Criterion/Editor/Images/";
		public static readonly string PREFS_TRIGGER_TYPE_SELECTED = "TriggerEditor.Type";
		public static readonly string PREFS_CURRENT_TRIGGER = "TriggerEditor.CurrentTrigger";
		public static readonly string PREFS_REFRESH_TRIGGER = "TriggerEditor.Refresh";

		[MenuItem("Mazer Maker/Windows/Trigger Editor")]
		public static TriggerEditor ShowEditor() {
			TriggerEditor window = GetWindow<TriggerEditor>();
			window.titleContent = new GUIContent("Trigger Editor");
			window.Show();
			return window;
		}

		public void Initialize() {
			initialized = true;

			fileSaver = new EditorFileSaver("");

			skin = AssetDatabase.LoadAssetAtPath<GUISkin>(IMAGE_PATH + "gui_skin.guiskin");

			imageBackground = AssetDatabase.LoadAssetAtPath<Texture>(IMAGE_PATH + "/bg_02.png");

			searchOptions = new KeywordSearchOptions();
			searchOptions.ActionName = false;
			searchOptions.ActionValue = false;
			searchOptions.ContainsAllKeywords = false;
			searchOptions.ConditionName = false;
			searchOptions.ConditionValue = false;
			searchOptions.IgnoreCapitalization = true;
			searchOptions.RuleName = true;

			// load data
			Refresh();

			// load disabled conditions based on which kind of trigger was selected
			int[] disabledConditions = new int[0];
			int triggerType = -1;
			try {
				triggerType = EditorPrefs.GetInt(PREFS_TRIGGER_TYPE_SELECTED, -1);
			} catch {

			}
			disabledConditions = new int[1] { triggerType };

			disabledConditionArray = new bool[conditionLoader.HighestUID];
			for (int c = 0; c < disabledConditions.Length; c++) {
				if (disabledConditions[c] >= 0 && disabledConditions[c] < disabledConditionArray.Length) {
					disabledConditionArray[disabledConditions[c]] = true;
				}
			}

			try {
				selectedTrigger = EditorPrefs.GetInt(PREFS_CURRENT_TRIGGER, -1);
			} catch {
				selectedTrigger = -1;
			}

			try {
				newTriggerName = triggerLoader.GetTrigger(selectedTrigger).Name;
			} catch {
				newTriggerName = "NONE";
			}

			regexMatchSpaces = new Regex(@"\s+");
			regularCharacters = new Regex(@"[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_]");

			EditorApplication.playmodeStateChanged -= PlayStateChanged;
			EditorApplication.playmodeStateChanged += PlayStateChanged;
		}

		void PlayStateChanged() {
			if (!Application.isPlaying && changesMade) {
				Save();
			}
		}

		void Refresh() {
			conditionLoader = new ConditionLoader();
			conditionLoader.Load();

			tagLoader = new TagLoader();
			tagLoader.Load();

			for (int c = 0; c < conditionSelectMenus.Count; c++) {
				conditionSelectMenus[c].Refresh(conditionLoader, tagLoader);
			}

			//string json = EditorPrefs.GetString(MazerMakerUtilities.PREFS_TRIGGER_TRIGGER_LIST, "");
			//triggerModels = JsonMapper.ToObject<TriggerModel[]>(json);

			triggerLoader = new TriggerLoader();
			triggerLoader.Load();
			triggerModels = new TriggerModel[triggerLoader.HighestUID];
			for (int t = 0; t < triggerLoader.TriggerModels.Length; t++) {
				triggerModels[t] = triggerLoader.TriggerModels[t];
			}
			if (triggerModels.Length < triggerLoader.HighestUID) {
				System.Array.Resize<TriggerModel>(ref triggerModels, triggerLoader.HighestUID);
			}
		}

		void OnFocus() {
			Refresh();
		}

		void OnLostFocus() {
			if (changesMade) {
				Save();
			}
		}

		void OnDestroy() {
			if (selectedTrigger != -1) {
				selectedTrigger = -1;
				newTriggerName = selectedTrigger.ToString();
				EditorPrefs.SetInt(PREFS_CURRENT_TRIGGER, selectedTrigger);
				SequenceEditor.UpdateMap();
			}
			initialized = false;
		}




		public void OnGUI() {
			if (Event.current.type != EventType.layout &&
				EditorPrefs.GetBool(PREFS_REFRESH_TRIGGER, false)) {
				Initialize();
				EditorPrefs.SetBool(PREFS_REFRESH_TRIGGER, false);
			}

			if (!initialized || triggerLoader == null || skin == null ||
			   regexMatchSpaces == null || regularCharacters == null) {
				Initialize();
			}


			if (updateTriggersFromSearch && Event.current.type != EventType.layout) {
				UpdateTriggerListFromSearch();
				return;
			}

			if (triggerModels != null && triggerModels.Length > 0 && initialized) {
				DrawTriggerEditWindow(new Rect(0, 0, Screen.width, Screen.height));
				Repaint();
			} else if (!initialized) {
				Initialize();
			} else {
				updateTriggersFromSearch = true;
				DrawEmptyWindow(new Rect(0, 0, Screen.width, Screen.height));
			}
		}

		private void DrawEmptyWindow(Rect guiArea) {
			GUI.DrawTextureWithTexCoords(guiArea, imageBackground,
										 new Rect(0, 0, guiArea.width, guiArea.height));

			DrawSearchBox();
			if (skin == null) {
				skin = AssetDatabase.LoadAssetAtPath<GUISkin>(IMAGE_PATH + "gui_skin.guiskin");
			}
			if (skin != null) {
				GUILayout.Label("Please select a trigger in the scene view to edit its details.", skin.FindStyle("title"));
			}
		}

		void DrawSearchBox() {
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PrefixLabel(new GUIContent("Search",
													   "Type in any phrase to only show triggers that include that phrase in" +
													   " their name. If you use a comma to separate phrases, triggers that" +
													   " include either phrase will be displayed."));
			searchField = GUILayout.TextArea(searchField, skin.textArea);
			if (EditorGUI.EndChangeCheck()) {
				updateTriggersFromSearch = true;
			}
		}

		void UpdateTriggerListFromSearch() {
			List<TriggerModel> tempList = new List<TriggerModel>();
			if (searchField == "") {
				tempList.AddRange(triggerLoader.TriggerModels);
			} else {
				List<int> matchedUIDs = new List<int>();
				matchedUIDs = KeywordSearchWindow.GetTriggersByName(triggerLoader.TriggerModels, searchField.Split(','), searchOptions);
				for (int t = 0; t < triggerLoader.TriggerModels.Length; t++) {
					if (triggerLoader.TriggerModels[t] == null) {
						continue;
					}
					if (matchedUIDs.Contains(triggerLoader.TriggerModels[t].UID)) {
						tempList.Add(triggerLoader.TriggerModels[t]);
					}
				}
			}

			triggerModels = new TriggerModel[triggerLoader.HighestUID];
			for (int t = 0; t < tempList.Count; t++) {
				if (tempList[t] == null) {
					continue;
				}
				triggerModels[tempList[t].UID] = tempList[t];
			}
			updateTriggersFromSearch = false;
		}

		string lastTooltip = "";
		public void DrawTriggerEditWindow(Rect guiArea) {

			if (skin == null) {
				skin = AssetDatabase.LoadAssetAtPath<GUISkin>(IMAGE_PATH + "gui_skin.guiskin");
			}
			GUILayout.BeginArea(guiArea);

			GUI.DrawTextureWithTexCoords(guiArea, imageBackground,
										 new Rect(0, 0, guiArea.width, guiArea.height));

			GUILayout.BeginVertical();

			//if(changesMade) {
			//	if(GUILayout.Button(new GUIContent(iconSaveWarning, "Click this to permanently save the changes to this trigger."),
			//						skin.button, GUILayout.Width(80))) {
			//		Save();
			//	}
			//} else {
			//	if(GUILayout.Button(new GUIContent(iconSave, "Click this to permanently save the changes to this trigger."),
			//						skin.button, GUILayout.Width(80))) {
			//		Save();
			//	}
			//}

			GUILayout.Box("", skin.FindStyle("box_02"), GUILayout.Height(1));

			GUILayout.Label("Info:", skin.label);
			GUILayout.Box(new GUIContent(lastTooltip,
										 "This is the info box. It displays useful information about anything you" +
										 " move your cursor over."),
						  skin.box, GUILayout.Height(80), GUILayout.MinWidth(guiArea.width - 8));

			// search box
			DrawSearchBox();
			GUILayout.Space(8);
			// trigger list title
			string triggerListName = "Triggers";
			if (searchField != "") {
				triggerListName = "Triggers for search \"" + searchField + "\"";
			}
			GUILayout.Label(triggerListName, skin.label);
			// trigger list
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, true);
			int deleteIndex = -1;
			for (int t = 0; t < triggerModels.Length; t++) {
				if (triggerModels[t] == null || triggerModels[t].UID == -1) {
					continue;
				}
				GUIStyle buttonStyle = skin.button;
				int triggerUID = -1;
				try {
					triggerUID = triggerModels[t].UID;
				} catch {
					continue;
				}
				if (selectedTrigger == triggerUID) {
					buttonStyle = skin.FindStyle("button_01_selected");
					if (buttonStyle == null) {
						Debug.LogWarning("Could not find selected button style");
						buttonStyle = skin.button;
					}
				}

				GUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent((triggerModels[t].UID) + ".",
											  "The UID of this trigger."),
								skin.FindStyle("big_numbers"), GUILayout.Width(60));
				if (GUILayout.Button(new GUIContent(triggerModels[t].Name,
												  "Select this trigger to edit its conditions."),
									buttonStyle, GUILayout.Height(28))) {
					if (selectedTrigger == triggerUID) {
						selectedTrigger = -1;
						newTriggerName = "NULL_TRIGGER";
					} else {
						selectedTrigger = triggerUID;
						newTriggerName = triggerModels[selectedTrigger].Name;
					}

					EditorPrefs.SetInt(PREFS_CURRENT_TRIGGER, selectedTrigger);
					conditionSelectMenus.Clear();
					SequenceEditor.UpdateMap();
					GUILayout.EndHorizontal();
					GUI.FocusControl("");
					break;
				}
				if (selectedTrigger == triggerUID) {
					if (GUILayout.Button(new GUIContent("X", "Delete this trigger."), skin.button,
						GUILayout.Width(28), GUILayout.Height(28))) {
						deleteIndex = selectedTrigger;
					}
				}
				GUILayout.EndHorizontal();

				if (selectedTrigger == triggerUID) {

					// trigger menu count is tracked seperately because we can skip conditions we don't want to 
					// allow trigger renaming
					GUILayout.BeginHorizontal();
					GUILayout.Space(50);
					GUILayout.Label(new GUIContent("Change Name",
												   "The name of the trigger and its associated sequence."), skin.label,
													GUILayout.Width(100));
					EditorGUI.BeginChangeCheck();
					newTriggerName = GUILayout.TextField(newTriggerName, skin.textField);

					if (EditorGUI.EndChangeCheck()) {

						newTriggerName = regexMatchSpaces.Replace(newTriggerName, "_");
						nameError = "";
						string inital = newTriggerName;
						newTriggerName = newTriggerName.ToLower();
						if (newTriggerName != inital) {
							nameError += "\nYou cannot have capitol letters in condition names.";
							inital = newTriggerName;
						}
						newTriggerName = RemoveSpecialCharacters(newTriggerName);
						if (newTriggerName != inital) {
							nameError += "\nYou cannot have special characters in condition names.";
							inital = newTriggerName;
						}
						triggerModels[selectedTrigger].Name = newTriggerName;
						changesMade = true;
						//EditorPrefs.SetString(MazerMakerUtilities.PREFS_TRIGGER_TRIGGER_LIST, JsonMapper.ToJson(triggerModels));
					}
					GUILayout.EndHorizontal();

					int deletedCriterion = -1;
					EditorGUI.BeginChangeCheck();
					List<TriggerConditionModel> conditions = new List<TriggerConditionModel>();

					// create condition select menus for all of the conditions listed for this trigger
					int conditionMenuCount = 0;
					for (int c = 0; c < triggerModels[t].Conditions.Length; c++) {
						//					Debug.LogWarning("Getting condition " + triggerModels[t].Conditions[c].UID);
						conditions.Add(triggerModels[t].Conditions[c]);
						if (conditionMenuCount >= conditionSelectMenus.Count) {
							conditionSelectMenus.Add(new ConditionSelectMenu(conditionLoader, tagLoader));
							conditionSelectMenus[conditionMenuCount].LastEntrySelected = triggerModels[t].Conditions[c].UID;
						}
						conditionMenuCount++;
					}
					// for all the conditions we can see
					for (int c = 0; c < conditions.Count; c++) {

						TriggerConditionModel condition = conditions[c];
						int index = 0;
						for (int i = 0; i < triggerModels[t].Conditions.Length; i++) {
							if (triggerModels[t].Conditions[i] == condition) {
								index = i;
								break;
							}
						}

						// store values to manually check for a UID change
						bool changed = false;
						int uidValue = -1;
						if (condition.UID == (int)ConditionType.SHMUP_OBJECT_UID) {
							int.TryParse(condition.LowerBound.ToString(), out uidValue);
						}
						EditorGUI.BeginChangeCheck();
						// leaving comment so we can place in searchable list
						if (DrawCondition.DrawCompressed(ref condition, conditionSelectMenus[c], conditionLoader,
																   guiArea.width - 24, 2000 + c, 2500 + c, null, skin)) {
							deletedCriterion = index;
						}
						// check to see if we changed the uid, since ChangeCheck only checks if we've hit a button
						if (condition.UID == (int)ConditionType.SHMUP_OBJECT_UID) {
							int newUIDValue = uidValue;
							int.TryParse(condition.LowerBound.ToString(), out newUIDValue);
							if (newUIDValue != uidValue) {
								changed = true;
							}
						}
						if (EditorGUI.EndChangeCheck() || changed) {
							triggerModels = EditTriggers(triggerModels, triggerModels[t], index, condition);
						}
						GUILayout.Box("", skin.FindStyle("box_02"), GUILayout.Height(1));

					}
					if (deletedCriterion > -1) {
						triggerModels = EditTriggers(triggerModels, triggerModels[t], deletedCriterion, null);

					}
					GUILayout.BeginHorizontal();
					if (GUILayout.Button(new GUIContent("+ Condition", "Add another condition to this rule so that it only happens" +
						" under more specific situations."), skin.button, GUILayout.Height(70))) {
						triggerToAddConditionTo = t;
					}
					if (EditorGUI.EndChangeCheck()) {
						changesMade = true;
					}
					GUILayout.EndHorizontal();
					GUILayout.Box("", skin.FindStyle("box_02"), GUILayout.Height(1));
					GUILayout.Space(1);
					GUILayout.Box("", skin.FindStyle("box_02"), GUILayout.Height(1));
				}
			}

			GUILayout.Space(4);
			if (GUILayout.Button(new GUIContent("+ NEW TRIGGER", "Click this to add a new trigger to the bottom of the list."),
				skin.button, GUILayout.Height(48))) {
				TriggerModel newTrigger = triggerLoader.AddTrigger("NewTrigger" + triggerLoader.HighestUID, new TriggerConditionModel[0]);
				triggerLoader.Save(fileSaver);

				System.Array.Resize<TriggerModel>(ref triggerModels, triggerLoader.HighestUID);
				triggerModels[newTrigger.UID] = newTrigger;

				// update the sequence database to make sure that we have a matching sequence to our new trigger
				SequenceLoader sequenceLoader = new SequenceLoader();
				sequenceLoader.Load();
				if (sequenceLoader.GetSequence(newTrigger.UID) == null) {
					sequenceLoader.AddSequence(newTrigger.Name, newTrigger.UID);
					sequenceLoader.Save(fileSaver);
				}

				//EditorPrefs.SetString(MazerMakerUtilities.PREFS_TRIGGER_TRIGGER_LIST, JsonMapper.ToJson(triggerLoader.TriggerModels));

				selectedTrigger = newTrigger.UID;
				EditorPrefs.SetInt(PREFS_CURRENT_TRIGGER, selectedTrigger);
				newTriggerName = triggerModels[selectedTrigger].Name;
				// set the control we should focus on to the trigger name field
				GUI.FocusControl("");
				changesMade = true;
			}
			GUILayout.Space(2);

			GUILayout.Space(30);
			// add extra space so we can scroll past the bottom. It's nicer for editing triggers
			GUILayout.Space(40);
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			GUILayout.EndArea();

			if (deleteIndex > -1) {
				if (EditorUtility.DisplayDialog("Confirm Delete",
					"Do you want to delete trigger " + deleteIndex + ": " + triggerModels[deleteIndex].Name + "?\n" +
					"This will also delete the corresponding action map!", "YEAH!", "NO NO NO!")) {
					triggerModels[deleteIndex] = null;
					triggerLoader.Remove(deleteIndex);
					triggerLoader.Save(fileSaver);

					SequenceLoader sequenceLoader = new SequenceLoader();
					sequenceLoader.Load();
					sequenceLoader.Remove(deleteIndex);
					sequenceLoader.Save(fileSaver);

					selectedTrigger = -1;
					EditorPrefs.SetInt(PREFS_CURRENT_TRIGGER, selectedTrigger);

					changesMade = true;
				}
			}

			if (Event.current.type == EventType.Repaint && nameError != "") {
				lastTooltip = nameError;
			} else if ((Event.current.type == EventType.Repaint && GUI.tooltip != lastTooltip)) {
				lastTooltip = GUI.tooltip;
				if (lastTooltip == "") {
					lastTooltip = "Move the cursor over something to get information about it in this box.";
				}
			}

			// we need to add conditions not on layout events, so we don't generate error with the layout group repaint
			if (triggerToAddConditionTo > -1 && triggerToAddConditionTo < triggerModels.Length && Event.current.type != EventType.layout) {

				TriggerConditionModel newCondition = new TriggerConditionModel();
				newCondition.UID = (int)ConditionType.NONE;
				newCondition.LowerBound = "";
				newCondition.UpperBound = "";

				triggerModels = EditTriggers(triggerModels, triggerModels[triggerToAddConditionTo],
					triggerModels[triggerToAddConditionTo].Conditions.Length, newCondition);

				// clear the condition select menus so we don't use old data and set the new condition to a uid other than NONE
				for (int c = 0; c < conditionSelectMenus.Count; c++) {
					if (triggerModels[triggerToAddConditionTo].Conditions.Length > c) {
						conditionSelectMenus[c].LastEntrySelected = triggerModels[triggerToAddConditionTo].Conditions[c].UID;
					} else {
						conditionSelectMenus[c].LastEntrySelected = 0;
					}
				}

				triggerToAddConditionTo = -1;

				//EditorPrefs.SetString(MazerMakerUtilities.PREFS_TRIGGER_TRIGGER_LIST, JsonMapper.ToJson(triggerModels));

				changesMade = true;
			}
		}

		int triggerToAddConditionTo = -1;

		TriggerModel[] EditTriggers(TriggerModel[] triggers, TriggerModel triggerToEdit, int index, TriggerConditionModel newCondition) {
			for (int r = 0; r < triggers.Length; r++) {
				if (triggers[r] == null) {
					continue;
				}
				if (triggers[r].UID == triggerToEdit.UID) {
					if (index >= triggers[r].Conditions.Length) {
						System.Array.Resize<TriggerConditionModel>(ref triggers[r].Conditions, triggers[r].Conditions.Length + 1);
						triggers[r].Conditions[triggers[r].Conditions.Length - 1] = newCondition;
					} else {
						if (newCondition == null) {
							// delete the condition
							triggers[r].Conditions = triggers[r].Conditions.RemoveAt(index);
							int conditionSelectIndex = 0;
							for (int c = 0; c < triggers[r].Conditions.Length; c++) {
								int conditionArraySlot = triggers[r].Conditions[c].UID;
								if (disabledConditionArray.Length > conditionArraySlot && conditionArraySlot >= 0 && disabledConditionArray[conditionArraySlot]) {
									continue;
								} else {
									conditionSelectMenus[conditionSelectIndex].LastEntrySelected = triggers[r].Conditions[c].UID;
									conditionSelectIndex++;
								}
							}
						} else {
							triggers[r].Conditions[index] = newCondition;
						}
					}
				}
			}
			//EditorPrefs.SetString(MazerMakerUtilities.PREFS_TRIGGER_TRIGGER_LIST, JsonMapper.ToJson(triggers));
			changesMade = true;
			return triggers;
		}

		private void Save() {
			fileSaver = new EditorFileSaver("");

			for (int t = 0; t < triggerModels.Length; t++) {
				if (triggerModels[t] == null || triggerModels[t].UID < 0) {
					continue;
				}
				triggerLoader.TriggerModels[triggerModels[t].UID] = triggerModels[t];
			}
			triggerLoader.Save(fileSaver);

			SequenceLoader sequenceLoader = new SequenceLoader();
			sequenceLoader.Load();
			for (int t = 0; t < triggerModels.Length; t++) {
				if (triggerModels[t] == null || triggerModels[t].UID < 0) {
					continue;
				}
				sequenceLoader.SequenceModels[triggerModels[t].UID].Name = triggerModels[t].Name;
			}
			sequenceLoader.Save(fileSaver);

			EditorPrefs.SetInt(PREFS_CURRENT_TRIGGER, selectedTrigger);
			//EditorPrefs.SetString(MazerMakerUtilities.PREFS_TRIGGER_TRIGGER_LIST, JsonMapper.ToJson(triggerLoader.TriggerModels));
			EditorPrefs.SetBool(SequenceEditor.PREFS_UPDATE_ACTION_MAP_WINDOW, true);

			changesMade = false;
		}

		string RemoveSpecialCharacters(string s) {
			return regularCharacters.Replace(s, delegate (Match match) {
				return "";
			});
		}
	}
}