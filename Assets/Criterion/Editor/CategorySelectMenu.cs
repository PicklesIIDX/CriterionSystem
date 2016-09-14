using UnityEngine;
using UnityEditor;

namespace PickleTools.UnityEditor {
	public delegate void EntryHandler <T> (CategorySelectMenu<T> menu, T item);

	public class CategorySelectMenu<T> {

		public event EntryHandler<T> EntrySelected;

		private GenericMenu menu;
		public GenericMenu Menu {
			get { return menu; }
		}

		private T currentSelection;
		public T CurrentSelection {
			get { return currentSelection; }
		}

		public CategorySelectMenu(){
			menu = new GenericMenu();
		}

		public virtual void DrawMenu(Rect rect){
			menu.DropDown(rect);
		}

		public void AddCategory(T categoryTitle, T[] categoryEntries){
			for(int i = 0; i < categoryEntries.Length; i ++){
				menu.AddItem(new GUIContent(categoryTitle + "/" + categoryEntries[i]),
				             false, SelectMenuEntry, categoryEntries[i]);
			}
		}

		void SelectMenuEntry(object obj){
			currentSelection = (T)obj;
			if(EntrySelected != null){
				EntrySelected(this, currentSelection);
			}
		}
	}
}

