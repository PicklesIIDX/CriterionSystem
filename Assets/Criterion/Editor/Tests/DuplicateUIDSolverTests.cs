using NUnit.Framework;
using PickleTools.Criterion;
using System.Collections.Generic;

[TestFixture]
public class DuplicateUIDSolverTests {

	DuplicateUIDSolver solver;

	int duplicateObject0UID = 0;
	int nonDuplicateObjectUID = 1;
	int duplicateObjectValueUID = -1;

	TriggerModel[] triggerModels = new TriggerModel[0];
	ConditionModel[] conditionModels = new ConditionModel[0];
	SequenceModel[] sequenceModels = new SequenceModel[0];
	ActionModel[] actionModels = new ActionModel[0];

	[SetUp]
	public void Init() {

		// we need this to match up with what the class is using, which is comparing against
		// actual object UIDs only
		duplicateObjectValueUID = (int)ValueTypeLoader.ValueType.MAZER_OBJECT;

		// create a trigger that uses the uid of the duplicate objects
		triggerModels = new TriggerModel[1];
		triggerModels[0] = new TriggerModel();
		triggerModels[0].UID = 0;
		triggerModels[0].Name = "_duplicate_trigger_test";
		TriggerConditionModel conditionModel = new TriggerConditionModel();
		conditionModel.UID = 0;
		conditionModel.LowerBound = duplicateObject0UID;
		conditionModel.UpperBound = duplicateObject0UID;
		triggerModels[0].Conditions = new TriggerConditionModel[1] { conditionModel };

		// add a condition for our trigger loader to find a use of the duplicated UID
		conditionModels = new ConditionModel[1];
		conditionModels[0] = new ConditionModel();
		conditionModels[0].Name = "_duplicate_trigger_test_condition";
		conditionModels[0].Description = "This is a test object and should be deleted.";
		conditionModels[0].UID = 0;
		conditionModels[0].ValueUID = duplicateObjectValueUID;


		sequenceModels = new SequenceModel[1];
		sequenceModels[0] = new SequenceModel();
		sequenceModels[0].UID = 0;
		sequenceModels[0].Name = "_duplicate_sequence_test";
		sequenceModels[0].Actions = new SequenceActionModel[1];
		sequenceModels[0].Actions[0] = new SequenceActionModel();
		sequenceModels[0].Actions[0].UID = 0;
		sequenceModels[0].Actions[0].Parameters = new object[1] { duplicateObject0UID };

		actionModels = new ActionModel[1];
		ActionParameterModel actionParameterModel = new ActionParameterModel();
		actionParameterModel.Name = "_duplicate_action_parameter_test";
		actionParameterModel.Index = 0;
		actionParameterModel.ValueType = duplicateObjectValueUID;
		actionParameterModel.Description = "This is a test parameter and should be deleted.";
		actionModels[0] = new ActionModel();
		actionModels[0].UID = 0;
		actionModels[0].Name = "_duplicate_action_test";
		actionModels[0].Description = "This is a test action and should be deleted.";
		actionModels[0].Parameters = new ActionParameterModel[1] { actionParameterModel };

		solver = new DuplicateUIDSolver();
	}

	[TearDown]
	public void Dispose()
	{
		
	}

#region objects

	[Test]
	/// <summary>
	/// Tests to ensure that trigger condition values are updated
	/// </summary>
	public void UpdateTriggersUsingDuplicateObjects()
	{
		// get all the triggers using the duplicate UID
		DuplicateUIDSolver.DuplicateContainer[] usings =
			                  solver.FindUsesOfDuplicateValuesInTriggers(triggerModels, conditionModels, 
			                                                             duplicateObject0UID, duplicateObjectValueUID);
		// assign UIDs to triggers that use the offending duplicate uids as values in their conditions
		// this is where you would let the user select, but for testing, we'll just assign a value that is not the 
		// duplicate Object UID
		for(int u = 0; u < usings.Length; u++) {
			usings[u].UpdatedUID = nonDuplicateObjectUID;
		}

		// update the trigger loader with the updated triggers
		TriggerModel[] fixedData = solver.SolveDuplicateUIDValuesInTriggers(usings, triggerModels);
		for(int d = 0; d < fixedData.Length; d++) {
			if(fixedData[d] == null) {
				continue;
			}
			triggerModels[fixedData[d].UID] = fixedData[d];
		}

		// finally, we search again for any conditions that use the duplicate object UID
		usings = solver.FindUsesOfDuplicateValuesInTriggers(triggerModels, conditionModels, 
		                                                    duplicateObject0UID, duplicateObjectValueUID);
		Assert.AreEqual(0, usings.Length, "We still have triggers using UID {0} as a condition value!",
						new object[1] { duplicateObject0UID });

	}

