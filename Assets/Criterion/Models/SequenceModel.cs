using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion {

//	[System.Serializable]
	public class SequenceModel : ICriterionData {
		private int uid = -1;
		public int UID {
			get { return uid; }
			set { uid = value; }
		}
		private string name = "";
		public string Name {
			get { return name; }
			set { name = value; }
		}
		public SequenceActionModel[] Actions = new SequenceActionModel[0];

		public override string ToString ()
		{
			string actionStrings = "\n";
			for(int a = 0; a < Actions.Length; a ++){
				actionStrings += Actions[a].ToString();
			}

			return string.Format ("[SequenceModel: UID: {0}, Name: {1}]\n" +
				"Actions: {2}\n",
				UID,
				Name,
				actionStrings
			);
		}
	}

	public class SequenceActionModel {
		public int UID = -1;
		//public string Name = "NONE";
		public object[] Parameters = new object[0];
		public SequenceActionModel[] Then = new SequenceActionModel[0];

		public override string ToString(){
			string parameterString = "";
			for(int i = 0; i < Parameters.Length; i ++){
				if(Parameters[i] == null){
					parameterString += "null, ";
				} else {
					parameterString += Parameters[i].ToString() + ", ";
				}
			}
			string thenStrings = "";
			for(int i = 0; i < Then.Length; i ++){
				if(Then[i] == null){
					UnityEngine.Debug.LogError("Then action for " + UID + " is null!");
				}
				thenStrings += Then[i].ToString() + "\n";
			}
			return string.Format("\n========\naction: {0}\n=parameters: {1}\n=then: {2}\n=======", 
			                     UID, parameterString, thenStrings);
		}

		public object GetParameter(int id){
			if(Parameters.Length <= id){
				System.Array.Resize<object>(ref Parameters, id+1);
				Parameters[id] = "";
			}
			if(Parameters[id] == null){
				Parameters[id] = "";
			}
			return Parameters[id];
		}

		public void SetParameter(int id, object value){
			if(Parameters.Length <= id){
				System.Array.Resize<object>(ref Parameters, id+1);
			}
			Parameters[id] = value;
		}

		public T GetParameter<T>(int parameterIndex) {
			if(parameterIndex >= Parameters.Length) {
				return default(T);
			}
			T value = default(T);
			try {
				value = (T)System.Convert.ChangeType(Parameters[parameterIndex], typeof(T));
			} catch {
				Debug.LogError("Could not convert " + Parameters[parameterIndex].ToString() + "(" + parameterIndex +
					")" + " to type " + typeof(T));
			}
			if(value == null) {
				throw new System.Exception("[SequenceModel.cs]: parameter " + parameterIndex + " of action " + UID +
					" could not be converted to type of " + typeof(T) + ". Please check the action" +
					" definition within the responses database and ensure it matches the calling" +
					" class's expectations.");
			}
			return value;
		}

		public T GetEnumParameter<T>(int parameterIndex) {
			if(parameterIndex >= Parameters.Length) {
				return default(T);
			}
			T type = (T)System.Enum.Parse(typeof(T), Parameters[parameterIndex].ToString());
			if(type == null) {
				throw new System.Exception("[SequenceModel.cs]: parameter " + parameterIndex + " of action " + UID +
					" could not be converted to type of " + typeof(T) + ". Please check the action" +
					" definition within the responses database and ensure it matches the calling" +
					" class's expectations.");
			}
			return type;
		}
	}
}


