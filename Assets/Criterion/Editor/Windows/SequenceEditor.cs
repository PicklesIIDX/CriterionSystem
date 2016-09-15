using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using PickleTools.Criterion.ActionLookup;
using PickleTools.FileAccess;
using PickleTools.UnityEditor;

namespace PickleTools.Criterion {
	public class SequenceEditor : EditorWindow {

		public class TimelineAction {
			public SequenceActionModel Data;

			public int chainDepth = 0; // horizontal
			public int trackDepth = 0; // vertical
			public Rect PositionRect;

			// state data for display
			public bool IsDraggedOn = false;
			public SequenceActionModel PeerData = null;
			public SequenceActionModel ParentData = null;

			public TimelineAction(SequenceActionModel data) {
				Data = data;
			}
		}

		public enum BoxDirectionType {
			NONE,
			TOP_LEFT,
			RIGHT,
			TOP_MIDDLE,
			END_RIGHT,
			LEFT_MIDDLE,
			BOTTOM_LEFT,
			TOP_RIGHT,
			END_BOTTOM,
			LEFT,
			BOTTOM_MIDDLE
		}

		public class MapBox {
			public int chainDepth = 0;
			public int trackDepth = 0;
			public BoxDirectionType direction = BoxDirectionType.NONE;

		}

		private int selectedTrigger = -1;

		private SequenceLoader sequenceLoader;
		private SequenceModel[] sequenceModels = new SequenceModel[0];
		private string error;
		private ActionLoader actionLoader;
		private ConditionLoader conditionLoader;

		private int totalTrackDepth = 0;
		private int totalChainLength = 0;

		private TimelineAction selectedAction;
		private TimelineAction selectedPossibleLocation;
		private List<int> sequenceHistory = new List<int>();
		private int sequenceHistoryIndex = 0;

		public bool IsSelectingAction {
			get { return selectedNode != null; }
		}

		float headerHeight = 32;

		SequenceMap.ActionNode hoveredNode;
		SequenceMap.ActionNode selectedNode;
		SequenceMap.ActionNode replaceNode;
		SequenceMap.ActionNode selectedAddNode;

		private Texture bgTexture;
		private Texture nodeElbow;
		private Texture nodeAdd;
		private Texture nodeAddClick;
		private Texture pathHorizontal;
		private Texture pathVertical;
		private Texture iconStart;
		private Texture iconStartHover;
		private Texture iconStartClick;
		private Texture iconNull;
		private Texture iconNullHover;
		private Texture iconNullClick;
		private Texture iconMoveObject;
		private Texture iconMoveObjectHover;
		private Texture iconMoveObjectClick;
		private Texture iconShowDialogue;
		private Texture iconShowDialogueHover;
		private Texture iconShowDialogueClick;
		private Texture iconShowDialogueChoice;
		private Texture iconShowDialogueChoiceHover;
		private Texture iconShowDialogueChoiceClick;
		private Texture iconUpdateWorldState;
		private Texture iconUpdateWorldStateHover;
		private Texture iconUpdateWorldStateClick;
		private Texture iconSpriteAnimation;
		private Texture iconSpriteAnimationHover;
		private Texture iconSpriteAnimationClick;
		private Texture iconPlaySound;
		private Texture iconPlaySoundHover;
		private Texture iconPlaySoundClick;
		private Texture iconLinkAction;
		private Texture iconLinkActionHover;
		private Texture iconLinkActionClick;
		private Texture iconChangeScene;
		private Texture iconChangeSceneHover;
		private Texture iconChangeSceneClick;
		private Texture iconChangeInventory;
		private Texture iconChangeInventoryHover;
		private Texture iconChangeInventoryClick;
		private Texture iconSetState;
		private Texture iconSetStateHover;
		private Texture iconSetStateClick;
		private Texture iconEnableCommand;
		private Texture iconEnableCommandHover;
		private Texture iconEnableCommandClick;
		private Texture iconSetHull;
		private Texture iconSetHullHover;
		private Texture iconSetHullClick;
		private Texture iconSetGameTime;
		private Texture iconSetGameTimeHover;
		private Texture iconSetGameTimeClick;
		private Texture iconSetScrollSpeed;
		private Texture iconSetScrollSpeedHover;
		private Texture iconSetScrollSpeedClick;
		private Texture iconWait;
		private Texture iconWaitHover;
		private Texture iconWaitClick;
		private Texture iconFullScreenEffect;
		private Texture iconFullScreenEffectHover;
		private Texture iconFullScreenEffectClick;
		private Texture iconUpdateBehaviorVariable;
		private Texture iconUpdateBehaviorVariableHover;
		private Texture iconUpdateBehaviorVariableClick;
		private Texture iconSetWeapon;
		private Texture iconSetWeaponHover;
		private Texture iconSetWeaponClick;
		private Texture iconObjectSpawn;
		private Texture iconObjectSpawnHover;
		private Texture iconObjectSpawnClick;
		private Texture iconDebugLog;
		private Texture iconDebugLogHover;
		private Texture iconDebugLogClick;
		private Texture iconDisplayDoc;
		private Texture iconDisplayDocHover;
		private Texture iconDisplayDocClick;
		private Texture iconDialogueTimer;
		private Texture iconDialogueTimerHover;
		private Texture iconDialogueTimerClick;
		private Texture iconScreenShake;
		private Texture iconScreenShakeHover;
		private Texture iconScreenShakeClick;
		private Texture iconChangeDirectorBehavior;
		private Texture iconChangeDirectorBehaviorHover;
		private Texture iconChangeDirectorBehaviorClick;
		private Texture iconChangeDirectorPatterns;
		private Texture iconChangeDirectorPatternsHover;
		private Texture iconChangeDirectorPatternsClick;

