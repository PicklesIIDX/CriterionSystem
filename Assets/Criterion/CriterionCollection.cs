using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PickleTools.Criterion {
	/// <summary>
	/// This is the class that handles loading up a database of triggers and actions.
	/// Instantiate this at the start of your game.
	/// Use its loading functions to load up the databases that should be searched
	/// through for the currently relevant interactions in your game.
	/// 
	/// </summary>
	public class CriterionCollection {

		List<Action>[] actions = new List<Action>[0];

		public CriterionCollection(){
		}

		/// <summary>
		/// Loads all triggers and responses into the collection.
		/// TODO: Allow specifying the databases.
		/// </summary>
		public void Initialize(TriggerModel[] loadedTriggers, int highestTriggerUID, 
		                       CriterionDataLoader<ConditionModel> conditionLoader, SequenceModel[] loadedSequences,
		                      int highestSequenceUID, int highestActionUID){
			LoadTriggers(loadedTriggers, highestTriggerUID, conditionLoader);
			LoadSequences(loadedSequences, highestSequenceUID, highestActionUID);
		}
			
		TriggerObject[] triggers = new TriggerObject[0];
		public TriggerObject[] Triggers {
			get { return triggers; }
		}
		/// <summary>
		/// Loads the triggers.
		/// </summary>
		void LoadTriggers(TriggerModel[] loadedTriggers, int highestTriggerUID, CriterionDataLoader<ConditionModel> conditionLoader){
			triggers = new TriggerObject[highestTriggerUID];

			// TriggerLoader should load databases into TriggerModels
			for(int t = 0; t < loadedTriggers.Length; t ++){
				if(loadedTriggers[t] == null){
					continue;
				}
				// This converts those trigger models into TriggerObjects
				TriggerObject trigger = new TriggerObject(loadedTriggers[t], conditionLoader);
				triggers[trigger.UID] = trigger;
			}
		}

		SequenceObject[] sequences = new SequenceObject[0];
		public SequenceObject[] Sequences {
			get { return sequences; }
		}
		/// <summary>
		/// Loads the responses.
		/// </summary>
		void LoadSequences(SequenceModel[] loadedSequences, int highestSequenceUID,
		                  int highestActionUID){
			
			for(int r = 0; r < sequences.Length; r ++){
				if(sequences[r] == null){
					continue;
				}
				sequences[r].Destroy();
			}
			sequences = new SequenceObject[highestSequenceUID];
			List<Action> allActions = new List<Action>();
			for(int r = 0; r < loadedSequences.Length; r ++){
				if(loadedSequences[r] == null){
					continue;
				}
				SequenceObject sequence = new SequenceObject(loadedSequences[r]);
				sequences[sequence.UID] = sequence;
				sequence.LinkTrigger(triggers[sequence.UID]);
				allActions.AddRange(sequence.GetAllActions());
			}

			// sort the actions into an array which indicates action type by UID
			// all actions of the same UID are added to that action list

			actions = new List<Action>[highestActionUID];
			for(int a = 0; a < actions.Length; a ++){
				actions[a] = new List<Action>();
			}
			for(int a = 0; a < allActions.Count; a ++){
				actions[allActions[a].UID].Add(allActions[a]);
			}
		}

		/// <summary>
		/// Retrieves a list of actions that have been created by responses in our currently loaded databases.
		/// This is used to link your game code to response actions, and know when to trigger your game code.
		/// Subscribe to the returned Action's ActionPerformed event to get the Action's parameters to use
		/// in your game's function. A list is returned because many responses will trigger the same event. But,
		/// each Action returned has specific parameters to perform a specific action for a response.
		/// </summary>
		/// <returns>A list of actions from rules that will trigger your game code with set response parameters.</returns>
		/// <param name="actionType">Action type.</param>
		public Action[] GetActions(int uid){
			if(uid < actions.Length && uid >= 0 && actions[uid] != null){
				return actions[uid].ToArray();
			} else {
				Debug.LogWarning("[CriterionCollection.cs]: The action of uid " + uid + " does not exist in " +
					"the loaded sequences. Returning an empty list.");
				return new Action[0];
			}
		}

		public TriggerObject GetTrigger(int uid){
			if(uid < triggers.Length && uid >= 0){
				return triggers[uid];
			} else {
				Debug.LogWarning("[CriterionCollection.cs]: The trigger of uid " + uid + " does not exist in " +
					"the loaded triggers. Returning null.");
				return null;
			}
		}

		public SequenceObject GetSequence(int uid){
			if(uid < sequences.Length && uid >= 0){
				return sequences[uid];
			} else {
				Debug.LogWarning("[CriterionCollection.cs]: The sequence of uid " + uid + " does not exist in " +
					"the loaded sequences. Returning null.");
				return null;
			}
		}
	}

}