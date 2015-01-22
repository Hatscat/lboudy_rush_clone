using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map : MonoBehaviour {

	public const byte HOLE = 0;
	public const byte BLANK_CELL = 1;
	public const byte SPAWN_CELL_R = 2;
	public const byte SPAWN_CELL_G = 3;
	public const byte SPAWN_CELL_B = 4;
	public const byte EXTRAC_CELL_R = 5;
	public const byte EXTRAC_CELL_G = 6;
	public const byte EXTRAC_CELL_B = 7;
	public const byte WARP_CELL_Y = 8;
	public const byte WARP_CELL_P = 9;
	public const byte WARP_CELL_C = 10;
	public const byte ARROW_CELL = 11;
	public const byte CONV_CELL = 12;
	public const byte SPLIT_CELL = 13;
	public const byte STOP_CELL = 14;

	public const byte DIR_LEFT = 15;
	public const byte DIR_RIGHT = 16;
	public const byte DIR_UP = 17;
	public const byte DIR_DOWN = 18;

	public static byte[,,] map_cells;
	public static List<int> map_warps;
	public static int map_size, cols_nb, rows_nb, floors_nb;

	public GameObject[] lvlInfos;
	//public Texture2D[] mapLvl1, mapLvl2, mapLvl3;
	public GameObject mapCube;
	public Material blank, spawnR, spawnG, spawnB, extracR, extracG, extracB, warpY, warpP, warpC, arrow, conv, split, stop;

	private byte _current_lvl;
	private Transform _transform;

///////////////////////////////////////////////////////////////////////////////////////////////////

	public static Vector3 map_cell_to_cartesian (int col, int row, int floor) {

		return new Vector3(col-cols_nb*.5f, floor, row-rows_nb*.5f);
	}

	public static int[] cartesian_to_map_cell (Vector3 pos) {

		return new int[]{ Mathf.RoundToInt(pos.x+cols_nb*.5f), Mathf.RoundToInt(pos.z+rows_nb*.5f), Mathf.RoundToInt(pos.y) };
	}

///////////////////////////////////////////////////////////////////////////////////////////////////

	void Awake () {
		
		_current_lvl = 0;
		_transform = transform;
	}

	void Init_map (byte lvl_nb) {

		Manager.current_lvl_nb = lvl_nb;

		for (int i =_transform.childCount; i-->0;) {
			Destroy(_transform.GetChild(i).gameObject);
		}
	}

	void LateUpdate () {

		if (_current_lvl != Manager.current_lvl_nb) {
			_current_lvl = Manager.current_lvl_nb;
			Init_map_next();
		}
    }

	void Init_map_next () {

		map_warps = new List<int>();
		//print(Manager.current_lvl_nb);
		init_map_cells(lvlInfos[Manager.current_lvl_nb-1].GetComponent<Lvl_infos>().levelDesign);

		switch (Manager.current_lvl_nb) {
			case 1:
				//init_map_cells(mapLvl1);
				// tmp set some player cells manually here :
				set_cell(5, 4, 2, SPLIT_CELL);
			break;
			case 2:
				//init_map_cells(mapLvl2);
				// tmp set some player cells manually here :
				set_cell(14, 6, 1, ARROW_CELL*DIR_RIGHT);
				set_cell(18, 6, 1, ARROW_CELL*DIR_UP);
				set_cell(18, 12, 1, ARROW_CELL*DIR_LEFT);
				set_cell(10, 12, 1, ARROW_CELL*DIR_DOWN);
				set_cell(10, 14, 1, ARROW_CELL*DIR_RIGHT);
				set_cell(17, 14, 1, ARROW_CELL*DIR_UP);
				set_cell(23, 14, 1, STOP_CELL);
				set_cell(21, 14, 1, CONV_CELL*DIR_UP);
				set_cell(17, 25, 1, CONV_CELL*DIR_RIGHT);
				set_cell(6, 17, 1, CONV_CELL*DIR_DOWN);
				set_cell(18, 9, 1, CONV_CELL*DIR_LEFT);
				set_cell(17, 21, 1, ARROW_CELL*DIR_LEFT);
				set_cell(10, 21, 1, ARROW_CELL*DIR_DOWN);
				set_cell(11, 17, 1, SPLIT_CELL);
				set_cell(17, 19, 1, SPLIT_CELL);

				set_cell(16, 6, 1, WARP_CELL_Y);
				map_warps.Add((int)WARP_CELL_Y);
				map_warps.Add(16);
				map_warps.Add(6);
				map_warps.Add(1);
				set_cell(18, 11, 1, WARP_CELL_Y);
				map_warps.Add((int)WARP_CELL_Y);
				map_warps.Add(18);
				map_warps.Add(11);
				map_warps.Add(1);

			break;
			case 3:
				//init_map_cells(mapLvl3);
				// tmp set some player cells manually here :
				set_cell(7, 8, 2, SPLIT_CELL);
			break;
		}
	}
	
	void init_map_cells (Texture2D[] map_floors) {
		
		cols_nb = (int)map_floors[0].width;
		rows_nb = (int)map_floors[0].height;
		floors_nb = (int)map_floors.Length;
		map_size = cols_nb * rows_nb * floors_nb;
		map_cells = new byte[cols_nb, rows_nb, floors_nb];
		
		for (int i = map_size; i-->0;) {
			
			int c = (int)(i / floors_nb % cols_nb);
			int r = (int)(i / floors_nb / cols_nb);
			int f = i%floors_nb;
			Color32 p = map_floors[f].GetPixel(c, r);
			string hex = p.r.ToString("X2") + p.g.ToString("X2") + p.b.ToString("X2");
			byte cell_kind = BLANK_CELL;

			switch (hex) {

				case "000000": // hole
					cell_kind = HOLE;
				break;
				case "FFFFFF": // blank
					cell_kind = BLANK_CELL;
				break;
				// red spawn cell
				case "FF0000":
					cell_kind = SPAWN_CELL_R * DIR_LEFT;
				break;
				case "FF7700":
					cell_kind = SPAWN_CELL_R * DIR_RIGHT;
				break;
				case "FF0077":
					cell_kind = SPAWN_CELL_R * DIR_UP;
				break;
				case "FF7777":
					cell_kind = SPAWN_CELL_R * DIR_DOWN;
				break;
				// green spawn cell
				case "00FF00":
					cell_kind = SPAWN_CELL_G * DIR_LEFT;
				break;
				case "77FF00":
					cell_kind = SPAWN_CELL_G * DIR_RIGHT;
				break;
				case "00FF77":
					cell_kind = SPAWN_CELL_G * DIR_UP;
				break;
				case "77FF77":
					cell_kind = SPAWN_CELL_G * DIR_DOWN;
				break;
				// blue spawn cell
				case "0000FF":
					cell_kind = SPAWN_CELL_B * DIR_LEFT;
				break;
				case "7700FF":
					cell_kind = SPAWN_CELL_B * DIR_RIGHT;
				break;
				case "0077FF":
					cell_kind = SPAWN_CELL_B * DIR_UP;
				break;
				case "7777FF":
					cell_kind = SPAWN_CELL_B * DIR_DOWN;
				break;
				case "770000": // red extraction cell
					cell_kind = EXTRAC_CELL_R;
				break;
				case "007700": // green extraction cell
					cell_kind = EXTRAC_CELL_G;
				break;
				case "000077": // blue extraction cell
					cell_kind = EXTRAC_CELL_B;
				break;
				case "FFFF00": // yellow warp cell
					cell_kind = WARP_CELL_Y;
					map_warps.Add((int)cell_kind);
					map_warps.Add(c);
					map_warps.Add(r);
					map_warps.Add(f);
				break;
				case "FF00FF": // purple warp cell
					cell_kind = WARP_CELL_P;
					map_warps.Add((int)cell_kind);
					map_warps.Add(c);
					map_warps.Add(r);
					map_warps.Add(f);
				break;
				case "00FFFF": // cyan warp cell
					cell_kind = WARP_CELL_C;
					map_warps.Add((int)cell_kind);
					map_warps.Add(c);
					map_warps.Add(r);
					map_warps.Add(f);
				break;
			}
			
			set_cell(c, r, f, cell_kind);
		}
	}

	void set_cell (int c, int r, int f, byte cell_kind) {

		string cell_name = "c" + c + "r" + r + "f" + f;
		Transform rendered_cell = _transform.FindChild(cell_name);

		map_cells[c,r,f] = cell_kind;

		if (cell_kind == HOLE) {
			if (rendered_cell != null) {
				Destroy(rendered_cell);
			}
		} else {

			Quaternion rotation = Quaternion.identity;
			Material mat = blank;

			if (cell_kind%DIR_LEFT == 0) {
				rotation.SetLookRotation(Vector3.left);
				cell_kind /= DIR_LEFT;
			} else if (cell_kind%DIR_RIGHT == 0) {
				rotation.SetLookRotation(Vector3.right);
				cell_kind /= DIR_RIGHT;
			} else if (cell_kind%DIR_UP == 0) {
				cell_kind /= DIR_UP;
			} else if (cell_kind%DIR_DOWN == 0) {
				rotation.SetLookRotation(Vector3.back);
				cell_kind /= DIR_DOWN;
			}

			switch (cell_kind) {
				case BLANK_CELL:
				break;
				case SPAWN_CELL_R:
					mat = spawnR;
				break;
				case SPAWN_CELL_G:
					mat = spawnG;
				break;
				case SPAWN_CELL_B:
					mat = spawnB;
				break;
				case EXTRAC_CELL_R:
					mat = extracR;
				break;
				case EXTRAC_CELL_G:
					mat = extracG;
				break;
				case EXTRAC_CELL_B:
					mat = extracB;
				break;
				case WARP_CELL_Y:
					mat = warpY;
				break;
				case WARP_CELL_P:
					mat = warpP;
				break;
				case WARP_CELL_C:
					mat = warpC;
				break;
				case ARROW_CELL:
					mat = arrow;
				break;
				case CONV_CELL:
					mat = conv;
				break;
				case SPLIT_CELL:
					mat = split;
				break;
				case STOP_CELL:
					mat = stop;
				break;
			}

			if (rendered_cell == null) {
				GameObject cell = Instantiate(mapCube, map_cell_to_cartesian(c,r,f), rotation) as GameObject;
				cell.name = cell_name;
				cell.transform.parent = _transform;
				cell.renderer.sharedMaterial = mat;
			} else {
				rendered_cell.rotation = rotation;
				rendered_cell.renderer.sharedMaterial = mat;
			}
		}
	}

}
