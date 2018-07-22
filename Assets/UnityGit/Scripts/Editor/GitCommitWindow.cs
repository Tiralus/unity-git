
namespace UnityGit
{
	using UnityEditor;
	using UnityEngine;

	public class GitCommitWindow : EditorWindow
	{
		private bool commit = false;
		
		/// <summary>
		/// The current position of the scroll bar in the GUI.
		/// </summary>
		private Vector2 scrollPosition;
		
		public static GitCommitWindow ShowCommitWindow()
		{
			var window = GetWindow<GitCommitWindow>();
			window.titleContent = new GUIContent("Commit");
			return window;
		}

		public void OnGui()
		{	
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			EditorGUILayout.BeginVertical();
			EditorGUILayout.BeginHorizontal();
			string title = EditorGUILayout.TextField("Title: ");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			string message = EditorGUILayout.TextField("Message: ");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			commit = GUILayout.Button("Commit");
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}

		public void CallGitCommit(string title, string message)
		{
			
		}

		private void OnInspectorUpdate()
		{
		}
	}
}
