cd bin
start "center" ServerCenter.exe
start "node" node.exe nodejs/app.js
start "lobby" Lobby.exe nostore
start "bridge" ServerBridge.exe

