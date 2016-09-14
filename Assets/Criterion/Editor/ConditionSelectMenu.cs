using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using PickleTools.UnityEditor;

namespace PickleTools.Criterion {
	public delegate void ConditionSelectHandler (ConditionSelectMenu menu, int item);

	[System.Serializable]
	public class ConditionSelectMenu {

		public event ConditionSelectHandler EntrySelected;

		private GenericMenu menu;
		public GenericMenu Menu {
			get { return menu; }
		}

		private int currentSelection;
		public int CurrentSelection {
			get { return currentSelection; }
		}

		[SerializeField]
		public int LastEntrySelected = -1;

		ConditionLoader conditionLoader;

		public ConditionSelectMenu(ConditionLoader newConditionLoader = null, TagLoader newTagLoader = null){
			menu = new GenericMenu();

			Refresh(newConditionLoader, newTagLoader);

			EntrySelected += HandleEntrySelected;
		}

		void OnDestroy(){
			EntrySelected -= HandleEntrySelected;
		}

		/// <summary>
		/// Reloads the conditions and tags to ensure our list is up to date.
		/// </summary>
		public void Refresh(ConditionLoader newConditionLoader, TagLoader newTagLoader){
			if(menu == null){
				menu = new GenericMenu();
			}

			conditionLoader = newConditionLoader;
			conditionLoader = new ConditionLoader();
			conditionLoader.Load();

			TagLoader tagLoader = newTagLoader;
			if(tagLoader == null){
				tagLoader = new TagLoader();;
				tagLoader.Load();
			}



			for(int t = 0; t < tagLoader.TagModels.Length; t ++){
				if(tagLoader.TagModels[t] == null){
					continue;
				}
				List<ConditionModel> categoryEntries = new List<ConditionModel>();
				for(int c = 0; c < conditionLoader.ConditionModels.Length; c ++){
					if(conditionLoader.ConditionModels[c] == null){
						continue;
					}
					for(int cTag = 0; cTag < conditionLoader.ConditionModels[c].Tags.Count; cTag ++){
						if(conditionLoader.ConditionModels[c].Tags[cTag] == tagLoader.TagModels[t].UID){
							categoryEntries.Add(conditionLoader.ConditionModels[c]);
						}
					}
				}
				AddCategory(tagLoader.TagModels[t].Name, categoryEntries.ToArray());
			}
		}

		public void AddCategory(string categoryTitle, ConditionModel[] categoryEntries){
			for(int i = 0; i < categoryEntries.Length; i ++){
				int uid = 0;
				string categoryName = "no_name";
				if(categoryEntries[i] != null){
					categoryName = categoryEntries[i].Name;
					uid = categoryEntries[i].UID;
				}
				menu.AddItem(new GUIContent(categoryTitle + "/" + categoryName),
					false, SelectMenuEntry, uid);
			}
		}

		void HandleEntrySelected(ConditionSelectMenu menu, int item){
			LastEntrySelected = item;
		}

		public int DrawSelectMenu(string tooltip, Vector2 mousePosition, float screenWidth, GUISkin skin, params GUILayoutOption[] options){
			//if(conditionLoader == null){
			//	conditionLoader = new ConditionLoader();
			//	conditionLoader.Load();
			//}
			ConditionModel model = conditionLoader.GetCondition(LastEntrySelected);
			if(model == null){
				return LastEntrySelected;
			}
			GUIContent buttonTooltip = new GUIContent(model.Name, tooltip);
			if(GUILayout.Button(buttonTooltip, skin.button, options)){
				float posX = Event.current.mousePosition.x - screenWidth * 0.2f;
				float posY = Event.current.mousePosition.y;
				menu.DropDown(new Rect(posX, posY, screenWidth * 0.4f, 0));
			}
			return LastEntrySelected;
		}

		void SelectMenuEntry(object obj){
			LastEntrySelected = currentSelection = (int)obj;

			if(EntrySelected != null){
				EntrySelected(this, currentSelection);
			}
		}
	}

}