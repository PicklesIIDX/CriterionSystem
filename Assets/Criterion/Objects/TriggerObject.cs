using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PickleTools.Criterion {

	public delegate void TriggerActivatedHandler (object sender, object[] queryRuntimeData);

	public class TriggerObject {
		public event TriggerActivatedHandler TriggerPassed;

		private int uid = -1;
		public int UID {
			get { return uid; }
		}
	
		private List<TriggerConditionObject>[] conditionList = new List<TriggerConditionObject>[0];
		public int ConditionCount {
			get { 
				int count = 0;
				for(int l = 0; l < conditionList.Length; l ++){
					if(conditionList[l] == null){
						continue;
					}
					for(int c = 0; c < conditionList[l].Count; c ++){
						count ++;
					}
				}
				return count;
			}
		}

		int score;
		public int Score {
			get { return score; }
		}

		public TriggerObject(){
			
		}

		public TriggerObject(TriggerModel model, ConditionLoader conditionLoader){
			uid = model.UID;
			conditionList = new List<TriggerConditionObject>[conditionLoader.ConditionModels.Length];
			for(int c = 0; c < model.Conditions.Length; c ++){
				ConditionModel conditionModel = conditionLoader.GetCondition(model.Conditions[c].UID);

				if(conditionList[model.Conditions[c].UID] == null) {
					conditionList[model.Conditions[c].UID] = new List<TriggerConditionObject>();
				}

				if(ValueTypeLoader.IsBoolValue(conditionModel.ValueUID)) {
					TriggerConditionObjectBool boolCondition = new TriggerConditionObjectBool(model.Conditions[c]);
					conditionList[model.Conditions[c].UID].Add(boolCondition);
				} else if(ValueTypeLoader.IsFloatValue(conditionModel.ValueUID)) {
					TriggerConditionObjectFloat floatCondition = new TriggerConditionObjectFloat(model.Conditions[c]);
					conditionList[model.Conditions[c].UID].Add(floatCondition);
				} else {
					TriggerConditionObjectObject objectCondition = new TriggerConditionObjectObject(model.Conditions[c]);
					conditionList[model.Conditions[c].UID].Add(objectCondition);
				}
			}
		}

		public void ActivateTrigger(object[] queryRuntimeData){
			if(TriggerPassed != null){
				TriggerPassed(this, queryRuntimeData);
			}
		}

		public void AddCondition(int conditionUID, float min, float max){
			if(conditionList[conditionUID] == null){
				List<TriggerConditionObject> conditions = conditionList[conditionUID];
				if(conditions == null){
					conditions = new List<TriggerConditionObject>();
				}
				conditions.Add(new TriggerConditionObjectFloat(conditionUID, min, max));
			} else {
				conditionList[conditionUID].Add(new TriggerConditionObjectFloat(conditionUID, min, max));
			}
		}
		public void AddCondition(int conditionUID, bool value){
			if(conditionList[conditionUID] == null){
				List<TriggerConditionObject> conditions = conditionList[conditionUID];
				if(conditions == null){
					conditions = new List<TriggerConditionObject>();
				}
				conditions.Add(new TriggerConditionObjectBool(conditionUID, value));
			} else {
				conditionList[conditionUID].Add(new TriggerConditionObjectBool(conditionUID, value));
			}
		}
		public void AddCondition(int conditionUID, object value){
			if(conditionList[conditionUID] == null){
				List<TriggerConditionObject> conditions = conditionList[conditionUID];
				if(conditions == null){
					conditions = new List<TriggerConditionObject>();
				}
				conditions.Add(new TriggerConditionObjectObject(conditionUID, (int)ValueTypeLoader.ValueType.TEXT, value));
			} else {
				conditionList[conditionUID].Add(new TriggerConditionObjectObject(conditionUID, 
				                                                                 (int)ValueTypeLoader.ValueType.TEXT, value));
			}
		}

		public bool EvaluateQuery(Query query){
			score = 0;

			// string for debugging
			string conditionString = "\n";

			///
			/////////////////////////////////////////////////
					/// SET DEBUG VALUE
					int debugTriggerType = 0;
			/////////////////////////////////////////////////
			/// 

			if(debugTriggerType != (int)TriggerLookup.TriggerType.NONE && uid == debugTriggerType){
				Debug.LogWarning("========================== Checking Trigger: " + (TriggerLookup.TriggerType)debugTriggerType);
			}
			List<TriggerConditionObject> passedConditions = new List<TriggerConditionObject>();
			// look through all conditions for this trigger by UID
			for(int u = 0; u < conditionList.Length; u ++){
				// if there is no list for this UID, then we continue
				if(conditionList[u] == null || conditionList[u].Count == 0){
					continue;
				}
				// debug

				passedConditions.Clear();
				// now we go through every Evaluation in this query
				for(int e = 0; e < query.EvalList[u].Count; e++){
					QueryEvaluation evaluation = query.EvalList[u][e];
					// and we then check every condition for this condition UID (in case we have multiple conditions of 
					// the same uid)
					for(int c = 0; c < conditionList[u].Count; c ++){
						// if we have already passed this condition from a previous
						// evalution, we don't need to evaluate it again

						TriggerConditionObject condition = conditionList[u][c];

						if(passedConditions.Contains(condition)) {
							continue;
						}

						bool increaseScore = false;

						if(uid == debugTriggerType){
							conditionString += conditionList[u][c].ToString();
						}

						if(ValueTypeLoader.IsBoolValue(condition.ValueID)) {
							bool boolValue = false;
							condition.GetValue(out boolValue);
							if(uid == debugTriggerType) {
								UnityEngine.Debug.LogWarning((ValueTypeLoader.ValueType)condition.ValueID + ": " +
									boolValue + " == " + evaluation.ToString());
							}
							if(evaluation != null && evaluation.Evaluate(boolValue)) {
								increaseScore = true;
							} else if(evaluation == null && boolValue == false) {
								increaseScore = true;
							}
						} else if(ValueTypeLoader.IsFloatValue(condition.ValueID)) {
							float lowerValue = -1.0f;
							float upperValue = -1.0f;
							condition.GetLowerBound(out lowerValue);
							condition.GetUpperBound(out upperValue);
							if(uid == debugTriggerType) {
								UnityEngine.Debug.LogWarning((ValueTypeLoader.ValueType)condition.ValueID + ": " +
									lowerValue + " <= " + evaluation.ToString() + " <= " + upperValue);
							}
							if(evaluation != null && evaluation.Evaluate(lowerValue, upperValue)) {
								increaseScore = true;
							} else if(evaluation == null && lowerValue == 0) {
								increaseScore = true;
							}
						} else {
							object objectValue = null;
							condition.GetValue(out objectValue);
							if(uid == debugTriggerType && evaluation != null) {
								UnityEngine.Debug.LogWarning((ValueTypeLoader.ValueType)condition.ValueID + ": " +
									objectValue + " == " + evaluation.ToString());
							}
							if(evaluation != null && evaluation.Evaluate(objectValue)) {
								increaseScore = true;
							} else if(evaluation == null && objectValue == null) {
								increaseScore = true;
							}
						}

						if(increaseScore){
							score ++;
							passedConditions.Add(condition);
							break;
						}
					}
				}
			}


			if(uid == debugTriggerType){
				UnityEngine.Debug.LogWarning("[TriggerModel.cs]: Evaulated trigger uid " + uid + ": " + score + "/" +
					ConditionCount + "\n" + query.ToString() + "\nConditions:\n" + conditionString);
			}
			if(score >= ConditionCount){
				return true;
			}
			return false;
		}
	}

}