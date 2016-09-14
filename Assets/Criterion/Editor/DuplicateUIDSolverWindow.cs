using UnityEngine;
using UnityEditor;
using System.Collections;
using PickleTools.Criterion;
using System.Collections.Generic;
using PickleTools.Criterion.ActionLookup;

public class DuplicateUIDSolverWindow : EditorWindow {
	
	GUISkin skin;

	DuplicateUIDSolver solver;

	Vector2 selectionScroll = Vector2.zero;
	bool confirmAssignment = false;
	bool introScreen = true;
	Texture imageBackground;

	const string GUI_SKIN_PATH = "Assets/PickleTools/Criterion/Images/gui_skin.guiskin";
	const string IMAGE_BACKGROUND_PATH = "Assets/PickleTools/Criterion/Images/background.png";

	[MenuItem("Criterion/Tools/Duplicate UID Solver")]
	public static void ShowEditor(){
		DuplicateUIDSolverWindow window = GetWindow<DuplicateUIDSolverWindow>();
		window.titleContent = new GUIContent("UID Solver");
		window.Initialize();
		if(window.requireUserInput) {
			window.Show();
		} else {
			window.Close();
		}
	}

	TriggerModel[] triggerModels = new TriggerModel[0];
	ConditionModel[] conditionModels = new ConditionModel[0];
	SequenceModel[] sequenceModels = new SequenceModel[0];
	ActionModel[] actionModels = new ActionModel[0];
	TagModel[] tagModels = new TagModel[0];


	void Scenario1(){
		// a trigger 
		// is using a 
		// duplicate condition
		triggerModels = new TriggerModel[1];
		triggerModels[0] = new TriggerModel();
		triggerModels[0].Name = "_duplicate_trigger_using_duplicate_condition";
		triggerModels[0].UID = 0;
		TriggerConditionModel condition = new TriggerConditionModel();
		condition.UID = 0;
		condition.LowerBound = 0;
		condition.UpperBound = 0;
		triggerModels[0].Conditions = new TriggerConditionModel[1] { condition };

		conditionModels = new ConditionModel[2];
		conditionModels[0] = new ConditionModel();
		conditionModels[0].Name = "_duplicate_condition_01";
		conditionModels[0].UID = 0;
		conditionModels[1] = new ConditionModel();
		conditionModels[1].Name = "_duplicate_condition_02";
		conditionModels[1].UID = 0;
	}

	void Scenario2(){
		// two conditions
		// and two actions
		// are using a
		// duplicate trigger and duplicate sequence
		triggerModels = new TriggerModel[2];
		triggerModels[0] = new TriggerModel();
		triggerModels[0].Name = "_duplicate_trigger_00";
		triggerModels[0].UID = 0;
		TriggerConditionModel condition = new TriggerConditionModel();
		condition.UID = 0;
		condition.LowerBound = 0;
		condition.UpperBound = 0;
		triggerModels[0].Conditions = new TriggerConditionModel[1] { condition };
		triggerModels[1] = new TriggerModel();
		triggerModels[1].Name = "_duplicate_trigger_01";
		triggerModels[1].UID = 0;
		condition.UID = 99;
		condition.LowerBound = 99;
		condition.UpperBound = 99;
		triggerModels[1].Conditions = new TriggerConditionModel[1] { condition };

		sequenceModels = new SequenceModel[2];
		sequenceModels[0] = new SequenceModel();
		sequenceModels[0].Name = "_duplicate_sequence_00";
		sequenceModels[0].UID = 0;
		SequenceActionModel action = new SequenceActionModel();
		action.UID = 0;
		sequenceModels[0].Actions = new SequenceActionModel[1] { action };
		sequenceModels[1] = new SequenceModel();
		sequenceModels[1].Name = "_duplicate_sequence_01";
		sequenceModels[1].UID = 0;
		action.UID = 99;
		sequenceModels[1].Actions = new SequenceActionModel[1] { action };
			
	}

