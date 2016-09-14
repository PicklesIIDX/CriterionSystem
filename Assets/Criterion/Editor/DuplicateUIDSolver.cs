using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


namespace PickleTools.Criterion {
	

	public class DuplicateUIDSolver {

		public class DuplicateContainer {
			public int UID = -1;
			public int Index01 = -1;
			public int Index02 = -1;
			public int[] ActionDepths = new int[0];
			public int UpdatedUID = -1;
			public string Name = "none";
			public DuplicateContainer(string name, int baseUID, int index01 = -1, int index02 = -1) {
				Name = name;
				UID = baseUID;
				Index01 = index01;
				Index02 = index02;
			}

			public override string ToString() {
				string actionDepthsString = "[";
				for(int i = 0; i < ActionDepths.Length; i ++){
					actionDepthsString += ActionDepths[i] + ",";
				}
				actionDepthsString += "]";
				return string.Format("[DuplicateContainer]\n" +
				                     "UID:{0}, Index01{1}, Index02:{2}, ActionDepths:{3},\n" +
				                     "Name:{4}, UpdatedUID:{5}",
				                     UID, Index01, Index02, actionDepthsString, Name, UpdatedUID);
			}
		}

		/// <summary>
		/// Finds and returns all models with duplicate UIDs.
		/// </summary>
		/// <returns>The duplicate models.</returns>
		/// <param name="models">Data that may be duplicated.</param>
		public ICriterionData[] GetDuplicateData(ICriterionData[] models) {
			List<ICriterionData> duplicates = new List<ICriterionData>();
			List<int> singleUIDs = new List<int>();
			for(int m = 0; m < models.Length; m++) {
				if(models[m] == null){
					continue;
				}
				if(singleUIDs.Contains(models[m].UID)) {
					for(int d = 0; d < models.Length; d++) {
						if(models[d] == null){
							continue;
						}
						if(models[m].UID == models[d].UID && !duplicates.Contains(models[d])) {
							duplicates.Add(models[d]);
						}
					}
				} else { singleUIDs.Add(models[m].UID); }
			}
			return duplicates.ToArray();
		}

