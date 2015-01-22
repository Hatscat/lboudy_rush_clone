using UnityEngine;
using System.Collections;

public class Static_cube : MonoBehaviour {

	private Transform _transform, _map;
	private GameObject _manager;
	private Map _map_script;
	private Material _original_material;
	private bool _is_previewing, _changed;
	private byte _original_cell_kind;
	private int _col, _row, _floor;

	void Start () {

		_transform = transform;
		_map = _transform.parent;
		_manager = GameObject.Find("manager");
		_map_script = _map.GetComponent<Map>();
		_original_material = renderer.sharedMaterial;
		_col = int.Parse(_transform.name.Substring(1, _transform.name.IndexOf("r")-1));
		_row = int.Parse(_transform.name.Substring(_transform.name.IndexOf("r")+1, _transform.name.IndexOf("f")-_transform.name.IndexOf("r")-1));
		_floor = int.Parse(_transform.name.Substring(_transform.name.IndexOf("f")+1, 1)); // 10 floor max...

		_original_cell_kind = is_inside_map_cell(_col,_row,_floor) ? _original_cell_kind = Map.map_cells[_col,_row,_floor] : Map.HOLE;
	}
	
	void Update () {
		
		if (_is_previewing) {
			_is_previewing = false; // 2 updates needed
		} else if (!_changed && _original_material != renderer.sharedMaterial) {
			renderer.sharedMaterial = _original_material;
		}
	}

	void preview (byte cell_kind) {

		//print(_transform.name + ", " + _original_cell_kind + ", " + _col + ", " + _row + ", " + _floor);
		if (_original_cell_kind == Map.BLANK_CELL && !_changed) {
			_is_previewing = true;

			Quaternion rotation = Quaternion.identity;
			Material mat = _map_script.blank;

			if (cell_kind%Map.DIR_LEFT == 0) {
				rotation.SetLookRotation(Vector3.left);
				cell_kind /= Map.DIR_LEFT;
			} else if (cell_kind%Map.DIR_RIGHT == 0) {
				rotation.SetLookRotation(Vector3.right);
				cell_kind /= Map.DIR_RIGHT;
			} else if (cell_kind%Map.DIR_UP == 0) {
				cell_kind /= Map.DIR_UP;
			} else if (cell_kind%Map.DIR_DOWN == 0) {
				rotation.SetLookRotation(Vector3.back);
				cell_kind /= Map.DIR_DOWN;
			}

			switch (cell_kind) {
				case Map.ARROW_CELL:
					mat = _map_script.arrow;
				break;
				case Map.CONV_CELL:
					mat = _map_script.conv;
				break;
				case Map.SPLIT_CELL:
					mat = _map_script.split;
				break;
				case Map.STOP_CELL:
					mat = _map_script.stop;
				break;
			}

			_transform.rotation = rotation;
			renderer.sharedMaterial = mat;
		}
	}

	void valid_new_kind (byte cell_kind) {

		if (_is_previewing) {

			_changed = true;
			_manager.SendMessage("decrease_input_amount", cell_kind);
			Map.map_cells[_col,_row,_floor] = cell_kind;

		} else if (_changed) {

			_changed = false;
			_manager.SendMessage("increase_input_amount", Map.map_cells[_col,_row,_floor]);
			Map.map_cells[_col,_row,_floor] = Map.BLANK_CELL;
		}
	}

	bool is_inside_map_cell (int c, int r, int f) {

		return 	c>=0 && c<Map.map_cells.GetLength(0)
				&& r>=0 && r<Map.map_cells.GetLength(1)
				&& f>=0 && f<Map.map_cells.GetLength(2);
	}
}
