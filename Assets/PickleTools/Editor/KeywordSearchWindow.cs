using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using PickleTools.Criterion;

namespace PickleTools.Criterion {

	[System.Serializable]
	public class KeywordSearchOptions {
		public bool ContainsAllKeywords = false;
		public bool IgnoreCapitalization = true;
		public bool RuleName = true;
		public bool ConditionName = false;
		public bool ConditionValue = false;
		public bool ActionName = false;
		public bool ActionValue = false;
	}

	public class KeywordSearchWindow : EditorWindow {

		string keywords = "";
		string[] keywordList = new string[0];

		TriggerLoader triggerLoader;
		SequenceLoader sequenceLoader;
		ConditionLoader conditionLoader;
		ActionLoader actionLoader;

		List<int> matchedItems = new List<int>();
		Vector2 scrollPosition = Vector2.zero;

		[SerializeField]
		KeywordSearchOptions options;

		[MenuItem("Window/Queryer/Keyword Search")]
		static void ShowEditor(){
			KeywordSearchWindow window = CreateInstance<KeywordSearchWindow>();
			window.titleContent = new GUIContent("Keyword Search Window");
			window.Show();
		}

		void OnFocus(){

			triggerLoader = new TriggerLoader();
			triggerLoader.Load();
			conditionLoader = new ConditionLoader();
			conditionLoader.Load();
			sequenceLoader = new SequenceLoader();
			sequenceLoader.Load();
			actionLoader = new ActionLoader();
			actionLoader.Load();

			if(options == null){
				options = new KeywordSearchOptions();
			}
		}

		void OnGUI(){
			bool updateSearch = false;
			// show toggle options
			EditorGUI.BeginChangeCheck();
			options.RuleName = EditorGUILayout.Toggle("Search by rule name", options.RuleName);
			options.ConditionName = EditorGUILayout.Toggle("Search by fact name", options.ConditionName);
			options.ConditionValue = EditorGUILayout.Toggle("Search by fact value", options.ConditionValue);
			options.ActionName = EditorGUILayout.Toggle("Search by action name", options.ActionName);
			options.ActionValue = EditorGUILayout.Toggle("Search by action value", options.ActionValue);
			options.ContainsAllKeywords = EditorGUILayout.Toggle("Contains all keywords", options.ContainsAllKeywords);
			options.IgnoreCapitalization = EditorGUILayout.Toggle("Ignore Capitalization", options.IgnoreCapitalization);
			if(EditorGUI.EndChangeCheck()){
				updateSearch = true;
			}
			// display text box to enter keywords
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(new GUIContent("Keywords", "Use commas to separate keywords"));
			keywords = GUILayout.TextField(keywords, GUILayout.ExpandWidth(true));
			GUILayout.EndHorizontal();
			string[] newKeywordList = keywords.Split(',');
			for(int k = 0; k < newKeywordList.Length; k ++){
				newKeywordList[k] = newKeywordList[k].TrimEnd(' ').TrimStart(' ');
			}

			if(newKeywordList.Length != keywordList.Length){
				updateSearch = true;
			} else {
				for(int k = 0; k < keywordList.Length; k ++){
					if(keywordList[k] != newKeywordList[k]){
						updateSearch = true;
						break;
					}
				}
			}

			if(updateSearch){
				keywordList = newKeywordList;
				matchedItems.Clear();
				// parse keywords
				// name search
				if(options.RuleName){
					matchedItems.AddRange(GetTriggersByName(triggerLoader.TriggerModels, keywordList, options));
				}
				// rule by fact search
				if(options.ConditionName){
					matchedItems.AddRange(GetTriggersByConditionName(triggerLoader.TriggerModels, keywordList, options, conditionLoader));
				}
				// rule by fact value search
				if(options.ConditionValue){
					matchedItems.AddRange(GetTriggersByConditionValue(triggerLoader.TriggerModels, keywordList, options));
				}
				// response by action name
				if(options.ActionName){
					matchedItems.AddRange(GetSequencesByActionName(sequenceLoader.SequenceModels, keywordList, options));
				}
				// response by action value
				if(options.ActionValue){
					matchedItems.AddRange(GetSequencesByActionValue(sequenceLoader.SequenceModels, keywordList, options));
				}
				// rule/response by database

			}
			// display list of rules/responses that match
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			for(int m = 0; m < matchedItems.Count; m ++){
				if(GUILayout.Button(triggerLoader.GetTrigger(matchedItems[m]).Name)){
					Debug.Log("[KeywordSearchWindow.cs]: Selected item: " + matchedItems[m]);
					// TODO: Select in Trigger Edit Window && Action Map Window
				}
			}
			EditorGUILayout.EndScrollView();
		}

