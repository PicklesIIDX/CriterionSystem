using UnityEngine;
using System.Collections.Generic;
using LitJson;
using PickleTools.FileAccess;

namespace PickleTools.Criterion {
	public class TagLoader {

		TagModel[] tagModels = new TagModel[0];
		public TagModel[] TagModels { 
			get { return tagModels; }
		}

		private string[] tagNames = new string[0];
		public string[] TagNames {
			get { return tagNames; }
		}

		public int HighestUID = 0;

		private static readonly string RESOURCE_PATH = "data/tags";

		public TagLoader(){

		}

		public void Load(string resourcePath = ""){
			if(resourcePath == ""){
				resourcePath = RESOURCE_PATH;
			}
			tagModels = new TagModel[0];
			tagNames = new string[0];
			Debug.Log("[Loading Resources From]: " + resourcePath);
			TextAsset[] databaseFiles =  Resources.LoadAll<TextAsset>(resourcePath);
			for(int d = 0; d < databaseFiles.Length; d ++){
				List<TagModel> loadedTags = JsonMapper.ToObject<List<TagModel>>(databaseFiles[d].text);
				for(int i = 0; i < loadedTags.Count; i ++){
					if(loadedTags[i].UID >= HighestUID){
						HighestUID = loadedTags[i].UID + 1;
					}
				}
				System.Array.Resize(ref tagModels, HighestUID);
				System.Array.Resize(ref tagNames, HighestUID);
				for(int i = 0; i < loadedTags.Count; i ++){
					tagModels[loadedTags[i].UID] = loadedTags[i];
					tagNames[loadedTags[i].UID] = loadedTags[i].Name;
				}
			}
		}

		public TagModel GetTag(int uid){
			if(uid < tagModels.Length){
				return tagModels[uid];
			} else {
				return null;
			}
		}


		public void AddTag(string name){
			TagModel tagModel = new TagModel();
			tagModel.UID = HighestUID;
			tagModel.Name = name;

			HighestUID ++;

			System.Array.Resize<TagModel>(ref tagModels, HighestUID + 1);
			tagModels[tagModel.UID] = tagModel;

		}

		public void Remove(int uid){
			tagModels[uid] = null;
		}

		public void Save(IFileSaver fileSaver){
			List<TagModel> saveList = new List<TagModel>();
			for(int t = 0; t < tagModels.Length; t ++){
				if(tagModels[t] != null){
					saveList.Add(tagModels[t]);
				}
			}
			JsonWriter writer = new JsonWriter();
			writer.PrettyPrint = true;
			JsonMapper.ToJson(saveList, writer);
			fileSaver.Save(RESOURCE_PATH, "tags.json", writer.ToString(), true);

			#if UNITY_EDITOR
			string relativePath = fileSaver.BasePath.Remove(0, fileSaver.BasePath.IndexOf("Assets/"));
			string filePath = relativePath + "/" + RESOURCE_PATH + "/" + "tags.json";
			UnityEditor.AssetDatabase.ImportAsset(filePath, UnityEditor.ImportAssetOptions.ForceUpdate);
			#endif
		}

	}
}
