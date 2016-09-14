using UnityEngine;
using PickleTools.FileAccess;

namespace PickleTools.Criterion {

	public class MemoryFragmentObject {
		int uid = -1;
		public int UID {
			get { return uid; }
		}

		string name = "NONE";
		public string Name {
			get { return name; }
		}

		int valueID = 0;
		public int ValueID {
			get { return valueID; }
		}

		float expiration = 0.0f;
		public float Expiration {
			get { return expiration; }
			set { expiration = value; }
		}

		QueryEvaluation evaluation;
		public QueryEvaluation Evaluation {
			get { return evaluation; }
		}


		public MemoryFragmentObject(){
			
		}

		public MemoryFragmentObject(MemoryFragmentModel model){
			uid = model.ConditionUID;
			name = model.Name;
			valueID = model.ValueID;
			expiration = model.Expiration;
			ValueTypeLoader.ValueType valueType = (ValueTypeLoader.ValueType)valueID;
			switch(valueType){
			case ValueTypeLoader.ValueType.TRUE_FALSE:
				bool boolValue = false;
				bool.TryParse(model.Value, out boolValue);
				evaluation = new QueryEvaluationBool(uid, boolValue);
				break;
			case ValueTypeLoader.ValueType.NUMBER_DECIMAL:
				float floatValue = 0.0f;
				float.TryParse(model.Value, out floatValue);
				evaluation = new QueryEvaluationFloat(uid, floatValue);
				break;//
			default:
				evaluation = new QueryEvaluationObject(uid, model.Value);
				break;
			}
		}

		public MemoryFragmentObject(int conditionUID, string conditionName, int conditionValueID, QueryEvaluation queryEvaluation){
			uid = conditionUID;
			name = conditionName;
			valueID = conditionValueID;
			evaluation = queryEvaluation;
		}

		public void UpdateAt(float deltaTime){
			if(expiration > 0.0f){
				expiration -= deltaTime;
				if(expiration <= 0.0f){
					evaluation.Reset();
				}
			}
		}

		public MemoryFragmentModel ToMemoryFragmentModel(){
			MemoryFragmentModel model = new MemoryFragmentModel();
			model.ConditionUID = uid;
			model.Expiration = expiration;
			model.Name = name;
			evaluation.GetValue(out model.Value);
			model.ValueID = valueID;
			return model;
		}
	}

	public class Memory {

		MemoryFragmentObject[] fragments = new MemoryFragmentObject[0];
		public MemoryFragmentObject[] Fragments {
			get { return fragments; }
		}

		QueryHandler queryHandler;

		ConditionLoader conditionLoader;

		public Memory(QueryHandler handler, ConditionLoader singletonConditionLoader){
			queryHandler = handler;
			// create an entry for every condition
			conditionLoader = singletonConditionLoader;
			InitializeFragments(conditionLoader.HighestUID);
		}

		public Memory(MemoryModel model, QueryHandler handler, ConditionLoader singletonConditionLoader){
			queryHandler = handler;
			conditionLoader = singletonConditionLoader;
			if(conditionLoader == null){
				conditionLoader = new ConditionLoader();
				conditionLoader.Load();
			}
			InitializeFragments(conditionLoader.HighestUID);
			for(int m = 0; m < model.Fragments.Length; m ++){
				Debug.LogWarning(model.Fragments[m].ConditionUID + "/" + fragments.Length);
				fragments[model.Fragments[m].ConditionUID] = new MemoryFragmentObject(model.Fragments[m]);
			}
		}

		void InitializeFragments(int highestConditionUID){
			fragments = new MemoryFragmentObject[highestConditionUID];
			for(int m = 0; m < fragments.Length; m ++){
				fragments[m] = new MemoryFragmentObject();
			}
		}

		public void InitializeConditions(ConditionModel[] conditions){
			for(int i = 0; i < conditions.Length; i ++){
				if(conditions[i] == null){
					continue;
				}
				if(conditions[i].Initialize && fragments[conditions[i].UID].Evaluation == null) {
					if(ValueTypeLoader.IsBoolValue(conditions[i].ValueUID)) {
						bool boolValue = false;
						bool.TryParse(conditions[i].DefaultValue.ToString(), out boolValue);
						EditMemory(conditions[i].UID, boolValue, 0);
					} else if(ValueTypeLoader.IsFloatValue(conditions[i].ValueUID)) {
						float floatValue = -1.0f;
						float.TryParse(conditions[i].DefaultValue.ToString(), out floatValue);
						EditMemory(conditions[i].UID, floatValue, 0);
					}
				}
			}
		}

		public void Save(IFileSaver fileSaver, int slot = 0){
			MemoryLoader loader = new MemoryLoader();
			loader.Save(ToMemoryModel(), slot, fileSaver);
		}

		public void UpdateAt(float deltaTime){
			for(int f = 0; f < fragments.Length; f ++){
				fragments[f].UpdateAt(deltaTime);
			}
		}

		MemoryModel ToMemoryModel(){
			MemoryModel model = new MemoryModel();
			model.Fragments = new MemoryFragmentModel[fragments.Length];
			for(int f = 0; f < fragments.Length; f ++){
				model.Fragments[f] = fragments[f].ToMemoryFragmentModel();
			}
			return model;
		}

