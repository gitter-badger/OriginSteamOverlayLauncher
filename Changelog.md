* 120ef5d (HEAD -> master) A second attempt at fixing CommandlineProxy behavior for Tarkov
* a410dfc Move ReLaunch inside ValidatePath and add sanity for DetectedCommandline
* 5a146ce (origin/master) Fix exception in PostGameWaitTime timeout
* c5172cf Convert old-style complex logging to newer method with better comments
* bea55f4 Use arguments from DetectedCommandline when launching game process
* 5afbe29 Revert "Only use launcher behavior if CommandlineProxy is disabled"
* dd7d9f2 Do not relaunch using CommandlineProxy if DetectedCommandline is populated
* ce6d9f9 Only use launcher behavior if CommandlineProxy is disabled
* 7a11ca0 Update the changelog for previous commit
* 1c61b9e Reissue minimize window message after the game exits
* ba49762 Make sure to use copied arguments not the ones from the INI
* 299ec44 Refactored CommandlineProxy support and more code cleanup
* c246446 Support for CommandlineProxy and lots of code cleanup
* 09a874b Further improved process detection when parsing process tree with children
* 236e2cd Implemented utility function to cleanup system tray area after exit
* b3fdc2c Improved process detection so that OSOL can track more launchers
* a65fb24 Updated assembly version
* 2e5045e Fixed hard coded INI read buffer limit of 255 characters
* a7f5892 Added MonitorPath and fixed GameArgs not being read into StartInfo OSOL can now use MonitorPath to monitor a remote executable instead of GamePath reducing process acquisition desyncs
* edcd71c README cleanups
* c809243 OSOL can now be renamed arbitrarily in prep for mediating launcher support
* b086dc7 INI overview changes to reflect MinimizeLauncher option
* e5d4a56 Added MinimizeLauncher option to INI by request
* 9ef5368 Fixed LauncherURI not being used to launch games via URI
* dd40c86 Fix CS in camel typed method
* 2c2cc0e OSOL now displays an INI settings overview when run with /help cli arg
* daff13b Refactoring and exposed more options via INI Added ReLaunch, DoNotClose, and ProxyTimeout options to the INI to expose more customizable behavior wrt the launcher
* c7b0cf2 Fixed incorrect default value in ProcessAcquisitionTimeout option
* f0ceae9 Exposed process acquisition timeout in the INI
* 29d2d72 Added a single global mutex
* c90b426 Update README.md
* 46f1b67 README grammar fix
* 79f2e1d Update README.md
* 967cf1b Updated README with links for bug reporting and wiki
* 6302177 Updated Battle.net launcher uri string table
* 47a8f5b Updated Battle.net launcher strings for Destiny 2
* 83b4e37 (tag: v1.05c) Fixed non-launcher game execution and timing
* 72d4038 (tag: v1.05b) Push git log output into Changelog.md before builds
* 00a1dda Add our Changelog.md output to our project build package
* a735a80 Make INI loading smarter when using old configs
* 386d3e0 Refactoring of code base and more tuneables Major code cleanup, user tuneable wait times, loosened search timing of launcher process, launcher process is now optional, and pre-launcher event support in URI mode.
* ce19cde Remove duplicated README and replace with a file link
* 8311da5 Fix our localized README
* 6afffb0 Added customizable post-game wait time to INI
* 378ceb4 Included a donation link for interested parties
* e59f857 (tag: v1.04) Fixed null path validation bug in external process delegate
* fe58b63 Fixed a path validation bug causing a persistent error on startup
* 0912fe0 Added support for pre-launcher post-game executables
* 8171ec9 Typo fix since we now support more than Origin
* e9e5831 More improvements to process detection
* 420db38 (tag: v1.03) Fix urgent thread wait bug causing performance issues
* 072e7c1 Fix the README getting out of sync
* 3525763 Bug fixes and improvements to sanity checking paths
* 5eeeacb Preliminary support for launcher URIs
* 26ef103 Fix our gitignore syntax
* 3736e97 Improved launcher process detection and config support for URIs
* 3f817ca (tag: 1.02) LauncherMode support for launching Origin by itself
* be07453 Config support code for LauncherMode option
* c400e86 Create README.md
* bf93026 (tag: 1.01) Small change for extra sanity in case validator fails
* 752ce03 Refactored process detection
* 8f1465a More refactoring and code cleanups
* 7f9525e Added logging and some code cleanup
* 2bd6ed4 Create README.md
* 6beacee Create README.md
* 8261978 Create README.md
* a57b403 (tag: 1.0) Add files via upload
* 7d8a1a9 Update README.md
* 9ab4ad7 Create README.md
* 7b66d50 Initial commit
