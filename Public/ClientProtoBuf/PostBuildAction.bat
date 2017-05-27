echo generating ClientProtoBuf.dll...

PUSHD %1
.\Precompile\precompile.exe ..\..\Bin\ClientProtoBuf.dll -o:..\..\Bin\ProtobufSerializer.dll -t:ProtobufSerializer
POPD

echo success.