		public bool TryGetValue(int uid, out bool boolValue){
			boolValue = false;
			if(fragments[uid] != null && fragments[uid].Evaluation != null){
				fragments[uid].Evaluation.GetValue(out boolValue);
				return true;
			}
			return false;
		}

		public bool TryGetValue(int uid, out float floatValue){
			floatValue = 0.0f;
			if(fragments[uid] != null && fragments[uid].Evaluation != null){
				fragments[uid].Evaluation.GetValue(out floatValue);
				return true;
			}
			return false;
		}

		public bool TryGetValue(int uid, out object value){
			value = 0;
			if(fragments[uid] != null && fragments[uid].Evaluation != null){
				fragments[uid].Evaluation.GetValue(out value);
				return true;
			}
			return false;
		}

		void AddFragments(int fragmentCount){
			int originalSize = fragments.Length;
			if(fragments.Length <= fragmentCount){
				Debug.LogWarning("<color=#dd3333>[Memory.cs]:</color> We tried to edit a memory of condition uid " + fragmentCount + " but that doesn't exist!" +
					" Please check how this is being added and create the actual condition!");
				System.Array.Resize<MemoryFragmentObject>(ref fragments, fragmentCount+1);
				for(int i = originalSize; i < fragments.Length; i ++){
					fragments[i] = new MemoryFragmentObject(i, i.ToString(), (int)ValueTypeLoader.ValueType.TEXT, new QueryEvaluationObject(i, null));
				}
			}
		}

		public void EditMemory(int uid, bool newValue, float expiration = 0.0f){
			if(uid >= fragments.Length){
				AddFragments(uid);
			}
			if(fragments[uid] == null || fragments[uid].Evaluation == null){
				fragments[uid] = new MemoryFragmentObject(uid, conditionLoader.ConditionModels[uid].Name,
					conditionLoader.ConditionModels[uid].ValueUID, new QueryEvaluationBool(uid, newValue));
			}
			fragments[uid].Evaluation.UpdateValue(newValue);
			fragments[uid].Expiration = expiration;
			SendQuery();
		}
		public void EditMemory(int uid, float newValue, float expiration = 0.0f){
			if(uid >= fragments.Length){
				AddFragments(uid);
			}
			if(fragments[uid] == null || fragments[uid].Evaluation == null){
				fragments[uid] = new MemoryFragmentObject(uid, conditionLoader.ConditionModels[uid].Name,
					conditionLoader.ConditionModels[uid].ValueUID, new QueryEvaluationFloat(uid, newValue));
			}
			fragments[uid].Evaluation.UpdateValue(newValue);
			fragments[uid].Expiration = expiration;
			SendQuery();
		}
		public void IncrementMemory(int uid, float newValue, float expiration = 0.0f){
			if(uid >= fragments.Length){
				AddFragments(uid);
			}
			if(fragments[uid] == null || fragments[uid].Evaluation == null){
				fragments[uid] = new MemoryFragmentObject(uid, conditionLoader.ConditionModels[uid].Name,
					conditionLoader.ConditionModels[uid].ValueUID, new QueryEvaluationFloat(uid, newValue));
			}
			fragments[uid].Evaluation.IncrementValue(newValue);
			fragments[uid].Expiration = expiration;
			SendQuery();
		}
		public void EditMemory(int uid, object newValue, float expiration = 0.0f){
			if(uid >= fragments.Length){
				AddFragments(uid);
			}
			if(fragments[uid] == null || fragments[uid].Evaluation == null){
				fragments[uid] = new MemoryFragmentObject(uid, conditionLoader.ConditionModels[uid].Name,
					conditionLoader.ConditionModels[uid].ValueUID, new QueryEvaluationObject(uid, newValue));
			}
			fragments[uid].Evaluation.UpdateValue(newValue);
			fragments[uid].Expiration = expiration;
			SendQuery();
		}

		/// <summary>
		/// Used to send queries because we usually want to make a query when we update the memory
		/// </summary>
		void SendQuery(){
			Query query = new Query(conditionLoader);
			queryHandler.AddQuery(query);
		}

		public void AddMemoriesToQuery(ref Query query){
			for(int f = 0; f < fragments.Length; f ++){
				if(fragments[f] == null || 
					fragments[f].Evaluation == null ||
					fragments[f].Evaluation.GetType() == typeof(QueryEvaluation)){
					continue;
				}

				if(ValueTypeLoader.IsBoolValue(fragments[f].ValueID)) {
					bool boolValue = false;
					fragments[f].Evaluation.GetValue(out boolValue);
					query.Add(fragments[f].UID, boolValue);
				} else if(ValueTypeLoader.IsFloatValue(fragments[f].ValueID)) {
					float floatValue = -1.0f;
					fragments[f].Evaluation.GetValue(out floatValue);
					query.Add(fragments[f].UID, floatValue);
				} else {
					object objectValue = "";
					fragments[f].Evaluation.GetValue(out objectValue);
					query.Add(fragments[f].UID, objectValue);
				}
			}
		}
	}
}
