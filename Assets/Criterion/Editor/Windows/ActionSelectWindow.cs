using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using PickleTools.Criterion.ActionLookup;

public delegate void ActionTypeSelectHandler(int actionUID);

namespace PickleTools.Criterion {
	public class ActionSelectWindow : EditorWindow {

		public static int IconTileSize = 90;

		class PresentationParams {
			public int Border = 12;
			public int TileSep = 8;
			public int LabelHeight = 15;
			public int LabelOffset = 4;
			public int ScrollbarWidth = 16;
			public int TileSizeMin = 32;
			public int TileSizeMax = 512;
			public int NumTilesPerRow = 0;
			public int TileHeight = 0;
			public int TileWidth = 0;
			public int NumRows = 0;
			int tileSizeInt = 128;

			public Vector2 RectDims;

			public void Update(int numTiles) {
				tileSizeInt = TileSize;
				int w = Screen.width - ScrollbarWidth - Border * 2;
				NumTilesPerRow = Mathf.Max((w + TileSep) / (tileSizeInt + TileSep), 1);
				NumRows = (numTiles + NumTilesPerRow - 1) / NumTilesPerRow;
				TileWidth = tileSizeInt + TileSep;
				TileHeight = tileSizeInt + TileSep + LabelOffset + LabelHeight;
				RectDims = new Vector2(w, NumRows * TileHeight + Border * 2);
			}

			public int TileSize {
				get { return IconTileSize; }
				set { IconTileSize = Mathf.Clamp(value, TileSizeMin, TileSizeMax); }
			}

			public int GetYForIndex(int index) {
				int tileSep = 8;
				int labelHeight = 15;
				int tileHeight = tileSizeInt + tileSep + LabelOffset + labelHeight;
				return tileHeight * (index / NumTilesPerRow);
			}
		}

		PresentationParams presentParams = new PresentationParams();

		ActionTypeSelectHandler callback = null;
		int callbackData = -1;

		string searchFilter = "";
		int selectedIndex = -1;
		bool makeSelectionVisible = false;
		Vector2 scroll = Vector2.zero;

		class SelectActionData {
			public int ActionUID = -1;
			public Texture Icon = null;
			public string ActionName = "";
		}

		[SerializeField]
		private static List<SelectActionData> actionSelections = new List<SelectActionData>();
		[SerializeField]
		private static List<SelectActionData> displayedTypes = new List<SelectActionData>();

		int selectedAction = (int)ActionType.NONE;

		private List<int> selectableActions = new List<int>();

		CriterionDataLoader<ActionModel> actionLoader;

		void OnEnable() {
			actionLoader = new CriterionDataLoader<ActionModel>();
			actionLoader.Load();

			selectableActions = new List<int>();
			for (int a = 0; a < actionLoader.Models.Length; a++) {
				if (actionLoader.Models[a] == null) {
					continue;
				}
				selectableActions.Add(actionLoader.Models[a].UID);
			}

			CollectObjectTypePrefabs();
			UpdateFilter();
		}

		void OnDestroy() {
			
		}

		void CollectObjectTypePrefabs() {
			
			bool updateSelectObjectData = false;
			//check to see if we should update

			for (int i = 0; i < selectableActions.Count; i++) {
				if (i >= actionSelections.Count || selectableActions[i] != actionSelections[i].ActionUID || actionSelections[i].Icon == null) {
					updateSelectObjectData = true;
					break;
				}
			}
			if (updateSelectObjectData) {
				actionSelections.Clear();
				for (int i = 0; i < selectableActions.Count; i++) {
					SelectActionData data = new SelectActionData();
					data.ActionUID = selectableActions[i];
					ActionModel model = actionLoader.GetData(data.ActionUID);
					if (model != null) {
						data.ActionName = model.Name;
					}

					// get icon
					switch (data.ActionUID) {
						default:
							data.Icon = AssetDatabase.LoadAssetAtPath<Texture>(
								"PickleTools/Criterion/Images/action_map_icon_null.png");
							break;
					}
					actionSelections.Add(data);
				}
			}
		}


		void PerformSelection() {
			Mathf.Clamp(selectedIndex, 0, displayedTypes.Count);
			callbackData = displayedTypes[selectedIndex].ActionUID;
			if (selectedIndex != -1 && callback != null) {
				callback(callbackData);
			}
		}


		void UpdateFilter() {
			int newSelectedObject = selectedAction;
			string s = searchFilter.ToLower();
			if (s != "") {
				displayedTypes = (from d in actionSelections where d.ActionName.ToString().ToLower().IndexOf(s) != -1 select d)
					.OrderBy(d => d.ActionName.ToString())
						.ToList();
			} else {
				displayedTypes = (from d in actionSelections select d)
					.OrderBy(d => d.ActionName.ToString())
						.ToList();
			}

			selectedIndex = -1;
			for (int i = 0; i < displayedTypes.Count; i++) {
				if (newSelectedObject == displayedTypes[i].ActionUID) {
					selectedIndex = 1;
					break;
				}
			}
		}