	void Scenario4(){
		// a trigger
		triggerModels = new TriggerModel[1];
		triggerModels[0] = new TriggerModel();
		triggerModels[0].Name = "_trigger_00";
		triggerModels[0].UID = 0;
		TriggerConditionModel condition = new TriggerConditionModel();
		condition.UID = 0;  // condition using object
		condition.LowerBound = 0; // the duplicate object uid
		condition.UpperBound = 0;
		triggerModels[0].Conditions = new TriggerConditionModel[1] { condition };
		// is using a
		// duplicate condition

		conditionModels = new ConditionModel[2];
		conditionModels[0] = new ConditionModel();
		conditionModels[0].Name = "_condition_using_object_00";
		conditionModels[0].UID = 0;
		conditionModels[0].ValueUID = (int)ValueTypeLoader.ValueType.MAZER_OBJECT;
		conditionModels[1] = new ConditionModel();
		conditionModels[1].Name = "_condition_using_object_01";
		conditionModels[1].UID = 0;
		conditionModels[1].ValueUID = (int)ValueTypeLoader.ValueType.MAZER_OBJECT;

	}

	void Scenario5(){
		// a sequence
		sequenceModels = new SequenceModel[1];
		sequenceModels[0] = new SequenceModel();
		sequenceModels[0].Name = "_sequence_00";
		sequenceModels[0].UID = 0;
		SequenceActionModel sequenceAction = new SequenceActionModel();
		sequenceAction.UID = 0; // action using object
		sequenceAction.Parameters = new object[1] { 0 }; // the duplicate object uid
		sequenceModels[0].Actions = new SequenceActionModel[1] { sequenceAction };

		// is using a
		// duplicate action

		actionModels = new ActionModel[2];
		actionModels[0] = new ActionModel();
		actionModels[0].Name = "_action_using_object_00";
		actionModels[0].UID = 0;
		ActionParameterModel actionParam = new ActionParameterModel();
		actionParam.Name = "_object_using_param";
		actionParam.ValueType = (int)ValueTypeLoader.ValueType.MAZER_OBJECT;
		actionModels[0].Parameters = new ActionParameterModel[1] { actionParam };
		actionModels[1] = new ActionModel();
		actionModels[1].Name = "_action_using_object_01";
		actionModels[1].UID = 0;
		actionParam = new ActionParameterModel();
		actionParam.Name = "_object_using_param";
		actionParam.ValueType = (int)ValueTypeLoader.ValueType.MAZER_OBJECT;
		actionModels[1].Parameters = new ActionParameterModel[1] { actionParam };

	}

	void LoadExternalData(){
		// triggers
		CriterionDataLoader<TriggerModel> triggerLoader = new CriterionDataLoader<TriggerModel>("triggers", "triggers");
		triggerLoader.Load("", false);
		triggerModels = triggerLoader.Models;
		// conditions
		CriterionDataLoader<ConditionModel> conditionLoader = new CriterionDataLoader<ConditionModel>("conditions", "conditions");
		conditionLoader.Load("", false);
		conditionModels = conditionLoader.Models;
		// sequences
		CriterionDataLoader<SequenceModel> sequenceLoader = new CriterionDataLoader<SequenceModel>("sequences", "sequences");
		sequenceLoader.Load("", false);
		sequenceModels = sequenceLoader.Models;
		// actions
		CriterionDataLoader<ActionModel> actionLoader = new CriterionDataLoader<ActionModel>("actions", "actions");
		actionLoader.Load("", false);
		actionModels = actionLoader.Models;
		// tags
		CriterionDataLoader<TagModel> tagLoader = new CriterionDataLoader<TagModel>("tags", "tags");
		tagLoader.Load("", false);
		tagModels = tagLoader.Models;
	}

