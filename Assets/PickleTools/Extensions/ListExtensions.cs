using UnityEngine;
using System.Collections.Generic;
using System.Threading;

namespace PickleTools.Extensions.ListExtensions {
	public static class ListExtensions {

		public static bool Contains<T>(this List<T> source, List<T> checkList){
			for(int i = 0; i < checkList.Count; i ++){
				if(source.Contains(checkList[i])){
					return true;
				}
			}
			return false;
		}
		
		public static string ToString<T>(this List<T> source){
			string list = "";
			for(int i = 0; i < source.Count; i ++){
				list += source[i].ToString();
				if(i != source.Count - 1){
					list += ", ";
				}
			}
			return list;
		}
		
		public static void Shuffle<T>(this List<T> list){
			int n = list.Count;
			while (n > 1){
				n --;
				int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
		
	}

	public static class ThreadSafeRandom{
		[System.ThreadStatic] 
		private static System.Random Local;
		
		public static System.Random ThisThreadsRandom {
			get { 
				if(Local == null){
					Local = new System.Random(unchecked(System.Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId));
				}
				return Local;
			}
		}
	}
}
