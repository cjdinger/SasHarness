# SAS Program Harness
***
This repository is contains an example that shows 
how to use SAS Integration Technologies from a Microsoft .NET
application.  This version is implemented in C#.

Written by [Chris Hemedinger](http://blogs.sas.com/content/sasdummy) as part of
a published paper at SAS Global Forum 2013.

## About this example
This sample application has the following features:
- Connects to a SAS Workspace session using a server that you define in a dialog window
- Allows you to connect to a "local" instance of SAS -- no configuration required.
- Features three windows: a program editor, a log viewer, and a listing viewer.  (Does that seem familiar?)
- Allows you to run a SAS program on a background thread, keeping the main user interface responsive
- Retrieves the SAS log and listing output, and colors each line of output as appropriate (errors, warnings, notes, page boundaries)

For more details, see the sasCommunity page for this paper:
[Create Your Own Client Apps Using SAS Integration Technologies](http://www.sascommunity.org/wiki/Create_Your_Own_Client_Apps_Using_SAS_Integration_Technologies)

