using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion {

	public delegate void ActionHandler (Action action, object[] runtimeData);

	/// <summary>
	/// A single generic action. The uid indicates
	/// this action's types and what parameters it has.
	/// Actions can fire other actions sequentially through the
	/// then Action array.
	/// </summary>
	public class Action {

		public event ActionHandler ActionPerformed;
		public event ActionHandler ActionCompleted;

		int uid = -1;
		public int UID {
			get { return uid; }
		}

		object[] parameters = new object[0];
		public object[] Parameters {
			get { return parameters; }
		}
			
		Action[] then = new Action[0];
		public Action[] Then {
			get { return then; }
		}

		public Action(SequenceActionModel model){
			uid = model.UID;

			parameters = new object[model.Parameters.Length];
			for(int o = 0; o < model.Parameters.Length; o ++){
				parameters[o] = model.Parameters[o];
			}

			then = new Action[model.Then.Length];
			for(int t = 0; t < model.Then.Length; t ++){
				then[t] = new Action(model.Then[t]);
			}
		}


		public void ActionComplete(object[] runtimeData){
			//UnityEngine.Debug.Log(Time.timeSinceLevelLoad + " [Action.cs]: Completing action: " + uid + "\n" + ToString());
			if(ActionCompleted != null){
				ActionCompleted(this, runtimeData);
			}
			for(int t = 0; t < then.Length; t ++){
				then[t].PerformAction(runtimeData);
			}
		}

		public void ActionCancel(object[] runtimeData){
			if(ActionCompleted != null){
				ActionCompleted(this, runtimeData);
			}
			for(int t = 0; t < then.Length; t ++){
				then[t].ActionCancel(runtimeData);
			}
		}

		public void PerformAction(object[] newRuntimeData){
			//UnityEngine.Debug.Log(Time.timeSinceLevelLoad + " [Action.cs]: Performing action: " + uid + "\n" + ToString());
			if(ActionPerformed != null){
				ActionPerformed(this, newRuntimeData);
			}
		}

		public T GetParameter<T>(int parameterIndex){
			if(parameterIndex >= parameters.Length){
				return default(T);
			}
			T value = default(T);
			try{
				value = (T)System.Convert.ChangeType(parameters[parameterIndex], typeof(T));
			} catch {
				Debug.LogError("Could not convert " + parameters[parameterIndex].ToString() + "(" + parameterIndex +
					")" + " to type " + typeof(T));
			}
			if(value == null){
				throw new System.Exception("[ObjectSpeakerAnimator.cs]: parameter " + parameterIndex + " of action " + UID + 
					" could not be converted to type of " + typeof(T) + ". Please check the action" +
					" definition within the responses database and ensure it matches the calling" +
					" class's expectations.");
			}
			return value;
		}

		public T GetEnumParameter<T>(int parameterIndex){
			if(parameterIndex >= parameters.Length){
				return default(T);
			}
			T type = (T)System.Enum.Parse(typeof(T), parameters[parameterIndex].ToString());
			if(type == null){
				throw new System.Exception("[ObjectSpeakerAnimator.cs]: parameter " + parameterIndex + " of action " + UID + 
					" could not be converted to type of " + typeof(T) + ". Please check the action" +
					" definition within the responses database and ensure it matches the calling" +
					" class's expectations.");
			}
			return type;
		}

		public static T GetRuntimeData<T>(object[] runtimeData, int index){
			if(runtimeData == null || index >= runtimeData.Length){
				return default(T);
			}
			T value = (T)System.Convert.ChangeType(runtimeData[index], typeof(T));
			if(value == null){
				throw new System.Exception("[ObjectSpeakerAnimator.cs]: runtimeData " + index + " of action " + 
					" could not be converted to type of " + typeof(T) + ". Please check the object" +
					" which queried the response and ensure it matches the calling" +
					" class's expectations.");
			}
			return value;
		}

		public static T GetEnumRuntimeData<T>(object[] runtimeData, int index){
			if(runtimeData == null || index >= runtimeData.Length){
				return default(T);
			}
			T type = (T)System.Enum.Parse(typeof(T), runtimeData[index].ToString());
			if(type == null){
				throw new System.Exception("[ObjectSpeakerAnimator.cs]: runtimeData " + index + " of action " + 
					" could not be converted to type of " + typeof(T) + ". Please check the object" +
					" which queried the response and ensure it matches the calling" +
					" class's expectations.");
			}
			return type;
		}

		public static void SetRuntimeData(ref object[] runtimeData, int index, object value){
			if(runtimeData == null || runtimeData.Length <= index){
				System.Array.Resize<object>(ref runtimeData, index+1);
			}
			runtimeData[index] = value;
		}

		CriterionDataLoader<ActionModel> actionLoader;

		public override string ToString(){
			if(actionLoader == null){
				actionLoader = new CriterionDataLoader<ActionModel>();
				actionLoader.Load();
			}
			if(actionLoader.GetData(uid) == null){
				actionLoader.Load();
			}
			ActionModel model = actionLoader.GetData(uid);
		
			string actionString = "Action: " + UID + "\n";
			actionString += "Parameters------------\n";
			for(int i = 0; i < parameters.Length; i ++){
				string paramName = "";
				if(model != null && model.Parameters.Length > i){
					paramName = model.Parameters[i].Name;
				}
				actionString += paramName + ": " + parameters[i].ToString() + "\n";
			}

			actionString += "Then Action------------\n";
			for(int i = 0; i < then.Length; i ++){
				actionString += then[i].ToString();
			}
			return actionString;
		}

		public string ToString(object[] runtimeData){
			if(actionLoader == null){
				actionLoader = new CriterionDataLoader<ActionModel>();
				actionLoader.Load();
			}
			if(actionLoader.GetData(uid) == null){
				actionLoader.Load();
			}
			ActionModel model = actionLoader.GetData(uid);

			string actionString = "Action: " + UID + "\n";
			actionString += "Parameters------------\n";
			for(int i = 0; i < parameters.Length; i ++){
				string paramName = "";
				if(model != null && model.Parameters.Length > i){
					paramName = model.Parameters[i].Name;
				}
				actionString += paramName + ": " + parameters[i].ToString() + "\n";
			}
			if(runtimeData != null){
				actionString += "Runtime Data------------\n";
				for(int i = 0; i < runtimeData.Length; i ++){
					actionString += runtimeData[i].ToString() + "\n";
				}
			}
			actionString += "Then Action------------\n";
			for(int i = 0; i < then.Length; i ++){
				actionString += then[i].ToString();
			}
			return actionString;
		}
	}
}