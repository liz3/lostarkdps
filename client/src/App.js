import React, {Suspense} from 'react'
import { Provider } from "react-redux";
import {setup} from "goober"
import store from "./common/store";
import start from "./common/index"
import Wrapper from "./components/Wrapper"
start(store);
setup(React.createElement)

function App() {
  return (
   <Suspense fallback={null}>
    <Provider store={store}>
    <Wrapper />
    </Provider>
   </Suspense>
  );
}

export default App;