	void SaveExternalData(){

		PickleTools.FileAccess.IFileSaver fileSaver = new PickleTools.FileAccess.EditorFileSaver("");
		// save the changes to the data
		// objects are already saved because we access the prefabs directly!
		// triggers
		CriterionDataLoader<TriggerModel> triggerLoader = new CriterionDataLoader<TriggerModel>("triggers", "triggers");
		triggerLoader.Load(triggerModels);
		triggerLoader.Save(fileSaver);
		// conditions
		CriterionDataLoader<ConditionModel> conditionLoader = new CriterionDataLoader<ConditionModel>("conditions", "conditions");
		conditionLoader.Load(conditionModels);
		conditionLoader.Save(fileSaver);
		// sequences
		CriterionDataLoader<SequenceModel> sequenceLoader = new CriterionDataLoader<SequenceModel>("sequences", "sequences");
		sequenceLoader.Load(sequenceModels);
		sequenceLoader.Save(fileSaver);
		// actions
		CriterionDataLoader<ActionModel> actionLoader = new CriterionDataLoader<ActionModel>("actions", "actions");
		actionLoader.Load(actionModels);
		actionLoader.Save(fileSaver);
		// tags
		CriterionDataLoader<TagModel> tagLoader = new CriterionDataLoader<TagModel>("tags", "tags");
		tagLoader.Load(tagModels);
		tagLoader.Save(fileSaver);

	}

	void Initialize(){


		skin = AssetDatabase.LoadAssetAtPath<GUISkin>(GUI_SKIN_PATH);
		imageBackground = AssetDatabase.LoadAssetAtPath<Texture>(IMAGE_BACKGROUND_PATH);

		solver = new DuplicateUIDSolver();

		// test data
		triggerModels = new TriggerModel[0];
		conditionModels = new ConditionModel[0];
		sequenceModels = new SequenceModel[0];
		actionModels = new ActionModel[0];
		tagModels = new TagModel[0];

		// Use for testing
		//Scenario9();
		//DebugData();
		// Use with actual data
		LoadExternalData();

		GetDuplicates();

	}

	Stack<int> solveStack = new Stack<int>();
	int currentUIDToSolve = -1;

	Dictionary<int, List<ICriterionData>> solvedModels = new Dictionary<int, List<ICriterionData>>();

	DuplicateUIDSolver.DuplicateContainer[] triggerUsings = new DuplicateUIDSolver.DuplicateContainer[0];
	DuplicateUIDSolver.DuplicateContainer[] conditionUsings = new DuplicateUIDSolver.DuplicateContainer[0];
	DuplicateUIDSolver.DuplicateContainer[] sequenceUsings = new DuplicateUIDSolver.DuplicateContainer[0];
	DuplicateUIDSolver.DuplicateContainer[] actionUsings = new DuplicateUIDSolver.DuplicateContainer[0];
	DuplicateUIDSolver.DuplicateContainer[] itemUsings = new DuplicateUIDSolver.DuplicateContainer[0];
	DuplicateUIDSolver.DuplicateContainer[] levelUsings = new DuplicateUIDSolver.DuplicateContainer[0];
	DuplicateUIDSolver.DuplicateContainer[] patternUsings = new DuplicateUIDSolver.DuplicateContainer[0];
	DuplicateUIDSolver.DuplicateContainer[] propertyUsings = new DuplicateUIDSolver.DuplicateContainer[0];
	DuplicateUIDSolver.DuplicateContainer[] tagUsings = new DuplicateUIDSolver.DuplicateContainer[0];

	string duplicateType = "";
	bool requireUserInput = false;

	const string TYPE_OBJECT = "TYPE_OBJECT";
	const string TYPE_TRIGGER = "TYPE_TRIGGER";
	const string TYPE_CONDITION = "TYPE_CONDITION";
	const string TYPE_SEQUENCE = "TYPE_SEQUENCE";
	const string TYPE_ACTION = "TYPE_ACTION";
	const string TYPE_ITEM = "TYPE_ITEM";
	const string TYPE_LEVEL = "TYPE_LEVEL";
	const string TYPE_PATTERN = "TYPE_PATTERN";
	const string TYPE_PROPERTY = "TYPE_PROPERTY";
	const string TYPE_TAG = "TYPE_TAG";

