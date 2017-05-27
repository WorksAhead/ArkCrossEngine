@echo off
set wait_time = 30000

echo quit lobby
..\AdminTool\AdminTool.exe ../AdminTool/quitLobby.scp
echo wait lobby last save ...
..\AdminTool\AdminTool.exe ../AdminTool/sleep.scp 30000

echo quit DSNode
..\AdminTool\AdminTool.exe ../AdminTool/quitDSNode.scp
echo wait DSNode save DB ... 
..\AdminTool\AdminTool.exe ../AdminTool/sleep.scp 60000

@taskkill /im node.exe /f
@taskkill /im DashFireServer.exe /f
@taskkill /im ServerBridge.exe /f
@taskkill /im Lobby.exe /f
@taskkill /im DataStoreNode.exe /f
@taskkill /im ServerCenter.exe /f
echo stop DFM servers done.