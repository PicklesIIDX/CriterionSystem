using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion {

	[System.Serializable]
	public class TriggerModel : ICriterionData {

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
		public TriggerConditionModel[] Conditions = new TriggerConditionModel[0];

		public override string ToString ()
		{
			string conditionStrings = "";
			for(int c = 0; c < Conditions.Length; c ++){
				conditionStrings += "\n" + Conditions[c].ToString();
			}

			return string.Format ("[TriggerModel: UID: {0}, Name: {1}]\n" +
				"Conditions: {2}\n",
				UID,
				Name,
				conditionStrings
			);
		}
	}

	[System.Serializable]
	public class TriggerConditionModel {

		public int UID = -1;
		public object LowerBound = null;
		public object UpperBound = null;

		public override string ToString ()
		{
			return string.Format ("[TriggerConditionModel]\n" +
				"UID: {0}\n" +
				"LowerBound: {1}\n" +
				"UpperBound: {2}\n",
				UID,
				LowerBound,
				UpperBound
			);
		}
	}
}