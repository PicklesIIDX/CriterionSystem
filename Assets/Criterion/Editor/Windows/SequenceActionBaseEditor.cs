using UnityEngine;
using UnityEditor;
using System.Collections;

namespace PickleTools.Criterion {
	public class SequenceActionBaseEditor : Editor, ISequenceActionEditor {
		
		public event PreviewActionHandler PreviewMapAction;
		public event SequenceActionHandler UndoPerformed;
		public event DeleteSequenceActionHandler DeletePerformed;
		public event SequenceActionHandler ChangesMade;

		protected SequenceActionModel sequenceActionModel;
		public SequenceActionModel SequenceActionModel {
			get { return sequenceActionModel; }
		}

		ActionLoader actionLoader;
		
		protected GUISkin skin;
		Texture iconUndoInactive;
		Texture iconUndoActive;

		protected bool madeChanges = false;
		public bool MadeChanges {
			get { return madeChanges; }
		}
		protected string lastTooltip = "";
		public string LastTooltip {
			get { return lastTooltip; }
		}
		private string actionDescription = "";

		PickleTools.ValueTypes.DropOutStack<SequenceActionModel> undoStack = new PickleTools.ValueTypes.DropOutStack<SequenceActionModel>(50);

		protected string PREFS_UNSAVED_ACTION_DATA = "SequenceEditor.UnsavedActionData.ShowDialogue";
		private static readonly string PREFS_UNDO_LIST = "SequenceEditor.UndoList.ShowDialogue";

		static readonly string IMAGE_PATH = "PickleTools/Criterion/Images/";

		public virtual void Initialize (SequenceActionModel actionData, ActionLoader newActionLoader, 
		                                ConditionLoader newConditionLoader)
		{
			sequenceActionModel = actionData;
			actionLoader = newActionLoader;
			if(actionLoader == null) {
				actionLoader = new ActionLoader();
				actionLoader.Load();
			}
			try {
				actionDescription = actionLoader.GetAction(sequenceActionModel.UID).Description;
			} catch {
				actionDescription = "Could not find a description for this action.";
			}

			skin = AssetDatabase.LoadAssetAtPath<GUISkin>("");

			iconUndoInactive = AssetDatabase.LoadAssetAtPath<Texture>(IMAGE_PATH + "icon_undo_inactive.png");
			iconUndoActive = AssetDatabase.LoadAssetAtPath<Texture>(IMAGE_PATH + "icon_undo_active.png");
			
			// load up current unsaved action data
			string unsavedData = EditorPrefs.GetString(PREFS_UNSAVED_ACTION_DATA, "");
			SequenceActionModel savedData = LitJson.JsonMapper.ToObject<SequenceActionModel>(unsavedData);
			if(savedData != null && savedData.Parameters != null){
				sequenceActionModel = savedData;
			}
			
			// load up undo list
			string undoData = EditorPrefs.GetString(PREFS_UNDO_LIST, "");
			SequenceActionModel[] undoSaveList = LitJson.JsonMapper.ToObject<SequenceActionModel[]>(undoData);
			if(undoSaveList != null){
				for(int u = undoSaveList.Length - 1; u >= 0; u --){
					undoStack.Push(undoSaveList[u]);
				}
			}
			
			if(undoStack.Count == 0){
				madeChanges = false;
			} else {
				MadeChange();
			}
			GUI.FocusControl("");
		}

		public virtual void Deinitialize(){
			sequenceActionModel = null;
			undoStack.Clear();
			EditorPrefs.SetString(PREFS_UNSAVED_ACTION_DATA, "");
			EditorPrefs.SetString(PREFS_UNDO_LIST, "");
			SceneView.RepaintAll();
		}
		
		public virtual void RegisterUndo(SequenceActionModel data){
			SequenceActionModel duplicate = new SequenceActionModel(){
				UID = data.UID,
				Then = data.Then
			};
			duplicate.Parameters = new object[data.Parameters.Length];
			for(int p = 0; p < data.Parameters.Length; p ++){
				duplicate.Parameters[p] = data.Parameters[p];
			}
			undoStack.Push(duplicate);

			// save the undo data for reload on recompile/reopen
			SequenceActionModel[] undoArray = new SequenceActionModel[undoStack.Count];
			for(int u = 0; u < undoArray.Length; u ++){
				undoArray[u] = undoStack.Pop();
			}
			for(int u = undoArray.Length-1; u >= 0; u -- ){
				undoStack.Push(undoArray[u]);
			}
			EditorPrefs.SetString(PREFS_UNDO_LIST, LitJson.JsonMapper.ToJson(undoArray));
			MadeChange();
		}

