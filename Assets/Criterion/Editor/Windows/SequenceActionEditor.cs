using UnityEngine;
using UnityEditor;

namespace PickleTools.Criterion {

	public class SequenceActionEditor : SequenceActionBaseEditor, ISequenceActionEditor {
		
		Vector2 scrollPosition = Vector2.zero;

		ActionLoader actionLoader;

		readonly string IMAGE_PATH = "PickleTools/Criterion/Images/";

		public override void Initialize(SequenceActionModel actionData, ActionLoader newActionLoader,
		                                ConditionLoader newConditionLoader){

			base.Initialize(actionData, newActionLoader, newConditionLoader);

			PREFS_UNSAVED_ACTION_DATA = "SequenceEditor.UnsavedActionData.MapActionEditor" + GetInstanceID();

			sequenceActionModel = actionData;

			actionLoader = newActionLoader;
			if(actionLoader == null) {
				actionLoader = new ActionLoader();
				actionLoader.Load();
			}
		}

		public override void Deinitialize(){
			sequenceActionModel = null;
		}

		public override void Draw(Rect drawSpace){
			if(sequenceActionModel == null){
				return;
			}

			if(skin == null){
				skin = AssetDatabase.LoadAssetAtPath<GUISkin>(IMAGE_PATH + "gui_skin.guiskin");
			}

			GUI.BeginGroup(drawSpace, skin.window);

			base.DrawHeader(drawSpace);

			scrollPosition = GUILayout.BeginScrollView(scrollPosition, skin.scrollView);

			EditorGUILayout.BeginVertical();
			ActionModel model = actionLoader.GetAction(sequenceActionModel.UID);
			if(model == null){
				
			} else {
				for(int p = 0; p < model.Parameters.Length; p ++){
					EditorGUI.BeginChangeCheck();
					object value = DrawValue.DrawValueField(sequenceActionModel.Parameters[p], model.Parameters[p].ValueType,
					                                        new int[1]{ 1000 + p }, new GUIContent(model.Parameters[p].Name, model.Parameters[p].Description),
						skin, GUILayout.Width(drawSpace.width - 8));
					
					if(EditorGUI.EndChangeCheck()){
						MadeChange();
						RegisterUndo(sequenceActionModel);
						if(ValueTypeLoader.IsBoolValue(model.Parameters[p].ValueType)){
							bool boolValue = false;
							bool.TryParse(value.ToString(), out boolValue);
							sequenceActionModel.SetParameter(p, boolValue);
						} else if (ValueTypeLoader.IsFloatValue(model.Parameters[p].ValueType)){
							float floatValue = 0.0f;
							float.TryParse(value.ToString(), out floatValue);
							sequenceActionModel.SetParameter(p, floatValue);
						} else {
							sequenceActionModel.SetParameter(p, value.ToString());
						}
						EditorPrefs.SetString(PREFS_UNSAVED_ACTION_DATA, LitJson.JsonMapper.ToJson(sequenceActionModel));
					}

				}
			}

			GUILayout.EndVertical();
			
			GUILayout.EndScrollView();

			GUI.EndGroup();

			base.PostDraw();
		}
	}
}
