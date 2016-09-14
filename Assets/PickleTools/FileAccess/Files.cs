using UnityEngine;
using UnityEditor;
using System.Collections;

namespace PickleTools.FileAccess {
	
	public class Files {

		public static T[] GetAtPath<T>(string path) {
			ArrayList arrayList = new ArrayList();
			if (!System.IO.Directory.Exists(Application.dataPath + "/" + path)) {
				Debug.LogWarning("[BuilderDrawFunctions.cs]: There is no directory " + Application.dataPath + "/" + path);
				return null;
			}

			string[] fileEntries = System.IO.Directory.GetFiles(Application.dataPath + "/" + path);
			foreach (string fileName in fileEntries) {
				int assetPathIndex = fileName.IndexOf("Assets", System.StringComparison.Ordinal);
				string localPath = fileName.Substring(assetPathIndex);
				Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

				if (t != null) {
					arrayList.Add(t);
				}
			}
			T[] result = new T[arrayList.Count];
			for (int i = 0; i < arrayList.Count; i++)
				result[i] = (T)arrayList[i];

			return result;
		}

	}

}