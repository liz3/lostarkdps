import { combineReducers } from "redux";
import * as types from "./types";

const damageState = (state = {}, action) => {
	switch (action.type) {
		case types.UPDATE_USER_DATA_SKILL:
		case types.UPDATE_USER_DATA: {
			return { ...state, [action.target]: action.data };
		}
		case types.RESET_DATA:
			return {};
		default:
			return state;
	}
};
const wsState = (state = "disconnected", action) => {
	switch (action.type) {
		case types.UPDATE_WS_STATE:
			return action.data;
		default:
			return state;
	}
};
const raidState = (state = 0, action) => {
	switch (action.type) {
		case types.SET_RAID_STATE:
			return action.value;
		default:
			return state;
	}
};
const raidTimer = (
	state = { running: false, start: 0, duration: 0 },
	action
) => {
	switch (action.type) {
		case types.RESET_DATA: {
			if (!state.running)
				return { running: false, start: 0, duration: 0 };
			return state;		
		}
		case types.UPDATE_USER_DATA:
			return state.running && state.start === 0
				? { ...state, start: Date.now() }
				: state;
		case types.SET_RAID_STATE: {
			if (action.value === 0 && state.running && state.start !== 0) {
				return {
					running: false,
					start: 0,
					duration: (Date.now() - state.start) / 1000,
				};
			} else if (
				!state.running &&
				action.value !== 0 &&
				state.start === 0
			) {
				return {
					running: true,
					start: 0,
					duration: 0,
				};
			}
			return state;
		}
		default:
			return state;
	}
};
const user = (state = null, action) => {
	switch (action.type) {
		case types.UPDATE_THIS_PLAYER:
			return action.value;
		default:
			return state;
	}
};
export default combineReducers({
	damageState,
	wsState,
	raidState,
	user,
	raidTimer,
});
