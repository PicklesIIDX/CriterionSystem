using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion {

	[System.Serializable]
	public class MemoryModel : ICriterionData {
		
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
		public MemoryFragmentModel[] Fragments = new MemoryFragmentModel[0];

	}

	[System.Serializable]
	public class MemoryFragmentModel {
		public int ConditionUID = -1;
		public string Name = "NONE";
		public string Value = "0";
		public int ValueID = -1;
		public float Expiration = 0.0f;
	}
}