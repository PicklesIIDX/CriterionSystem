using System.Collections;

namespace PickleTools.ValueTypes {
	public struct DropOutStack<T> {

		private T[] items;
		private int top;
		private bool countChanged;
		private int count;
		public int Count {
			get {
				if(countChanged){
					count = 0;
					for(int i = 0; i < items.Length; i ++){
						if(items[i] != null){
							count ++;
						}
					}
				}
				return count; 
			}
		}

		public DropOutStack(int capacity = 0){
			items = new T[capacity];
			top = 0;
			count = 0;
			countChanged = false;
		}

		public void Push(T item){
			items[top] = item;
			top = (top + 1) % items.Length;
			countChanged = true;
		}

		public T Pop(){
			top = (items.Length + top - 1) % items.Length;
			T item = items[top];
			items[top] = default(T);
			countChanged = true;
			return item;
		}

		public void Clear(){
			for(int i = 0; i < items.Length; i ++){
				items[i] = default(T);
			}
			countChanged = true;
		}

	}
}