	// TODO: Add item property

	void GetDuplicates(){
		ICriterionData[] duplicates = new ICriterionData[0];
		duplicateType = "";
		// check for each type of duplicate

		// order is very important,
		// we can't check for duplicates of something until all
		// data that uses it has been sorted
		// this is because if we solve for a data type,
		// then anything that references that duplicate will no longer find duplicates,
		// because they have already been solved


		// sequences
		if(duplicates.Length == 0) {
			duplicates = solver.GetDuplicateData(sequenceModels);
			if(duplicates.Length > 0) {
				GetDuplicates(TYPE_SEQUENCE, sequenceModels);
			}
		}
		// triggers
		if(duplicates.Length == 0) {
			duplicates = solver.GetDuplicateData(triggerModels);
			if(duplicates.Length > 0) {
				GetDuplicates(TYPE_TRIGGER, triggerModels);
			}
		}
		// conditions
		if(duplicates.Length == 0) {
			duplicates = solver.GetDuplicateData(conditionModels);
			if(duplicates.Length > 0) {
				GetDuplicates(TYPE_CONDITION, conditionModels);
			}
		}
		// actions
		if(duplicates.Length == 0) {
			duplicates = solver.GetDuplicateData(actionModels);
			if(duplicates.Length > 0) {
				GetDuplicates(TYPE_ACTION, actionModels);
			}
		}
		// tags
		if(duplicates.Length == 0) {
			duplicates = solver.GetDuplicateData(tagModels);
			if(duplicates.Length > 0) {
				GetDuplicates(TYPE_PROPERTY, tagModels);
			}
		}

		// if we have any duplicates that require the user's attention,
		// then we wait till the user gives input
		requireUserInput = GetUsesOfDuplicate();
	}


	void GetDuplicates(string type, ICriterionData[] models) {
		duplicateType = type;
		// get highest UID
		int highestUID = 0;
		for(int i = 0; i < models.Length; i++) {
			if(models[i] == null){
				continue;
			}
			if(models[i].UID > highestUID) {
				highestUID = models[i].UID;
			}
		}
		// fix duplicate UIDs
		solvedModels = solver.SolveDuplicateDataUID(models, highestUID);
		// sort therough the solved UIDs so we can properly assign data that references these UIDs
		foreach(int key in solvedModels.Keys) {
			solveStack.Push(key);
		}
	}