	[Test]
	/// <summary>
	/// Tests to ensure that sequence action parameter values are updated
	/// </summary>
	public void UpdateSequencesUsingDuplicateObjects() {
		DuplicateUIDSolver.DuplicateContainer[] usings =
							  solver.FindUsesOfDuplicateValuesInSequences(sequenceModels,
			                                                              actionModels, duplicateObject0UID,
			                                                              duplicateObjectValueUID);
		
		for(int u = 0; u < usings.Length; u++) {
			usings[u].UpdatedUID = nonDuplicateObjectUID;
		}

		// update the trigger loader with the updated triggers
		SequenceModel[] fixedData = solver.SolveDuplicateActionValuesInSequences(usings, sequenceModels);
		for(int d = 0; d < fixedData.Length; d++) {
			if(fixedData[d] == null) {
				continue;
			}
			sequenceModels[fixedData[d].UID] = fixedData[d];
		}

		// finally, we search again for any conditions that use the duplicate object UID
		usings = solver.FindUsesOfDuplicateValuesInSequences(sequenceModels, actionModels, 
		                                                     duplicateObject0UID, duplicateObjectValueUID);
		Assert.AreEqual(0, usings.Length, "We still have sequences using UID {0} as an action parameter value!",
						new object[1] { duplicateObject0UID });

	}

#endregion

#region triggers
	[Test]
	public void FindDuplicateTriggers(){

		triggerModels = new TriggerModel[2];
		triggerModels[0] = new TriggerModel();
		triggerModels[0].Name = "_duplicate_trigger_01";
		triggerModels[0].UID = 0;
		triggerModels[0].Conditions = new TriggerConditionModel[0];
		triggerModels[1] = new TriggerModel();
		triggerModels[1].Name = "_duplicate_trigger_02";
		triggerModels[1].UID = 0;
		triggerModels[1].Conditions = new TriggerConditionModel[0];

		ICriterionData[] duplicates = solver.GetDuplicateData(triggerModels);

		Assert.GreaterOrEqual(duplicates.Length, 2, "Did not find the expected number of duplicates. Instead found {0}.",
							  new object[1] { duplicates.Length });
	}

	[Test]
	public void SolveDuplicateTriggers(){
		triggerModels = new TriggerModel[2];
		triggerModels[0] = new TriggerModel();
		triggerModels[0].Name = "_duplicate_trigger_01";
		triggerModels[0].UID = 0;
		triggerModels[0].Conditions = new TriggerConditionModel[0];
		triggerModels[1] = new TriggerModel();
		triggerModels[1].Name = "_duplicate_trigger_02";
		triggerModels[1].UID = 0;
		triggerModels[1].Conditions = new TriggerConditionModel[0];

		solver.SolveDuplicateDataUID(triggerModels, 1);

		ICriterionData[] duplicates = solver.GetDuplicateData(triggerModels);

		Assert.AreEqual(duplicates.Length, 0, "Found {0} duplicates when we were expecting 0!",
							  new object[1] { duplicates.Length });
	}

