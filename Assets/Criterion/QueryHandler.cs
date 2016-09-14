using System.Collections.Generic;
using PickleTools.Extensions.ListExtensions;

namespace PickleTools.Criterion {
	public class QueryHandler {

		TriggerObject[] triggers = new TriggerObject[0];
		SequenceObject[] responses = new SequenceObject[0];
		Stack<Query> queryStack = new Stack<Query>();

		List<Memory> memories = new List<Memory>();

		public void Initialize(TriggerObject[] triggerList, SequenceObject[] responseList){
			triggers = triggerList;
			responses = responseList;
		}

		/// <summary>
		/// When a game action is performed that can trigger a response
		/// in your database, add that state as a query through this function.
		/// </summary>
		/// <param name="q">A Query formed from the calling object's state.</param>
		public void AddQuery(Query query){
			queryStack.Push(query);
		}

		public void AddMemory(Memory newMemory){
			if(!memories.Contains(newMemory)){
				memories.Add(newMemory);
			}
		}


		/// <summary>
		/// Call this each update you want to evaluate the queries that have been added.
		/// </summary>
		public void EvaluateStackedQueries(){
			Query[] queryList = new Query[queryStack.Count];
			queryStack.CopyTo(queryList, 0);
			queryStack.Clear();
			EvaluateCurrentQueries(queryList);
		}

		void EvaluateCurrentQueries(Query[] evalList){
			for(int q = 0; q < evalList.Length; q ++){
				QueryTriggers(evalList[q]);
			}
		}

		public void QueryTriggers(Query query, bool getMostSpecificResponse = true){
			List<TriggerObject> passedTriggers = new List<TriggerObject>();
			int highestScore = 0;
			for(int m = 0; m < memories.Count; m ++){
				memories[m].AddMemoriesToQuery(ref query);
			}
			for(int t = 0; t < triggers.Length; t++){
				if(triggers[t] == null){
					continue;
				}
				int conditionCount = triggers[t].ConditionCount;
				if(getMostSpecificResponse && (highestScore > conditionCount || triggers[t].UID == -1) ||
					conditionCount == 0){
					continue;
				} else {
					
					if(triggers[t].EvaluateQuery(query)){
						passedTriggers.Add(triggers[t]);
						if(triggers[t].Score > highestScore){
							highestScore = triggers[t].Score;
						}
					} else {
						continue;
					}
				}
			}
			if(passedTriggers.Count > 0){

				// sort by score
				passedTriggers.Sort(delegate (TriggerObject a, TriggerObject b){
					return b.Score.CompareTo(a.Score);
				});
				// find the cutoff for the highest value...
				int cutoffIndex = 0;
				for(int p = 0; p < passedTriggers.Count; p ++){
					if(passedTriggers[p].Score < highestScore){
						cutoffIndex = p;
						break;
					}
				}
				// remove entries after cutoff
				if(cutoffIndex > 0){
					passedTriggers = passedTriggers.GetRange(0, cutoffIndex);
				}
				// shuffle rules so we get a random, most specific rule
				passedTriggers.Shuffle();
			}

			int maxTriggers = query.MaximumTriggers;
			if(maxTriggers == 0){
				maxTriggers = passedTriggers.Count;
			}
			for(int i = 0; i < maxTriggers; i ++){
				if(i >= passedTriggers.Count){
					break;
				}
				// This will inform us that this query was the one that activated the rule and
				// caused the response to be performed
				query.Triggered(responses[passedTriggers[i].UID]);
				// activate the rule and perform linked responses
				passedTriggers[i].ActivateTrigger(query.RuntimeData);
			}
		}


		public bool QueryTrigger(int triggerUID, Query query){
			for(int t = 0; t < triggers.Length; t ++){
				if(triggers[t] == null){
					continue;
				}
				if(triggers[t].UID == triggerUID){
					for(int m = 0; m < memories.Count; m ++){
						memories[m].AddMemoriesToQuery(ref query);
					}
					if(triggers[t].EvaluateQuery(query)){
						triggers[t].ActivateTrigger(query.RuntimeData);
						return true;
					}
					break;
				}
			}
			return false;
		}
	}
}