	bool GetUsesOfDuplicate(){
		if(solveStack.Count > 0) {
			currentUIDToSolve = solveStack.Pop();
			triggerUsings = new DuplicateUIDSolver.DuplicateContainer[0];
			conditionUsings = new DuplicateUIDSolver.DuplicateContainer[0];
			sequenceUsings = new DuplicateUIDSolver.DuplicateContainer[0];
			actionUsings = new DuplicateUIDSolver.DuplicateContainer[0];
			itemUsings = new DuplicateUIDSolver.DuplicateContainer[0];
			levelUsings = new DuplicateUIDSolver.DuplicateContainer[0];
			patternUsings = new DuplicateUIDSolver.DuplicateContainer[0];
			propertyUsings = new DuplicateUIDSolver.DuplicateContainer[0];
			tagUsings = new DuplicateUIDSolver.DuplicateContainer[0];
			// create an entry for each type of using the duplicate
			switch(duplicateType){
				case TYPE_CONDITION:
					triggerUsings = solver.FindUsesOfDuplicateConditionsInTriggers(triggerModels, currentUIDToSolve);
					sequenceUsings = solver.FindUsesOfDuplicateConditionsInSequences(sequenceModels, actionModels,
					                                                               currentUIDToSolve,
					                                                               (int)ActionType.UPDATE_WORLD_STATE,
					                                                              	0);
					break;
				case TYPE_SEQUENCE:
					// if it's a sequence, we just update the sequence uids
					break;
				case TYPE_OBJECT:
					triggerUsings = solver.FindUsesOfDuplicateValuesInTriggers(
						triggerModels, conditionModels, currentUIDToSolve, (int)ValueTypeLoader.ValueType.MAZER_OBJECT);
					sequenceUsings = solver.FindUsesOfDuplicateValuesInSequences(
						sequenceModels, actionModels, currentUIDToSolve, (int)ValueTypeLoader.ValueType.MAZER_OBJECT);
					break;
				case TYPE_ACTION:
					sequenceUsings = solver.FindUsesOfDuplicateActionsInSequences(sequenceModels, currentUIDToSolve);
					break;
				case TYPE_PATTERN:
					triggerUsings = solver.FindUsesOfDuplicateValuesInTriggers(
						triggerModels, conditionModels, currentUIDToSolve, (int)ValueTypeLoader.ValueType.PATTERN);
					sequenceUsings = solver.FindUsesOfDuplicateValuesInSequences(
						sequenceModels, actionModels, currentUIDToSolve, (int)ValueTypeLoader.ValueType.PATTERN);
					break;
				case TYPE_LEVEL:
					triggerUsings = solver.FindUsesOfDuplicateValuesInTriggers(
						triggerModels, conditionModels, currentUIDToSolve, (int)ValueTypeLoader.ValueType.LEVEL);
					sequenceUsings = solver.FindUsesOfDuplicateValuesInSequences(
						sequenceModels, actionModels, currentUIDToSolve, (int)ValueTypeLoader.ValueType.LEVEL);
					break;
				case TYPE_TRIGGER:
					triggerUsings = solver.FindUsesOfDuplicateValuesInTriggers(
						triggerModels, conditionModels, currentUIDToSolve, (int)ValueTypeLoader.ValueType.TRIGGER);
					sequenceUsings = solver.FindUsesOfDuplicateValuesInSequences(
						sequenceModels, actionModels, currentUIDToSolve, (int)ValueTypeLoader.ValueType.TRIGGER);
					break;
				case TYPE_ITEM:
					triggerUsings = solver.FindUsesOfDuplicateValuesInTriggers(
						triggerModels, conditionModels, currentUIDToSolve, (int)ValueTypeLoader.ValueType.ITEM);
					sequenceUsings = solver.FindUsesOfDuplicateValuesInSequences(
						sequenceModels, actionModels, currentUIDToSolve, (int)ValueTypeLoader.ValueType.ITEM);
					break;
				case TYPE_TAG:
					tagUsings = solver.FindUsesOfDuplicateTagsInConditions(conditionModels, currentUIDToSolve);
					break;
				default:
					Debug.LogWarning("Need a way to find dupliactes for: " + duplicateType);
					return false;
			}
			//Debug.LogWarning("trigger:" + triggerUsings.Length + ", " +
			//				 "condition:" + conditionUsings.Length + ", " +
			//				 "sequence:" + sequenceUsings.Length + ", " +
			//				 "action:" + actionUsings.Length + ", " +
			//				 "item:" + itemUsings.Length + ", " +
			//				 "level:" + levelUsings.Length + ", " +
			//				 "pattern:" + patternUsings.Length + ", " +
			//				 "property:" + propertyUsings.Length + ", " +
			//				 "tag:" + tagUsings.Length + ", ");
			return true;
		}
		return false;
	}

	ICriterionData GetDataModel(int uid, ICriterionData[] models){
		ICriterionData model = null;
		for(int m = 0; m < models.Length; m ++){
			if(models[m].UID == uid){
				model = models[m];
				break;
			}
		}
		return model;
	}

