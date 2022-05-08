import React from "react";
import { styled } from "goober";
import { useSelector } from "react-redux";
import StatusLine from "./StatusLine";
import Row from "./Row";

const WrapperStyle = styled("div")`
	width: 100%;

	height: 100%;
`;
const ContentWraper = styled("div")`
	width: 100%;
	height: 100%;
	overflow-y: scroll;
	max-height: calc(100% - 50px);
`;

const Wrapper = () => {
	const rows = useSelector((state) => {
		const entries = Object.values(state.damageState);
		const total = entries.reduce((acc, val) => acc + val.damage, 1);
		return entries
			.map((entry) => {
				const percentage = entry.damage / total;
				return {
					percentage,
					id: entry.id,
				};
			})
			.sort((a, b) => b.percentage - a.percentage);
	});
	return (
		<WrapperStyle>
			<StatusLine />
			<ContentWraper>
				{rows.map((row) => (
					<Row key={row.id} data={row} />
				))}
			</ContentWraper>
		</WrapperStyle>
	);
};
export default Wrapper;
