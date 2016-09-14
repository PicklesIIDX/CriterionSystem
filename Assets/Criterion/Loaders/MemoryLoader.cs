using UnityEngine;
using System.Collections.Generic;
using LitJson;
using PickleTools.FileAccess;

namespace PickleTools.Criterion {
	
	public class MemoryLoader {

		List<MemoryModel> memoryModels = new List<MemoryModel>();
		public List<MemoryModel> MemoryModels { 
			get { return memoryModels; }
		}

		private static readonly string RESOURCE_PATH = "data/memory";

		public MemoryLoader(){

		}

		public void Load(IFileSaver fileSaver, string resourcePath = ""){
			if(resourcePath == ""){
				resourcePath = RESOURCE_PATH;
			}
			memoryModels.Clear();
			Debug.Log("[Loading Resources From]: " + resourcePath);
			TextAsset[] databaseFiles =  Resources.LoadAll<TextAsset>(resourcePath);
			for(int d = 0; d < databaseFiles.Length; d ++){
				memoryModels.Add(JsonMapper.ToObject<MemoryModel>(databaseFiles[d].text));
				Debug.Log("[MemoryLoader.cs]: Loaded memory:\n" + databaseFiles[d].text);
			}
			if(memoryModels.Count == 0){
				memoryModels.Add(new MemoryModel());
				Save(memoryModels[0], 0, fileSaver);
			}
		}

		public MemoryModel GetMemory(int slot){
			if(slot < memoryModels.Count){
				return memoryModels[slot];
			}
			return null;
		}

		public void Save(MemoryModel model, int slot, IFileSaver fileSaver){
			JsonWriter writer = new JsonWriter();
			writer.PrettyPrint = true;
			// save to the resource path
			JsonMapper.ToJson(model, writer);
			fileSaver.Save(RESOURCE_PATH, "slot_" + slot + ".json", writer.ToString());
			return;
		}
	}
}