using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace PickleTools.UnityEditor {

	public class SearchableSelectList : PopupWindowContent {

		//private TriggerLoader triggerLoader;
		private GUISkin skin;
		private Vector2 drawTriggerListScrollPosition = Vector2.zero;
		//private List<TriggerModel> triggerList = new List<TriggerModel>();
		private string searchText = "";
		//private KeywordSearchOptions searchOptions;
		private List<string> searchSelectionList = new List<string>();
		private List<string> fullSelectionList = new List<string>();

		bool updateLayout = false;

		System.Action<int> selectCallback;
		int selectedIndex;

		public SearchableSelectList(List<string> entries, System.Action<int> callback, int index = -1, GUISkin guiSkin = null) {
			fullSelectionList.AddRange(entries);
			skin = guiSkin;
			selectCallback = callback;
			selectedIndex = index;
			UpdateLayout();
		}

		public override Vector2 GetWindowSize() {
			base.editorWindow.minSize = new Vector2(400, 400);
			return base.GetWindowSize();
		}

		public override void OnOpen() {
			base.OnOpen();

			UpdateLayout();

			// move scroll to have selected trigger visible in the list
			if(selectedIndex >= 0 && selectedIndex < fullSelectionList.Count) {
				drawTriggerListScrollPosition.y = (selectedIndex * 20.0f) - base.GetWindowSize().y * 0.75f;
			}
		}

		public override void OnGUI(Rect rect) {
			// show a list of tagged entries
			GUI.BeginGroup(rect, skin.box);
			GUILayout.Space(4);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Search", skin.label, GUILayout.Width(60));
			EditorGUI.BeginChangeCheck();
			GUI.SetNextControlName("SearchableSelectList.SearchField");
			searchText = GUILayout.TextArea(searchText, skin.textArea, GUILayout.Width(rect.width - 74));
			GUI.FocusControl("SearchableSelectList.SearchField");

			if(EditorGUI.EndChangeCheck()) {
				updateLayout = true;
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(2);
			// select a tag to change the list
			drawTriggerListScrollPosition = GUILayout.BeginScrollView(drawTriggerListScrollPosition, skin.box);
			// list has a scroll bar
			for(int t = 0; t < searchSelectionList.Count; t++) {
				if(searchSelectionList[t] == null) {
					continue;
				}
				if(GUILayout.Button(new GUIContent(searchSelectionList[t]), skin.button, GUILayout.Height(20.0f))) {
					selectCallback(fullSelectionList.IndexOf(searchSelectionList[t]));
				}
			}
			// search bar at the top of the list
			// searches all items and filters the shown list

			GUILayout.EndScrollView();

			GUI.EndGroup();

			if(Event.current.type != EventType.layout && updateLayout) {
				updateLayout = false;
				UpdateLayout();
			}
		}

		void UpdateLayout() {
			
			searchSelectionList = new List<string>();
			if(searchText != "") {
				searchText = searchText.ToLower();
				for(int i = 0; i < fullSelectionList.Count; i ++){
					if(fullSelectionList[i].ToLower().Contains(searchText)){
						searchSelectionList.Add(fullSelectionList[i]);
					}
				}
			} else {
				searchSelectionList.AddRange(fullSelectionList);
			}

		}

		public override void OnClose() {
			base.OnClose();
		}
	}

}