		void HandleKeyboardShortcuts() {
			Event ev = Event.current;

			int numTilesPerRow = presentParams.NumTilesPerRow;
			int newSelectedIndex = selectedIndex;
			if (ev.type == EventType.KeyDown) {
				switch (ev.keyCode) {
					case KeyCode.Escape:
						if (searchFilter.Length > 0) {
							searchFilter = "";
							UpdateFilter();
							newSelectedIndex = selectedIndex;
						} else {
							Close();
						}
						ev.Use();
						break;
					case KeyCode.Return:
						PerformSelection();
						ev.Use();
						Close();
						break;
					case KeyCode.RightArrow:
						newSelectedIndex++;
						break;
					case KeyCode.LeftArrow:
						newSelectedIndex--;
						break;
					case KeyCode.DownArrow:
						if (newSelectedIndex + numTilesPerRow < displayedTypes.Count) {
							newSelectedIndex += numTilesPerRow;
						}
						break;
					case KeyCode.UpArrow:
						if (newSelectedIndex - numTilesPerRow >= 0) {
							newSelectedIndex -= numTilesPerRow;
						}
						break;
					case KeyCode.Home:
						newSelectedIndex = 0;
						break;
					case KeyCode.End:
						newSelectedIndex = displayedTypes.Count - 1;
						break;
					default:
						return;
				}

				newSelectedIndex = Mathf.Clamp(newSelectedIndex, 0, displayedTypes.Count - 1);
				if (newSelectedIndex != selectedIndex) {
					selectedIndex = newSelectedIndex;
					ev.Use();
					makeSelectionVisible = true;
				}
			}
		}

		void OnGUI() {
			HandleKeyboardShortcuts();
			presentParams.Update(displayedTypes.Count);

			if (makeSelectionVisible) {
				scroll.y = presentParams.GetYForIndex(selectedIndex);
				makeSelectionVisible = false;
			}

			GUILayout.BeginArea(new Rect(0, 0, Screen.width, Screen.height));

			GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

			GUILayout.Label("Action Type: ");
			GUILayout.Space(8);

			string searchControlName = "actionTypePickerSearch";
			GUI.SetNextControlName(searchControlName);
			string newSearchFilter = GUILayout.TextField(searchFilter, EditorStyles.toolbar, GUILayout.Width(140));
			if (GUIUtility.keyboardControl == 0) {
				GUI.FocusControl(searchControlName);
			}
			if (newSearchFilter != searchFilter) {
				searchFilter = newSearchFilter;
				UpdateFilter();
			}
			if (newSearchFilter.Length > 0) {
				if (GUILayout.Button("", EditorStyles.toolbar, GUILayout.ExpandWidth(false))) {
					searchFilter = "";
					UpdateFilter();
				}
			} else {
				GUILayout.Label("", EditorStyles.toolbar);
			}

			GUILayout.FlexibleSpace();
			presentParams.TileSize = (int)EditorGUILayout.Slider(presentParams.TileSize, presentParams.TileSizeMin, presentParams.TileSizeMax, GUILayout.Width(120));
			GUILayout.EndHorizontal();

			if (selectedIndex < 0 || selectedIndex >= actionSelections.Count) {
				if (actionSelections.Count == 0) {
					selectedIndex = -1;
				} else {
					selectedIndex = 0;
				}
			}

			DrawObjectTypePrefabs(displayedTypes);

			GUILayout.EndArea();
		}

		void DrawObjectTypePrefabs(List<SelectActionData> objectDatas) {
			Event ev = Event.current;
			int tileSize = presentParams.TileSize;

			scroll = GUILayout.BeginScrollView(scroll);
			Rect r = GUILayoutUtility.GetRect(presentParams.RectDims.x, presentParams.RectDims.y);
			if (ev.type == EventType.mouseDown && ev.button == 0 && r.Contains(ev.mousePosition)) {
				int selX = ((int)ev.mousePosition.x - presentParams.Border) / presentParams.TileWidth;
				int selY = ((int)ev.mousePosition.y - presentParams.Border) / presentParams.TileHeight;
				int selID = selY * presentParams.NumTilesPerRow + selX;
				if (selX < presentParams.NumTilesPerRow && selY < presentParams.NumRows) {
					selectedIndex = Mathf.Clamp(selID, 0, objectDatas.Count - 1);
					Repaint();
				}

				if (ev.clickCount == 2) {
					PerformSelection();
					Close();
				}

				ev.Use();
			}

			r.x += presentParams.Border;
			r.y += presentParams.Border;

			int ix = 0;
			float x = r.x;
			float y = r.y;
			int index = 0;

			foreach (SelectActionData data in objectDatas) {
				string aTypeString = data.ActionName;

				Texture tex = data.Icon;

				Rect spriteRect = new Rect(x, y, tileSize, tileSize);
				Rect labelRect = new Rect(x, y + tileSize + presentParams.LabelOffset, tileSize, presentParams.LabelHeight);
				if (selectedIndex == index) {
					GUI.Label(labelRect, "", EditorStyles.toolbar);
				}

				if (tex == null) {
					GUI.Box(spriteRect, new GUIContent("NO ICON"));
				} else {
					GUI.DrawTexture(spriteRect, tex);
				}

				GUI.Label(labelRect, aTypeString, EditorStyles.miniLabel);

				if (++ix >= presentParams.NumTilesPerRow) {
					ix = 0;
					x = r.x;
					y += presentParams.TileHeight;
				} else {
					x += presentParams.TileWidth;
				}
				index++;
			}

			GUILayout.EndScrollView();
		}

		void InitForActionInCollection(int actionUID) {
			selectedAction = actionUID;
			searchFilter = "";
			UpdateFilter();

			for (int i = 0; i < actionSelections.Count; i++) {
				if (selectedAction == actionSelections[i].ActionUID) {
					selectedIndex = i;
					break;
				}
			}
			if (selectedIndex != -1) {
				makeSelectionVisible = true;
			}
		}


		public static void DoPickAction(int currentUID, string title, ActionTypeSelectHandler callback, int callbackData) {
			ActionSelectWindow popup = EditorWindow.GetWindow(typeof(ActionSelectWindow), true, title, true) as ActionSelectWindow;
			popup.InitForActionInCollection(currentUID);
			popup.callback = callback;
			popup.callbackData = callbackData;
		}
	}
}