		/// <summary>
		/// Solves duplicates in data by assigning them new UIDs starting from one higher than the highest UID.
		/// </summary>
		/// <returns>Models that have been changed to have new UIDs, grouped by their duplicate UID.</returns>
		/// <param name="models">Models with duplicate UIDs.</param>
		/// <param name="highestUID">The highest UID of this model type.</param>
		public Dictionary<int, List<ICriterionData>> SolveDuplicateDataUID(ICriterionData[] models, int highestUID) {
			Dictionary<int, List<ICriterionData>> duplicateGroups = new Dictionary<int, List<ICriterionData>>();
			List<ICriterionData> duplicates = new List<ICriterionData>();
			int highestOffset = 1;
			List<int> singleUIDs = new List<int>();
			for(int m = 0; m < models.Length; m++) {
				if(models[m] == null){
					continue;
				}
				if(singleUIDs.Contains(models[m].UID)) {
					int duplicateUID = models[m].UID;
					bool skipFirst = false;
					duplicateGroups.Add(duplicateUID, new List<ICriterionData>());
					for(int d = 0; d < models.Length; d++) {
						if(models[d] == null){
							continue;
						}
						if(duplicateUID == models[d].UID && !skipFirst) {
							duplicateGroups[duplicateUID].Add(models[d]);
							skipFirst = true;
							//Debug.LogWarning("Assigned UID " + models[d].Name + ":" + models[d].UID + " -> " + models[d].UID);
							continue;
						}
						if(duplicateUID == models[d].UID && !duplicates.Contains(models[d])) {
							if(highestUID + highestOffset + 1 >= models.Length) {
								System.Array.Resize<ICriterionData>(ref models, highestUID + highestOffset + 1);
							}
							//Debug.LogWarning("Assigned UID " + models[d].Name + ":" + models[d].UID + " -> " + (highestUID + highestOffset));
							models[d].UID = highestUID + highestOffset;
							highestOffset++;
							duplicateGroups[duplicateUID].Add(models[d]);
							duplicates.Add(models[d]);

						}
					}
				} else {
					singleUIDs.Add(models[m].UID);
				}
			}

			return duplicateGroups;
		}

#region triggers
		public DuplicateContainer[] FindUsesOfDuplicateValuesInTriggers(TriggerModel[] triggerModels, 
		                                                                ConditionModel[] conditionModels,
		                                                                int duplicateUID, int valueUID){
			
			List<DuplicateContainer> usings = new List<DuplicateContainer>();
			for(int t = 0; t < triggerModels.Length; t++) {
				if(triggerModels[t] == null){
					continue;
				}
				TriggerModel trigger = triggerModels[t];
				//Debug.LogWarning("Inspecting trigger " + triggerModels[t].UID + ": " + triggerModels[t].Name);
				for(int c = 0; c < trigger.Conditions.Length; c ++){
					ConditionModel conditionModel = null;
					for(int cm = 0; cm < conditionModels.Length; cm ++){
						if(conditionModels[cm].UID == trigger.Conditions[c].UID){
							conditionModel = conditionModels[cm];
							break;
						}
					}
					if(conditionModel == null) {
						continue;
					}
					if(conditionModel.ValueUID == valueUID){
						int conditionValue = -1;
						int.TryParse(trigger.Conditions[c].LowerBound.ToString(), out conditionValue);
						//Debug.LogWarning(conditionModel.Name + " value is " + conditionValue + " == " + duplicateUID);
						if(conditionValue == duplicateUID){
							//Debug.LogWarning(string.Format("Found duplicate trigger {0}, condition {1}, " +
							//							   "uid {2}, value {3}", trigger.UID, c,
							//							   duplicateUID, valueUID));
							usings.Add(new DuplicateContainer(triggerModels[t].Name + " :: " + conditionModel.Name, 
							                                  triggerModels[t].UID, c));
						}
					}
				}
			}

			return usings.ToArray();
		}

		public TriggerModel[] SolveDuplicateUIDValuesInTriggers(DuplicateContainer[] usings, TriggerModel[] triggerModels){
			for(int u = 0; u < usings.Length; u ++){
				for(int m = 0; m < triggerModels.Length; m ++){
					if(triggerModels[m].UID == usings[u].UID){
						triggerModels[m].Conditions[usings[u].Index01].UpperBound =
		                triggerModels[m].Conditions[usings[u].Index01].LowerBound =
								usings[u].UpdatedUID;
						break;
					}
				}
			}
			return triggerModels;
		}


#endregion


#region sequences
		public DuplicateContainer[] FindUsesOfDuplicateValuesInSequences(SequenceModel[] sequenceModels,
		                                                                 ActionModel[] actionModels,
		                                                 int duplicateUID, int valueUID){
			List<DuplicateContainer> usings = new List<DuplicateContainer>();
			for(int t = 0; t < sequenceModels.Length; t++) {
				if(sequenceModels[t] == null) {
					continue;
				}
				SequenceModel sequence = sequenceModels[t];
				usings.AddRange(SearchInSequenceActions(sequenceModels[t].Actions, actionModels,
				                                        duplicateUID, valueUID, sequence.UID, sequence.Name,
				                                        new int[0]));
			}

			return usings.ToArray();
		}

