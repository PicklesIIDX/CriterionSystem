using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion {

	public class QueryEvaluation {

		protected int conditionUID;
		public int ConditionUID {
			get { return conditionUID; }
			set { conditionUID = value; }
		}
		public bool Evaluated = false;

		public virtual bool Evaluate(bool value) { Debug.LogError("[QueryEvaluation.cs]: Implement bool for " + conditionUID); return false; }
		public virtual bool Evaluate(float min, float max) { Debug.LogError("[QueryEvaluation.cs]: Implement float for " + conditionUID); return false;  }
		public virtual bool Evaluate(object value) { Debug.LogError("[QueryEvaluation.cs]: Implement object for " + conditionUID); return false;  }
		public override string ToString() { return "WARNING: This has no value as it is an abstract QueryEvaluation!"; } 
		public virtual void UpdateValue(bool newValue) { Debug.LogError("[QueryEvaluation.cs]: Implement this function!"); }
		public virtual void UpdateValue(float newValue) { Debug.LogError("[QueryEvaluation.cs]: Implement this function!"); }
		public virtual void UpdateValue(object newValue) { Debug.LogError("[QueryEvaluation.cs]: Implement this function!"); }
		public virtual void IncrementValue(float additionalValue) { Debug.LogError("[QueryEvaluation.cs]: Implement this function!"); }
		public virtual void GetValue(out bool value) { Debug.LogError("[QueryEvaluation.cs]: Implement this function!"); value = false;}
		public virtual void GetValue(out float value) { Debug.LogError("[QueryEvaluation.cs]: Implement this function!");  value = 0;}
		public virtual void GetValue(out object value) { Debug.LogError("[QueryEvaluation.cs]: Implement this function!"); value = ""; }
		public virtual void GetValue(out string value) { Debug.LogError("[QueryEvaluation.cs]: Implement this function!"); value = ""; }
		public virtual void Reset() { Debug.LogError("[QueryEvaluation.cs]: Implement this function!"); }
	}

	public class QueryEvaluationBool : QueryEvaluation {

		bool boolValue = false;

		public QueryEvaluationBool(int fact, bool compareValue){
			conditionUID = fact;
			boolValue = compareValue;
		}

		public override bool Evaluate(bool value){
			return boolValue == value;
		}

		public override string ToString(){
			return boolValue.ToString();
		}

		public override void GetValue (out bool value)
		{
			value = boolValue;
		}

		public override void UpdateValue(bool newValue){
			boolValue = newValue;
		}

		public override void Reset ()
		{
			boolValue = false;
		}
	}

	public class QueryEvaluationFloat : QueryEvaluation {

		float floatValue = 0.0f;

		public QueryEvaluationFloat(int fact, float compareValue){
			conditionUID = fact;
			floatValue = compareValue;
		}

		public override bool Evaluate(float min, float max){
			return (min <= floatValue) && (floatValue <= max);
		}

		public override string ToString(){
			return floatValue.ToString();
		}

		public override void GetValue(out float value){
			value = floatValue;
		}

		public override void UpdateValue(float newValue){
			floatValue = newValue;
		}

		public override void Reset () {
			floatValue = 0;
		}

		public override void IncrementValue (float additionalValue)
		{
			floatValue += additionalValue;
		}
	}

	public class QueryEvaluationObject : QueryEvaluation {

		object objectValue = "";

		public QueryEvaluationObject(int fact, object compareValue){
			conditionUID = fact;
			objectValue = compareValue;
		}

		public override bool Evaluate(object value){
			return object.Equals(objectValue, value);
		}

		public override string ToString(){
			if(objectValue == null){
				return "";
			}
			return objectValue.ToString();
		}

		public override void GetValue(out object value){
			value = objectValue;
		}

		public override void GetValue(out string value){
			value = objectValue.ToString();
		}

		public override void UpdateValue(object newValue){
			objectValue = newValue;
		}

		public override void Reset () {
			objectValue = "";
		}
	}
}