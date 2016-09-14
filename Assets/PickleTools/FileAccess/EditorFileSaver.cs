using UnityEngine;
using System.Collections;
using System.IO;

namespace PickleTools.FileAccess {
	
	public class EditorFileSaver : IFileSaver {
		
		#region IFileSaver implementation

		private string basePath = "";
		public string BasePath {
			get { return basePath; }
		}

		/// <summary>
		/// Save the specified data as file name in the relativeDirectoryPath.
		/// </summary>
		/// <param name="relativeDirectoryPath">Relative directory path to be appended to the base path. Should not 
		/// have '/'s on either side.</param>
		/// <param name="fileName">File name. Should not have leading '/', but should include file type.</param>
		/// <param name="data">Data to save.</param>
		/// <param name="overwrite">If true, it will overwrite any file that exists. If not, this will return false
		/// if a file exists.</param>
		public bool Save (string relativeDirectoryPath, string fileName, string data, bool overwrite = false)
		{
			string directoryPath = basePath + "/" + relativeDirectoryPath;
			if(!Directory.Exists(directoryPath)){
				Directory.CreateDirectory(directoryPath);
			}
			string filePath = directoryPath + "/" + fileName;
			if(!overwrite && File.Exists(filePath)){
				Debug.LogWarning("<color=#555555>[EditorFileSaver.cs]:</color> Could not save because file " + filePath + 
					" already exists!");
				return false;
			}

			File.WriteAllText(filePath, data);
			Debug.Log("<color=#555555>[EditorFileSaver.cs]:</color> Saved file to " + filePath);
			return true;
		}

		/// <summary>
		/// Load the specified and data from the relative path.
		/// </summary>
		/// <param name="relativePath">Relative path to append to the base path. This should not have a leading '/',
		/// but should include file name and file type.</param>
		/// <param name="data">Data.</param>
		public bool Load (string relativePath, out string data)
		{
			data = "";
			string path = basePath + "/" + relativePath;
			if(!File.Exists(path)){
				Debug.LogWarning("<color=#555555>[EditorFileSaver.cs]:</color> Could not load file " + path + " because it does not exist!");
				return false;
			}
			data = File.ReadAllText(path);
			Debug.Log("<color=#555555>[EditorFileSaver.cs]:</color> Loaded file from " + path);
			return true;
		}


		/// <summary>
		/// Delete the specified file at the relative directory path with file name.
		/// </summary>
		/// <param name="relativeDirectoryPath">Relative directory path.</param>
		/// <param name="fileName">File name.</param>
		public bool Delete (string relativeDirectoryPath, string fileName){
			string directoryPath = basePath + "/" + relativeDirectoryPath;
			if(!Directory.Exists(directoryPath)) {
				return false;
			}
			string filePath = directoryPath + "/" + fileName;
			if(!File.Exists(filePath)) {
				return false;
			}

			File.Delete(filePath);
			Debug.Log("<color=#555555>[EditorFileSaver.cs]:</color> Deleted file at " + filePath);
			string metaFilePath = filePath.Substring(0, filePath.IndexOf(".")) + ".meta";
			File.Delete(metaFilePath);
			Debug.Log("<color=#555555>[EditorFileSaver.cs]:</color> Deleted file at " + metaFilePath);

			return true;
		}

		public string[] GetFiles(string relativeDirectoryPath){
			string[] fileNames = new string[0];

			string directoryPath = basePath + "/" + relativeDirectoryPath;
			if (!Directory.Exists(directoryPath)) {
				return fileNames;
			}

			fileNames = Directory.GetFiles(directoryPath);
			Debug.Log("<color=#555555>[EditorFileSaver.cs]:</color> Got " + fileNames.Length + " files from " + directoryPath);
			return fileNames;
		}
			
		#endregion

		public EditorFileSaver(){
			basePath = Application.persistentDataPath + "/data";
			Debug.Log("<color=#555555>[EditorFileSaver.cs]:</color> Establishing file directory as " + basePath);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:PickleTools.FileAccess.EditorFileSaver"/> class.
		/// </summary>
		/// <param name="path">Path relative to the project directory. We append Application.persistentDataPath + "/" to 
		/// this parameter to create the basePath.</param>
		public EditorFileSaver(string path = ""){
			if(path != ""){
				basePath = Application.persistentDataPath + "/" + path;
			} else {
				basePath = Application.persistentDataPath;
			}
			Debug.Log("<color=#555555>[EditorFileSaver.cs]:</color> Establishing file directory as " + basePath);
		}

	}
}