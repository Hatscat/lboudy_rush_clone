using UnityEngine;
using System.Collections;

public class Moving_cube : MonoBehaviour {
	
	private Transform _transform, _parent;
	private Vector3 _pivot_point, _axis;
	private Quaternion _dest_rot;
	private byte _goal;
	private uint _current_step;
	private int[] _current_cell_pos, _next_cell_pos;
	private float _dir, _conv_dir;
    private bool _spawn, _fall, _go_to_right, _stop, _conv, _warp, _warped, _dead, _started;
	
///////////////////////////////////////////////////////////////////////////////////////////////////
	
	void init_live (ArrayList data) {
		
		_current_cell_pos[0] = (int)data[0];
		_current_cell_pos[1] = (int)data[1];
		_current_cell_pos[2] = (int)data[2];
		_dir = Mathf.Atan2(_parent.forward.z, _parent.forward.x) + ((int)_parent.forward.z!=0 ? Mathf.PI : 0);
		_next_cell_pos = _current_cell_pos;
		_goal = (byte)data[3]; // extration cell
	}

	void Awake () {
		
		_transform = transform;
		_parent = transform.parent;
		_current_step = Manager.step_count;
		_dest_rot = _transform.rotation;
        _go_to_right = _stop = _conv = _warp = _warped = _started = _dead = false;
        _spawn = true;
		_current_cell_pos = new int[3];
		_next_cell_pos = new int[3];
	}
	
	void Update () {

		if (Manager.step_count > _current_step) { // new step

			init_step();
			 
			if (!is_inside_map_cell(_current_cell_pos)) {
				//print (is_inside_map_cell(_current_cell_pos));
				_fall = true;
				_next_cell_pos[2]--;

			} else {
				
				byte current_cell = Map.map_cells[_current_cell_pos[0],_current_cell_pos[1],_current_cell_pos[2]];
				float cell_dir = 0;
				
				if (current_cell%Map.DIR_LEFT == 0) {
					cell_dir = Mathf.PI;
					current_cell /= Map.DIR_LEFT;
				} else if (current_cell%Map.DIR_RIGHT == 0) {
					cell_dir = 0;
					current_cell /= Map.DIR_RIGHT;
				} else if (current_cell%Map.DIR_UP == 0) {
					cell_dir = Mathf.PI * 1.5f;
					current_cell /= Map.DIR_UP;
				} else if (current_cell%Map.DIR_DOWN == 0) {
					cell_dir = Mathf.PI * 0.5f;
					current_cell /= Map.DIR_DOWN;
				}
				
				switch (current_cell) {
					case Map.HOLE:
						_fall = true;
						_next_cell_pos[2]--;
					break;
					case Map.EXTRAC_CELL_R:
					case Map.EXTRAC_CELL_G:
					case Map.EXTRAC_CELL_B:
						if (_goal == current_cell) {
                            // todo: anim !
                            if (_dead) {
                                Destroy(_parent.gameObject);
                            } else {
                                _dead = true;
                            }
                        } else {
                            _next_cell_pos = get_next_cell_pos(_dir);
                        }
					break;
					case Map.WARP_CELL_Y:
					case Map.WARP_CELL_P:
					case Map.WARP_CELL_C:
                        if (_warp) {
                            _warp = false;
                            _warped = true;
                            _next_cell_pos = get_next_cell_pos(_dir);
                        } else {
                            _warp = true;
                            _next_cell_pos = get_warped_cell_pos(current_cell);
                        }
					break;
					case Map.ARROW_CELL:
						_dir = cell_dir;
                        _next_cell_pos = get_next_cell_pos(_dir);
					break;
					case Map.CONV_CELL:
						_conv = true;
						_conv_dir = cell_dir;
						_next_cell_pos = get_next_cell_pos(_conv_dir);
					break;
					case Map.SPLIT_CELL:
						if (Manager.step_count%2 > 0) {
							_dir += Mathf.PI * 0.5f;
						} else {
							_dir += Mathf.PI * 1.5f;
						}
                        _next_cell_pos = get_next_cell_pos(_dir);
					break;
					case Map.STOP_CELL:
						if (_stop) {
							_stop = false;
                            _next_cell_pos = get_next_cell_pos(_dir);
						} else {
							_stop = true;
						}
					break;
					default:
                        if (!_spawn) {
                            _next_cell_pos = get_next_cell_pos(_dir);
                        }
					break;
				}

				if (!_fall && is_inside_map_cell(_next_cell_pos)
				    && Map.map_cells[_next_cell_pos[0],_next_cell_pos[1],_next_cell_pos[2]+1] != Map.HOLE) {
					// turn to right
					if (_go_to_right) {
						_go_to_right = false;
						_dir += Mathf.PI * 0.5f;
                        _next_cell_pos = get_next_cell_pos(_dir);
					} else {
						_next_cell_pos = _current_cell_pos;
						_go_to_right = true;
					}
				}
			}

            if (!_spawn && !_fall && !_warp && !_conv && !_go_to_right) {
				_pivot_point = new Vector3(_transform.position.x+Mathf.Cos(_dir)*0.5f, _transform.position.y-0.5f, _transform.position.z-Mathf.Sin(_dir)*0.5f);
				_axis = new Vector3(-Mathf.Sin(_dir),0,-Mathf.Cos(_dir));
				_dest_rot = Quaternion.AngleAxis(90,_axis) * _transform.rotation;
			}
		}

		if (Manager.current_state == Manager.STATE_ANIMATION) {

            if (_spawn && _started) {
                float size_ratio = Time.deltaTime / Manager.step_speed;
                _transform.localScale += new Vector3(size_ratio, size_ratio, size_ratio);
                _parent.Translate(Vector3.up * Time.deltaTime / Manager.step_speed);
			} else if (_fall) {
				_parent.Translate(Vector3.down * Time.deltaTime/Manager.step_speed);
			} else if (_conv) {
                _transform.Translate(new Vector3(Mathf.Cos(_conv_dir), 0, -Mathf.Sin(_conv_dir)) * Time.deltaTime / Manager.step_speed, Space.World);
            } else if (_warp) {
                float size_ratio = Time.deltaTime / Manager.step_speed;
                _transform.localScale -= new Vector3(size_ratio, size_ratio, size_ratio);
            } else if (_dead) {
                float size_ratio = Time.deltaTime / Manager.step_speed;
                _transform.localScale -= new Vector3(size_ratio, size_ratio, size_ratio);
                _parent.Translate(Vector3.down * Time.deltaTime / Manager.step_speed);
			} else if (!_stop && !_go_to_right) {
                if (_warped) {
                    float size_ratio = Time.deltaTime / Manager.step_speed;
                    _transform.localScale += new Vector3(size_ratio, size_ratio, size_ratio);
                }
				_transform.RotateAround(_pivot_point, _axis, Mathf.Lerp(0, 90, Time.deltaTime/Manager.step_speed));
			}
		}
	}
	
