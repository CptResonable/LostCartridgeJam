#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DME {
	/// <summary>
	/// This window shows how you can listen for and consume user input events
	/// from the Scene View. Super useful for making editor tools!
	/// </summary>
	public class MeshEditor : EditorWindow {
		public static event Brush.EmptyDelegate enableBrushEvent;
		public static event Brush.EmptyDelegate disableBrushEvent;
		
		private Brush brush;
		private string brushPath; // File path of brush prefab.
		private string terrainObjectPath; // File path of terrain object prefab.
		
		public static Camera sceneViewCamera;
		
		// Terrain object creator
		private bool objectCreatorActive = false;
		private string newTerrainName = "TerrainObject";
		private Vector2Int chunkSize = new Vector2Int(200, 200);
		private float vertexDensity = 0.5f;
		private bool isFlatShaded = false;

		// Input
		public static bool lmb = false;
		public static bool lmb_down = false;
		public static bool lmb_up = false;
		public static Vector2 mousePosition;

		Vector2 newChunkCoords;

		// GUI styles
		GUIStyle style_header = new GUIStyle();
		GUIStyle style_bold = new GUIStyle();
		
		#region Enums 
		public enum Buttons { undo, addChunk };
		public enum Directions { globalY, normal, brushToCamera, cameraZ, contract };
		public enum Floats { radius, editStrength, editSmoothing, paintStrength, paintSmoothing, smoothingStrength };
		public enum Bools { enableDisable, subtract, editMesh, paint, smooth };
		public enum Chanel { All, Red, Green, Blue, Alpha };
		private enum PaintType { Off, Color1, Color2, Color3};
		private enum GrassPaintType { Off, AddGrass, RemoveGrass};
		#endregion
		
		#region Delegates
		public delegate void EmptyDelegate();
		public delegate void ColorDelegate(Color32 color);
		public delegate void DirectionDelegate(Directions direction);
		public delegate void ButtonDelegate(Buttons button);
		public delegate void FloatDelegate(Floats setting, float value);
		public delegate void BoolDelegate(Bools setting, bool value);
		public delegate void PaintChannelDelegates(PaintChannels activeChannels);
		#endregion

		#region Events
		
		// Input events
		public static event EmptyDelegate lmb_downEvent;
		public static event EmptyDelegate lmb_upEvent;

		// Setting change events
		public static event DirectionDelegate directionEvent;
		public static event ColorDelegate colorEvent;
		public static event ButtonDelegate buttonEvent;
		public static event FloatDelegate floatEvent;
		public static event BoolDelegate boolEvent;
		public static event PaintChannelDelegates paintChannelEvent;
		#endregion

		#region Properties
		#region Editor properties
		public static bool isActive = false;
		private bool editorActive {
			get => isActive;
			set {
				if (isActive != value) {
					isActive = value;

					if (isActive) {

						if (brush == null) {
							GameObject goBrush = Instantiate(AssetDatabase.LoadAssetAtPath(brushPath, typeof(GameObject))) as GameObject;
							goBrush.name = "Brush";
							goBrush.transform.position = currentTerrainObject.transform.position;
							brush = goBrush.GetComponent<Brush>();
						}
						
						SendAllValues();
					}
					else {
						if (brush != null) {
							DestroyImmediate(brush.gameObject);
						}
					}
				}
			}
		}
		
		[SerializeField] [HideInInspector] MeshTerrain _currentTerrainObject;
		MeshTerrain currentTerrainObject {
			get { return _currentTerrainObject; }
			set {
				if (_currentTerrainObject != value) {
					_currentTerrainObject = value;

					if (_currentTerrainObject != null)
						editorActive = true;
					else
						editorActive = false;
				}
			}
		}
		#endregion

		#region Brush properties

		Color32 _paintColor;
		Color32 paintColor {
			get { return _paintColor; }
			set {
				_paintColor = value;
				if (colorEvent != null)
					colorEvent.Invoke(_paintColor);
				
				// if (!paintTypeColors.ContainsValue(_paintColor) && paintType != PaintType.Off)
				// 	paintType = PaintType.Off;
			}
		}

		// Direction.
		Directions _direction = Directions.globalY;
		Directions direction {
			get { return _direction; }
			set {
				if (_direction != value) {
					_direction = value;
					if (directionEvent != null)
						directionEvent.Invoke(direction);
				}
			}
		}

		// Floats.
		[SerializeField] [HideInInspector] float _brushSize;
		float brushSize {
			get { return _brushSize; }
			set {
				if (_brushSize != value) {
					_brushSize = value;
					if (floatEvent != null)
						floatEvent.Invoke(Floats.radius, _brushSize);
				}
			}
		}

		[SerializeField] [HideInInspector] float _editStrength;
		float brushStrength {
			get { return _editStrength; }
			set {
				if (_editStrength != value) {
					_editStrength = value;
					if (floatEvent != null)
						floatEvent.Invoke(Floats.editStrength, _editStrength);
				}
			}
		}

		[SerializeField] [HideInInspector] float _editSmoothing;
		float brushSmoothing {
			get { return _editSmoothing; }
			set {
				if (_editSmoothing != value) {
					_editSmoothing = value;
					if (floatEvent != null)
						floatEvent.Invoke(Floats.editSmoothing, _editSmoothing);
				}
			}
		}

		[SerializeField] [HideInInspector] float _paintStrength;
		float paintStrength {
			get { return _paintStrength; }
			set {
				if (_paintStrength != value) {
					_paintStrength = value;
					if (floatEvent != null)
						floatEvent.Invoke(Floats.paintStrength, _paintStrength);
				}
			}
		}

		[SerializeField] [HideInInspector] float _paintSmoothing;
		float paintSmoothing {
			get { return _paintSmoothing; }
			set {
				if (_paintSmoothing != value) {
					_paintSmoothing = value;
					if (floatEvent != null)
						floatEvent.Invoke(Floats.paintSmoothing, _paintSmoothing);
				}
			}
		}

		[SerializeField] [HideInInspector] float _smoothStrength;
		float smoothStrength {
			get { return _smoothStrength; }
			set {
				if (_smoothStrength != value) {
					_smoothStrength = value;
					if (floatEvent != null)
						floatEvent.Invoke(Floats.smoothingStrength, _smoothStrength);
				}
			}
		}

		// Bools.
		[SerializeField] [HideInInspector] bool _brushEditMesh;
		bool brushEditMesh {
			get { return _brushEditMesh; }
			set {
				if (_brushEditMesh != value) {
					_brushEditMesh = value;

					if (_brushEditMesh == true)
						brushSmooth = false;

					if (boolEvent != null)
						boolEvent.Invoke(Bools.editMesh, _brushEditMesh);
				}
			}
		}

		[SerializeField] [HideInInspector] bool _brushSmooth;
		bool brushSmooth {
			get { return _brushSmooth; }
			set {
				if (_brushSmooth != value) {
					_brushSmooth = value;

					if (_brushSmooth == true) {
						_brushPaint = false;
						_brushEditMesh = false;
					}

					if (boolEvent != null)
						boolEvent.Invoke(Bools.smooth, _brushSmooth);
				}
			}
		}

		[SerializeField] [HideInInspector] bool _brushEnabled;
		bool brushEnabled {
			get { return _brushEnabled; }
			set {
				if (_brushEnabled != value) {
					_brushEnabled = value;

					if (boolEvent != null)
						boolEvent.Invoke(Bools.enableDisable, _brushEnabled);
				}
			}
		}

		[SerializeField] [HideInInspector] bool _brushPaint;
		bool brushPaint {
			get { return _brushPaint; }
			set {
				if (_brushPaint != value) {
					_brushPaint = value;

					if (_brushPaint == true)
						brushSmooth = false;

					if (boolEvent != null)
						boolEvent.Invoke(Bools.paint, _brushPaint);
				}
			}
		}

		[SerializeField] [HideInInspector] bool _brushSubtract;
		bool brushSubtract {
			get { return _brushSubtract; }
			set {
				if (_brushSubtract != value) {
					_brushSubtract = value;
					if (boolEvent != null)
						boolEvent.Invoke(Bools.subtract, _brushSubtract);
				}
			}
		}
		
		// Paint type
		private PaintType _paintType = PaintType.Off;
		PaintType paintType {
			get { return _paintType; }
			set {
				if (_paintType != value) {
					_paintType = value;
					
					if (paintType == PaintType.Color1) {
						paintChannels = new PaintChannels(true, true, paintChannels.b, paintChannels.a);
						_paintColor.r = 0;
						_paintColor.g = 0;
					}
					else if (paintType == PaintType.Color2) {
						paintChannels = new PaintChannels(true, true, paintChannels.b, paintChannels.a);
						_paintColor.r = 255;
						_paintColor.g = 0;
					}
					else if (paintType == PaintType.Color3) {
						paintChannels = new PaintChannels(false, true, paintChannels.b, paintChannels.a);
						_paintColor.g = 255;
					}
					else {
						paintChannels = new PaintChannels(false, false, false, paintChannels.a);
					}
				}
			}
		}
		
		// Grass paint type
		private GrassPaintType _grassPaintType = GrassPaintType.Off;
		GrassPaintType grassPaintType {
			get { return _grassPaintType; }
			set {
				if (_grassPaintType != value) {
					_grassPaintType = value;

					if (_grassPaintType == GrassPaintType.Off) {
						paintChannels = new PaintChannels(paintChannels.r, paintChannels.g, paintChannels.b, false);
					}
					else if (_grassPaintType == GrassPaintType.AddGrass) {
						paintChannels = new PaintChannels(paintChannels.r, paintChannels.g, paintChannels.b, true);
						_paintColor.a = 32;
					}
					else if (_grassPaintType == GrassPaintType.RemoveGrass) {
						paintChannels = new PaintChannels(paintChannels.r, paintChannels.g, paintChannels.b, true);
						_paintColor.a = 0;
					}
				}
			}
		}
		
		// Paint channels
		private PaintChannels _paintChannels = new PaintChannels(false, false, false, false);

		PaintChannels paintChannels {
			get { return _paintChannels; }
			set {
				_paintChannels = value;
				paintChannelEvent?.Invoke(_paintChannels);
			}
		}
		
		#endregion
		#endregion
		
		[MenuItem("Window/Mesh editor")]
		public static void Open() {
			MeshEditor win = GetWindow<MeshEditor>();
			win.titleContent = new GUIContent("Mesh editor");
			win.Show();
		}

		void OnEnable() {
			if (Application.isPlaying)
				return;
			
			// Get the file path for brush prefab.
			brushPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("dmeBrushPrefab")[0]);
			terrainObjectPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("dmeTerrainObjectPrefab")[0]);

			CreateGUIStyles();
			SceneView.duringSceneGui += OnSceneGUI;
			Brush.requestValuesEvent += Brush_OnRequestValuesEvent;
		}
		
		private void OnDisable() {
			SceneView.duringSceneGui -= OnSceneGUI;
			
			if (brush != null) {
				DestroyImmediate(brush.gameObject);
			}
		}
		
		private void Brush_OnRequestValuesEvent() {
			SendAllValues();
		}

		private void CreateGUIStyles() {
			style_header.alignment = TextAnchor.UpperCenter;
			style_header.fontStyle = FontStyle.Bold;
			style_header.fontSize = 14;

			style_bold.alignment = TextAnchor.UpperCenter;
			style_bold.fontStyle = FontStyle.Bold;
		}

		private void SendAllValues() {
			colorEvent?.Invoke(_paintColor);
			directionEvent?.Invoke(direction);
			floatEvent?.Invoke(Floats.radius, _brushSize);
			floatEvent?.Invoke(Floats.editStrength, _editStrength);
			floatEvent?.Invoke(Floats.editSmoothing, _editSmoothing);
			floatEvent?.Invoke(Floats.paintStrength, _paintStrength);
			floatEvent?.Invoke(Floats.paintSmoothing, _paintSmoothing);
			floatEvent?.Invoke(Floats.smoothingStrength, _smoothStrength);
			boolEvent?.Invoke(Bools.editMesh, _brushEditMesh);
			boolEvent?.Invoke(Bools.smooth, _brushSmooth);
			boolEvent?.Invoke(Bools.enableDisable, _brushEnabled);
			boolEvent?.Invoke(Bools.paint, _brushPaint);
			boolEvent?.Invoke(Bools.subtract, _brushSubtract);
			paintChannelEvent?.Invoke(_paintChannels);
		}
		
		void OnGUI() {
			
			// UI
            #region First row
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            
            GUILayout.BeginVertical(EditorStyles.helpBox);
			GUILayout.Label("Mesh editor", style_header);
			GUILayout.Space(5);

			// Object selector
			GUILayout.Label("Current object", style_bold);
			currentTerrainObject = EditorGUILayout.ObjectField(currentTerrainObject as Object, typeof(MeshTerrain), true) as MeshTerrain;
			
			// Object creator
			if (!objectCreatorActive) {
				if (GUILayout.Button("Create new terrain object"))
					objectCreatorActive = true;
			}
			else {
				GUILayout.BeginVertical(EditorStyles.helpBox);
				
				if (GUILayout.Button("Cancel object creation"))
					objectCreatorActive = false;

				newTerrainName = EditorGUILayout.TextField("Name", newTerrainName);
				chunkSize = EditorGUILayout.Vector2IntField("Chunk size", chunkSize);
				vertexDensity = EditorGUILayout.FloatField("Vertex density", vertexDensity);
				isFlatShaded = EditorGUILayout.Toggle("Flat shaded", isFlatShaded);

				if (GUILayout.Button("Create terrain object")) {
					objectCreatorActive = false;
					GameObject goNewTerrainObject = Instantiate(AssetDatabase.LoadAssetAtPath(terrainObjectPath, typeof(GameObject))) as GameObject;
					goNewTerrainObject.name = newTerrainName;
					MeshTerrain newTerrainObject = goNewTerrainObject.GetComponent<MeshTerrain>();
					newTerrainObject.Initialize(newTerrainName, chunkSize, vertexDensity, isFlatShaded);
					currentTerrainObject = newTerrainObject;
					objectCreatorActive = false;
				}
				
				GUILayout.EndVertical();
			}

			if (currentTerrainObject == null) // Return if no selected object.
				return;
			
			editorActive = EditorGUILayout.Toggle("Editor active", editorActive);
			GUILayout.EndVertical();
			
			if (!editorActive) // Return if editor inactive.
				return;

			#region Chunks
			if (currentTerrainObject != null) {
				GUILayout.BeginVertical(EditorStyles.helpBox);
				GUILayout.Label("Chunk", style_header);
				GUILayout.Space(5);

				if (currentTerrainObject != null) {

					// Chunk creator
					newChunkCoords = EditorGUILayout.Vector2Field("", newChunkCoords);
					if (GUILayout.Button("Create chunk at ^^ coords")) {
						currentTerrainObject.AddChunk(new Vector2Int((int)newChunkCoords.x, (int)newChunkCoords.y));
					}
				}
				GUILayout.EndVertical();
			}
			#endregion Chunks

			#region Tools
			if (currentTerrainObject != null) {
				GUILayout.BeginVertical(EditorStyles.helpBox);
				GUILayout.Label("Tools", style_header);
				GUILayout.Space(5);

				// Undo button.
				if (GUILayout.Button("Undo")) {
					if (buttonEvent != null)
						buttonEvent.Invoke(Buttons.undo);
				}
				
				// Fix seams button
				if (GUILayout.Button("Fix chunk seams")) {
					currentTerrainObject.FixSeams();
				}
				
				// Save prefab button
				if (GUILayout.Button("Save prefab")) {
					currentTerrainObject.SavePrefab();
				}

				GUILayout.BeginHorizontal();
				brushEnabled = EditorGUILayout.Toggle("Brush enabled", brushEnabled);
				brushSmooth = EditorGUILayout.Toggle("Smooth", brushSmooth); 
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				brushEditMesh = EditorGUILayout.Toggle("Edit mesh", brushEditMesh);
				brushPaint = EditorGUILayout.Toggle("Paint", brushPaint);
				GUILayout.EndHorizontal();

				brushSize = EditorGUILayout.Slider("Brush size", brushSize, 0, 30);
				EditorGUILayout.EndVertical();
			}

            #endregion Tools

            GUILayout.EndHorizontal();
			#endregion First row

			#region Second row
			if (currentTerrainObject != null) {
				GUILayout.BeginHorizontal(EditorStyles.helpBox);

				#region Mesh editor
				if (brushEditMesh) {
					GUILayout.BeginVertical(EditorStyles.helpBox);
					GUILayout.Label("Mesh editor", style_header);
					GUILayout.Space(5);

					// Direction settings
					GUILayout.BeginHorizontal();
					direction = (Directions)EditorGUILayout.EnumPopup("Edit direction", direction);
					brushSubtract = EditorGUILayout.Toggle("Invert direction", brushSubtract);
					GUILayout.EndHorizontal();

					brushStrength = EditorGUILayout.Slider("Strength", brushStrength, 0, 0.5f);
					brushSmoothing = EditorGUILayout.Slider("Smoothing", brushSmoothing, 0, 2);

					GUILayout.EndVertical();
				}
				#endregion Mesh editor

				#region Mesh painter
				if (brushPaint) {
					GUILayout.BeginVertical(EditorStyles.helpBox);
					GUILayout.Label("Mesh painter", style_header);
					GUILayout.Space(5);

					paintStrength = EditorGUILayout.Slider("Strength", paintStrength, 0, 1);
					paintSmoothing = EditorGUILayout.Slider("Smoothing", paintSmoothing, 0, 2);
					paintColor = EditorGUILayout.ColorField(paintColor);
					paintType = (PaintType)EditorGUILayout.EnumPopup(paintType);
					grassPaintType = (GrassPaintType)EditorGUILayout.EnumPopup(grassPaintType);
					GUILayout.EndVertical();
				}
				#endregion Mesh painter

				#region Mesh smoother
				if (brushSmooth) {
					GUILayout.BeginVertical(EditorStyles.helpBox);
					GUILayout.Label("Mesh smoother", style_header);
					GUILayout.Space(5);

					smoothStrength = EditorGUILayout.Slider("Strength", smoothStrength, 0, 0.25f);
					GUILayout.EndVertical();
				}
				#endregion Mesh smoother

				GUILayout.EndHorizontal();
			}
			#endregion Second row
		}

        void OnSceneGUI(SceneView sceneView) {
	        if (!editorActive)
		        return;
	        
			sceneViewCamera = sceneView.camera;
			// Event.current houses information on scene view input this cycle
			Event current = Event.current;
			mousePosition = new Vector2(current.mousePosition.x, sceneView.camera.pixelRect.height - current.mousePosition.y);

			// If user has pressed the Left Mouse Button, do something and
			// swallow it so nothing else hears the event
			if (current.type == EventType.MouseDown && current.button == 0) {
				
				// While this tool is open, only allow the user to select scene
				// objects with a Collider component on them
				if (!Select<Collider>(current)) {
					// If nothing with Collider found, unselect everything
					Selection.activeGameObject = null;
				}

				if (lmb_downEvent != null)
					lmb_downEvent.Invoke();

				lmb = true;
				lmb_down = true;
			}
			else
				lmb_down = false;

			if (current.type == EventType.MouseUp && current.button == 0) {
				// While this tool is open, only allow the user to select scene
				// objects with a Collider component on them
				if (!Select<Collider>(current)) {
					// If nothing with Collider found, unselect everything
					Selection.activeGameObject = null;
				}

				if (lmb_upEvent != null)
					lmb_upEvent.Invoke();

				lmb = false;
				lmb_up = true;
			}
			else
				lmb_up = false;

			// After you've done all your custom event interpreting and swallowing,
			// you have to call this code to make sure swallowed events don't bleed out.
			// Not sure why, but that's the rules.
			if (current.type == EventType.Layout)
				HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));

			#region Input

			// Left mouse button
			if (current.type == EventType.MouseDown && current.button == 0) {
				lmb = true;
				lmb_down = true;
			}
			else
				lmb_down = false;

			if (current.type == EventType.MouseUp && current.button == 0) {
				lmb = false;
				lmb_up = true;
			}
			else
				lmb_up = false;

			// Toggle brush enabled.
			if (current.type == EventType.KeyDown && current.keyCode == KeyCode.M)
				brushEnabled = !brushEnabled;

			// Subtract toggle.
			if (current.type == EventType.KeyDown && current.keyCode == KeyCode.G)
				brushSubtract = !brushSubtract;

			// Change brush size.
			if (current.type == EventType.KeyDown && current.keyCode == KeyCode.F1)
				brushSize -= 0.5f;
			if (current.type == EventType.KeyDown && current.keyCode == KeyCode.F2)
				brushSize += 0.5f;

            #endregion
        }


        /// <summary>
        /// When user attempts to select an object, this sees if they selected an
        /// object with the given component. This will swallow the event and select
        /// the object if successful.
        /// </summary>
        /// <param name="e">Event from OnSceneGUI</param>
        /// <typeparam name="T">Component type</typeparam>
        /// <returns>Returns the object</returns>
        public static GameObject Select<T>(Event e) where T : UnityEngine.Component {
			Camera cam = Camera.current;

			if (cam != null) {
				RaycastHit hit;
				Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

				if (Physics.Raycast(ray, out hit)) {
					if (hit.collider != null) {
						GameObject gameObj = hit.collider.gameObject;
						if (gameObj.GetComponent<T>() != null) {
							e.Use();
							UnityEditor.Selection.activeGameObject = gameObj;
							return gameObj;
						}
					}
				}
			}

			return null;
		}
	}
}
#endif