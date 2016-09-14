using UnityEngine;

namespace PickleTools.Criterion {
	public class MemoryView : MonoBehaviour {

		Memory memory;
		public Memory Memory {
			get { return memory; }
			set { memory = value;}
		}

		public void Intialize(Memory singletonMemory){
			memory = singletonMemory;
		}
	}
}