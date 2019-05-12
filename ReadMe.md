# Quick Scan

Scans two sets of folders for like or unlike files, filtered on type.  
Used typically for comparing code lines, launching comparison tool.

- Added ability to send paths (quoted or not) on command line.
- Added extensions for web development.
- Could maybe have different profiles - Dotnet, web, system, invoked by command line switch...

Added indicators for folder states, and cleaned up tree presentation somewhat.

Need to adjust the status flags to populate at the time the files are added, have a local field, and if the field is changed by a new file comparison, propgate to the parent.  This should walk the states up the tree without requiring much overhead.
To allow the states to be refreshed after shutting down folders, there shoud be a way to reset and refresh.  This would involve two static delegates that each folder subscribes to, initially to reset all, then to refresh all.  
    (Refer LinqPad sample "Collectable Class with ResetAll static method")
