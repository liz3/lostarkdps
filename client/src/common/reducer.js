import { combineReducers } from "redux";
import *  as types from "./types"


const damageState = (state = {}, action) => {
	switch(action.type) {
		case types.UPDATE_USER_DATA: {
			return {...state, [action.target]: action.data}
		}
		case types.RESET_DATA:
			return {};
		default: 
		return state;
	}
}
const wsState = (state = "disconnected", action) => {
	switch(action.type) {
	case types.UPDATE_WS_STATE:
		return action.data;
		default: 
		return state;
	}
}
const raidState = (state = 0, action) => {
	switch(action.type) {
	case types.SET_RAID_STATE:
		return action.value;
		default: 
		return state;
	}
}
const user = (state = null, action) => {
	switch(action.type) {
	case types.UPDATE_THIS_PLAYER:
		return action.value;
		default: 
		return state;
	}
}
export default combineReducers({
	damageState,
	wsState,
	raidState,
	user
})