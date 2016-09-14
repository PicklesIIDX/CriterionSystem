using UnityEngine;
using System.Collections.Generic;
using LitJson;
using PickleTools.FileAccess;

namespace PickleTools.Criterion {
	
	public class SequenceLoader {

		SequenceModel[] sequenceModels = new SequenceModel[0];
		public SequenceModel[] SequenceModels { 
			get { return sequenceModels; }
		}

		public int HighestUID = 0;

		private static readonly string SEQUENCE_PATH = "data/sequences";

		public SequenceLoader(){

		}

		public void Load(string sequencePath = ""){
			if(sequencePath == ""){
				sequencePath = SEQUENCE_PATH;
			}
			sequenceModels = new SequenceModel[0];
			Debug.Log("[Loading Resources From]: " + sequencePath);
			TextAsset[] databaseFiles =  Resources.LoadAll<TextAsset>(sequencePath);
			for(int d = 0; d < databaseFiles.Length; d ++){
				List<SequenceModel> loadedSequences = new List<SequenceModel>();
				loadedSequences = JsonMapper.ToObject<List<SequenceModel>>(databaseFiles[d].text);
				for(int i = 0; i < loadedSequences.Count; i ++){
					if(loadedSequences[i].UID >= HighestUID){
						HighestUID = loadedSequences[i].UID + 1;
					}
				}
				System.Array.Resize(ref sequenceModels, HighestUID + 1);
				for(int i = 0; i < loadedSequences.Count; i ++){
					sequenceModels[loadedSequences[i].UID] = loadedSequences[i];
				}
			}
		}

		public void Load(TextAsset database){
			sequenceModels = new SequenceModel[0];
			List<SequenceModel> loadedSequences =  JsonMapper.ToObject<List<SequenceModel>>(database.text);
			for(int i = 0; i < loadedSequences.Count; i ++){
				if(loadedSequences[i].UID > HighestUID){
					HighestUID = loadedSequences[i].UID;
				}
			}
			sequenceModels = new SequenceModel[loadedSequences.Count];
			for(int i = 0; i < loadedSequences.Count; i ++){
				sequenceModels[loadedSequences[i].UID] = loadedSequences[i];
			}
		}

		public SequenceModel GetSequence(int uid){
			if(uid > -1 && uid < sequenceModels.Length){
				return sequenceModels[uid];
			} else {
				return null;
			}
		}

		public SequenceModel AddSequence(string name){
			SequenceModel newSequence = new SequenceModel();
			newSequence.Name = name;
			newSequence.UID = HighestUID;
			newSequence.Actions = new SequenceActionModel[0];

			HighestUID ++;

			System.Array.Resize<SequenceModel>(ref sequenceModels, HighestUID + 1);
			sequenceModels[newSequence.UID] = newSequence;

			return newSequence;
		}

		public SequenceModel AddSequence(string name, int uid){
			SequenceModel newSequence = new SequenceModel();
			newSequence.Name = name;
			newSequence.UID = uid;
			newSequence.Actions = new SequenceActionModel[0];

			if(newSequence.UID >= HighestUID){
				HighestUID = newSequence.UID + 1;
				System.Array.Resize<SequenceModel>(ref sequenceModels, HighestUID + 1);
			}
			sequenceModels[newSequence.UID] = newSequence;

			return newSequence;
		}

		public void Remove(int uid){
			sequenceModels[uid] = null;
		}

		public void Save(IFileSaver fileSaver){
			List<SequenceModel> saveList = new List<SequenceModel>();
			for(int t = 0; t < sequenceModels.Length; t ++){
				if(sequenceModels[t] != null){
					saveList.Add(sequenceModels[t]);
				}
			}
			JsonWriter writer = new JsonWriter();
			writer.PrettyPrint = true;
			JsonMapper.ToJson(saveList, writer);
			fileSaver.Save(SEQUENCE_PATH, "sequences.json", writer.ToString(), true);

			#if UNITY_EDITOR
			string relativePath = fileSaver.BasePath.Remove(0, fileSaver.BasePath.IndexOf("Assets/"));
			string filePath = relativePath + "/" + SEQUENCE_PATH + "/" + "sequences.json";
			UnityEditor.AssetDatabase.ImportAsset(filePath, UnityEditor.ImportAssetOptions.ForceUpdate);
			#endif
		}
	}
}