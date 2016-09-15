namespace PickleTools.UnityEditor {

	public class EditorDeltaTimeCheck {
		private long lastTime;
		private System.Diagnostics.Stopwatch watch;
		
		private float editorDeltaTime;
		public float EditorDeltaTime{
			get { 
				editorDeltaTime = (float)((watch.ElapsedMilliseconds - lastTime)) / 1000.0f;
				if(lastTime != watch.ElapsedMilliseconds){
					lastTime = watch.ElapsedMilliseconds;
				}
				return editorDeltaTime; 
			}
		}

		public EditorDeltaTimeCheck(){
			watch = new System.Diagnostics.Stopwatch();
			watch.Start();
		}

		public void UpdateDelta(){
			lastTime = watch.ElapsedMilliseconds;
		}
	}

}