	void OnGUI(){
		GUI.DrawTextureWithTexCoords(new Rect(0, 0, position.width, position.height), imageBackground,
		                             new Rect(0, 0, position.width, position.height));
		if(!requireUserInput){
			GUILayout.Label("No duplicates found. All clear!");
			if(GUILayout.Button("Close",skin.button)){
				Close();
			}
		} else if(introScreen) {
			GUILayout.Label("WARNING: PLEASE READ THIS\n" +
							"We have found duplicate data! This happens when you and another person make new" +
							" content at the same time. It's okay! It's just that I need your help figuring out" +
							" which data should be used for these duplicates. \n\n" +
							"In the following screens I'll show you all the duplicates I've found." +
							" If there is any content that referneces these duplicates, you'll have to pick" +
							" which of the duplicates are supposed to be used by the data.\n\n" +
			                "Let's get started!", skin.box, GUILayout.Width(position.width - 12));
			if(GUILayout.Button("I have read the above statement.", skin.button)){
				introScreen = false;
			}

			EditData();
		} else {
			string dataTypeString = "none";
			switch(duplicateType){
				case TYPE_TAG:
					dataTypeString = "tags";
					break;
				case TYPE_ITEM:
					dataTypeString = "items";
					break;
				case TYPE_LEVEL:
					dataTypeString = "levels";
					break;
				case TYPE_OBJECT:
					dataTypeString = "objects";
					break;
				case TYPE_ACTION:
					dataTypeString = "actions";
					break;
				case TYPE_TRIGGER:
					dataTypeString = "triggers";
					break;
				case TYPE_PATTERN:
					dataTypeString = "patterns";
					break;
				case TYPE_PROPERTY:
					dataTypeString = "properties";
					break;
				case TYPE_SEQUENCE:
					dataTypeString = "sequences";
					break;
				case TYPE_CONDITION:
					dataTypeString = "conditions";
					break;
			}
			GUILayout.Space(4);
			GUILayout.Label("We have found the following duplicate " + dataTypeString + ".", skin.label);

			GUILayout.BeginVertical(skin.box, GUILayout.Width(position.width - 12));
			if(solvedModels.ContainsKey(currentUIDToSolve)) {
				for(int d = 0; d < solvedModels[currentUIDToSolve].Count; d++) {
					GUILayout.Label(currentUIDToSolve + ": " +
									solvedModels[currentUIDToSolve][d].Name + " -> " +
									solvedModels[currentUIDToSolve][d].UID, skin.label);
				}
			}
			GUILayout.EndVertical();

			GUILayout.Space(10);

			bool foundReference = (triggerUsings.Length > 0 || conditionUsings.Length > 0 || sequenceUsings.Length > 0 ||
			   						actionUsings.Length > 0 || itemUsings.Length > 0 || levelUsings.Length > 0 ||
								   patternUsings.Length > 0 || propertyUsings.Length > 0 || tagUsings.Length > 0);
			if(foundReference) {
				// list duplicates that all share the same original UID
				GUILayout.Label("And we found other data references these duplicates.\n" +
				                "Please select which " + dataTypeString + " this data is supposed to use.\n" +
				                "Contact your team members if you do know know.", skin.label, GUILayout.Width(position.width - 12));


				selectionScroll = GUILayout.BeginScrollView(selectionScroll, false, true);

				// allow the user to select which solved duplicates should be 
				// used for each data that referred to the duplicates
				if(triggerUsings.Length > 0) {
					GUILayout.Label("Triggers:", skin.label);
					GUILayout.BeginVertical(skin.box, GUILayout.Width(position.width - 24));
					for(int u = 0; u < triggerUsings.Length; u++) {
						DrawUsingSelection(triggerUsings[u]);
					}
					GUILayout.EndVertical();
				}
				if(conditionUsings.Length > 0) {
					GUILayout.Label("Conditions:", skin.label);
					GUILayout.BeginVertical(skin.box, GUILayout.Width(position.width - 24));
					for(int u = 0; u < conditionUsings.Length; u++) {
						DrawUsingSelection(conditionUsings[u]);
					}
					GUILayout.EndVertical();
				}
				if(sequenceUsings.Length > 0) {
					GUILayout.Label("Sequences:", skin.label);
					GUILayout.BeginVertical(skin.box, GUILayout.Width(position.width - 24));
					for(int u = 0; u < sequenceUsings.Length; u++) {
						DrawUsingSelection(sequenceUsings[u]);
					}
					GUILayout.EndVertical();
				}
				if(actionUsings.Length > 0) {
					GUILayout.Label("Actions:", skin.label);
					GUILayout.BeginVertical(skin.box, GUILayout.Width(position.width - 24));
					for(int u = 0; u < actionUsings.Length; u++) {
						DrawUsingSelection(actionUsings[u]);
					}
					GUILayout.EndVertical();
				}
				if(itemUsings.Length > 0) {
					GUILayout.Label("Items:", skin.label);
					GUILayout.BeginVertical(skin.box, GUILayout.Width(position.width - 24));
					for(int u = 0; u < itemUsings.Length; u++) {
						DrawUsingSelection(itemUsings[u]);
					}
					GUILayout.EndVertical();
				}
				if(levelUsings.Length > 0) {
					GUILayout.Label("Levels:", skin.label);
					GUILayout.BeginVertical(skin.box, GUILayout.Width(position.width - 24));
					for(int u = 0; u < levelUsings.Length; u++) {
						DrawUsingSelection(levelUsings[u]);
					}
					GUILayout.EndVertical();
				}
				if(patternUsings.Length > 0) {
					GUILayout.Label("Patterns:", skin.label);
					GUILayout.BeginVertical(skin.box, GUILayout.Width(position.width - 24));
					for(int u = 0; u < patternUsings.Length; u++) {
						DrawUsingSelection(patternUsings[u]);
					}
					GUILayout.EndVertical();
				}
				if(propertyUsings.Length > 0) {
					GUILayout.Label("Properties:", skin.label);
					GUILayout.BeginVertical(skin.box, GUILayout.Width(position.width - 24));
					for(int u = 0; u < propertyUsings.Length; u++) {
						DrawUsingSelection(propertyUsings[u]);
					}
					GUILayout.EndVertical();
				}
				if(tagUsings.Length > 0) {
					GUILayout.Label("Tags:", skin.label);
					GUILayout.BeginVertical(skin.box, GUILayout.Width(position.width - 24));
					for(int u = 0; u < tagUsings.Length; u++) {
						DrawUsingSelection(tagUsings[u]);
					}
					GUILayout.EndVertical();
				}

				GUILayout.EndScrollView();
			}

			GUILayout.Space(4);

			if(foundReference && GUILayout.Button("Confirm Assignment", skin.button, GUILayout.Height(40))){
				confirmAssignment = true;
			} else if (!foundReference && GUILayout.Button("Continue", skin.button, GUILayout.Height(40))){
				confirmAssignment = true;
			}

			GUILayout.Space(4);

			EditData();
		}
	}


