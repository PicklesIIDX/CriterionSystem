using System.Collections.Generic;

namespace PickleTools.Criterion {

	public class Query {

		List<QueryEvaluation>[] evalList = new List<QueryEvaluation>[0];
		public List<QueryEvaluation>[] EvalList {
			get { return evalList; }
		}

		int maximumTriggers = 0;
		public int MaximumTriggers {
			get { return maximumTriggers; }
			set { maximumTriggers = value; }
		}

		object[] runtimeData;
		public object[] RuntimeData {
			get { return runtimeData; }
			set { runtimeData = value; }
		}

		System.Action<SequenceObject> triggeredCallback;
		public System.Action<SequenceObject> TriggeredCallback {
			set { triggeredCallback = value; }
		}

		public Query(CriterionDataLoader<ConditionModel> conditionLoader){
			CriterionDataLoader<ConditionModel> loader = conditionLoader;
			evalList = new List<QueryEvaluation>[loader.HighestUID];
			for(int c = 0; c < evalList.Length; c ++){
				evalList[c] = new List<QueryEvaluation>();
			}
		}

		public void Clear(){
			for(int e = 0; e < evalList.Length; e ++){
				for(int v = 0; v < evalList[e].Count; v ++){
					evalList[e][v].ConditionUID = -1;
				}
			}
		}

		public void Add(int conditionUID, float value){
			List<QueryEvaluation> evaluations = evalList[conditionUID];
			if(evaluations == null){
				evaluations = new List<QueryEvaluation>();
			}
			QueryEvaluationFloat eval = new QueryEvaluationFloat(conditionUID, value);
			evaluations.Add(eval);
			evalList[conditionUID] = evaluations;
		}
		public void Add(int conditionUID, bool value){
			List<QueryEvaluation> evaluations = evalList[conditionUID];
			if(evaluations == null){
				evaluations = new List<QueryEvaluation>();
			}
			QueryEvaluationBool eval = new QueryEvaluationBool(conditionUID, value);
			evaluations.Add(eval);
			evalList[conditionUID] = evaluations;
		}
		public void Add(int conditionUID, object value){
			List<QueryEvaluation> evaluations = evalList[conditionUID];
			if(evaluations == null){
				evaluations = new List<QueryEvaluation>();
			}
			QueryEvaluationObject eval = new QueryEvaluationObject(conditionUID, value);
			evaluations.Add(eval);
			evalList[conditionUID] = evaluations;
		}

		public void Triggered(SequenceObject response){
			if(triggeredCallback != null){
				triggeredCallback(response);
			}
		}

		public override string ToString ()
		{
			string contextEntries = "";
			for(int e = 0; e < evalList.Length; e ++){
				for(int v = 0; v < evalList[e].Count; v ++){
					if(evalList[e] != null){
						contextEntries += (ConditionLookup.ConditionType)evalList[e][v].ConditionUID +
							": " + evalList[e][v].ToString() + "\n";
					}
				}
			}
			if(runtimeData != null){
				contextEntries += "=======RUNTIME DATA=======\n";
				for(int r = 0; r < runtimeData.Length; r ++){
					if(runtimeData[r] != null){
						contextEntries += "[" + r + "] " + runtimeData[r].ToString() + "\n";
					}
				}

			}
			return string.Format ("[Query]:\n=======CONDITION LIST=======\n{0}\n==============", contextEntries);
		}
	}
}