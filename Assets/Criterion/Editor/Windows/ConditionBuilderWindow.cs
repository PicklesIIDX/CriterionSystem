using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using LitJson;
using PickleTools.FileAccess;
using System.Text.RegularExpressions;


namespace PickleTools.Criterion {
	public class ConditionBuilderWindow : EditorWindow {

		ConditionModel[] conditions = new ConditionModel[0];
		ConditionModel selectedCondition = null;

		bool conditionEdited = false;

		ConditionLoader conditionLoader;
		TagLoader tagLoader;

		ConditionModel newCondition = new ConditionModel();

		string errorMessage = "";

		string newTagType = "";
		Vector2 tagListScrollPosition = Vector2.zero;

		GUISkin skin;
		Texture imageBackground;
		Texture iconSave;
		Texture iconSaveWarning;
		ConditionSelectMenu conditionSelectMenu;
		private static readonly string CURRENT_SELECTION_STRING = "ConditionBuilder.CurrentSelection";

		public static readonly string PREFS_REFRESH_CONDITION_BUILDER = "ConditionBuiler.Refresh";

		Regex regexMatchSpaces;
		Regex regularCharacters;
		string nameError = "";
		string tagError = "";

		string[] valueTypeNames = new string[0];

		[MenuItem("Mazer Maker/Windows/Condition Builder")]
		public static void ShowEditor() {
			ConditionBuilderWindow window = CreateInstance<ConditionBuilderWindow>();
			window.titleContent = new GUIContent("Condition Builder");
			window.Initialize();
			window.Show();
		}

		void Initialize() {
			valueTypeNames = System.Enum.GetNames(typeof(ValueTypeLoader.ValueType));

			skin = AssetDatabase.LoadAssetAtPath<GUISkin>("PickleTools/Criterion/Images/gui_skin.guiskin");
			imageBackground = AssetDatabase.LoadAssetAtPath<Texture>("PickleTools/Criterion/Images/background.png");
			iconSave = AssetDatabase.LoadAssetAtPath<Texture>("PickleTools/Criterion/Images/icon_save.png");
			iconSaveWarning = AssetDatabase.LoadAssetAtPath<Texture>("PickleTools/Criterion/Images/icon_save_warning.png");

			conditionLoader = new ConditionLoader();
			conditionLoader.Load();

			conditions = new ConditionModel[conditionLoader.ConditionModels.Length];
			System.Array.Copy(conditionLoader.ConditionModels, conditions, conditionLoader.ConditionModels.Length);

			//string currentSelectedJSON = EditorPrefs.GetString(CURRENT_SELECTION_STRING, "");
			//if(currentSelectedJSON != "") {
			//selectedCondition = JsonMapper.ToObject<ConditionModel>(currentSelectedJSON);
			//if(conditionLoader.GetCondition(selectedCondition.UID) == null) {
			//	selectedCondition = null;
			//}

			//}
			if (selectedCondition == null && conditions.Length > 0) {
				selectedCondition = conditions[0];
			}


			// TODO: Check for duplicates among DBs

			tagLoader = new TagLoader();
			tagLoader.Load();

			conditionSelectMenu = new ConditionSelectMenu();

			string[] tagTypes = new string[tagLoader.TagModels.Length];
			for (int t = 0; t < tagTypes.Length; t++) {
				tagTypes[t] = tagLoader.TagModels[t].Name;
			}

			for (int t = 0; t < tagTypes.Length; t++) {
				List<ConditionModel> categoryEntries = new List<ConditionModel>();
				for (int c = 0; c < conditions.Length; c++) {
					if (conditions[c] == null) {
						continue;
					}
					for (int fTag = 0; fTag < conditions[c].Tags.Count; fTag++) {
						if (conditions[c].Tags[fTag] == t) {
							categoryEntries.Add(conditions[c]);
						}
					}
				}
				conditionSelectMenu.AddCategory(tagTypes[t], categoryEntries.ToArray());
			}
			conditionSelectMenu.LastEntrySelected = selectedCondition.UID;
			conditionSelectMenu.EntrySelected -= HandleEntrySelected;
			conditionSelectMenu.EntrySelected += HandleEntrySelected;

			regexMatchSpaces = new Regex(@"\s+");
			regularCharacters = new Regex(@"[^abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_]");
		}

		string RemoveSpecialCharacters(string s) {
			return regularCharacters.Replace(s, delegate (Match match) {
				return "";
			});
		}

		void OnDestroy() {
			if (conditionSelectMenu != null) {
				conditionSelectMenu.EntrySelected -= HandleEntrySelected;
			}
		}