	void init_step () {
		
		_current_step = Manager.step_count;
		_current_cell_pos = _next_cell_pos;

        _transform.rotation = _dest_rot;
        
        if (_started) {
            _spawn = false;
            _transform.position = Map.map_cell_to_cartesian(_current_cell_pos[0], _current_cell_pos[1], _current_cell_pos[2] + 1);
            if (!_warp) {
                _transform.localScale = new Vector3(1f, 1f, 1f);
            }
        } else {
            _transform.localScale = new Vector3(0f, 0f, 0f);
        }

        _started = true;
        _fall = _conv = _warped = false;
	}
	
	bool is_inside_map_cell (int[] pos) {
		
		return 	pos[0]>=0 && pos[0]<Map.map_cells.GetLength(0)
				&& pos[1]>=0 && pos[1]<Map.map_cells.GetLength(1)
				&& pos[2]>=0 && pos[2]<Map.map_cells.GetLength(2);
	}
	
	int[] get_next_cell_pos (float direction) {
		
		return new int[3] {
			Mathf.RoundToInt(_current_cell_pos[0] + Mathf.Cos(direction)),
			Mathf.RoundToInt(_current_cell_pos[1] - Mathf.Sin(direction)),
			_current_cell_pos[2]
		};
	}

    int[] get_warped_cell_pos (int cell_kind) {

        int[] pos = new int[3];
        for (int i = 0, c = Map.map_warps.Count; i < c; i+=4)
        {
            if (cell_kind == Map.map_warps[i] && (Map.map_warps[i + 1] != _current_cell_pos[0] || Map.map_warps[i + 2] != _current_cell_pos[1] || Map.map_warps[i + 3] != _current_cell_pos[2]))
            {
                pos[0] = Map.map_warps[i + 1];
                pos[1] = Map.map_warps[i + 2];
                pos[2] = Map.map_warps[i + 3];
                break;
            }
        }
        return pos;
    }
}
