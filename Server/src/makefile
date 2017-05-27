dashfire : 
	mcs -out:dashfire.dll -unsafe -r:../lib/Google.ProtocolBuffers.dll,../lib/log4net.dll,../lib/Lidgren.Network.dll \
	 *.cs sdk/*.cs utils/*.cs Message/*.cs config/*.cs object/*.cs bullet/*.cs RoomServer/*.cs ../../ServerInclude/dcore_api/*.cs ../../ServerInclude/*.cs
install :
	cp dashfire.dll ../bin/msg.xml ../bin/id.xml ../lib/*.dll resource/*.xml resource ~/platform_new/bin/ -r
clean :
	rm dashfire.dll