		void OnGUI() {
			if (Event.current.type != EventType.layout &&
				EditorPrefs.GetBool(PREFS_REFRESH_CONDITION_BUILDER, false)) {
				Initialize();
				EditorPrefs.SetBool(PREFS_REFRESH_CONDITION_BUILDER, false);
			}
			if (skin == null || tagLoader == null) {
				Initialize();
			}

			Rect guiArea = new Rect(0, 0, Screen.width, Screen.height);
			GUI.DrawTextureWithTexCoords(guiArea, imageBackground,
				new Rect(0, 0, guiArea.width, guiArea.height));

			if (errorMessage != "") {
				EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
			}
			// button for new fact
			GUILayout.Space(4);
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Create New Condition", skin.label);
			EditorGUI.BeginChangeCheck();
			newCondition.Name = GUILayout.TextField(newCondition.Name, skin.textField);
			if (EditorGUI.EndChangeCheck()) {

				newCondition.Name = regexMatchSpaces.Replace(newCondition.Name, "_");
				nameError = "";
				string inital = newCondition.Name;
				newCondition.Name = newCondition.Name.ToLower();
				if (newCondition.Name != inital) {
					nameError += "\nYou cannot have capitol letters in condition names.";
					inital = newCondition.Name;
				}
				newCondition.Name = RemoveSpecialCharacters(newCondition.Name);
				if (newCondition.Name != inital) {
					nameError += "\nYou cannot have special characters in condition names.";
					inital = newCondition.Name;
				}
			}
			if (GUILayout.Button("+", skin.button, GUILayout.Width(position.width * 0.1f)) && newCondition.Name != "") {
				// clicking will create an empty entry for fact and select it
				conditionLoader.AddCondition(
					newCondition.Name,
					"",
					"This is a new condition. You should update its properties :D!",
					new List<int> { 0 },
					(int)ValueTypeLoader.ValueType.TEXT);
				EditorFileSaver fileSaver = new EditorFileSaver("");
				conditionLoader.Save(fileSaver);
				Initialize();
				conditionEdited = false;
				newCondition = conditionLoader.GetCondition(conditionLoader.HighestUID - 1);
				// set our selection to the new fact
				selectedCondition = newCondition;
				// update the current fields with our new values
				selectedCondition.UID = newCondition.UID;
				selectedCondition.Name = newCondition.Name;
				selectedCondition.DefaultValue = newCondition.DefaultValue;
				selectedCondition.Initialize = newCondition.Initialize;
				selectedCondition.ValueUID = newCondition.ValueUID;
				selectedCondition.Description = newCondition.Description;
				selectedCondition.Tags = newCondition.Tags;
				conditionSelectMenu.LastEntrySelected = selectedCondition.UID;
				newCondition = new ConditionModel();
				newCondition.Name = "";
			}
			GUILayout.EndHorizontal();
			if (nameError != "") {
				EditorGUILayout.HelpBox(nameError, MessageType.Error);
			}
			GUILayout.Box("", skin.FindStyle("box_02"), GUILayout.Height(1));
			// show dropdown list of facts
			EditorGUILayout.BeginHorizontal();
			float posX = Event.current.mousePosition.x - position.width * 0.2f;
			float posY = Event.current.mousePosition.y;
			EditorGUI.BeginChangeCheck();
			conditionSelectMenu.DrawSelectMenu("Select the condition to edit.",
				new Vector2(posX, posY),
				position.width * 0.9f,
				skin);
			if (EditorGUI.EndChangeCheck()) {
				GUI.FocusControl("");
			}

			if (GUILayout.Button("X", skin.button, GUILayout.Width(position.width * 0.1f))) {
				if (selectedCondition.UID > 0) {
					conditionLoader.Remove(selectedCondition.UID);
					EditorFileSaver fileSaver = new EditorFileSaver("");
					conditionLoader.Save(fileSaver);
					conditionEdited = false;
					selectedCondition = null;
					Initialize();
				} else {
					errorMessage = "You cannot delete the NONE condition!";
				}
			}
			EditorGUILayout.EndHorizontal();
			// selected fact
			if (selectedCondition != null) {
				// can change properties which are locally saved
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel("Condition Name", skin.label);
				selectedCondition.Name = GUILayout.TextArea(selectedCondition.Name, skin.textField);
				EditorGUILayout.EndHorizontal();

				int valueType = selectedCondition.ValueUID;
				EditorGUI.BeginChangeCheck();
				valueType = EditorGUILayout.Popup("Value Type", valueType, valueTypeNames, skin.button);
				selectedCondition.ValueUID = valueType;

				if (EditorGUI.EndChangeCheck()) {
					ValueTypeLoader.GetDefaultValue(ref selectedCondition.DefaultValue, valueType);
				}
				selectedCondition.DefaultValue = DrawValue.DrawValueField(selectedCondition.DefaultValue,
				                                                          valueType, new int[1]{ 5000 + selectedCondition.UID},
				                                                          new GUIContent("Default Value",
											  "Select the default value for this condition when it's created."), skin);
				GUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(new GUIContent("Initialize If Null", "If checked, this will be" +
														   " added to the player's world state if it doesn't" +
														   " have a value when the game starts. A condition" +
														   " must be initialized in order for it to be checked" +
														   " for triggers."), skin.label);
				selectedCondition.Initialize = GUILayout.Toggle(selectedCondition.Initialize, "", skin.toggle);
				GUILayout.EndHorizontal();

				selectedCondition.Description = GUILayout.TextField(selectedCondition.Description, skin.textArea,
																	GUILayout.Height(80),
																	GUILayout.Width(position.width - 7));

				GUILayout.Box("", skin.FindStyle("box_02"), GUILayout.Height(1));

				GUILayout.Label("Tags", skin.label);
				GUILayout.Box("", skin.FindStyle("box_02"), GUILayout.Height(1));
				int deleteParameter = -1;
				tagListScrollPosition = GUILayout.BeginScrollView(tagListScrollPosition);
				if (selectedCondition.Tags == null) {
					selectedCondition.Tags = new List<int>();
				}
				for (int t = 0; t < selectedCondition.Tags.Count; t++) {
					int tagID = selectedCondition.Tags[t];
					if (DrawTag(ref tagID)) {
						deleteParameter = t;
					}
					selectedCondition.Tags[t] = tagID;
				}
				GUILayout.Space(4);
				if (GUILayout.Button("Add Tag", skin.button, GUILayout.Height(36))) {
					selectedCondition.Tags.Add(0);
					tagListScrollPosition.y = 999999999;
				}
				GUILayout.EndScrollView();
				// remove the indicated tag
				if (deleteParameter > -1) {
					selectedCondition.Tags.RemoveAt(deleteParameter);
				}

				if (EditorGUI.EndChangeCheck()) {
					conditionEdited = true;
				}
				if (conditionEdited) {
					EditorGUILayout.HelpBox("You have made changes! Don't forget to save them!",
						MessageType.Warning);
				}

				GUILayout.Box("", skin.FindStyle("box_02"), GUILayout.Height(1));
				// new tag
				GUILayout.Space(4);
				GUILayout.BeginHorizontal();

				EditorGUILayout.PrefixLabel("Create New Tag", skin.label);
				EditorGUI.BeginChangeCheck();
				newTagType = GUILayout.TextField(newTagType, skin.textField);
				if (EditorGUI.EndChangeCheck()) {
					newTagType = newTagType.ToUpper();
					newTagType = regexMatchSpaces.Replace(newTagType, "_");
					tagError = "";
					string inital = newTagType;
					newTagType = newTagType.ToUpper();
					if (newTagType != inital) {
						tagError += "\nYou cannot have capitol letters in condition names.";
						inital = newTagType;
					}
					newTagType = RemoveSpecialCharacters(newTagType);
					if (newTagType != inital) {
						tagError += "\nYou cannot have special characters in condition names.";
						inital = newTagType;
					}
				}
				if (GUILayout.Button("+", skin.button, GUILayout.Width(position.width * 0.1f)) && newTagType != "") {
					tagLoader.AddTag(newTagType);
					EditorFileSaver fileSaver = new EditorFileSaver("");
					tagLoader.Save(fileSaver);
					Initialize();
					newTagType = "";
				}
				GUILayout.EndHorizontal();
				if (tagError != "") {
					EditorGUILayout.HelpBox(tagError, MessageType.Error);
				}

				// pressing button will save fact changes, including name
				bool saveData = false;
				if (conditionEdited && GUILayout.Button(iconSaveWarning, skin.button)) {
					saveData = true;
				} else if (!conditionEdited && GUILayout.Button(iconSave, skin.button)) {
					saveData = true;
				}
				if (saveData) {
					conditionLoader.ConditionModels[selectedCondition.UID] = selectedCondition;
					EditorFileSaver fileSaver = new EditorFileSaver("");
					conditionLoader.Save(fileSaver);
					EditorPrefs.SetString(CURRENT_SELECTION_STRING, JsonMapper.ToJson(selectedCondition));
					Initialize();
					conditionEdited = false;
					errorMessage = "";
				}
			}
			Repaint();
		}

		void OnLostFocus() {
			EditorPrefs.SetString(CURRENT_SELECTION_STRING, JsonMapper.ToJson(selectedCondition));
		}

		bool DrawTag(ref int tag) {
			bool deleted = false;
			EditorGUILayout.BeginHorizontal();
			string[] tagNames = tagLoader.TagNames;
			tag = EditorGUILayout.Popup(tag, tagNames, skin.button, GUILayout.Height(24));
			if (GUILayout.Button("X", skin.button, GUILayout.Width(position.width * 0.1f))) {
				deleted = true;
			}
			EditorGUILayout.EndHorizontal();
			return deleted;
		}

		public void SelectCondition(int conditionUID) {
			if (conditionUID >= 0 && conditionUID < conditions.Length) {
				selectedCondition = conditions[conditionUID];
			} else {
				selectedCondition = conditions[0];
			}
		}

		void HandleEntrySelected(ConditionSelectMenu menu, int item) {
			GUI.FocusControl("");
			for (int c = 0; c < conditions.Length; c++) {
				if (conditions[c] == null) {
					continue;
				}
				if (conditions[c].UID == item) {
					selectedCondition = conditions[c];
					return;
				}
			}
		}
	}
}