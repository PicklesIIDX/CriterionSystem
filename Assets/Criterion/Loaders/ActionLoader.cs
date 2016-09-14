using UnityEngine;
using System.Collections.Generic;
using LitJson;
using PickleTools.FileAccess;

namespace PickleTools.Criterion {
	public class ActionLoader {

		private static readonly string RESOURCE_PATH = "data/actions";

		ActionModel[] actionModels = new ActionModel[0];
		public ActionModel[] ActionModels {
			get { return actionModels; }
		}

		public int HighestUID = 0;

		public void Load(string resourcePath = ""){
			if(resourcePath == ""){
				resourcePath = RESOURCE_PATH;
			}
			actionModels = new ActionModel[0];
			Debug.Log("[Loading Resources From]: " + resourcePath);
			TextAsset[] databaseFiles =  Resources.LoadAll<TextAsset>(resourcePath);
			for(int d = 0; d < databaseFiles.Length; d ++){
				List<ActionModel> loadedActions = JsonMapper.ToObject<List<ActionModel>>(databaseFiles[d].text);
				for(int i = 0; i < loadedActions.Count; i ++){
					if(loadedActions[i].UID >= HighestUID){
						HighestUID = loadedActions[i].UID + 1;
					}
				}
				System.Array.Resize(ref actionModels, HighestUID);
				for(int i = 0; i < loadedActions.Count; i ++){
					actionModels[loadedActions[i].UID] = loadedActions[i];
					actionModels[loadedActions[i].UID].DatabasePath = databaseFiles[d].name;
				}
			}
		}


		public ActionModel GetAction(int uid){
			if(uid < actionModels.Length && uid > -1){
				return actionModels[uid];
			} else {
				return null;
			}
		}

		public ActionModel AddAction(string name, string description, 
							ActionParameterModel[] parameters, string databasePath = ""){
			ActionModel newAction = new ActionModel();
			newAction.Name = name;
			newAction.DatabasePath = databasePath;
			newAction.Description = description;
			newAction.Parameters = parameters;
			// get uid
			int emptyUID = 0;
			for(int i = 1; i < actionModels.Length; i ++){
				if(actionModels[i] == null || actionModels[i-1] == null ||
					(actionModels[i].UID > actionModels[i-1].UID + 1)){
					emptyUID = actionModels[i-1].UID+1;
					break;
				} else {
					emptyUID = actionModels[i].UID + 1;
				}
			}

			newAction.UID = emptyUID;
			if(newAction.UID + 1 >= actionModels.Length){
				System.Array.Resize<ActionModel>(ref actionModels, newAction.UID + 1);
			}
			actionModels[newAction.UID] = newAction;
			return newAction;
		}

		public void Remove(int uid){
			actionModels[uid] = null;
		}

		public void Save(IFileSaver fileSaver){
			List<ActionModel> saveList = new List<ActionModel>();
			for(int t = 0; t < actionModels.Length; t ++){
				if(actionModels[t] != null){
					saveList.Add(actionModels[t]);
				}
			}
			// sort all actions into databases
			Dictionary<string, List<ActionModel>> databaseDict = new Dictionary<string, List<ActionModel>>();
			for(int a = 0; a < saveList.Count; a ++){
				if(saveList[a] == null){
					continue;
				}
//				Debug.LogWarning(actionModels[a].DatabasePath + ", " + actionModels[a].Name);
				string databaseKey = saveList[a].DatabasePath;
				try {
					databaseKey = saveList[a].DatabasePath.Remove(0, saveList[a].DatabasePath.IndexOf(RESOURCE_PATH) + RESOURCE_PATH.Length + 1);
				} catch {
					
				}
				if(!databaseDict.ContainsKey(databaseKey)){
					databaseDict.Add(databaseKey, new List<ActionModel>());
				}
				databaseDict[databaseKey].Add(saveList[a]);
			}
			// save each database file
			foreach(string key in databaseDict.Keys){
				// save to file path
				JsonWriter writer = new JsonWriter();
				writer.PrettyPrint = true;
				JsonMapper.ToJson(databaseDict[key], writer);
				fileSaver.Save(RESOURCE_PATH, key + ".json", writer.ToString(), true);

				#if UNITY_EDITOR
				string relativePath = fileSaver.BasePath.Remove(0, fileSaver.BasePath.IndexOf("Assets/"));
				string filePath = relativePath + "/" + RESOURCE_PATH + "/" + key + ".json";
				UnityEditor.AssetDatabase.ImportAsset(filePath, UnityEditor.ImportAssetOptions.ForceUpdate);
				#endif
			}
		}
	}
}