		private List<DuplicateContainer> SearchInSequenceActions(SequenceActionModel[] actions, ActionModel[] actionModels,
		                                    int duplicateUID, int valueUID, int sequenceUID, string sequenceName, int[] depth){
			List<DuplicateContainer> usings = new List<DuplicateContainer>();
			System.Array.Resize<int>(ref depth, depth.Length + 1);
			for(int a = 0; a < actions.Length; a++) {
				depth[depth.Length - 1] = a;
				usings.AddRange(SearchInSequenceActions(actions[a].Then, actionModels, duplicateUID, valueUID, 
				                                        sequenceUID, sequenceName, depth));
				ActionModel actionModel = null;
				for(int am = 0; am < actionModels.Length; am++) {
					if(actionModels[am].UID == actions[a].UID) {
						actionModel = actionModels[am];
						break;
					}
				}
				if(actionModel == null) {
					//Debug.LogWarning("Couldn't find ation of uid " + actions[a].UID);
					continue;
				}
				for(int p = 0; p < actions[a].Parameters.Length; p++) {
					if(p >= actionModel.Parameters.Length){
						continue;
						//Debug.LogError("Couldn't get parameter " + p + " for action " + a + "(" + actions[a].UID + ")" +
						//               " depth " + depth.Length +
						//               " of sequence " + sequenceUID);
					}
					if(actionModel.Parameters[p].ValueType == valueUID) {
						int parameterValue = -1;
						int.TryParse(actions[a].Parameters[p].ToString(), out parameterValue);
						//Debug.LogWarning("Checking " + sequenceName + " :: " + actionModel.Name + ", " + parameterValue + " == " + duplicateUID);
						if(parameterValue == duplicateUID) {
							DuplicateContainer container = new DuplicateContainer(sequenceName + " :: " + actionModel.Name, 
							                                                      sequenceUID, a, p);
							container.ActionDepths = depth;
							usings.Add(container);
						}
					}
				}
			}
			return usings;
		}

		public DuplicateContainer[] FindUsesOfDuplicateConditionsInSequences(SequenceModel[] sequenceModels,
		                                                                     ActionModel[] actionModels,
														 					int duplicateUID, int actionUID, 
		                                                                     int paramIndex) {
			List<DuplicateContainer> usings = new List<DuplicateContainer>();
			for(int t = 0; t < sequenceModels.Length; t++) {
				if(sequenceModels[t] == null) {
					continue;
				}
				SequenceModel sequence = sequenceModels[t];
				usings.AddRange(FindConditionsInActions(sequence.Actions, actionModels, duplicateUID, actionUID, paramIndex,
				                                        sequence.UID, sequence.Name, new int[0]));
			}

			return usings.ToArray();
		}

		List<DuplicateContainer> FindConditionsInActions(SequenceActionModel[] actions, ActionModel[] actionModels,
		                                                 int duplicateUID, int actionUID,
		                                                 int paramIndex, int sequenceUID, string sequenceName, int[] depth){
			List<DuplicateContainer> usings = new List<DuplicateContainer>();
			System.Array.Resize<int>(ref depth, depth.Length + 1);
			for(int a = 0; a < actions.Length; a ++){
				depth[depth.Length - 1] = a;
				usings.AddRange(FindConditionsInActions(actions[a].Then, actionModels, duplicateUID, actionUID, paramIndex, 
				                                        sequenceUID, sequenceName, depth));
				if(actions[a].UID == actionUID) {
					int parameterValue = -1;
					int.TryParse(actions[a].Parameters[paramIndex].ToString(), out parameterValue);
					if(parameterValue == duplicateUID) {
						string actionName = "none";
						for(int i = 0; i < actionModels.Length; i++){
							if(actionModels[i].UID == actions[a].UID){
								actionName = actionModels[i].Name;
							}
						}
						DuplicateContainer container = new DuplicateContainer(sequenceName + " :: " + actionName,
						                                                      sequenceUID, a, paramIndex);
						container.ActionDepths = depth;
						usings.Add(container);
					}

				}
			}
			return usings;
		}

