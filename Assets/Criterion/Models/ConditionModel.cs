using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PickleTools.Criterion{
	
	[System.Serializable]
	public class ConditionModel : ICriterionData {

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
		public int ValueUID = -1;
		public object DefaultValue = 0;
		public bool Initialize = false;
		public string Description = "...";
		public List<int> Tags = new List<int>();

		public override string ToString(){
			string tagString = "";
			for(int t = 0; t < Tags.Count; t ++){
				tagString += Tags[t] + ", ";
			}

			return string.Format("[ConditionModel: UID: {0}, Name: {1}]\n" +
				"ValueUID: {2}\n" +
				"DefaultValue: {3}\n" +
				"Description: {4}\n" +
				"Tags: {5}\n",
				UID,
				Name,
				ValueUID,
				DefaultValue,
				Description,
				tagString
			);
		}

	}
}