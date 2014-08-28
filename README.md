Beyond the Box: Distributed Computing with ZeroMQ
===

This is the full set of materials from my presentation at devLink 2014.

Please take note of the following:

- `Source/chatz` and `Source/tickz` run on Windows
- `Source/valuz` and `Source/dealz` run on OS X
- `Source/lib` contains supporting libraries for various languages and platforms
- The rust bindings for ZeroMQ have been heavily modified to run on Windows, but the source has gone missing
- `Source/valuz` assumes that pyzmq and haskell-zeromq-4 are both installed
- The deal.host project in `Source/dealz` expects, as an input arg, the IP address of the machine wherein chatz.server and tickz.server are running
- The web-based GUI in `Source\dealz` is woefully underdeveloped (because JavaScript is frustrating)
- All ports are hard-coded (which is really really bad practice)

All source code and associated materials (including the Keynote presentation) are offer AS-IS _with no claims of suitability or guarantees of performance or support of any kind._
===

That having been said, please feel free to direct question and comments to the issue tracker. Also, if you fork this, I'd enjoy looking over any pull requests you submit.

Have fun and happy coding!

-- Paul Blasucci
