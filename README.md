Source code for the BorsukSoftware.Utils.Pytest library

## Purpose
This library is intended to simplify the processing of pytest output logs within .net for further processing.

Our main intended use is within the upload to Conical, but we've made it available as a standalone library for others who may have different requirements.

## Usage
The tool can process the output of a pytest run, when the -r[..] flag has been specified.

It will extract any error message, std out as well as logger information.

## How it works
The library relies on the pytest log file matching a set of conventions. This means that if your logging code decides to output lines which can conflict with this output format, then all bets are off. 

If this causes problems for you, then please do get in touch with sample outputs which we can use to help improve the library.

## FAQ
#### We've found a bug, what do we do?
Either raise an issue with as much detail as abouve. Alternatively, do feel free to raise a PR for the proposed fix with testing evidence.

#### We've got some suggestions / improvements?
See above :-)

#### Anything else?
Get in touch with us.