		public SequenceModel[] SolveDuplicateActionValuesInSequences(DuplicateContainer[] usings, SequenceModel[] sequenceModels) {
			for(int u = 0; u < usings.Length; u++) {
				for(int m = 0; m < sequenceModels.Length; m++) {
					if(sequenceModels[m].UID == usings[u].UID) {
						SequenceActionModel[] depthActions = sequenceModels[m].Actions;
						for(int a = 0; a < usings[u].ActionDepths.Length; a++) {
							if(a == usings[u].ActionDepths.Length - 1){
								depthActions[usings[u].Index01].Parameters[usings[u].Index02] = usings[u].UpdatedUID;
								//Debug.LogWarning("Solved " + usings[u].Name +  " with value " + usings[u].UpdatedUID);
								break;
							} else {
								depthActions = depthActions[usings[u].ActionDepths[a]].Then;
							}
						}
					}
				}
			}
			return sequenceModels;
		}

#endregion

#region tags
		public DuplicateContainer[] FindUsesOfDuplicateTagsInConditions(ConditionModel[] conditions, int duplicateUID){
			List<DuplicateContainer> usings = new List<DuplicateContainer>();
			for(int c = 0; c < conditions.Length; c++) {
				for(int t = 0; t < conditions[c].Tags.Count; t ++){
					if(conditions[c].Tags[t] == duplicateUID){
						usings.Add(new DuplicateContainer(conditions[c].Name, conditions[c].UID, t));
					}
				}
			}

			return usings.ToArray();
		}

		public ConditionModel[] SolveDuplicateTagsInConditions(DuplicateContainer[] usings, ConditionModel[] conditions) {
			for(int u = 0; u < usings.Length; u++) {
				for(int c = 0; c < conditions.Length; c++) {
					if(conditions[c].UID == usings[u].UID) {
						//Debug.LogWarning("solved " + usings[u].Name + " value " + usings[u].UpdatedUID);
						conditions[c].Tags[usings[u].Index01] = usings[u].UpdatedUID;
						break;
					}
				}
			}
			return conditions;
		}
#endregion

#region conditions

		public DuplicateContainer[] FindUsesOfDuplicateConditionsInTriggers(TriggerModel[] triggerModels, int duplicateUID) {

			List<DuplicateContainer> usings = new List<DuplicateContainer>();
			for(int t = 0; t < triggerModels.Length; t++) {
				if(triggerModels[t] == null) {
					continue;
				}
				TriggerModel trigger = triggerModels[t];
				for(int c = 0; c < trigger.Conditions.Length; c++) {
					if(trigger.Conditions[c].UID == duplicateUID){
						usings.Add(new DuplicateContainer(triggerModels[t].Name, triggerModels[t].UID, c));
					}
				}
			}

			return usings.ToArray();
		}
		public TriggerModel[] SolveDuplicateConditionUIDsInTriggers(DuplicateContainer[] usings, TriggerModel[] triggerModels) {
			for(int u = 0; u < usings.Length; u++) {
				for(int m = 0; m < triggerModels.Length; m++) {
					if(triggerModels[m].UID == usings[u].UID) {
						//Debug.LogWarning("Solved " + usings[u].Name + " with value " + usings[u].UpdatedUID);
						triggerModels[m].Conditions[usings[u].Index01].UID = usings[u].UpdatedUID;
						break;
					}
				}
			}
			return triggerModels;
		}
#endregion

#region actions
		public DuplicateContainer[] FindUsesOfDuplicateActionsInSequences(SequenceModel[] sequenceModels, int duplicateUID) {

			List<DuplicateContainer> usings = new List<DuplicateContainer>();
			for(int t = 0; t < sequenceModels.Length; t++) {
				if(sequenceModels[t] == null) {
					continue;
				}
				SequenceModel sequence = sequenceModels[t];
				for(int a = 0; a < sequence.Actions.Length; a++) {
					if(sequence.Actions[a].UID == duplicateUID) {
						usings.Add(new DuplicateContainer(sequenceModels[t].Name, sequenceModels[t].UID, a));
					}
				}
			}

			return usings.ToArray();
		}
		public SequenceModel[] SolveDuplicateActionsInSequences(DuplicateContainer[] usings, SequenceModel[] sequenceModels) {
			for(int u = 0; u < usings.Length; u++) {
				for(int m = 0; m < sequenceModels.Length; m++) {
					if(sequenceModels[m].UID == usings[u].UID) {
						sequenceModels[m].Actions[usings[u].Index01].UID = usings[u].UpdatedUID;
						break;
					}
				}
			}
			return sequenceModels;
		}
#endregion

	}
}