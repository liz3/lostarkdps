import {
	updateWsState,
	setRaidState,
	resetData,
	addDamageEntry,
	addSkillEntry,
	updateThisPlayer,
} from "./actions";

const processData = ({ type, data }, store) => {
	const { dispatch } = store;
	if (type === "PKTRaidBegin") {
		dispatch(setRaidState(1));
		dispatch(resetData());
	} else if (
		type === "PKTRaidResult" ||
		type === "PKTChaosDungeonRewardNotify" ||
		type === "PKTInitChaosDungeonRewardCount" ||
		type === "PKTReverseRuinRewardNotify"
	) {
		dispatch(setRaidState(0));
	} else if (type === "PKTSkillDamageNotify") {
		dispatch(addDamageEntry(data));
	} else if (type === "PKTSkillStartNotify") {
		dispatch(addSkillEntry(data));
	} else if (type === "NewZone") {
		dispatch(resetData());
	} else if (type === "PKTEnterDungeonInfo") {
		dispatch(setRaidState(2));
	} else if (type === "PKTInitPC") {
		dispatch(setRaidState(0));
		dispatch(resetData());
		dispatch(updateThisPlayer(data));
	} else if (type === "PKTRaidStatusUpdateNotify") {
		dispatch(setRaidState(0));
	}
};

const start = (store) => {
	window.__NATIVE__.getPort().then((port) => {
		if (port === null) return;
		let ws;
		let restarting = false;
		try {
			ws = new WebSocket(`ws://localhost:${port}/data`);
		} catch (err) {
			if (restarting) return;
			restarting = true;
			setTimeout(() => start(store), 2000);
			return;
		}
		ws.addEventListener("open", () => {
			store.dispatch(updateWsState("connected"));
		});
		ws.addEventListener("close", () => {
			store.dispatch(updateWsState("disconnected"));
			ws.close();
			if (restarting) return;
			restarting = true;
			setTimeout(() => start(store), 2000);
		});
		ws.addEventListener("error", () => {
			store.dispatch(updateWsState("disconnected"));
			ws.close();
			if (restarting) return;
			restarting = true;
			setTimeout(() => start(store), 2000);
		});
		ws.addEventListener("message", (message) => {
			const parsed = JSON.parse(message.data);
			processData(parsed, store);
		});
	});
};

export default start;
