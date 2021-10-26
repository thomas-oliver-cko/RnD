#!/bin/sh

sed -i 's/#TCPAddr 127.0.0.1/TCPAddr 0.0.0.0/g' /etc/clamav/clamd.conf
sed -i 's/#TCPSocket 3310/TCPSocket 3310/g' /etc/clamav/clamd.conf

freshclam -d
clamd
dotnet '"${appName}".dll'