import React, { useState } from "react";
import { styled } from "goober";
import { useSelector } from "react-redux";

const WrapperStyle = styled("div")`
	width: 100%;
`;
const RowBase = styled("div")`
	width: 100%;
	position: relative;
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 10px;
	& span {
		z-index: 15;
		font-size: 17px;
	}
`;
const AdvancedRows = styled("div")`
	box-shadow: 0 3px 6px rgba(0, 0, 0, 0.16), 0 3px 6px rgba(0, 0, 0, 0.23);
	& > div {
		background: #1d1d1d;
	}
	margin-left: 25px;
`;
const RowBackground = styled("div")`
	position: absolute;
	top: 0;
	left: 0;
	height: 100%;
	width: 100%;
	z-index: 5;
	transform-origin: center left;
	transition: transform 0.3s ease;
	box-shadow: 0 1px 3px rgba(0, 0, 0, 0.12), 0 1px 2px rgba(0, 0, 0, 0.24);
`;
const getDps = (damage, raidTimer) => {
	if ((raidTimer.start === 0 && raidTimer.duration === 0) || damage === 0)
		return 0;
	if (raidTimer.duration && !raidTimer.running) {
		return damage / raidTimer.duration;
	}
	const elapsed = (Date.now() - raidTimer.start) / 1000;
	return damage / elapsed;
};
const DamageRow = ({
	name,
	data,
	percentage,
	height = "50px",
	color,
	onClick,
}) => {
	const raidTimer = useSelector((state) => state.raidTimer);
	const dps = getDps(data.damage, raidTimer);
	return (
		<RowBase style={{ height }} onClick={onClick}>
			<RowBackground
				style={{
					background: color,
					transform: `scaleX(${percentage})`,
				}}
			/>
			<span>
				{(percentage * 100).toFixed(0)}% {name}: {data.attacks} Crits:{" "}
				{data.crits} FA/BA: {data.front_attacks}/{data.back_attacks}
			</span>
			<span>
				{data.damage.toLocaleString()}{" "}
				{dps > 0
					? `[${dps.toLocaleString(undefined, {
							maximumFractionDigits: 0,
					  })} DPS]`
					: ""}
			</span>
		</RowBase>
	);
};

const Row = ({ data }) => {
	const userEntry = useSelector((state) => state.damageState[data.id]);

	const [advanced, setAdvanced] = useState(false);
	const detailedBreakDown = advanced
		? Object.values(userEntry.spells)
				.map((skill) => {
					return {
						...skill,
						percentage:
							skill.damage /
							(userEntry.damage > 0 ? userEntry.damage : 1),
					};
				})
				.sort((a, b) => b.percentage - a.percentage)
		: null;
	return (
		<WrapperStyle>
			<DamageRow
				name={userEntry.name}
				percentage={data.percentage}
				data={userEntry}
				onClick={() => setAdvanced(!advanced)}
				color={"#265487"}
			/>
			{advanced && detailedBreakDown ? (
				<AdvancedRows>
					{detailedBreakDown.map((entry) => (
						<DamageRow
							key={entry.id}
							height={"38px"}
							name={entry.name}
							data={entry}
							percentage={entry.percentage}
							color={"#262c47"}
						/>
					))}
				</AdvancedRows>
			) : null}
		</WrapperStyle>
	);
};
export default Row;
