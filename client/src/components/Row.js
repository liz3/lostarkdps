import React, {useState, useMemo} from 'react';
import {styled} from "goober";
import {useSelector} from "react-redux"

const WrapperStyle = styled("div")`
width: 100%;



`
const RowBase = styled("div")`
width: 100%;
position: relative;
display: flex;
justify-content: space-between;
align-items: center;
padding: 10px;
& span {
	z-index: 15;

  font-size: 15px;
}
`
const AdvancedRows = styled("div")`

padding-left: 10px;
`
const RowBackground = styled("div")`
position: absolute;
top: 0;
left: 0;
height: 100%;
width: 100%;
z-index: 5;
transform-origin: center left;
`

const DamageRow = ({name, data, percentage, height = "55px", color, onClick}) => {

	return <RowBase style={{height}} onClick={onClick}>
		<RowBackground style={{background: color, transform: `scaleX(${percentage})`}} />
		<span>{name}: {data.attacks} Crits: {data.crits} FA/BA: {data.front_attacks}/{data.back_attacks} {(percentage * 100).toFixed(0)}%</span>
		<span>{data.damage.toLocaleString()}</span>
	</RowBase>
}

const Row = ({data}) => {
	const userEntry = useSelector(state => state.damageState[data.id])
	const [advanced, setAdvanced] = useState(false);
	const detailedBreakDown = useMemo(() => {
		if(!advanced)
			return null;
		return Object.values(userEntry.spells).map(skill => {
			return {...skill, percentage: skill.damage / (userEntry.damage > 0 ? userEntry.damage : 1)}
		}).sort((a,b) => b.percentage - a.percentage)
	}, [advanced, userEntry])
	return <WrapperStyle>
		<DamageRow name={userEntry.name} percentage={data.percentage} data={userEntry} onClick={() => setAdvanced(!advanced)} color={"#6f88a4"} />
		{advanced && detailedBreakDown ? <AdvancedRows>
			{detailedBreakDown.map(entry => <DamageRow key={entry.id} height={"35px"} name={entry.name} data={entry} percentage={entry.percentage} color={"#313b5e"} />)}
		</AdvancedRows> : null}
	</WrapperStyle>
}
export default Row
