import { createStore, applyMiddleware } from "redux";
import thunk from "redux-thunk";
import RootReducer from "./reducer.js";

const initialState = {};
const middleware = [thunk];

const store = createStore(
  RootReducer,
  initialState,
  applyMiddleware(...middleware)
);

window.__store = store
export default store;