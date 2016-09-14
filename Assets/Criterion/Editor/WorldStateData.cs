
namespace PickleTools.Criterion {
	public class WorldStateData {
		public int ConditionUID = 0;
		public string Value = "";
		public float Expiration = 0.0f;
		public bool ToggleBool = false;
		public bool IncrementNumber = false;

		public WorldStateData(int uid, object value, float expiration, bool toggleBool, bool incrementNumber) {
			ConditionUID = uid;
			Value = value.ToString();
			Expiration = expiration;
			ToggleBool = toggleBool;
			IncrementNumber = incrementNumber;
		}
	}
}
