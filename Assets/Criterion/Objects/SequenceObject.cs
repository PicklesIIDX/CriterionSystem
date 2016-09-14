using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PickleTools.Criterion.ActionLookup;

namespace PickleTools.Criterion {

	public delegate void SequenceHandler (SequenceObject sender);

	public class SequenceObject {

		public event SequenceHandler Completed;

		int uid = -1;
		public int UID {
			get { return uid; }
		}

		string name = "NONE";
		public string Name {
			get { return name; }
		}

		Action[] actions;
		public Action[] Actions {
			get { return actions; }
			set { actions = value; }
		}

		TriggerObject trigger;
		int actionCounter = 0;
		int totalActions = 0;

		public SequenceObject(){

		}

		public SequenceObject(SequenceModel model){
			uid = model.UID;
			name = model.Name;
			actions = new Action[model.Actions.Length];
			for(int a = 0; a < actions.Length; a ++){
				actions[a] = new Action(model.Actions[a]);
			}
			LinkAllActions(actions, true);
		}

		public void Destroy(){
			if(trigger != null){
				trigger.TriggerPassed -= HandleTriggerPassed;
			}
			LinkAllActions(actions, false);
		}

		public void LinkTrigger(TriggerObject triggerObject){
			trigger = triggerObject;
			trigger.TriggerPassed += HandleTriggerPassed;
		}

		// a recursive function to track all actions completing or
		// to remove that tracking
		void LinkAllActions(Action[] actionsToLink, bool link){
			for(int a = 0; a < actionsToLink.Length; a ++){
				if(actionsToLink[a].Then.Length > 0){
					LinkAllActions(actionsToLink[a].Then, link);
				}
				if(link){
					actionsToLink[a].ActionCompleted += HandleActionCompleted;
					totalActions ++;
				} else {
					actionsToLink[a].ActionCompleted -= HandleActionCompleted;
					totalActions --;
				}
			}
		}

		/// <summary>
		/// Gets a list of all actions, including Then actions.
		/// </summary>
		/// <returns>All the actions.</returns>
		public List<Action> GetAllActions(){
			List<Action> list = new List<Action>();
			for(int a = 0; a < actions.Length; a ++){
				list.AddRange(FindActions(actions[a]));
			}
			return list;
		}

		/// <summary>
		/// Recursively finds all actions and then actions of a given action.
		/// </summary>
		/// <returns>The actions.</returns>
		/// <param name="action">Action.</param>
		List<Action> FindActions(Action action){
			List<Action> list = new List<Action>();
			list.Add(action);
			for(int t = 0; t < action.Then.Length; t ++){
				list.AddRange(FindActions(action.Then[t]));
			}
			return list;
		}

		void HandleActionCompleted (Action action, object[] runtimeData) {
			actionCounter ++;
//			UnityEngine.Debug.LogWarning("Completed " + action.ActionName + " " + actionCounter + "/" + totalActions + " for sequence " + sequenceType);
			if(actionCounter == totalActions){
				actionCounter = 0;
				if(Completed != null){
					Completed(this);
				}
			}
		}

		public void TriggerSequence(object sender, object[] runtimeData){
			HandleTriggerPassed(sender, runtimeData);
		}

		void HandleTriggerPassed (object sender, object[] runtimeData)
		{
			bool showLog = false;
			if(showLog){
				for(int a = 0; a < actions.Length; a ++){
					// set which action types prevent a sequence from being played
					if(actions[a].UID == (int)ActionType.NONE && actions.Length == 1){
						showLog = false;
					}
				}
				if(showLog){
					UnityEngine.Debug.Log("[SequenceObject.cs]: Performing sequence " + name);
				}
			}

			if(actionCounter != 0){
//				UnityEngine.Debug.LogWarning("[SequenceObject.cs]: WARNING: You are starting sequence " + sequenceType + " before it has finished! This can cause issues with " +
//				                             " code expecting this sequence to complete!");
			}
			actionCounter = 0;
			for(int a = 0; a < actions.Length; a ++){
				actions[a].PerformAction(runtimeData);
			}
		}
	}
}