	void DrawUsingSelection(DuplicateUIDSolver.DuplicateContainer usingData){
		int selectedDuplicate = 0;
		GUILayout.Label(usingData.Name, skin.label);
		string[] selectionNames = new string[solvedModels[currentUIDToSolve].Count];
		for(int s = 0; s < selectionNames.Length; s++) {
			selectionNames[s] = solvedModels[currentUIDToSolve][s].Name;
			if(usingData.UpdatedUID == solvedModels[currentUIDToSolve][s].UID) {
				selectedDuplicate = s;
			}
		}
		EditorGUI.BeginChangeCheck();
		selectedDuplicate = GUILayout.Toolbar(selectedDuplicate, selectionNames);
		if(EditorGUI.EndChangeCheck() || usingData.UpdatedUID < 0) {
			usingData.UpdatedUID = solvedModels[currentUIDToSolve][selectedDuplicate].UID;
		}
	}

	void EditUsings(DuplicateUIDSolver.DuplicateContainer[] usings, ICriterionData[] models){
		
	}

	void EditData(){
		if(confirmAssignment){
			// solve with usings 
			switch(duplicateType){
				case TYPE_CONDITION:
					triggerModels = solver.SolveDuplicateConditionUIDsInTriggers(triggerUsings, triggerModels);
					sequenceModels = solver.SolveDuplicateActionValuesInSequences(sequenceUsings, sequenceModels);
					break;
				case TYPE_SEQUENCE:
					// we have no data to edit when a sequence is duplicated, because nothing 
					// references sequences directly
					break;
				case TYPE_OBJECT:
					triggerModels = solver.SolveDuplicateUIDValuesInTriggers(triggerUsings, triggerModels);
					sequenceModels = solver.SolveDuplicateActionValuesInSequences(sequenceUsings, sequenceModels);
					break;
				case TYPE_ACTION:
					sequenceModels = solver.SolveDuplicateActionsInSequences(sequenceUsings, sequenceModels);
					break;
				case TYPE_PATTERN:
					triggerModels = solver.SolveDuplicateUIDValuesInTriggers(triggerUsings, triggerModels);
					sequenceModels = solver.SolveDuplicateActionValuesInSequences(sequenceUsings, sequenceModels);
					break;
				case TYPE_LEVEL:
					triggerModels = solver.SolveDuplicateUIDValuesInTriggers(triggerUsings, triggerModels);
					sequenceModels = solver.SolveDuplicateActionValuesInSequences(sequenceUsings, sequenceModels);
					break;
				case TYPE_TRIGGER:
					triggerModels = solver.SolveDuplicateUIDValuesInTriggers(triggerUsings, triggerModels);
					sequenceModels = solver.SolveDuplicateActionValuesInSequences(sequenceUsings, sequenceModels);
					triggerModels = solver.SolveDuplicateConditionUIDsInTriggers(conditionUsings, triggerModels);
					break;
				case TYPE_ITEM:
					triggerModels = solver.SolveDuplicateUIDValuesInTriggers(triggerUsings, triggerModels);
					sequenceModels = solver.SolveDuplicateActionValuesInSequences(sequenceUsings, sequenceModels);
					break;
				case TYPE_PROPERTY:
					break;
				case TYPE_TAG:
					conditionModels = solver.SolveDuplicateTagsInConditions(tagUsings, conditionModels);
					break;
				default:
					if(duplicateType != "") {
						Debug.LogWarning("Need a way to find duplicates for: " + duplicateType);
					}
					break;
			}

			if(solveStack.Count <= 0) {
				GetDuplicates();
			} else {
				GetUsesOfDuplicate();
			}
			if(!requireUserInput) {
				// use for testing
				//DebugData();
				//// destroy fake data objects
				//for(int i = objectModels.Length - 1; i >= 0; i --){
				//	GameObject.DestroyImmediate(objectModels[i].gameObject);
				//}

				// use with actual data
				if(confirmAssignment) {
					SaveExternalData();
				}

				Close();
			}

			confirmAssignment = false;
		}
	}

	void DebugData(){
		for(int i = 0; i < triggerModels.Length; i++) {
			if(triggerModels[i] == null){
				continue;
			}
			Debug.LogWarning(triggerModels[i].ToString());
		}
		for(int i = 0; i < conditionModels.Length; i++) {
			if(conditionModels[i] == null) {
				continue;
			}
			Debug.LogWarning(conditionModels[i].ToString());
		}
		for(int i = 0; i < sequenceModels.Length; i++) {
			if(sequenceModels[i] == null) {
				continue;
			}
			Debug.LogWarning(sequenceModels[i].ToString());
		}
		for(int i = 0; i < actionModels.Length; i++) {
			if(actionModels[i] == null) {
				continue;
			}
			Debug.LogWarning(actionModels[i].ToString());
		}
		for(int i = 0; i < tagModels.Length; i++) {
			if(tagModels[i] == null) {
				continue;
			}
			Debug.LogWarning(tagModels[i].ToString());
		}
	}
}
