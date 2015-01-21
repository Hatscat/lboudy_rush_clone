using UnityEngine;
using System.Collections;

public class Manager : MonoBehaviour {

	public const byte STATE_MENU = 0;
	public const byte STATE_THINKING = 1;
	public const byte STATE_ANIMATION = 2;
	public const byte STATE_GAMEOVER = 3;
	public const byte STATE_VICTORY = 4;

	public static float step_speed;
	public static uint step_count;
	public static byte current_state;
	public static bool fail, success;
	public static byte current_lvl_nb;

	public string[] lvlNames;
	//public byte currentLvlNb;
	public byte[] cubesByLvl;

	public int stepBetweenSpawns;
	public float secBetweenEachStep;
	public GameObject redCube, greenCube, blueCube;

	public GUIStyle playBt, stopBt, menuBts;
	public GUIStyle[] actionsBts;
	//public Texture[] actionsBts;
	public Texture titleGui, failGui, victoryGui;

	private byte _cubes_nb_this_lvl;
	private int _map_ready;
	private float _new_step_timer;
	private Transform _map, _cubes;

	void Awake () {

		step_count = 0;
		_map_ready = 0;
		step_speed = secBetweenEachStep;
		current_lvl_nb = 0;
		current_state = STATE_MENU;
		fail = success = false;
		_new_step_timer = secBetweenEachStep;
		_map = GameObject.Find("map").transform;
		_cubes = GameObject.Find("cubes").transform;
	}

	void init_menu_state () {

		for (int i = _cubes.childCount; i-- > 0; ) {
			Destroy(_cubes.GetChild(i).gameObject);
		}
		current_state = STATE_MENU;
	}

	void init_think_state () {

		for (int i = _cubes.childCount; i-- > 0; ) {
			Destroy(_cubes.GetChild(i).gameObject);
		}
		fail = success = false;
		_cubes_nb_this_lvl = cubesByLvl[current_lvl_nb-1];
		current_state = STATE_THINKING;
	}

	void init_anim_state () {

		step_count = (uint)stepBetweenSpawns - 1;
		current_state = STATE_ANIMATION;
	}

	void Update () {

		if (current_state == STATE_ANIMATION) {

			if (fail) {

				current_state = STATE_GAMEOVER;

			} else if (success) {

				current_state = STATE_VICTORY;

			} else if (Time.time > _new_step_timer) {

				_new_step_timer = Time.time + secBetweenEachStep;
				step_count++;

				if (step_count>stepBetweenSpawns && _cubes.childCount<=0) {
					success = true;
				}

				for (int i=_cubes.childCount; i-->0;) {

					if (_cubes.GetChild(i).GetChild(0).position.y < -1) {
						fail = true;
						break;
					}

					for (int ii=_cubes.childCount; ii-->0;) {

						if (_cubes.GetChild(i) != _cubes.GetChild(ii)) {

							int[] pos1 = Map.cartesian_to_map_cell(_cubes.GetChild(i).GetChild(0).position);
							int[] pos2 = Map.cartesian_to_map_cell(_cubes.GetChild(ii).GetChild(0).position);
						
							if (pos1[0]==pos2[0] && pos1[1]==pos2[1] && pos1[2]==pos2[2]) {
								fail = true;
								break;
							}
						}
					}
				}

				if (step_count%stepBetweenSpawns==0 && _cubes_nb_this_lvl>0) {

					_cubes_nb_this_lvl--;

					for (int i = Map.map_size; i-->0;) {
						
						int c = (int)(i / Map.floors_nb % Map.cols_nb);
						int r = (int)(i / Map.floors_nb / Map.cols_nb);
						int f = i%Map.floors_nb;
						byte cell_content = Map.map_cells[c,r,f];
						Quaternion rotation = Quaternion.identity;
						
						if (cell_content%Map.DIR_LEFT == 0) {
							rotation.SetLookRotation(Vector3.left);
							cell_content /= Map.DIR_LEFT;
						} else if (cell_content%Map.DIR_RIGHT == 0) {
							rotation.SetLookRotation(Vector3.right);
							cell_content /= Map.DIR_RIGHT;
						} else if (cell_content%Map.DIR_UP == 0) {
							cell_content /= Map.DIR_UP;
						} else if (cell_content%Map.DIR_DOWN == 0) {
							rotation.SetLookRotation(Vector3.back);
							cell_content /= Map.DIR_DOWN;
						}

						if (cell_content == Map.SPAWN_CELL_R) {
							spawn_cube(redCube, c, r, f, rotation, Map.EXTRAC_CELL_R);
						} else if (cell_content == Map.SPAWN_CELL_G) {
							spawn_cube(greenCube, c, r, f, rotation, Map.EXTRAC_CELL_G);
						} else if (cell_content == Map.SPAWN_CELL_B) {
							spawn_cube(blueCube, c, r, f, rotation, Map.EXTRAC_CELL_B);
						}
					}
				}
			}
		}
	}

