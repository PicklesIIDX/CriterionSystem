using UnityEngine;
using System.Collections;

namespace PickleTools.Criterion {
	public class TagModel :ICriterionData {

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

	}
}