		public static List<int> GetTriggersByName(TriggerModel[] triggerModelArray, string[] keywords, KeywordSearchOptions searchOptions){
			List<int> matchedItemList = new List<int>();
			// name search
			for(int t = 0; t < triggerModelArray.Length; t ++){
				if(triggerModelArray[t] == null){
					continue;
				}
				string name = triggerModelArray[t].Name;
				if(matchedItemList.Contains(triggerModelArray[t].UID)){
					continue;
				}
				int keywordContainCount = 0;
				for(int k = 0; k < keywords.Length; k ++){
					string keywordName = keywords[k];
					
					if(searchOptions.IgnoreCapitalization){
						name = name.ToLower();
						keywordName = keywordName.ToLower();
					}
					
					if(name.Contains(keywordName)){
						keywordContainCount ++;
						if(!searchOptions.ContainsAllKeywords){
							matchedItemList.Add(triggerModelArray[t].UID);
							break;
						}
					}
					if(searchOptions.ContainsAllKeywords && keywordContainCount >= keywords.Length){
						matchedItemList.Add(triggerModelArray[t].UID);
					}
				}
			}
			return matchedItemList;
		}

		public static List<int> GetTriggersByConditionName(TriggerModel[] triggerModelArray, string[] keywords, 
			KeywordSearchOptions searchOptions, ConditionLoader loader){
			List<int> matchedItemList = new List<int>();
			for(int t = 0; t < triggerModelArray.Length; t ++){
				TriggerModel triggerModel = triggerModelArray[t];
				if(matchedItemList.Contains(triggerModel.UID) || triggerModel.Conditions.Length == 0){
					continue;
				}
				bool addTrigger = false;
				
				int keywordContainCount = 0;
				for (int k = 0; k < keywords.Length; k ++){
					string keywordName = keywords[k];
					if(searchOptions.IgnoreCapitalization){
						keywordName = keywordName.ToLower();
					}
					for(int c = 0; c < triggerModel.Conditions.Length; c ++){
						string conditionName = loader.GetCondition(triggerModel.Conditions[c].UID).Name;
						if(searchOptions.IgnoreCapitalization){
							conditionName = conditionName.ToLower();
						}

						if(conditionName.Contains(keywordName)){
							keywordContainCount ++;
							break;
						}
					}
				}
				if((searchOptions.ContainsAllKeywords && keywordContainCount >= keywords.Length) ||
				   (!searchOptions.ContainsAllKeywords && keywordContainCount > 0) ){
					addTrigger = true;
				}
				if(addTrigger){
					matchedItemList.Add(triggerModel.UID);
				}
			}
			return matchedItemList;
		}
		
		public static List<int> GetTriggersByConditionValue(TriggerModel[] triggerModelArray, string[] keywords, KeywordSearchOptions searchOptions){
			List<int> matchedItemList = new List<int>();
			
			for(int t = 0; t < triggerModelArray.Length; t ++){
				TriggerModel triggerModel = triggerModelArray[t];
				if(matchedItemList.Contains(triggerModel.UID) || triggerModel.Conditions.Length == 0){
					continue;
				}
					
				bool addTrigger = false;
				int keywordContainCount = 0;
				
				for (int k = 0; k < keywords.Length; k ++){
					string keywordName = keywords[k];
					if(searchOptions.IgnoreCapitalization){
						keywordName = keywordName.ToLower();
					}
					for(int c = 0; c < triggerModel.Conditions.Length; c ++){
						string upperBound = triggerModel.Conditions[c].UpperBound.ToString();
						string lowerBound = triggerModel.Conditions[c].LowerBound.ToString();
						if(searchOptions.IgnoreCapitalization){
							upperBound = upperBound.ToLower();
							lowerBound = lowerBound.ToLower();
						}

						if(upperBound.Contains(keywordName) ||
						   lowerBound.Contains(keywordName)){
							keywordContainCount ++;
							break;
						}
					}
				}
				if((searchOptions.ContainsAllKeywords && keywordContainCount >= keywords.Length) ||
				   (!searchOptions.ContainsAllKeywords && keywordContainCount > 0) ){
					addTrigger = true;
				}
				if(addTrigger){
					matchedItemList.Add(triggerModel.UID);
				}
			}
			return matchedItemList;
		}
		