	void spawn_cube (GameObject cube, int c, int r, int f, Quaternion cell_rot, byte goal) {

		GameObject clone = Instantiate(cube, Map.map_cell_to_cartesian(c,r,f), cell_rot) as GameObject; // +1
		clone.transform.parent = _cubes;
		clone.transform.GetChild(0).SendMessage("init_live", new ArrayList(){c,r,f,goal});
	}

	void OnGUI () {

		switch (current_state) {

			case STATE_MENU:

				GUI.DrawTexture(new Rect(Screen.width*0.1f,Screen.height*0.1f,Screen.width*0.8f,Screen.height*0.1f), titleGui, ScaleMode.ScaleToFit);

				for (int i=0; i<lvlNames.Length; i++) {

					float x = Screen.width*0.075f + Screen.width*(i%3)*0.3f;
					float y = Screen.height*0.33f + Screen.height*((byte)i/3|0)*0.2f;
					float w = Screen.width*0.25f;
					float h = Screen.height*0.1f;

					if (is_mouse_hover(x, y, w, h)) {
						if (_map_ready != i) {
							_map_ready = i;
							_map.SendMessage("Init_map", (byte)i);
						}
					}

					if (GUI.Button(new Rect(x, y, w, h), lvlNames[i], menuBts)) {
						init_think_state();
					}
				}
			break;
			case STATE_THINKING:

				float bt_size = Screen.height*0.05f;

				if (GUI.Button(new Rect(Screen.width*0.05f, Screen.height*0.05f, bt_size, bt_size), "", playBt)) {
					init_anim_state();
				}
				if (GUI.Button(new Rect(Screen.width*0.05f, Screen.height*0.125f, bt_size, bt_size), "", stopBt)) {
					init_menu_state();
				}

        		GUIUtility.RotateAroundPivot(-My_camera.longitude, new Vector2(Screen.width*0.9f + bt_size*0.5f, Screen.height*0.1f + bt_size*0.5f));

				if (GUI.Button(new Rect(Screen.width*0.9f, Screen.height*0.1f, bt_size, bt_size), "", actionsBts[0])) {
					print("ok");
				}

				GUI.matrix = Matrix4x4.identity;

			break;
			case STATE_ANIMATION:
			break;
			case STATE_GAMEOVER:
				GUI.Box(new Rect(0, 0, Screen.width,Screen.height), "");
				GUI.DrawTexture(new Rect(Screen.width*0.2f,Screen.height*0.4f,Screen.width*0.6f,Screen.height*0.2f), failGui, ScaleMode.ScaleToFit);
				if (Input.GetAxis("Fire1") > 0) {
					init_think_state();
				}
			break;
			case STATE_VICTORY:
				GUI.Box(new Rect(0, 0, Screen.width,Screen.height), "");
				GUI.DrawTexture(new Rect(Screen.width*0.2f,Screen.height*0.4f,Screen.width*0.6f,Screen.height*0.2f), victoryGui, ScaleMode.ScaleToFit);
				if (Input.GetAxis("Fire1") > 0) {
					init_menu_state();
				}
			break;

		}
	}

	static bool is_mouse_hover (float x, float y, float w, float h) {

		return Input.mousePosition.x > x && Input.mousePosition.x < x+w && Screen.height-Input.mousePosition.y > y && Screen.height-Input.mousePosition.y < y+h;
	}
}
