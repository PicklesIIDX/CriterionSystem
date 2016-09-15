using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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

		CriterionDataLoader<ConditionModel> conditionLoader;

		public ConditionSelectMenu(CriterionDataLoader<ConditionModel> newConditionLoader = null, CriterionDataLoader<TagModel> newTagLoader = null){
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
		public void Refresh(CriterionDataLoader<ConditionModel> newConditionLoader, CriterionDataLoader<TagModel> newTagLoader){
			if(menu == null){
				menu = new GenericMenu();
			}

			conditionLoader = newConditionLoader;
			conditionLoader = new CriterionDataLoader<ConditionModel>();
			conditionLoader.Load();

			CriterionDataLoader<TagModel> tagLoader = newTagLoader;
			if(tagLoader == null){
				tagLoader = new CriterionDataLoader<TagModel>();;
				tagLoader.Load();
			}



			for(int t = 0; t < tagLoader.Models.Length; t ++){
				if(tagLoader.Models[t] == null){
					continue;
				}
				List<ConditionModel> categoryEntries = new List<ConditionModel>();
				for(int c = 0; c < conditionLoader.Models.Length; c ++){
					if(conditionLoader.Models[c] == null){
						continue;
					}
					for(int cTag = 0; cTag < conditionLoader.Models[c].Tags.Count; cTag ++){
						if(conditionLoader.Models[c].Tags[cTag] == tagLoader.Models[t].UID){
							categoryEntries.Add(conditionLoader.Models[c]);
						}
					}
				}
				AddCategory(tagLoader.Models[t].Name, categoryEntries.ToArray());
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
			ConditionModel model = conditionLoader.GetData(LastEntrySelected);
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