using UnityEngine;
using System.Collections.Generic;
using LitJson;
using PickleTools.FileAccess;

namespace PickleTools.Criterion {
	public class ConditionLoader {

		ConditionModel[] conditionModels = new ConditionModel[0];
		public ConditionModel[] ConditionModels { 
			get { return conditionModels; }
		}

		string[] conditionNames = new string[0];
		public string[] ConditionNames {
			get {
				conditionNames = new string[conditionModels.Length];
				for(int i = 0; i < conditionModels.Length; i ++){
					if(conditionModels[i] == null){
						conditionNames[i] = "null";
					} else {
						conditionNames[i] = conditionModels[i].Name;
					}
				}
				return conditionNames;
			}
		}

		public int HighestUID = 0;

		private static readonly string RESOURCE_PATH = "data/conditions";

		public ConditionLoader(){
			
		}

		public void Load(string resourcePath = ""){
			if(resourcePath == ""){
				resourcePath = RESOURCE_PATH;
			}
			conditionModels = new ConditionModel[0];
			Debug.Log("[Loading Resources From]: " + resourcePath);
			TextAsset[] databaseFiles =  Resources.LoadAll<TextAsset>(resourcePath);
			for(int d = 0; d < databaseFiles.Length; d ++){
				List<ConditionModel> loadedConditions = JsonMapper.ToObject<List<ConditionModel>>(databaseFiles[d].text);
				for(int i = 0; i < loadedConditions.Count; i ++){
					if(loadedConditions[i].UID >= HighestUID){
						HighestUID = loadedConditions[i].UID + 1;
					}
				}
				System.Array.Resize(ref conditionModels, HighestUID);
				for(int i = 0; i < loadedConditions.Count; i ++){
					conditionModels[loadedConditions[i].UID] = loadedConditions[i];
				}
			}
		}

		public ConditionModel GetCondition(int uid){
			if(uid < conditionModels.Length && uid >= 0){
				return conditionModels[uid];
			} else {
				return null;
			}
		}

		public void AddCondition(string name, object defaultValue, string description, List<int> tags, int valueUID){

			ConditionModel newCondition = new ConditionModel();
			newCondition.UID = HighestUID;
			newCondition.Name = name;
			newCondition.DefaultValue = defaultValue;
			newCondition.Description = description;
			newCondition.Tags = tags;
			newCondition.ValueUID = valueUID;

			HighestUID ++;
			if(HighestUID + 1 >= conditionModels.Length) {
				System.Array.Resize<ConditionModel>(ref conditionModels, HighestUID + 1);
			}
			conditionModels[newCondition.UID] = newCondition;
		}

		public void Remove(int uid){
			conditionModels[uid] = null;
		}

		public void Save(IFileSaver fileSaver){
			List<ConditionModel> saveList = new List<ConditionModel>();
			for(int t = 0; t < conditionModels.Length; t ++){
				if(conditionModels[t] != null){
					saveList.Add(conditionModels[t]);
				}
			}
			JsonWriter writer = new JsonWriter();
			writer.PrettyPrint = true;
			JsonMapper.ToJson(saveList, writer);
			fileSaver.Save(RESOURCE_PATH, "conditions.json", writer.ToString(), true);

#if UNITY_EDITOR
			string relativePath = fileSaver.BasePath.Remove(0, fileSaver.BasePath.IndexOf("Assets/"));
			string filePath = relativePath + "/" + RESOURCE_PATH + "/" + "conditions.json";
			UnityEditor.AssetDatabase.ImportAsset(filePath, UnityEditor.ImportAssetOptions.ForceUpdate);
#endif
		}
	}
}