	[Test]
	public void UpdateLinkActionsUsingDuplicateTriggers(){
		triggerModels = new TriggerModel[2];
		triggerModels[0] = new TriggerModel();
		triggerModels[0].Name = "_duplicate_trigger_01";
		triggerModels[0].UID = 0;
		triggerModels[0].Conditions = new TriggerConditionModel[0];
		triggerModels[1] = new TriggerModel();
		triggerModels[1].Name = "_duplicate_trigger_02";
		triggerModels[1].UID = 0;
		triggerModels[1].Conditions = new TriggerConditionModel[0];

		sequenceModels = new SequenceModel[4];
		sequenceModels[0] = new SequenceModel();
		sequenceModels[0].Name = "_using_link_duplicate_01";
		sequenceModels[0].UID = 0;
		sequenceModels[1] = new SequenceModel();
		sequenceModels[1].Name = "_using_link_duplicate_02";
		sequenceModels[1].UID = 0;


		SequenceActionModel linkAction = new SequenceActionModel();
		linkAction.Parameters = new object[1] { 0 };
		// load the two sequences with an action that has the duplicate trigger UID as its value
		sequenceModels[2] = new SequenceModel();
		sequenceModels[2].Name = "_using_link_duplicate_03";
		sequenceModels[2].UID = 0;
		sequenceModels[2].Actions = new SequenceActionModel[1] { linkAction };
		sequenceModels[3] = new SequenceModel();
		sequenceModels[3].Name = "_using_link_duplicate_04";
		sequenceModels[3].UID = 0;
		sequenceModels[3].Actions = new SequenceActionModel[1] { linkAction };

		// find
		DuplicateUIDSolver.DuplicateContainer[] usings = solver.FindUsesOfDuplicateActionsInSequences(sequenceModels, 0);

		// update
		for(int i = 0; i < usings.Length; i ++){
			usings[i].UpdatedUID = 1;
		}

		// solve
		SequenceModel[] solvedModels = solver.SolveDuplicateActionsInSequences(usings, sequenceModels);
		for(int m = 0; m < solvedModels.Length; m ++){
			if(solvedModels[m] == null){
				continue;
			}
			sequenceModels[solvedModels[m].UID] = solvedModels[m];
		}

		// find again
		usings = solver.FindUsesOfDuplicateActionsInSequences(sequenceModels, 0);

		Assert.AreEqual(0, usings.Length, "We still have {0} sequences using the duplicate UID 0!",
						new object[1] { usings.Length });
	}
#endregion

#region sequences
	[Test]
	public void FindDuplicateSequences() {
		sequenceModels = new SequenceModel[2];
		sequenceModels[0] = new SequenceModel();
		sequenceModels[0].Name = "_duplicate_sequence_01";
		sequenceModels[0].UID = 0;
		sequenceModels[1] = new SequenceModel();
		sequenceModels[1].Name = "_duplicate_sequence_02";
		sequenceModels[1].UID = 0;

		ICriterionData[] duplicates = solver.GetDuplicateData(sequenceModels);

		Assert.GreaterOrEqual(duplicates.Length, 2, "Found {0} duplicates. Expecting >= 2",
							  new object[1] { duplicates.Length });
	}
	[Test]
	public void SovleDuplicateSequences() {
		sequenceModels = new SequenceModel[2];
		sequenceModels[0] = new SequenceModel();
		sequenceModels[0].Name = "_duplicate_sequence_01";
		sequenceModels[0].UID = 0;
		sequenceModels[1] = new SequenceModel();
		sequenceModels[1].Name = "_duplicate_sequence_02";
		sequenceModels[1].UID = 0;

		solver.SolveDuplicateDataUID(sequenceModels, 0);

		ICriterionData[] duplicates = solver.GetDuplicateData(sequenceModels);

		Assert.AreEqual(duplicates.Length, 0, "Found {0} duplicates when expecting 0.",
						new object[1] { duplicates.Length });
	}
	[Test]
	public void UpdateLinkActionsUsingSequences() {

		sequenceModels = new SequenceModel[4];
		sequenceModels[0] = new SequenceModel();
		sequenceModels[0].UID = 0;
		sequenceModels[1] = new SequenceModel();
		sequenceModels[1].UID = 0;

		SequenceActionModel linkAction = new SequenceActionModel();
		linkAction.UID = 0;
		linkAction.Parameters = new object[1] { 0 };

		sequenceModels[2] = new SequenceModel();
		sequenceModels[2].UID = 2;
		sequenceModels[2].Actions = new SequenceActionModel[1] { linkAction };
		sequenceModels[3] = new SequenceModel();
		sequenceModels[3].UID = 3;
		sequenceModels[3].Actions = new SequenceActionModel[1] { linkAction };

		actionModels = new ActionModel[1];
		ActionParameterModel duplicateParam = new ActionParameterModel();
		duplicateParam.ValueType = 0;
		actionModels[0] = new ActionModel();
		actionModels[0].UID = 0;
		actionModels[0].Parameters = new ActionParameterModel[1] { duplicateParam };

		// first we resolve the sequences that are duplicated

		Dictionary<int, List<ICriterionData>> solvedSequences = solver.SolveDuplicateDataUID(sequenceModels, 3);
	
		foreach(int key in solvedSequences.Keys){
			// then, we look for any data that used the duplicated sequence
			DuplicateUIDSolver.DuplicateContainer[] linkUsings = solver.FindUsesOfDuplicateValuesInSequences(sequenceModels, actionModels, key, 0);
			// we pick the new values for that data
			for(int u = 0; u < linkUsings.Length; u ++){
				linkUsings[u].UpdatedUID = 1;

			}
			// with the updated values, we then solve with the usings we have changed
			solver.SolveDuplicateActionsInSequences(linkUsings, sequenceModels);
		}

		DuplicateUIDSolver.DuplicateContainer[] usings = solver.FindUsesOfDuplicateActionsInSequences(sequenceModels, 0);

		Assert.AreEqual(0, usings.Length, "Expected 0 duplicates, but found {0}",
						new object[1] { usings.Length });
	}
#endregion


#region conditions
	[Test]
	public void FindDuplicateConditions() {
		conditionModels = new ConditionModel[2];
		conditionModels[0] = new ConditionModel();
		conditionModels[0].Name = "_duplicate_condition_01";
		conditionModels[0].UID = 0;
		conditionModels[1] = new ConditionModel();
		conditionModels[1].Name = "_duplicate_condition_02";
		conditionModels[1].UID = 0;

		ICriterionData[] duplicates = solver.GetDuplicateData(conditionModels);

		Assert.GreaterOrEqual(duplicates.Length, 2, "Expected 2 duplicates but instead got {0}",
							  new object[1] { duplicates.Length });
	}
	[Test]
	public void SolveDuplicateConditions() {
		conditionModels = new ConditionModel[2];
		conditionModels[0] = new ConditionModel();
		conditionModels[0].Name = "_duplicate_condition_01";
		conditionModels[0].UID = 0;
		conditionModels[1] = new ConditionModel();
		conditionModels[1].Name = "_duplicate_condition_02";
		conditionModels[1].UID = 0;

		solver.SolveDuplicateDataUID(conditionModels, 0);

		ICriterionData[] duplicates = solver.GetDuplicateData(conditionModels);

		Assert.AreEqual(0, duplicates.Length, "Expected 0 duplicates but instead got {0}",
							  new object[1] { duplicates.Length });
	}
	[Test]
	public void UpdateTriggersUsingConditions() {
		conditionModels = new ConditionModel[2];
		conditionModels[0] = new ConditionModel();
		conditionModels[0].Name = "_duplicate_condition_01";
		conditionModels[0].UID = 0;
		conditionModels[1] = new ConditionModel();
		conditionModels[1].Name = "_duplicate_condition_02";
		conditionModels[1].UID = 0;

		triggerModels = new TriggerModel[1];
		triggerModels[0] = new TriggerModel();
		triggerModels[0].Name = "_duplicate_trigger_using_duplicate_condition";
		triggerModels[0].UID = 0;
		TriggerConditionModel condition = new TriggerConditionModel();
		condition.UID = 0;
		condition.LowerBound = 0;
		condition.UpperBound = 0;
		triggerModels[0].Conditions = new TriggerConditionModel[1] { condition };

		Dictionary<int, List<ICriterionData>> solvedModels = solver.SolveDuplicateDataUID(conditionModels, 0);

		foreach(int key in solvedModels.Keys){
			DuplicateUIDSolver.DuplicateContainer[] usings = solver.FindUsesOfDuplicateConditionsInTriggers(triggerModels, key);

			for(int u = 0; u < usings.Length; u ++){
				usings[u].UpdatedUID = 1;
			}

			triggerModels = solver.SolveDuplicateConditionUIDsInTriggers(usings, triggerModels);
		}

		DuplicateUIDSolver.DuplicateContainer[] solvedUsings = solver.FindUsesOfDuplicateConditionsInTriggers(triggerModels, 0);

		Assert.AreEqual(0, solvedUsings.Length, "Expected 0 duplicates but found {0}",
						new object[1] { solvedUsings.Length });
	}
#endregion

#region actions
	[Test]
	public void FindDuplicateActions() {
		actionModels = new ActionModel[2];
		actionModels[0] = new ActionModel();
		actionModels[0].Name = "_duplicate_action_01";
		actionModels[0].UID = 0;
		actionModels[1] = new ActionModel();
		actionModels[1].Name = "_duplicate_action_02";
		actionModels[1].UID = 0;

		ICriterionData[] duplicates = solver.GetDuplicateData(actionModels);

		Assert.GreaterOrEqual(duplicates.Length, 2, "Expected 2 duplicates but instead got {0}",
							  new object[1] { duplicates.Length });
	}
	[Test]
	public void SolveDuplicateActions() {
		actionModels = new ActionModel[2];
		actionModels[0] = new ActionModel();
		actionModels[0].Name = "_duplicate_action_01";
		actionModels[0].UID = 0;
		actionModels[1] = new ActionModel();
		actionModels[1].Name = "_duplicate_action_02";
		actionModels[1].UID = 0;

		solver.SolveDuplicateDataUID(actionModels, 0);

		ICriterionData[] duplicates = solver.GetDuplicateData(actionModels);

		Assert.AreEqual(0, duplicates.Length, "Expected 0 duplicates but instead got {0}",
							  new object[1] { duplicates.Length });
	}
	[Test]
	public void UpdateSequencesUsingActions() {

		actionModels = new ActionModel[2];
		actionModels[0] = new ActionModel();
		actionModels[0].Name = "_duplicate_action_01";
		actionModels[0].UID = 0;
		actionModels[1] = new ActionModel();
		actionModels[1].Name = "_duplicate_action_02";
		actionModels[1].UID = 0;

		sequenceModels = new SequenceModel[1];
		sequenceModels[0] = new SequenceModel();
		sequenceModels[0].Name = "_duplicate_sequence_using_item_action";
		sequenceModels[0].UID = 0;
		SequenceActionModel sequenceAction = new SequenceActionModel();
		sequenceAction.UID = 0;
		sequenceAction.Parameters = new object[1];
		sequenceAction.Parameters[0] = 0;
		sequenceModels[0].Actions = new SequenceActionModel[1] { sequenceAction };

		Dictionary<int, List<ICriterionData>> solvedModels = solver.SolveDuplicateDataUID(actionModels, 0);

		foreach(int key in solvedModels.Keys) {
			DuplicateUIDSolver.DuplicateContainer[] usings = solver.FindUsesOfDuplicateActionsInSequences(sequenceModels, key);

			for(int u = 0; u < usings.Length; u++) {
				usings[u].UpdatedUID = 1;
			}
			sequenceModels = solver.SolveDuplicateActionsInSequences(usings, sequenceModels);
		}

		DuplicateUIDSolver.DuplicateContainer[] solvedUsings = solver.FindUsesOfDuplicateActionsInSequences(sequenceModels, 0);

		Assert.AreEqual(0, solvedUsings.Length, "There are still {0} duplicates for the level UID in these sequences's actions.",
						new object[1] { solvedUsings.Length });
	}
#endregion
}
