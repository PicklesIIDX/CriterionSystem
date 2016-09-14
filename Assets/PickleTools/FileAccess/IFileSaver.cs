using UnityEngine;
using System.Collections;

namespace PickleTools.FileAccess {
	public interface IFileSaver {

		string BasePath {
			get;
		}

		bool Save(string relativeDirectoryPath, string fileName, string data, bool overwrite = false);

		bool Delete(string relativeDirectoryPath, string fileName);

		bool Load(string fileName, out string data);

		string[] GetFiles(string relativeDirectoryPath);

	}
}