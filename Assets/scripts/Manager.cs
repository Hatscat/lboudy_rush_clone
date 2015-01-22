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

	public GameObject[] lvlInfos;

	public float secBetweenEachStep;
	public GameObject redCube, greenCube, blueCube;

	public GUIStyle playBt, stopBt, menuBts, actionQtt;
	public GUIStyle[] actionsBts;
	public Texture titleGui, failGui, victoryGui;

	private bool _can_click;
	private byte _current_input, _cubes_nb_this_lvl;
	private byte[] _inputs_amounts, _inputs_index;
	private int _map_ready, _steps_between_spawns;
	private float _new_step_timer;
	private Transform _map, _cubes;
	private Camera _camera;

	void Awake () {

		step_count = 0;
		_map_ready = 0;
		step_speed = secBetweenEachStep;
		current_lvl_nb = 0;
		current_state = STATE_MENU;
		fail = success = false;
		_can_click = true;
		_inputs_amounts = new byte[0xff];
		_inputs_index = new byte[10] {
			Map.ARROW_CELL*Map.DIR_LEFT,
			Map.ARROW_CELL*Map.DIR_RIGHT,
			Map.ARROW_CELL*Map.DIR_UP,
			Map.ARROW_CELL*Map.DIR_DOWN,
			Map.CONV_CELL*Map.DIR_LEFT,
			Map.CONV_CELL*Map.DIR_RIGHT,
			Map.CONV_CELL*Map.DIR_UP,
			Map.CONV_CELL*Map.DIR_DOWN,
			Map.SPLIT_CELL,
			Map.STOP_CELL
		};
		_new_step_timer = secBetweenEachStep;
		_map = GameObject.Find("map").transform;
		_cubes = GameObject.Find("cubes").transform;
		_camera = Camera.main;
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
		_map.SendMessage("Init_map_next"); // reset map
		_current_input = Map.HOLE;
		_cubes_nb_this_lvl = lvlInfos[current_lvl_nb-1].GetComponent<Lvl_infos>().cubesNbPerColor;
		_inputs_amounts[_inputs_index[0]] = lvlInfos[current_lvl_nb-1].GetComponent<Lvl_infos>().leftArrowsNb;
		_inputs_amounts[_inputs_index[1]] = lvlInfos[current_lvl_nb-1].GetComponent<Lvl_infos>().rightArrowsNb;
		_inputs_amounts[_inputs_index[2]] = lvlInfos[current_lvl_nb-1].GetComponent<Lvl_infos>().upArrowsNb;
		_inputs_amounts[_inputs_index[3]] = lvlInfos[current_lvl_nb-1].GetComponent<Lvl_infos>().downArrowsNb;
		_inputs_amounts[_inputs_index[4]] = lvlInfos[current_lvl_nb-1].GetComponent<Lvl_infos>().leftConvsNb;
		_inputs_amounts[_inputs_index[5]] = lvlInfos[current_lvl_nb-1].GetComponent<Lvl_infos>().rightConvsNb;
		_inputs_amounts[_inputs_index[6]] = lvlInfos[current_lvl_nb-1].GetComponent<Lvl_infos>().upConvsNb;
		_inputs_amounts[_inputs_index[7]] = lvlInfos[current_lvl_nb-1].GetComponent<Lvl_infos>().downConvsNb;
		_inputs_amounts[_inputs_index[8]] = lvlInfos[current_lvl_nb-1].GetComponent<Lvl_infos>().splitsNb;
		_inputs_amounts[_inputs_index[9]] = lvlInfos[current_lvl_nb-1].GetComponent<Lvl_infos>().stopsNb;
		
		current_state = STATE_THINKING;
	}

	void init_anim_state () {

		_steps_between_spawns = lvlInfos[current_lvl_nb-1].GetComponent<Lvl_infos>().stepsBetweenSpawns;
		step_count = (uint)_steps_between_spawns - 1;
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

				if (step_count>_steps_between_spawns && _cubes.childCount<=0) {
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

				if (step_count%_steps_between_spawns==0 && _cubes_nb_this_lvl>0) {

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

				for (int i=0; i<lvlInfos.Length; i++) {

					float x = Screen.width*0.075f + Screen.width*(i%3)*0.3f;
					float y = Screen.height*0.33f + Screen.height*((byte)i/3|0)*0.2f;
					float w = Screen.width*0.25f;
					float h = Screen.height*0.1f;
					int i1 = i+1;

					if (is_mouse_hover(x, y, w, h)) {
						if (_map_ready != i1) {
							_map_ready = i1;
							_map.SendMessage("Init_map", (byte)i1);
						}
					}

					if (GUI.Button(new Rect(x, y, w, h), lvlInfos[i].GetComponent<Lvl_infos>().lvlName, menuBts)) {
						init_think_state();
					}
				}
			break;
			case STATE_THINKING:

				float bt_size = Screen.height*0.06f;

				if (GUI.Button(new Rect(Screen.width*0.05f, Screen.height*0.05f, bt_size, bt_size), "", playBt)) {
					init_anim_state();
				}
				if (GUI.Button(new Rect(Screen.width*0.05f, Screen.height*0.15f, bt_size, bt_size), "", stopBt)) {
					init_menu_state();
				}

        		for (int i=0, j=0; i<10; i++) {
        			
        			if (_inputs_amounts[_inputs_index[i]] > 0) {

	        			float offset_x = Screen.width*0.9f;
	        			float offset_y = Screen.height*0.1f;
	        			float y = j * bt_size * 1.5f;
	        			float rotation = 0;

	        			if (_inputs_index[i]%Map.DIR_LEFT == 0) {
							rotation = 90;
						} else if (_inputs_index[i]%Map.DIR_RIGHT == 0) {
							rotation = -90;
						} else if (_inputs_index[i]%Map.DIR_UP == 0) {
							rotation = 180;
						} else if (_inputs_index[i]%Map.DIR_DOWN == 0) {
							rotation = 0;
						}

	        			GUIUtility.RotateAroundPivot(-My_camera.longitude + rotation, new Vector2(offset_x + bt_size*0.5f, offset_y + y + bt_size*0.5f));
						
						if (GUI.Button(new Rect(offset_x, offset_y + y, bt_size, bt_size), "", actionsBts[i])) {
							_current_input = _inputs_index[i];
						}

						GUI.matrix = Matrix4x4.identity;

						GUI.Box(new Rect(offset_x + bt_size*0.88f, offset_y + y + bt_size*0.88f, bt_size*0.5f, bt_size*0.5f), _inputs_amounts[_inputs_index[i]].ToString(), actionQtt);
						
						j++;
        			}
        		}



				Vector3 _mouse_pos = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1000));
				RaycastHit hit;
				Ray ray = new Ray(_camera.transform.position, _mouse_pos-_camera.transform.position);

				if (Physics.Raycast(ray, out hit)) {
					//print(hit.transform.name);
					
					hit.collider.SendMessage("preview", _current_input, SendMessageOptions.DontRequireReceiver);
					//Debug.DrawLine(_camera.transform.position, hit.transform.position, Color.red, 0.5f, true);

					if (Input.GetAxis("Fire1") > 0 && _can_click) {
						_can_click = false;
						hit.collider.SendMessage("valid_new_kind", _current_input, SendMessageOptions.DontRequireReceiver);
					}
				}

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

		if (Input.GetAxis("Fire1") == 0) {
			_can_click = true;
		}
	}

	static bool is_mouse_hover (float x, float y, float w, float h) {

		return Input.mousePosition.x > x && Input.mousePosition.x < x+w && Screen.height-Input.mousePosition.y > y && Screen.height-Input.mousePosition.y < y+h;
	}

	void increase_input_amount (byte input) {

		_inputs_amounts[input]++;
		_current_input = Map.HOLE;
	}

	void decrease_input_amount (byte input) {

		if (--_inputs_amounts[input] == 0) {
			_current_input = Map.HOLE;
		}
	}
}
