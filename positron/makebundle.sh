#!/bin/sh
export AS="as -arch i386"
export CC="cc -arch i386"
echo "mkbundle -o $2 $1 *.dll --deps -z"
mkbundle -o $2 $1 $PWD/*.dll --deps -z