		private Texture iconBack;
		private Texture iconForward;
		private Texture iconCopy;
		private Texture iconPaste;

		private Texture logoTexture;

		GUISkin skin;

		private const float kZoomMin = 0.5f;
		private const float kZoomMax = 2.0f;
		private Rect zoomArea = new Rect(0.0f, 0.0f, 600.0f, 300.0f - 100.0f);
		private float zoom = 1.0f;
		private Vector2 zoomCoordsOrigin = Vector2.zero;

		private bool isDraggingAction;
		private ISequenceActionEditor mapActionEditor;

		int nodeSpace = 64;
		int horizontalOffset = 32;

		SequenceMap map;

		EditorDeltaTimeCheck editorTimeCheck;
		float deltaTime = 0.0f;
		Vector2 logoOffset = Vector2.zero;

		private int minCopyBufferLength = 0;

		string lastTooltip = "";

		bool changesMade = false;

		public static readonly string PREFS_REFRESH_SEQUENCE_EDITOR = "SequenceEditor.Refresh";
		public static readonly string PREFS_CURRENT_SEQUENCE = "SequenceEditor.CurrentSequence";
		public static readonly string PREFS_SELECTED_NODE_CHAIN_DEPTH = "SequenceEditor.SelectedNodeChain";
		public static readonly string PREFS_SELECTED_NODE_TRACK_DEPTH = "SequenceEditor.SelectedNodeTrack";
		public static readonly string PREFS_UPDATE_ACTION_MAP_WINDOW = "SequenceEditor.RefreshActionMap";


		[MenuItem("Mazer Maker/Windows/Sequence Editor")]
		public static void ShowEditor() {
			SequenceEditor window = GetWindow<SequenceEditor>();
			window.titleContent = new GUIContent("Sequence Editor");
			window.Show();
			window.Initialize();
		}

		public static void UpdateMap() {
			SequenceEditor window = GetWindow<SequenceEditor>();
			window.Show();
		}

		public void OnFocus() {
			if (skin == null || sequenceLoader == null || actionLoader == null || conditionLoader == null ||
			    editorTimeCheck == null) {
				Initialize();
			} else {
				Refresh();
			}

		}

		void OnLostFocus() {
			if (changesMade) {
				Save();
			}
		}

		void Refresh() {
			sequenceLoader = new SequenceLoader();
			sequenceLoader.Load();
			sequenceModels = new SequenceModel[sequenceLoader.HighestUID + 1];
			for (int s = 0; s < sequenceLoader.SequenceModels.Length; s++) {
				sequenceModels[s] = sequenceLoader.SequenceModels[s];
			}

			actionLoader = new ActionLoader();
			actionLoader.Load();
			conditionLoader = new ConditionLoader();
			conditionLoader.Load();
		}

