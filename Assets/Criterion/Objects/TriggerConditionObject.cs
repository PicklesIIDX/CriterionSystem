using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion{
	public class TriggerConditionObject {

		protected int uid;
		public int UID {
			get { return uid; }
		}
		protected int valueID;
		public int ValueID {
			get { return valueID; }
		}

		public virtual void GetLowerBound(out float value)  { Debug.LogError("[EvalType.cs]: Implement this function!"); value = 0; }
		public virtual void GetUpperBound(out float value)  { Debug.LogError("[EvalType.cs]: Implement this function!"); value = 0; }
		public virtual void GetValue(out bool value)  { Debug.LogError("[EvalType.cs]: Implement this function!"); value = false; }
		public virtual void GetValue(out object value)  { Debug.LogError("[EvalType.cs]: Implement this function!"); value = ""; }

		public override string ToString ()
		{
			string valueString = "";
			switch(valueID){
			case 1: // bool
				bool boolValue = false;
				GetValue(out boolValue);
				valueString += (ConditionLookup.ConditionType)uid + ": " +
					"("+ (ValueTypeLoader.ValueType)valueID + ") " + boolValue + "\n";
				break;
			case 2: // float
				float lowerValue = 0;
				float upperValue = 0;
				GetLowerBound(out lowerValue);
				GetUpperBound(out upperValue);
				valueString += (ConditionLookup.ConditionType)uid + ": " +
					"("+ (ValueTypeLoader.ValueType)valueID + ") " + lowerValue + " >= =< " + upperValue + "\n";
				break;
			default: // object
				object objectValue = null;
				GetValue(out objectValue);
				valueString += (ConditionLookup.ConditionType)uid + ": " +
					"("+ (ValueTypeLoader.ValueType)valueID + ") " + objectValue + "\n";
				break;
			}
			return ("[ConditionModel]:" + valueString);
		}

	}

	public class TriggerConditionObjectBool: TriggerConditionObject {

		bool boolValue;

		public TriggerConditionObjectBool(TriggerConditionModel model){
			uid = model.UID;
			valueID = (int)ValueTypeLoader.ValueType.TRUE_FALSE;
			try {
				boolValue = System.Convert.ToBoolean(model.LowerBound);
			} catch (System.Exception e) {
				Debug.LogError("[TriggerConditionObjectBool]: LowerBound value of condition " + model +
					"cannot be converted to a bool because of exception: " + e.Message);
				boolValue = false;
			}
		}

		public TriggerConditionObjectBool(int conditionUID, bool value){
			uid = conditionUID;
			valueID = (int)ValueTypeLoader.ValueType.TRUE_FALSE;
			boolValue = value;
		}

		public override void GetValue (out bool value)
		{
			value = boolValue;
		}
	}

	public class TriggerConditionObjectFloat : TriggerConditionObject {

		float lowerValue;
		float upperValue;

		public TriggerConditionObjectFloat(TriggerConditionModel model){
			uid = model.UID;
			valueID = (int)ValueTypeLoader.ValueType.NUMBER_DECIMAL;
			try {
				lowerValue = System.Convert.ToSingle(model.LowerBound);
				upperValue = System.Convert.ToSingle(model.UpperBound);
			} catch (System.Exception e) {
				Debug.LogError("[TriggerConditionObjectFloat]: LowerBound value of condition " + model +
					"cannot be converted to a float because of exception: " + e.Message);
				lowerValue = 0;
				upperValue = 0;
			}
		}

		public TriggerConditionObjectFloat(int conditionUID, float lowerBound, float upperBound){
			uid = conditionUID;
			valueID = (int)ValueTypeLoader.ValueType.NUMBER_DECIMAL;
			lowerValue = lowerBound;
			upperValue = upperBound;
		}

		public override void GetLowerBound (out float value)
		{
			value = lowerValue;
		}

		public override void GetUpperBound (out float value)
		{
			value = upperValue;
		}
	}

	public class TriggerConditionObjectObject : TriggerConditionObject {

		object objectValue;

		public TriggerConditionObjectObject(TriggerConditionModel model, int vType = -1){
			uid = model.UID;
			if(vType == -1){
				valueID = (int)ValueTypeLoader.ValueType.TEXT;
			} else {
				valueID = vType;
			}
			objectValue = model.LowerBound;
		}

		public TriggerConditionObjectObject(int conditionUID, int conditionValueID, object value){
			uid = conditionUID;
			valueID = conditionValueID;
			objectValue = value;
		}

		public override void GetValue (out object value)
		{
			value = objectValue;
		}
	}
}