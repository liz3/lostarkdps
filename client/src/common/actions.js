import * as types from "./types";

const ensure = (target, prop, value) => {
	if (!target[prop]) target[prop] = value;
	return target[prop];
};

export const updateWsState = (state) => {
	return {
		type: types.UPDATE_WS_STATE,
		data: state,
	};
};
export const setRaidState = (value) => {
	return {
		type: types.SET_RAID_STATE,
		value,
	};
};
export const updateThisPlayer = (value) => {
	return {
		type: types.UPDATE_THIS_PLAYER,
		value,
	};
};
export const addSkillEntry = (payload) => {
	return (dispatch, getState) => {
		const { damageState } = getState();
		const user = ensure(damageState, payload.source_id, {
			id: payload.source_id,
			registered: 0,
			attacks: 0,
			crits: 0,
			damage: 0,
			back_attacks: 0,
			front_attacks: 0,
			spells: {},
			name: payload.name_known ? payload.source_name : payload.source_id,
			name_known: payload.name_known,
		});
		if (!user.name_known && payload.name_known)
			user.name = payload.source_name;
		user.registered++;
		const spell = ensure(user.spells, payload.skill_id, {
			registered: 0,
			attacks: 0,
			crits: 0,
			damage: 0,
			back_attacks: 0,
			front_attacks: 0,
			id: payload.skill_id,
			name: payload.skill_name,
		});
		spell.registered++;

		dispatch({
			type: types.UPDATE_USER_DATA_SKILL,
			target: user.id,
			data: user,
		});
	};
};
export const addDamageEntry = (payload) => {
	return (dispatch, getState) => {
		const { damageState } = getState();
		const user = ensure(damageState, payload.source_id, {
			id: payload.source_id,
			registered: 0,
			attacks: 0,
			crits: 0,
			damage: 0,
			back_attacks: 0,
			front_attacks: 0,
			spells: {},
			name: payload.name_known ? payload.source_name : payload.source_id,
			name_known: payload.name_known,
		});
		if (!user.name_known && payload.name_known)
			user.name = payload.source_name;
		user.attacks++;
		user.damage += payload.damage;
		if (payload.crit) user.crits++;
		switch (payload.type) {
			case "back_attack":
				user.back_attacks++;
				break;
			case "front_attack":
				user.front_attacks++;
				break;
			default:
				break;
		}
		const spell = ensure(user.spells, payload.skill_id, {
			registered: 0,
			attacks: 0,
			crits: 0,
			damage: 0,
			back_attacks: 0,
			front_attacks: 0,
			id: payload.skill_id,
			name: payload.skill_name,
		});
		spell.attacks++;
		spell.damage += payload.damage;
		if (payload.crit) spell.crits++;
		switch (payload.type) {
			case "back_attack":
				spell.back_attacks++;
				break;
			case "front_attack":
				spell.front_attacks++;
				break;
			default:
				break;
		}
		dispatch({
			type: types.UPDATE_USER_DATA,
			target: user.id,
			data: user,
		});
	};
};
export const resetData = () => {
	return {
		type: types.RESET_DATA,
	};
};
