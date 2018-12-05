#!/bin/bash
id="$1";

osascript <<EOF
    tell application "Terminal" to close (every window whose name contains "$id")
EOF

exit 0