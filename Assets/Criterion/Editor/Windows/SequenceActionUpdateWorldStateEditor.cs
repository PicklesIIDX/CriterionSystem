using UnityEngine;
using UnityEditor;
using System.Collections;
using PickleTools.Criterion;
using PickleTools.Criterion.ConditionLookup;

public class SequenceActionUpdateWorldStateEditor : SequenceActionBaseEditor, ISequenceActionEditor {

	WorldStateData worldStateData;
	ConditionSelectMenu conditionSelectMenu;
	Vector2 scrollPosition = Vector2.zero;
	ConditionLoader conditionLoader;

	public override void Initialize (SequenceActionModel actionData, ActionLoader newActionLoader, 
	                                 ConditionLoader newConditionLoader)
	{
		base.Initialize (actionData, newActionLoader, newConditionLoader);

		float floatValue = 0;
		float.TryParse(sequenceActionModel.GetParameter(2).ToString(), out floatValue);
		bool toggleBool = false;
		bool.TryParse(sequenceActionModel.GetParameter(3).ToString(), out toggleBool);
		bool incrementBool = true;
		bool.TryParse(sequenceActionModel.GetParameter(4).ToString(), out incrementBool);
		int uid = 0;
		int.TryParse(sequenceActionModel.GetParameter(0).ToString(), out uid);

		worldStateData = new WorldStateData(uid,
												sequenceActionModel.GetParameter(1),
		                                        floatValue, toggleBool, incrementBool);
		conditionSelectMenu = new ConditionSelectMenu();
		conditionSelectMenu.LastEntrySelected = worldStateData.ConditionUID;
		conditionSelectMenu.EntrySelected += HandleEntrySelected;
		conditionLoader = newConditionLoader;
	}

	void HandleEntrySelected (ConditionSelectMenu menu, int item)
	{
		MadeChange();
		RegisterUndo(sequenceActionModel);
		worldStateData.ConditionUID = item;
		sequenceActionModel.SetParameter(0, worldStateData.ConditionUID);
		sequenceActionModel.SetParameter(1, worldStateData.Value);
		sequenceActionModel.SetParameter(2, worldStateData.Expiration);
		sequenceActionModel.SetParameter(3, worldStateData.ToggleBool);
		sequenceActionModel.SetParameter(4, worldStateData.IncrementNumber);
	}

	public override void Deinitialize ()
	{
		base.Deinitialize ();
		conditionSelectMenu.EntrySelected -= HandleEntrySelected;
	}

	public override void Draw(Rect drawSpace){

		if(sequenceActionModel == null){
			return;
		}
		GUI.BeginGroup(drawSpace, skin.window);

		scrollPosition = GUILayout.BeginScrollView(scrollPosition, skin.scrollView);

		base.DrawHeader(drawSpace);

		GUILayout.BeginVertical();
		// draw the world state selection box
		WorldStateData previousWriteback = new WorldStateData(
			worldStateData.ConditionUID, worldStateData.Value.ToString(), worldStateData.Expiration,
			worldStateData.ToggleBool, worldStateData.IncrementNumber);
		GUILayout.BeginHorizontal();
		GUILayout.Label(new GUIContent("World State Entry", "This is the World State Entry to edit. Click to select from a " +
										"list of all variables created in the Fact Builder."), skin.label, 
		                GUILayout.Width(drawSpace.width * 0.67f));
		if(GUILayout.Button(new GUIContent("Edit Condition", "Edits this condition in the Condition Builder."), 
							skin.button,GUILayout.Width(drawSpace.width * 0.3f))){
			ConditionBuilderWindow window = EditorWindow.GetWindow<ConditionBuilderWindow>();
			int showConditionType = (int)ConditionType.NONE;
			try {
				showConditionType = worldStateData.ConditionUID;
			} catch { }
			window.SelectCondition(showConditionType);
		}
		GUILayout.EndHorizontal();
		worldStateData = DrawMemory.Draw(worldStateData, conditionLoader,  skin, conditionSelectMenu, drawSpace.width - 16);
		if(!worldStateData.Value.Equals(sequenceActionModel.GetParameter(1))){
			MadeChange();
			RegisterUndo(sequenceActionModel);
			sequenceActionModel.SetParameter(1, worldStateData.Value.ToString());
		}
		if(worldStateData.Expiration != previousWriteback.Expiration){
			MadeChange();
			RegisterUndo(sequenceActionModel);
			sequenceActionModel.SetParameter(2, worldStateData.Expiration);
		}
		if(worldStateData.ToggleBool != previousWriteback.ToggleBool){
			MadeChange();
			RegisterUndo(sequenceActionModel);
			sequenceActionModel.SetParameter(3, worldStateData.ToggleBool);
		}
		if(worldStateData.IncrementNumber != previousWriteback.IncrementNumber){
			MadeChange();
			RegisterUndo(sequenceActionModel);
			sequenceActionModel.SetParameter(4, worldStateData.IncrementNumber); 
		}
		
		GUILayout.Space(40);
		GUILayout.EndVertical();
		
		GUILayout.EndScrollView();

		GUI.EndGroup();

		base.PostDraw();
	}

	protected override void PerformUndo ()
	{
		base.PerformUndo ();
		int uid = 0;
		int.TryParse(sequenceActionModel.GetParameter(0).ToString(), out uid);
		worldStateData.ConditionUID = uid;
		worldStateData.Value = sequenceActionModel.GetParameter(1).ToString();
		float floatValue = 0;
		float.TryParse(sequenceActionModel.GetParameter(2).ToString(), out floatValue);
		worldStateData.Expiration = floatValue;
		bool boolToggle = false;
		bool.TryParse(sequenceActionModel.GetParameter(3).ToString(), out boolToggle);
		worldStateData.ToggleBool = boolToggle;
		bool boolIncrement = true;
		bool.TryParse(sequenceActionModel.GetParameter(4).ToString(), out boolIncrement);
		worldStateData.IncrementNumber = boolIncrement;

		conditionSelectMenu.LastEntrySelected = worldStateData.ConditionUID;
	}
}
