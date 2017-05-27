cd bin

echo quit lobby ...
nohup ./AdminTool ../AdminTool/quitLobby.scp 2>&1 >log/admintool.txt &

echo wait lobby last save ...
sleep 30

echo quit DataStoreNode ...
nohup ./AdminTool ../AdminTool/quitDSNode.scp 2>&1 >log/admintool.txt &

echo wait DataStoreNode save DB ...
sleep 60

pkill node
pkill mono
pkill ServerCenter

echo stop DFM servers done.
