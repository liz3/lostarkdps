import React from 'react';
import {styled} from "goober";
import {useSelector} from "react-redux"

const StatusLineStyle = styled("div")`
width: 100%;
height: 50px;

display: flex;
align-items: center;
padding: 0 5px;
justify-content: space-between;
background: ${props => props.color};
  -webkit-app-region: drag;
`

const getColor = (wsState, raid) => {
	if(wsState !== "connected")
		return "darkred";
	return raid !== 0 ? "green" : "#005d7b";
}

const StatusLine = () => {
	const wsState = useSelector(state => state.wsState);
	const inRaid = useSelector(state => state.raidState);
	const user = useSelector(state => state.user)
	return <StatusLineStyle color={getColor(wsState, inRaid)}>
		<p>Status: {wsState === "connected" ? "Connected" : "disconnected"} {user ? `- ${user.player_name} (${user.class_name})` : ''}</p>
		<p>{inRaid === 1 ? "In Raid" : inRaid === 2 ? "In Dungeon":  "Not in Raid"}</p>
	</StatusLineStyle>
}
export default StatusLine;