		protected virtual void PerformUndo(){
			GUI.FocusControl("");
			SequenceActionModel undoData = undoStack.Pop();
			sequenceActionModel.UID = undoData.UID;
			if(sequenceActionModel.Parameters.Length != undoData.Parameters.Length){
				System.Array.Resize<object>(ref sequenceActionModel.Parameters, undoData.Parameters.Length);
			}
			for(int p = 0; p < undoData.Parameters.Length; p ++){
				sequenceActionModel.Parameters[p] = undoData.Parameters[p];
			}
			EditorPrefs.SetString(PREFS_UNSAVED_ACTION_DATA, LitJson.JsonMapper.ToJson(sequenceActionModel));

			if(UndoPerformed != null){
				UndoPerformed(sequenceActionModel);
			}

			Repaint();
		}

		private float boxSize = 40.0f;

		public virtual void DrawHeader(Rect drawSpace){
			
			// top menu
			GUILayout.BeginHorizontal(GUILayout.Width(drawSpace.width - 8), GUILayout.Height(94));

			GUILayout.BeginVertical();
			string actionName = "no_name";
			ActionModel actionModel = actionLoader.GetAction(sequenceActionModel.UID);
			if(actionModel != null){
				actionName = actionModel.Name;
			}
			GUILayout.Label(new GUIContent(actionName, actionDescription), skin.label);
			if(lastTooltip == ""){
				lastTooltip = "Mouse over an element name to get information about it.";
			}
			GUILayout.Label(new GUIContent(lastTooltip,
			                               "This is the info box. It displays useful information when you place your" +
			                               " cursor over things!"), 
			                skin.box, GUILayout.Height(64), GUILayout.Width(drawSpace.width - 8 - boxSize));
			GUILayout.EndVertical();

			GUILayout.FlexibleSpace();

			GUILayout.BeginVertical();

			if(GUILayout.Button(new GUIContent("x", "Press this button to delete this action from the map." +
				" Other actions will be sorted so that this action's parent will inherit its then actions."), 
			                    skin.button, GUILayout.Height(18), GUILayout.Width(boxSize - 4))){
				if(DeletePerformed != null){
					DeletePerformed();
				}
			}

			if(undoStack.Count > 0){
				if(GUILayout.Button(new GUIContent(iconUndoActive, "Press this button to undo the previous change."), skin.button, 
				                    GUILayout.Height(boxSize), GUILayout.Width(boxSize - 4))){
					PerformUndo();
				}
			} else {
				GUILayout.Box(new GUIContent(iconUndoInactive, "You can only undo after you have made changes!"), skin.button, 
				              GUILayout.Height(boxSize), GUILayout.Width(boxSize - 4));
			}

			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		public virtual void Draw(Rect drawSpace){
			
		}

		protected virtual void PostDraw(){
			if((Event.current.type == EventType.Repaint && GUI.tooltip != lastTooltip)) {
				lastTooltip = GUI.tooltip;
				if(lastTooltip == "") {
					lastTooltip = "Move the cursor over something to get information about it in this box.";
				}
			}
		}

		protected virtual void PerformPreviewAction(bool preview){
			if(PreviewMapAction != null){
				PreviewMapAction(sequenceActionModel, preview);
			}
		}

		protected void BeginParameter(GUIContent label) {
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(label, skin.label);
			EditorGUI.BeginChangeCheck();
		}



		protected void BeginParameter(int parameter){
			if(sequenceActionModel == null){
				return;
			}
			GUIContent titleContent = new GUIContent("Parameter " + parameter);
			ActionModel model = actionLoader.GetAction(sequenceActionModel.UID);
			if(model != null){
				titleContent.text = model.Parameters[parameter].Name;
				titleContent.tooltip = model.Parameters[parameter].Description;
			}
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(titleContent, skin.label);
			EditorGUI.BeginChangeCheck();
			                   
		}

		protected void EndParameter(int parameter, object value) {
			GUILayout.EndHorizontal();
			if(EditorGUI.EndChangeCheck()) {
				MadeChange();
				RegisterUndo(sequenceActionModel);
				sequenceActionModel.SetParameter(parameter, value);
				EditorPrefs.SetString(PREFS_UNSAVED_ACTION_DATA, LitJson.JsonMapper.ToJson(sequenceActionModel));
			}
		}

		protected void MadeChange(){
			madeChanges = true;
			if(ChangesMade != null){
				ChangesMade(sequenceActionModel);
			}
		}
	}
}
