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

		ActionLoader actionLoader;

		public ActionSelectMenu(){
			menu = new GenericMenu();
			actionLoader = new ActionLoader();
			actionLoader.Load();

			TagLoader tagLoader = new TagLoader();
			tagLoader.Load();


			for(int t = 0; t < tagLoader.TagModels.Length; t ++){
				List<ActionModel> categoryEntries = new List<ActionModel>();
				for(int a = 0; a < actionLoader.ActionModels.Length; a ++){
					if(actionLoader.ActionModels[a] == null){
						continue;
					}
					for(int cTag = 0; cTag < actionLoader.ActionModels[a].Tags.Length; cTag ++){
						if(actionLoader.ActionModels[a].Tags[cTag] == tagLoader.TagModels[t].UID){
							categoryEntries.Add(actionLoader.ActionModels[a]);
						}
					}
				}
				AddCategory(tagLoader.TagModels[t].Name, categoryEntries.ToArray());
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
			ActionModel model = actionLoader.GetAction(LastEntrySelected);
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