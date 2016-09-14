using UnityEngine;
using System.Collections.Generic;
using LitJson;
using PickleTools.FileAccess;

namespace PickleTools.Criterion {
	public class TriggerLoader {

		TriggerModel[] triggerModels = new TriggerModel[0];
		public TriggerModel[] TriggerModels {
			get { return triggerModels; }
		}

		string[] triggerNames = new string[0];
		public string[] TriggerNames {
			get {
				triggerNames = new string[triggerModels.Length];
				for(int i = 0; i < triggerModels.Length; i++) {
					if(triggerModels[i] == null) {
						triggerNames[i] = "null";
					} else {
						triggerNames[i] = triggerModels[i].Name;
					}
				}
				return triggerNames;
			}
		}

		public int HighestUID = 0;

		private static readonly string RESOURCE_PATH = "data/triggers";

		public TriggerLoader(){

		}

		/// <summary>
		/// Loads all trigger databases at the given resource path.
		/// </summary>
		/// <param name="resourcePath">Resource path.</param>
		public void Load(string resourcePath = ""){
			if(resourcePath == ""){
				resourcePath = RESOURCE_PATH;
			}
			triggerModels = new TriggerModel[0];
			Debug.Log("[Loading Resources From]: " + resourcePath);
			TextAsset[] databaseFiles =  Resources.LoadAll<TextAsset>(resourcePath);
			for(int d = 0; d < databaseFiles.Length; d ++){
				List<TriggerModel> loadedTriggers = JsonMapper.ToObject<List<TriggerModel>>(databaseFiles[d].text);
				for(int i = 0; i < loadedTriggers.Count; i ++){
					if(loadedTriggers[i].UID >= HighestUID){
						HighestUID = loadedTriggers[i].UID + 1;
					}
				}
				System.Array.Resize(ref triggerModels, HighestUID);
				for(int i = 0; i < loadedTriggers.Count; i ++){
					triggerModels[loadedTriggers[i].UID] = loadedTriggers[i];
				}

			}
		}

		/// <summary>
		/// Load the specified triggerDatabase.
		/// </summary>
		/// <param name="triggerDatabase">Trigger database.</param>
		public void Load(TextAsset database){
			triggerModels = new TriggerModel[0];
			List<TriggerModel> loadedTriggers = JsonMapper.ToObject<List<TriggerModel>>(database.text);
			for(int i = 0; i < loadedTriggers.Count; i ++){
				if(loadedTriggers[i].UID >= HighestUID){
					HighestUID = loadedTriggers[i].UID + 1;
				}
			}
			triggerModels = new TriggerModel[HighestUID];
			for(int i = 0; i < loadedTriggers.Count; i ++){
				triggerModels[loadedTriggers[i].UID] = loadedTriggers[i];
			}
		}

		public void Load(TriggerModel[] models){
			triggerModels = new TriggerModel[models.Length];
			for(int i = 0; i < models.Length; i ++){
				triggerModels[i] = models[i];
				if(models[i].UID > HighestUID){
					HighestUID = models[i].UID;
				}
			}
		}

		/// <summary>
		/// Gets a specific trigger from a given UID.
		/// </summary>
		/// <returns>The trigger.</returns>
		/// <param name="uid">Uid.</param>
		public TriggerModel GetTrigger(int uid){
			if(uid < triggerModels.Length){
				return triggerModels[uid];
			} else {
				return null;
			}
		}

		public TriggerModel AddTrigger(string name, TriggerConditionModel[] conditions){
			
			TriggerModel newTrigger = new TriggerModel();
			newTrigger.UID = HighestUID;
			newTrigger.Name = name;
			newTrigger.Conditions = conditions;

			HighestUID ++;

			System.Array.Resize<TriggerModel>(ref triggerModels, HighestUID);
			triggerModels[newTrigger.UID] = newTrigger;
			return newTrigger;
		}

		public void Remove(int uid){
			triggerModels[uid] = null;
		}

		public void Save(IFileSaver fileSaver){
			List<TriggerModel> saveList = new List<TriggerModel>();
			for(int t = 0; t < triggerModels.Length; t ++){
				if(triggerModels[t] != null){
					saveList.Add(triggerModels[t]);
				}
			}
			JsonWriter writer = new JsonWriter();
			writer.PrettyPrint = true;
			JsonMapper.ToJson(saveList, writer);
			fileSaver.Save(RESOURCE_PATH, "triggers.json", writer.ToString(), true);

			#if UNITY_EDITOR
			string relativePath = fileSaver.BasePath.Remove(0, fileSaver.BasePath.IndexOf("Assets/"));
			string filePath = relativePath + "/" + RESOURCE_PATH + "/" + "triggers.json";
			UnityEditor.AssetDatabase.ImportAsset(filePath, UnityEditor.ImportAssetOptions.ForceUpdate);
			#endif
		}
	}
}