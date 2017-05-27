cd bin
start "center" ServerCenter.exe
start "dsnode" DataStoreNode.exe
start "node" node.exe nodejs/app.js
start "lobby" Lobby.exe
start "roomserver" DashFireServer.exe
start "bridge" ServerBridge.exe

