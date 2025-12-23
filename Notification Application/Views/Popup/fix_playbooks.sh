#!/bin/bash

# Read the file
file="Playbooks.cshtml"

# For each remaining </a> that closes a playbook card, we need to:
# 1. Remove the closing </a>
# 2. Add button HTML before </div>
# 3. Change opening <a href=...> to <div class="playbook-card">

# First pass: Convert all <a href="/Popup/SelectTemplate?type=X" class="playbook-card"> to <div class="playbook-card">
sed -i 's|<a href="/Popup/SelectTemplate?type=[0-9]" class="playbook-card">|<div class="playbook-card">|g' "$file"

# Second pass: For each closing </a> that follows a playbook meta section, replace with buttons + </div>
# We'll use a multi-line approach
perl -i -pe 'BEGIN{undef $/;} s|(</div>\s*)</a>|\1                <div class="playbook-buttons">\n                    <a href="/Popup/SelectTemplate" class="btn-use-playbook">Use Playbook</a>\n                    <a href="#" class="btn-preview">Preview</a>\n                </div>\n            </div>|g' "$file"

