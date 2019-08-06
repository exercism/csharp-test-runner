#!/bin/sh
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=true

name=`echo $1 | awk -F"-" '{for(i=1;i<=NF;i++){$i=toupper(substr($i,1,1)) substr($i,2)}} 1' OFS=""`
test_file="$2/${name}Test.cs"

sed -i 's/Skip = \"Remove to run test\"//g' $test_file
dotnet test $2