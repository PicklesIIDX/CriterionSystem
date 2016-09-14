using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion {

	[System.Serializable]
	public class ActionModel : ICriterionData {


		private int uid = -1;
		public int UID  {
			get { return uid; }
			set { uid = value; }
		}
		private string name = "";
		public string Name {
			get { return name; }
			set { name = value; }
		}
		public string Description = "...";
		public ActionParameterModel[] Parameters = new ActionParameterModel[0];
		public string DatabasePath = "";
		public int[] Tags = new int[0];

		public override string ToString ()
		{
			string paramStrings = "\n";
			for(int p = 0; p < Parameters.Length; p ++){
				paramStrings += Parameters[p].ToString();
			}
			string tagString = "";
			for(int t = 0; t < Tags.Length; t ++){
				tagString += Tags[t] + ", ";
			}
			return string.Format ("[ActionModel: UID: {0}, Name: {1}]\n" +
				"Description: {2}\n" +
				"Parameters: {3}\n" +
				"Tags: {4}\n",
				UID,
				Name,
				Description,
				paramStrings,
				tagString
			);
		}

	}

	[System.Serializable]
	public class ActionParameterModel {

		public int Index = 0;
		public string Name = "NONE";
		public int ValueType = 0;
		public string Description = "...";

		public override string ToString ()
		{
			return string.Format ("[ActionParameterModel: Index: {0}, Name: {1}]\n" +
				"ValueType: {2}\n" +
				"Description: {3}\n",
				Index,
				Name,
				ValueType,
				Description
			);
		}

	}
}