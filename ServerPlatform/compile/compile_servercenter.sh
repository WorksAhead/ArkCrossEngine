#!/bin/bash

#������ServerCenter�����ļ�Ŀ¼ִ�д˽ű�������

../compile/runscp ../compile/convert.scp ServerCenter
../compile/servercenter_makefile.sed Makefile.gen > Makefile.am
autoscan
../compile/servercenter_configure.sed configure.scan > configure.ac
touch Makefile.common.in
aclocal
autoheader
automake -a -c --foreign
autoconf
./configure
make -j4