		public void Initialize() {
			string imagePath = "PickleTools/Criterion/Images/";

			bgTexture = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "action_map_bg_tile.png");
			nodeElbow = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "action_map_node.png");
			nodeAdd = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "action_map_node_add.png");
			nodeAddClick = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "action_map_node_click.png");
			pathHorizontal = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "action_map_path_horizontal.png");
			pathVertical = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "action_map_path_vertical.png");

			iconStart = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "action_map_icon_start.png");
			iconStartHover = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "action_map_icon_start_hover.png");
			iconStartClick = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "action_map_icon_start_click.png");
			iconNull = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "action_map_icon_null.png");
			iconNullHover = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "action_map_icon_null_hover.png");
			iconNullClick = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "action_map_icon_null_click.png");

			iconBack = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "mazer_maker_icon_back.png");
			iconForward = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "mazer_maker_icon_forward.png");
			iconCopy = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "mazer_maker_icon_copy.png");
			iconPaste = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "mazer_maker_icon_paste.png");

			logoTexture = AssetDatabase.LoadAssetAtPath<Texture>(imagePath + "mazer_maker_logo.png");

			skin = AssetDatabase.LoadAssetAtPath<GUISkin>(imagePath + "gui_skin.guiskin");

			editorTimeCheck = new EditorDeltaTimeCheck();

			EditorPrefs.SetBool(PREFS_REFRESH_SEQUENCE_EDITOR, false);

			Refresh();

			map = new SequenceMap(null);

			string editingJSON = EditorPrefs.GetString(PREFS_CURRENT_SEQUENCE, "");
			SequenceModel editingSequence = LitJson.JsonMapper.ToObject<SequenceModel>(editingJSON);
			if (editingSequence != null) {
				if (sequenceModels.Length >= editingSequence.UID || sequenceModels[editingSequence.UID] == null) {

				} else {
					editingSequence.Name = sequenceModels[editingSequence.UID].Name;
					sequenceModels[editingSequence.UID] = editingSequence;
					map.SetSequence(sequenceModels[editingSequence.UID]);
				}
			}

			minCopyBufferLength = LitJson.JsonMapper.ToJson(new SequenceModel()).Length;

			EditorApplication.playmodeStateChanged -= PlayStateChanged;
			EditorApplication.playmodeStateChanged += PlayStateChanged;
		}

		void PlayStateChanged() {
			if (!Application.isPlaying && changesMade) {
				Save();
			}
		}

		private void DeselectNode() {
			if (selectedNode != null) {
				selectedNode.IsClicked = false;
				SelectNode(null);
				EditorPrefs.SetInt(PREFS_SELECTED_NODE_CHAIN_DEPTH, -1);
				EditorPrefs.SetInt(PREFS_SELECTED_NODE_TRACK_DEPTH, -1);
				if (mapActionEditor != null) {
					mapActionEditor.Deinitialize();
					mapActionEditor = null;
				}
			}
		}


		private void SelectNode(SequenceMap.ActionNode newNode) {
			selectedNode = newNode;
			if (selectedNode != null) {
				EditorPrefs.SetInt(PREFS_SELECTED_NODE_CHAIN_DEPTH, selectedNode.ChainDepth);
				EditorPrefs.SetInt(PREFS_SELECTED_NODE_TRACK_DEPTH, selectedNode.TrackDepth);
				if (selectedNode.SequenceActionModel != null) {
					if (mapActionEditor != null) {
						mapActionEditor.DeletePerformed -= HandleDeletePerformed;
					}
					int actionUID = selectedNode.SequenceActionModel.UID;
					switch (actionUID) {
						// TODO: make this automatic
						case (int)ActionType.UPDATE_WORLD_STATE:
							mapActionEditor = CreateInstance<SequenceActionUpdateWorldStateEditor>();
							break;
						default:
							mapActionEditor = CreateInstance<SequenceActionEditor>();
							break;
					}
					mapActionEditor.Initialize(selectedNode.SequenceActionModel, actionLoader, conditionLoader);
					mapActionEditor.DeletePerformed += HandleDeletePerformed;
					mapActionEditor.ChangesMade += HandleChangesMade;
				}
				selectedNode.IsClicked = true;
			}
			if (Event.current != null) {
				Event.current.Use();
			}
		}
		int deleteChain = -1;
		int deleteTrack = -1;
		void HandleDeletePerformed() {
			deleteChain = selectedNode.ChainDepth;
			deleteTrack = selectedNode.TrackDepth;
		}

		void HandleChangesMade(SequenceActionModel actionModel) {
			changesMade = true;
		}

		bool updateNodeMap = false;

		public void OnGUI() {
			if (Event.current.type != EventType.layout && EditorPrefs.GetBool(PREFS_REFRESH_SEQUENCE_EDITOR, false)) {
				Initialize();
				EditorPrefs.SetBool(PREFS_REFRESH_SEQUENCE_EDITOR, false);
			}

			if (editorTimeCheck == null || logoTexture == null || map == null) {
				Initialize();
			}
			if (Event.current.type != EventType.layout) {
				bool dataChanged = EditorPrefs.GetBool(PREFS_UPDATE_ACTION_MAP_WINDOW, false);
				if (dataChanged) {
					Initialize();
					return;
				}
			}

			////////////////////////
			// BACKGROUND
			////////////////////////
			deltaTime = editorTimeCheck.EditorDeltaTime;
			logoOffset.x += deltaTime * 0.1f;
			logoOffset.y -= deltaTime * 0.1f;
			GUI.DrawTextureWithTexCoords(new Rect(0, 0, Screen.width, Screen.height), logoTexture,
				new Rect(logoOffset.x, logoOffset.y,
					Screen.width / logoTexture.width,
					Screen.height / logoTexture.height),
				false);

			Repaint();




			////////////////////////
			// HEADER
			////////////////////////
			// draw the window header, which shows the selected trigger
			Rect headerRect = new Rect(0, 0, Screen.width, headerHeight);
			GUILayout.BeginArea(headerRect);
			GUILayout.BeginHorizontal(skin.FindStyle("title"));
			string titleString = "No Trigger Selected";
			SequenceModel sequence = sequenceLoader.GetSequence(selectedTrigger);
			if (sequence != null) {
				titleString = sequence.Name;
			}
			GUILayout.Label(new GUIContent(titleString,
										  "The name of the selected sequence. You can select a difference sequence using the" +
										   " Trigger Editor."),
							skin.FindStyle("title"), GUILayout.Height(headerHeight));

			// copy and paste buttons
			// they are only enabled if we have a trigger selected
			if (selectedTrigger >= 0) {
				if (GUILayout.Button(new GUIContent(iconCopy, "Copy this sequence to the clipboard."),
									skin.button, GUILayout.Height(headerHeight - 4), GUILayout.Width(headerHeight))) {
					EditorGUIUtility.systemCopyBuffer = LitJson.JsonMapper.ToJson(map.Sequence);
				}
			} else {
				GUILayout.Box(new GUIContent(iconCopy, "These is no sequence to copy!"),
							  skin.box, GUILayout.Height(headerHeight - 6), GUILayout.Width(headerHeight));
			}
			bool paste = false;
			if (EditorGUIUtility.systemCopyBuffer.Length >= minCopyBufferLength && selectedTrigger >= 0) {
				if (GUILayout.Button(new GUIContent(iconPaste,
												  "Paste a sequence that was copied to the clipboard."),
									skin.button, GUILayout.Height(headerHeight - 4), GUILayout.Width(headerHeight))) {
					paste = true;
				}
			} else {
				GUILayout.Box(new GUIContent(iconPaste,
												  "You need to copy another sequnece before you can paste!"),
							  skin.box, GUILayout.Height(headerHeight - 6), GUILayout.Width(headerHeight));
			}


			if (sequenceHistoryIndex > 1) {
				if (GUILayout.Button(new GUIContent(iconBack,
												  "Go back to the previously selected sequence."),
									skin.button, GUILayout.Height(headerHeight - 4), GUILayout.Width(headerHeight))) {
					GoToPreviousMap();
				}
			} else {
				GUILayout.Box(new GUIContent(iconBack,
												  "There is no previous sequence to return to."),
							  skin.box, GUILayout.Height(headerHeight - 6), GUILayout.Width(headerHeight));
			}
			if (sequenceHistoryIndex < sequenceHistory.Count - 1) {
				if (GUILayout.Button(new GUIContent(iconForward,
												  "Go forward to the previously selected sequence."),
									skin.button, GUILayout.Height(headerHeight - 4), GUILayout.Width(headerHeight))) {
					GoToNextMap();
				}
			} else {
				GUILayout.Box(new GUIContent(iconForward,
												  "There is no further sequence to go to."),
							  skin.box, GUILayout.Height(headerHeight - 6), GUILayout.Width(headerHeight));
			}
			zoom = GUILayout.HorizontalSlider(zoom, kZoomMin, kZoomMax, skin.horizontalSlider,
											  skin.horizontalSliderThumb, GUILayout.Width(120));

			//if(!changesMade && GUILayout.Button(iconSave, skin.button, GUILayout.Width(headerHeight), GUILayout.Height(headerHeight-6))){
			//	Save();
			//}
			//if(changesMade && GUILayout.Button(iconSaveWarning, skin.button, GUILayout.Width(headerHeight), GUILayout.Height(headerHeight-6))){
			//	Save();
			//}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();




			////////////////////////
			// MAP
			////////////////////////
			// if we don't have a selected trigger, don't draw anything!
			if (map.Sequence != null) {
				// info box
				Rect infoBoxRect = new Rect(0, headerHeight, (int)(Screen.width * 0.7f), 36);
				if (mapActionEditor == null) {
					infoBoxRect.width = Screen.width;
				}
				GUI.Box(infoBoxRect, new GUIContent(lastTooltip,
											 "This is the info box. It displays useful information about anything you" +
											 " move your cursor over."),
							  skin.box);

				// draw the map action editor in the editor window if we have an action selected
				if (selectedNode != null && selectedNode.SequenceActionModel != null && mapActionEditor != null) {
					zoomArea.width = Screen.width * 0.7f;
					Rect actionEditorRect = new Rect((int)(Screen.width * 0.7f), headerHeight,
													  (int)(Screen.width * 0.3f), Screen.height);
					mapActionEditor.Draw(actionEditorRect);
				} else {
					zoomArea.width = Screen.width;
				}

				zoomArea.height = Screen.height - headerHeight;
				zoomArea.y = headerHeight + infoBoxRect.height - 2;
				zoomArea = EditorZoomArea.Begin(zoom, zoomArea);

				// a rectangle for the space below the header
				Rect mapRect = new Rect(0, headerRect.height,
											 Screen.width, Screen.height - headerRect.height);
				totalChainLength = map.GetChainDepth(map.Sequence.Actions) + 4;
				totalTrackDepth = map.GetTrackDepth(map.Sequence.Actions) + 4;
				float mapWidth = totalChainLength * nodeSpace;
				float mapHeight = totalTrackDepth * nodeSpace;
				// create a space to draw the map based the current pan of the zoom area
				GUI.BeginGroup(new Rect(mapRect.x + zoomCoordsOrigin.x, mapRect.y + zoomCoordsOrigin.y, mapWidth, mapHeight));
				// draw tiled background
				GUI.DrawTextureWithTexCoords(new Rect(mapRect.x, mapRect.y + mapHeight, mapWidth, -mapHeight), bgTexture,
											 new Rect(0, 0, mapWidth / bgTexture.width, mapHeight / bgTexture.height), false);

				DrawNodeMap(mapRect);
				// if we a dragging an action node, draw an icon of that node next to the mouse
				if (selectedNode != null) {
					if (selectedNode.SequenceActionModel != null && isDraggingAction) {
						Texture dragNodeTexture = GetNodeIconTexture(selectedNode);
						GUI.color = new Color(1, 1, 1, 0.7f);
						GUI.DrawTexture(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, dragNodeTexture.width, dragNodeTexture.height), dragNodeTexture);
						GUI.color = new Color(1, 1, 1, 1);
					}
				}

				GUI.EndGroup();

				HandleInput(mapRect);

				EditorZoomArea.End();
			}

			if ((Event.current.type == EventType.Repaint && GUI.tooltip != lastTooltip)) {
				if (mapActionEditor != null && GUI.tooltip == mapActionEditor.LastTooltip) {
					// don't take the tooltip from the mapActionEditor!
				} else {
					lastTooltip = GUI.tooltip;
				}
				if (lastTooltip == "") {
					lastTooltip = "This is the Sequence Editor, where you can create scenes and interactions in the game!" +
						" You can pan the space below by pressing alt + left mouse button and dragging (or middle mouse + drag)." +
						" You can zoom in and out by scrolling the mouse wheel, or using the slider on the top right.";
				}
			}

			// Update our map if the currently selected trigger is different than the one we have stored
			int currentTrigger = selectedTrigger;
			try {
				selectedTrigger = EditorPrefs.GetInt(TriggerEditor.PREFS_CURRENT_TRIGGER, -1);
			} catch {
				selectedTrigger = -1;
			}
			if (map.Sequence == null || map.Sequence.UID != selectedTrigger) {
				if (selectedTrigger == -1) {
					DeselectNode();
					map.SetSequence(null);
					zoomCoordsOrigin = Vector2.zero;
				} else {
					for (int s = 0; s < sequenceModels.Length; s++) {
						if (sequenceModels[s] == null) {
							continue;
						}
						if (sequenceModels[s].UID == selectedTrigger) {
							map.SetSequence(sequenceModels[s]);
							zoomCoordsOrigin = Vector2.zero;
							// first, we clear all future history
							if (sequenceHistoryIndex < sequenceHistory.Count - 1) {
								int historySize = sequenceHistory.Count - 1;
								for (int i = historySize; i > sequenceHistoryIndex; i--) {
									sequenceHistory.RemoveAt(i);
								}
							}
							// then we add this to our history list
							if (sequenceHistory.Count == 0 ||
							   map.Sequence.UID != sequenceHistory[sequenceHistory.Count - 1]) {
								sequenceHistory.Add(map.Sequence.UID);
								sequenceHistoryIndex = sequenceHistory.Count - 1;
							}
							break;
						}
					}
				}
			}
			if (currentTrigger != selectedTrigger) {
				updateNodeMap = true;
			}
			if (updateNodeMap && Event.current.type != EventType.Layout) {
				return;
			}
			updateNodeMap = false;

			// try to convert data on the computer's clipboard to a sequence
			if (paste) {
				try {
					SequenceModel editingSequence = LitJson.JsonMapper.ToObject<SequenceModel>(EditorGUIUtility.systemCopyBuffer);
					if (editingSequence != null) {
						// overwrite the copy data's UID because we want to store that copied data
						// into this current sequence
						SequenceModel model = new SequenceModel();
						model.UID = map.Sequence.UID;
						model.Name = map.Sequence.Name;
						model.Actions = editingSequence.Actions;
						sequenceModels[map.Sequence.UID] = model;
						map.SetSequence(sequenceModels[map.Sequence.UID]);
						EditorPrefs.SetString(PREFS_CURRENT_SEQUENCE, LitJson.JsonMapper.ToJson(map.Sequence));
						changesMade = true;
					}
				} catch {
					Debug.LogWarning("[SequenceEditor.cs]: Could not paste. The data on the clipboard is likely not formatted correctly.\n" +
						EditorGUIUtility.systemCopyBuffer);
				}
			}

			// delete
			if (deleteChain >= 0 && deleteTrack >= 0) {
				DeselectNode();
				map.RemoveAction(deleteChain, deleteTrack);
				EditorPrefs.SetString(PREFS_CURRENT_SEQUENCE, LitJson.JsonMapper.ToJson(map.Sequence));
				changesMade = true;
				deleteChain = -1;
				deleteTrack = -1;
			}
		}

		private void HandleInput(Rect timelineRect) {
			Event currentEvent = Event.current;
			Vector2 offsetMousePosition = currentEvent.mousePosition;
			// offset mouse position based on where we have moved the zoom window
			offsetMousePosition.x -= zoomCoordsOrigin.x;
			offsetMousePosition.y -= zoomCoordsOrigin.y + headerHeight;

			switch (currentEvent.type) {
				case EventType.scrollWheel:
					// perform zoom
					// get zoom distance
					Vector2 delta = Event.current.delta;
					float zoomDelta = -delta.y / 150.0f;
					float oldZoom = zoom;
					zoom += zoomDelta;
					zoom = Mathf.Clamp(zoom, kZoomMin, kZoomMax);
					// reposition map to be relative to mouse position
					float zoomRatio = oldZoom / zoom;
					Rect differenceRect = zoomArea.ScaleSizeBy(zoomRatio);
					Vector2 screenMousePos = currentEvent.mousePosition * oldZoom;
					Vector2 mouseLocationRatio = new Vector2(screenMousePos.x / Screen.width, screenMousePos.y / Screen.height);
					// after scaling our zoomArea above by the ratio of zoom we are changing, we can now get the difference between
					// our old and new zoomArea view rects to reposition our zoom area
					// by incorporating the mouse location ratio, we can then adust the area relative to the mouse position
					zoomCoordsOrigin += new Vector2((differenceRect.width - zoomArea.width) * mouseLocationRatio.x,
													 (differenceRect.height - zoomArea.height) * mouseLocationRatio.y);
					Event.current.Use();

					break;

				case EventType.mouseDown:
					if (currentEvent.button == 0 && currentEvent.modifiers == EventModifiers.Alt || currentEvent.button == 2) {
						// don't allow selection with the pan commands
					} else {
						DeselectNode();
						SelectNode(GetNodeAtPosition(offsetMousePosition));
					}
					break;
				case EventType.mouseDrag:
					if ((Event.current.button == 0 && Event.current.modifiers == EventModifiers.Alt) ||
						Event.current.button == 2) {
						delta = Event.current.delta;
						delta /= zoom;
						zoomCoordsOrigin += delta;
						Event.current.Use();
					} else {

						// When dragging a node to replace another, update that node to be hovered
						if (selectedNode == null) {
							isDraggingAction = false;
							break;
						} else {
							SequenceMap.ActionNode possibleReplaceNode = GetNodeAtPosition(offsetMousePosition);
							if (possibleReplaceNode != null) {
								if (possibleReplaceNode != replaceNode) {
									if (replaceNode != null) {
										replaceNode.IsHovered = false;
									}
									replaceNode = possibleReplaceNode;
									replaceNode.IsHovered = true;
								}
							} else {
								if (replaceNode != null) {
									replaceNode.IsHovered = false;
									replaceNode = null;
								}
							}
							isDraggingAction = true;
						}
					}
					break;
				case EventType.mouseUp:
					// TODO: Undo command
					bool releaseNode = false;
					if (selectedNode != null && replaceNode != null) {
						if (selectedNode != replaceNode) {
							map.SwapActions(selectedNode, replaceNode);
							releaseNode = true;
							changesMade = true;
						}
					}
					// add a new action at a node
					if (selectedNode != null && replaceNode == null) {
						if (selectedNode.NodeType != SequenceMap.ActionNodeType.ACTION) {
							SelectNodeToAddAction(selectedNode);
							releaseNode = true;
						}
					}
					// release the selected node
					if (selectedNode != null && releaseNode) {
						DeselectNode();
					}
					if (replaceNode != null) {
						replaceNode.IsHovered = false;
						replaceNode = null;
					}
					isDraggingAction = false;
					break;
				default:
					break;
			}

			if (hoveredNode != null) {
				hoveredNode.IsHovered = false;
			}
			hoveredNode = GetNodeAtPosition(offsetMousePosition);
			if (hoveredNode != null) {
				hoveredNode.IsHovered = true;
			}
			Repaint();
		}

		private void GoToPreviousMap() {
			if (sequenceHistoryIndex > 1) {
				sequenceHistoryIndex--;
				for (int s = 0; s < sequenceModels.Length; s++) {
					if (sequenceModels[s] == null) {
						continue;
					}
					if (sequenceModels[s].UID == sequenceHistory[sequenceHistoryIndex]) {
						map.SetSequence(sequenceModels[s]);
						EditorPrefs.SetInt(TriggerEditor.PREFS_CURRENT_TRIGGER, map.Sequence.UID);
						zoomCoordsOrigin = Vector2.zero;
						break;
					}
				}
			}
		}

		private void GoToNextMap() {
			if (sequenceHistoryIndex < sequenceHistory.Count - 1) {
				sequenceHistoryIndex++;
				for (int s = 0; s < sequenceModels.Length; s++) {
					if (sequenceModels[s] == null) {
						continue;
					}
					if (sequenceModels[s].UID == sequenceHistory[sequenceHistoryIndex]) {
						map.SetSequence(sequenceModels[s]);
						EditorPrefs.SetInt(TriggerEditor.PREFS_CURRENT_TRIGGER, map.Sequence.UID);
						zoomCoordsOrigin = Vector2.zero;
						break;
					}
				}
			}
		}

		private void SelectNodeToAddAction(SequenceMap.ActionNode nodeToReplace) {
			selectedAddNode = nodeToReplace;
			ActionSelectWindow.DoPickAction((int)ActionType.NONE, "Select Action", SelectActionType, (int)ActionType.NONE);
		}

		private void SelectActionType(int actionUID) {
			SequenceActionModel action = new SequenceActionModel();
			ActionModel model = actionLoader.GetAction(actionUID);
			action.UID = model.UID;
			action.Parameters = new object[model.Parameters.Length];
			for (int p = 0; p < model.Parameters.Length; p++) {
				ValueTypeLoader.GetDefaultValue(ref action.Parameters[p], model.Parameters[p].ValueType);
			}
			// set custom parameters
			if (actionUID == (int)ActionType.CHANGE_DIRECTOR_BEHAVIOR) {
				action.Parameters[0] = 50;
				action.Parameters[1] = 1;
				action.Parameters[2] = 10;
				action.Parameters[3] = 10;
				action.Parameters[4] = 3;
				action.Parameters[5] = 6;
				action.Parameters[6] = 1;
				action.Parameters[7] = 0;

				action.Parameters[8] = 0;
				action.Parameters[9] = 10;
				action.Parameters[10] = 1;
				action.Parameters[11] = 1;
			}
			map.InsertAction(selectedAddNode.ChainDepth, selectedAddNode.TrackDepth, action);
			selectedAddNode = null;
			changesMade = true;
		}


		private SequenceMap.ActionNode GetNodeAtPosition(Vector2 mousePosition) {
			for (int n = 0; n < map.NodeList.Count; n++) {
				if (map.NodeList[n].IconRect.Contains(mousePosition)) {
					return map.NodeList[n];
				}
			}
			return null;
		}

		void DrawNodeMap(Rect areaRect) {
			// offset to match against BG
			areaRect.x += horizontalOffset;
			for (int p = 0; p < map.PathList.Count; p++) {
				Texture pathTexture = pathHorizontal;

				float width = nodeSpace;
				float height = pathTexture.height;
				if (map.PathList[p].NodeType == SequenceMap.ActionNodeType.PATH_VERTICAL) {
					pathTexture = pathVertical;
					width = pathTexture.width;
					height = nodeSpace;
				}
				GUI.DrawTexture(new Rect(areaRect.x + map.PathList[p].ChainDepth * nodeSpace + nodeSpace * 0.5f - pathVertical.width * 0.5f,
										 areaRect.y + map.PathList[p].TrackDepth * nodeSpace + nodeSpace * 0.5f - nodeElbow.height * 0.5f + pathHorizontal.height * 0.5f,
										 width, height), pathTexture);
			}
			for (int n = 0; n < map.NodeList.Count; n++) {
				Texture nodeTexture = iconNull;
				switch (map.NodeList[n].NodeType) {
					case SequenceMap.ActionNodeType.ACTION:
						nodeTexture = GetNodeIconTexture(map.NodeList[n]);
						if (map.NodeList[n].IsHovered) {
							GUI.tooltip = "An action node. Click it to edit what happens in the game when this action is called.";
						}
						break;
					case SequenceMap.ActionNodeType.ELBOW:
						if (map.NodeList[n].IsClicked) {
							nodeTexture = nodeAddClick;
						} else if (map.NodeList[n].IsHovered) {
							nodeTexture = nodeAdd;
							GUI.tooltip = "A node elbow, where you can click to add a new action between the two adjacent actions.";
						} else {
							nodeTexture = nodeElbow;
						}
						break;
					case SequenceMap.ActionNodeType.START:
						if (map.NodeList[n].IsClicked) {
							nodeTexture = iconStartClick;
						} else if (map.NodeList[n].IsHovered) {
							nodeTexture = iconStartHover;
							GUI.tooltip = "The start of the sequence. Actions aligned horizontally happen in sequence, where" +
								" the next one will happen after the previous one has completed. Actions aligned vertically " +
								"happen simultaneously, where they will all happen when the topmost action happens.";
						} else {
							nodeTexture = iconStart;
						}
						break;
					default:
						if (map.NodeList[n].IsClicked) {
							nodeTexture = iconNullClick;
						} else if (map.NodeList[n].IsHovered) {
							nodeTexture = iconNullHover;
						} else {
							nodeTexture = iconNull;
						}
						break;
				}
				if (nodeTexture == null) {
					return;
				}

				Rect nodeRect = new Rect(areaRect.x + map.NodeList[n].ChainDepth * nodeSpace + nodeSpace * 0.5f - nodeElbow.width * 0.5f,
										 areaRect.y + map.NodeList[n].TrackDepth * nodeSpace + nodeSpace * 0.5f - nodeTexture.height + nodeElbow.height * 0.5f,
										 nodeTexture.width, nodeTexture.height);
				map.NodeList[n].IconRect = nodeRect;
				GUI.DrawTexture(nodeRect, nodeTexture);

			}
		}

		Texture GetNodeIconTexture(SequenceMap.ActionNode actionNode) {
			Texture nodeTexture = iconNull;
			int actionType = -1;
			if (actionNode.SequenceActionModel != null) {
				actionType = actionNode.SequenceActionModel.UID;
			}
			switch (actionType) {
				// TODO: Automate this
				default:
					if (actionNode.IsClicked) {
						nodeTexture = iconNullClick;
					} else if (actionNode.IsHovered) {
						nodeTexture = iconNullHover;
					} else {
						nodeTexture = iconNull;
					}
					break;
			}
			return nodeTexture;
		}



		void Save() {
			sequenceLoader.SequenceModels[map.Sequence.UID] = map.Sequence;
			EditorFileSaver fileSaver = new EditorFileSaver("");
			sequenceLoader.Save(fileSaver);
			EditorPrefs.SetString(PREFS_CURRENT_SEQUENCE, LitJson.JsonMapper.ToJson(map.Sequence));
			changesMade = false;
		}
	}
}