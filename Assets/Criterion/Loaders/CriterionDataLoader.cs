using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using PickleTools.FileAccess;

namespace PickleTools.Criterion {
	public class CriterionDataLoader<T> {

		T[] models = new T[0];
		public T[] Models {
			get { return models; }
		}


		string[] names = new string[0];
		public string[] Names {
			get {
				names = new string[models.Length];
				for(int i = 0; i < models.Length; i++) {
					if(models[i] == null) {
						names[i] = "null";
					} else {
						names[i] = ((ICriterionData)models[i]).Name;
					}
				}
				return names;
			}
		}

		public int HighestUID = 0;

		private readonly string RESOURCE_PATH = "data/data";
		private readonly string RESOURCE_NAME = "data";

		private readonly bool SAVE_INDIVIDUAL_FILES = false;

		private bool hasDuplicates = false;

		public CriterionDataLoader(string dataPath = "data", string dataName = "data", bool saveIndividualFiles = false){
			RESOURCE_PATH = "data/" + dataPath;
			RESOURCE_NAME = dataName;
			SAVE_INDIVIDUAL_FILES = saveIndividualFiles;
		}

		private List<T> GetAllModelsFromText(TextAsset textAsst) {
			List<T> loadedData = JsonMapper.ToObject<List<T>>(textAsst.text);
			for(int i = 0; i < loadedData.Count; i++) {
				if(((ICriterionData)loadedData[i]).UID >= HighestUID) {
					HighestUID = ((ICriterionData)loadedData[i]).UID + 1;
				}
			}
			return loadedData;
		}

		/// <summary>
		/// Loads all databases at the given resource path.
		/// </summary>
		/// <param name="resourcePath">Resource path.</param>
		public void Load(string resourcePath = "", bool noDuplicates = true) {
			if(resourcePath == "") {
				resourcePath = RESOURCE_PATH;
			}
			models = new T[0];
			TextAsset[] databaseFiles = Resources.LoadAll<TextAsset>(resourcePath);
			for(int d = 0; d < databaseFiles.Length; d++) {
				List<T> loadedData = GetAllModelsFromText(databaseFiles[d]);
				if(loadedData.Count <= 0){
					continue;
				}
				if(noDuplicates) {
					System.Array.Resize(ref models, HighestUID);
					for(int i = 0; i < loadedData.Count; i++) {
						models[((ICriterionData)loadedData[i]).UID] = loadedData[i];
					}
				} else {
					hasDuplicates = true;
					System.Array.Resize(ref models, loadedData.Count);
					for(int i = 0; i < loadedData.Count; i++) {
						models[i] = loadedData[i];
					}
				}
			}
		}

		/// <summary>
		/// Load data from a single TextAsset.
		/// </summary>
		/// <param name="database">Data database.</param>
		public void Load(TextAsset database) {
			List<T> loadedData = GetAllModelsFromText(database);
			models = new T[HighestUID];
			for(int i = 0; i < loadedData.Count; i++) {
				models[((ICriterionData)loadedData[i]).UID] = loadedData[i];
			}
		}

		public void Load(T[] loadData) {
			models = new T[loadData.Length];
			for(int i = 0; i < loadData.Length; i++) {
				if(loadData[i] == null){
					continue;
				}
				models[i] = loadData[i];
				if(((ICriterionData)models[i]).UID > HighestUID) {
					HighestUID = ((ICriterionData)models[i]).UID;
				}
			}
		}

		/// <summary>
		/// Gets a specific piece data from a given UID.
		/// </summary>
		/// <returns>The data.</returns>
		/// <param name="uid">UID.</param>
		public T GetData(int uid) {
			if(uid < 0){
				return default(T);
			}
			if(hasDuplicates) {
				for(int i = 0; i < models.Length; i ++){
					if(((ICriterionData)models[i]).UID == uid){
						return models[i];
					}
				}
			} else {
				if(uid < models.Length) {
					return models[uid];
				}
			}

			return default(T);
		}

		public T AddData(string name) {

			ICriterionData newData = default(T) as ICriterionData;
			newData.UID = HighestUID + 1;
			newData.Name = name;

			HighestUID++;

			System.Array.Resize<T>(ref models, HighestUID + 1);
			T castData = (T)newData;
			models[newData.UID] = castData;
			return castData;
		}

		public void Remove(int uid) {
			T[] newModels = new T[models.Length];
			for(int i = 0; i < models.Length; i ++){
				if(i == uid){
					continue;
				}
				newModels[i] = models[i];
			}
			models = newModels;
		}

		public void Save(IFileSaver fileSaver) {
			JsonWriter writer = new JsonWriter();
			writer.PrettyPrint = true;
			List<T> saveList = new List<T>();

			// Save all data to one single file
			if(!SAVE_INDIVIDUAL_FILES) {
				for(int t = 0; t < models.Length; t++) {
					if(models[t] != null) {
						saveList.Add(models[t]);
					}
				}

				JsonMapper.ToJson(saveList, writer);
				fileSaver.Save(RESOURCE_PATH, RESOURCE_NAME + ".json", writer.ToString(), true);

#if UNITY_EDITOR
				string relativePath = fileSaver.BasePath.Remove(0, fileSaver.BasePath.IndexOf("Assets/"));
				string filePath = relativePath + "/" + RESOURCE_PATH + "/" + RESOURCE_NAME + ".json";
				UnityEditor.AssetDatabase.ImportAsset(filePath, UnityEditor.ImportAssetOptions.ForceUpdate);
#endif
			}
			else if(SAVE_INDIVIDUAL_FILES){
				for(int t = 0; t < models.Length; t ++){
					if(models[t] != null){
						saveList.Add(models[t]);
					}
				}
				for(int s = 0; s < saveList.Count; s ++){
					string fileName = RESOURCE_NAME + "_" + ((ICriterionData)saveList[s]).Name + "_" +
						((ICriterionData)saveList[s]).UID + ".json";
					JsonMapper.ToJson(saveList[s], writer);


#if UNITY_EDITOR
					if(!fileSaver.Save(RESOURCE_PATH, fileName, writer.ToString(), false)) {
						if(!UnityEditor.EditorUtility.DisplayDialog("Data already exists",
							"Would you like to overwrite " + ((ICriterionData)saveList[s]).Name + "?",
							"yes", "oh, no, not at all")) {
						} else {
							fileSaver.Save(RESOURCE_PATH, fileName, writer.ToString(), true);
						}
					}

					string relativePath = fileSaver.BasePath.Remove(0, fileSaver.BasePath.IndexOf("Assets/"));
					string filePath = relativePath + "/" + RESOURCE_PATH + "/" + fileName;
					UnityEditor.AssetDatabase.ImportAsset(filePath, UnityEditor.ImportAssetOptions.ForceUpdate);
#elif
		fileSaver.Save(RESOURCE_PATH, fileName, writer.ToString(), true);
#endif
				}

			}
		}
	}
}