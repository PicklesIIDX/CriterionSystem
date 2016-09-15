using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PickleTools.Criterion {
	public delegate void ActionSelectHandler (ActionSelectMenu menu, int item);

	[System.Serializable]
	public class ActionSelectMenu {

		public event ActionSelectHandler EntrySelected;

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

		CriterionDataLoader<ActionModel> actionLoader;

		public ActionSelectMenu(){
			menu = new GenericMenu();
			actionLoader = new CriterionDataLoader<ActionModel>();
			actionLoader.Load();

			CriterionDataLoader<TagModel> tagLoader = new CriterionDataLoader<TagModel>();
			tagLoader.Load();


			for(int t = 0; t < tagLoader.Models.Length; t ++){
				List<ActionModel> categoryEntries = new List<ActionModel>();
				for(int a = 0; a < actionLoader.Models.Length; a ++){
					if(actionLoader.Models[a] == null){
						continue;
					}
                    for(int cTag = 0; cTag < actionLoader.Models[a].Tags.Length; cTag ++){
						if(actionLoader.Models[a].Tags[cTag] == tagLoader.Models[t].UID){
							categoryEntries.Add(actionLoader.Models[a]);
						}
					}
				}
				AddCategory(tagLoader.Models[t].Name, categoryEntries.ToArray());
			}

			EntrySelected += HandleEntrySelected;
		}

		void OnDestroy(){
			EntrySelected -= HandleEntrySelected;
		}


		public void AddCategory(string categoryTitle, ActionModel[] categoryEntries){
			for(int i = 0; i < categoryEntries.Length; i ++){
				menu.AddItem(new GUIContent(categoryTitle + "/" + categoryEntries[i].Name),
					false, SelectMenuEntry, categoryEntries[i].UID);
			}
		}

		void HandleEntrySelected(ActionSelectMenu menu, int item){
			LastEntrySelected = item;
		}

		public int DrawSelectMenu(string tooltip, Vector2 mousePosition, float screenWidth, GUISkin skin, params GUILayoutOption[] options){
			ActionModel model = actionLoader.GetData(LastEntrySelected);
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
			currentSelection = (int)obj;
			if(EntrySelected != null){
				EntrySelected(this, currentSelection);
			}
		}
	}

}