		public static List<int> GetSequencesByActionName(SequenceModel[] sequenceModelArray, string[] keywords, KeywordSearchOptions searchOptions){
			List<int> matchedItemList = new List<int>();
			for(int s = 0; s < sequenceModelArray.Length; s ++){
				SequenceModel sequenceModel = sequenceModelArray[s];
				if(matchedItemList.Contains(sequenceModel.UID) || sequenceModel.Actions.Length == 0){
					continue;
				}
				bool addSequence = false;
				
				int keywordContainCount = 0;
				for (int k = 0; k < keywords.Length; k ++){
					string keywordName = keywords[k];
					if(searchOptions.IgnoreCapitalization){
						keywordName = keywordName.ToLower();
					}
					keywordContainCount += CheckActionName(sequenceModel.Actions, keywordName, searchOptions);
				}
				if((searchOptions.ContainsAllKeywords && keywordContainCount >= keywords.Length) ||
				   (!searchOptions.ContainsAllKeywords && keywordContainCount > 0) ){
					addSequence = true;
				}
				if(addSequence){
					matchedItemList.Add(sequenceModel.UID);
				}
			}
			return matchedItemList;
		}

		private static int CheckActionName(SequenceActionModel[] sequenceActionModels, string keyword, KeywordSearchOptions searchOptions){
			int matches = 0;
			ActionLoader actionLoader = new ActionLoader();
			actionLoader.Load();
			for(int a = 0; a < sequenceActionModels.Length; a ++){
				matches += CheckActionName(sequenceActionModels[a].Then, keyword, searchOptions);
				if(searchOptions.ContainsAllKeywords && matches > 0){
					break;
				}
				ActionModel actionModel = actionLoader.GetAction(sequenceActionModels[a].UID);
				string actionName = "";
				if(actionModel != null){
					actionName = actionModel.Name;
				}
				if(searchOptions.IgnoreCapitalization){
					actionName = actionName.ToLower();
				}
				if(actionName.Contains(keyword)){
					matches ++;
					break;
				}
			}
			return matches;
		}
		
		public static List<int> GetSequencesByActionValue(SequenceModel[] sequenceModelArray, string[] keywords, KeywordSearchOptions searchOptions){
			List<int> matchedItemList = new List<int>();
			
			for(int s = 0; s < sequenceModelArray.Length; s ++){
				SequenceModel sequenceModel = sequenceModelArray[s];
				if(matchedItemList.Contains(sequenceModel.UID) || sequenceModel.Actions.Length == 0){
					continue;
				}
				
				bool addSequence = false;
				int keywordContainCount = 0;

				for (int k = 0; k < keywords.Length; k ++){
					string keywordName = keywords[k];
					if(searchOptions.IgnoreCapitalization){
						keywordName = keywordName.ToLower();
					}
					keywordContainCount += CheckActionValue(sequenceModel.Actions, keywordName, searchOptions);
				}
				if((searchOptions.ContainsAllKeywords && keywordContainCount >= keywords.Length) ||
				   (!searchOptions.ContainsAllKeywords && keywordContainCount > 0) ){
					addSequence = true;
				}
				if(addSequence){
					matchedItemList.Add(sequenceModel.UID);
				}
			}
			return matchedItemList;
		}

		static int CheckActionValue(SequenceActionModel[] sequenceActionModel, string keyword, KeywordSearchOptions searchOptions){
			int matches = 0;
			for(int a = 0; a < sequenceActionModel.Length; a ++){
				matches += CheckActionValue(sequenceActionModel[a].Then, keyword, searchOptions);
				if(searchOptions.ContainsAllKeywords && matches > 0){
					break;
				}
				bool matchedParameter = false;
				for(int p = 0; p < sequenceActionModel[a].Parameters.Length; p ++){
					string parameterValue = sequenceActionModel[a].Parameters[p].ToString();
					if(searchOptions.IgnoreCapitalization){
						parameterValue = parameterValue.ToLower();
					}
					
					if(parameterValue.Contains(keyword)){
						matches ++;
						matchedParameter = true;
						break;
					}
				}
				if(matchedParameter){
					break;
				}
			}
			return matches;
		}

//		public static List<int> SampleSearchFunction(TriggerModel[] triggerModelArray, string[] keywords, KeywordSearchOptions searchOptions){
//			List<int> matchedItemList = new List<int>();
//			
//			for(int t = 0; t < triggerModelArray.Length; t ++){
//				if(matchedItemList.Contains(triggerModelArray[t].UID)){
//					continue;
//				}
//				int keywordContainCount = 0;
//				for (int k = 0; k < keywords.Length; k ++){
//					string keywordName = keywords[k];
//	
//				}
//			}
//		}
	}

}