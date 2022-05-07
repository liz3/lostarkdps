const { contextBridge, ipcRenderer } = require('electron')
contextBridge.exposeInMainWorld('__NATIVE__',{
  getPort: () => {
    return ipcRenderer.invoke("get_la_port")
  }
})