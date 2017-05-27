#!/bin/bash

export USER_ROOT_DIR=/home/mrdTomcat
export MONO_ROOT_DIR=/home/mrdTomcat/mono

cd $USER_ROOT_DIR
cd df
cd bin

unzip ../../bin.zip
mv mysql.data.dll MySql.Data.dll

cd ..
echo $MONO_ROOT_DIR > mono_path

cd ..

echo "all done."

#----------------config--------------------
