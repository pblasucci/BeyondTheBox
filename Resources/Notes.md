### Beyond the Box: Distributed Computing with ZeroMQ

As fast and powerful as the PC has become, it's still not enough. Modern computing demands moving code up and out -- from threads to processes to machines to clusters. To do so intelligently requires solid tools and flexible techniques. In this example-driven talk, you'll learn how ZeroMQ, a library for distributed computing, provides the means to rise up to the challenges of modern computing. In particular, we will explore how a language-agnostic, pattern-based approach to message exchange may be used to deliver sophisticated and compelling distributed programming solutions.

_Duration: ~75 minutes_

_Media: slides, code, print_

=====

#### Outline

1.  Introduction
    1.  About the presenter
    *   About ZeroMQ
        1.  ZeroMQ is NOT middleware (i.e. it's not a message queue)
        *   ZeroMQ can be used anytime logic needs to be split (threads, processes, machines, networks)
        *   "Sockets" aren't really like actual (network) sockets... even if they seem familiar
        *   Build in layers
            1.  Wire protocol sits just above transport protocol (ZMTP)
            *   Socket behavior sits just above wire protocol (libzmq)
            *   App code sits just above socket behavior
        *   Communication and community
            1.  FOSS
            *   Designed and built by members of AMQP team
            *   Well-defined protocols
            *   Vibrant community (ZeroMQ GitHub org: 374 unique contributors - 12 Jun 2014)
            *   Written in portable C/C++
                1.   Native "ports" to Java, C#, Erlang
            *   45+ language bindings
*   Basic ZeroMQ Concepts
    1.  Sockets passing (sending/receiving) messages
        1.  Sends or Receives messages according to socket type
        *   Message transmission may be blocking or non-blocking
            1.  send/recv vs poll
        *   Connects or Binds to an endpoint according to topology
        *   No shared state
        *   Binary data (0 or more bytes) -- app provides meaning
    *   Transport Unification
        1.  Change transport by changing address (string)
        *   Popular transports include
            1.  TCP
            *   IPC -- doesn't work on Windows!
            *   InProc (actor-based multi-threading)
    *   One node (i.e. process, context, et cetera) can have many sockets
        1.  Combining sockets helps seperate concerns
        *   Combining sockets improves overall flexibility
*   ZeroMQ in detail
    1.  Message-exchange patterns
        1.  Determined by connecting certain Socket "roles"
        *   Client/Server
        *   Remote procedure call
        *   Workflow (parallel, repeat, et cetera)
        *   Data distribution
    *   Socket role
        1.  Determines message-exchange behavior
        *   send/recv pattern
        *   routing strategy
        *   compatible peers
        *   "mute state" action
    *   Basic socket roles
        1.  REQ   -- strictly synchronous send/recv, zmq handles tracking
        *   REP   -- strictly synchronous recv/send, zmq handles tracking
        *   PUSH  -- send messages downstream
        *   PULL  -- recv upstream messages
        *   PUB   -- send topical data
        *   SUB   -- recv topical data, filtered by topic
*   More important concepts
    1.  Context
        1.  Heavywieght (thread-safe)
            1. Sockets are lightweight (NOT thread-safe)
        *   "Owns" Sockets
        *   IS TRANSPORT for INPROC channels
        *   Mulitple contexts in one process is like mulitple processes
    *   Multi-part messages
        1.  Framing for structure (1 or more frames)
        *   Must fit in memory
        *   Guarenteed to be delivered whole or not at all
    *   Advanced socket roles
        1.  DEALER -- interleaved send/recv, app must do tracking)
        *   ROUTER -- interleaved recv/send, app must do tracking)
        *   XPUB -- like PUB, but subscription info is shared
        *   XSUB -- like XSUB, but subscription info is shared
        *   PAIR -- exclusive channel between two INPROC nodes
*   Useful tooling
    *   Proxy
        1.  In-the-box abstraction for shuttling messages between sockets
        *   Also called: device, streamer, queue, forwarder
        *   Not useful for custom routing logic
    *   Diagnostics
        1.  Exposed at the Socket-level
        *   Stream of events delivered via PAIR socket
*   Things we didn't cover
    1.  Multicast protocols
    *   Security framework
    *   Non-ZMQ peers
*   Conclusion
    1.  Question & Answer
    *   Links about ZeroMQ
    *   Links about the presentation
    *   Links about the presenter

=====

#### Examples

<p><em>tickz</em></p>
<pre>
tickz.server (C, Server)
  # broadcast stock data
  PUB tcp://*:9003
  &lt;&lt; [ "(?stock:[A-Z][A-Z0-9]+)" ]
     [ timestamp :f64            ]
     [ price     :f64            ]

tickz.client (VB, WinForms)
  # receive stock data from server
  SUB tcp://localhost:9003
  # user adds explicit stock symbols for which to receive data
  ?&gt; [ "(?stock:[A-Z][A-Z0-9]+)" ]*
</pre>
<p><em>chatz</em></p>
<pre>
chatz.server (Rust, Console)
  # keeps track of connected (expiring) users
  # returns list of connected users
  ROUTER tcp://*:9001
    -&gt; [ "(?usr:\w+)(\037(?msg:\w+))?" ]
    &lt;- [ "(?usr:\w+)" ]+
  # broadcasts one user's message to entire group
  PUB tcp://*:9002
    &lt;&lt; [ "(?usr:\w+)\037(?msg:\w+)" ]

chatz.client (C#, WPF)
  # sends messages to server for broadcast
  REQ tcp://localhost:9001
  # gets messages broadcast by anyone in the group
  SUB tcp://localhost:9002
  ?&gt; [ "" ]
</pre>
<p><em>valuz</em></p>
<pre>
valuz.source (Python, Console)
  # sends batch data to worker(s) for processing
  PUSH tcp://*:9004
  &lt;- [ "(?stock:[A-Z][A-Z0-9]+)"        ]
     [ action :(BUY = 0uy | SELL = 1uy) ]
     [ shares :i32                      ]
     [ price  :f64                      ]
  # receives control signal from reducer
  SUB tcp://localhost:9006
  ?&gt; [ command :START = 0uy ]

valuz.reduce (Python, Console)
  # gets results of individual calculation from worker(s)
  PULL tcp://*:9005
  -&gt; [  command :(READY = 2uy)  ]
  -&gt; [ "(?stock:[A-Z][A-Z0-9]+)" ]
     [ value :f64                ]
  # sends control signals to source, worker(s)
  PUB tcp://*:9006
  &lt;&lt; [ comand :START = 0uy ] # to source, when worker(s) ready
  &lt;&lt; [ comand :EXIT  = 0uy ] # to worker(s), after completion
  # displays aggregate calculation

valuz.worker (Haskell, Console)
  # get input data from source
  # performs calculation
  PULL tcp://localhost:9004
  # sends "ready" signal to reducer
  # sends calculated result to reducer
  PUSH tcp://localhost:9005
  # receives control signal from reducer
  SUB  tcp://localhost:9006
  ?&gt; [ command :EXIT = 1uy ]
</pre>
<p><em>dealz</em></p>
<pre>
dealz (F#, ?FunScript, ?WebAPI, ?ServiceStack)
  # chatz, on bg thread
  REQ tcp://localhost:9001
  SUB tcp://localhost:9002
  ?&gt; [ "" ]
  # tickz, on bg thread
  SUB tcp://localhost:9003
  ?&gt; [ "(?sym:[A-Z][A-Z0-9]+)" ]*
  # valuz, on bg thread
  PUSH tcp://*:9004
  &lt;- [ "(?stock:[A-Z][A-Z0-9]+)"        ]
     [ action :(BUY = 0uy | SELL = 1uy) ]
     [ shares :i32                      ]
     [ price  :f64                      ]
  PULL tcp://*:9005
  -&gt; [ command :(READY = 2uy) ]
  -&gt; [ "(?stock:[A-Z][A-Z0-9]+)" ]
     [ value :f64                ]
  PUB  tcp://*:9006
  &lt;&lt; [ comand :EXIT = 0uy ] # to worker, after completion
  # monitor, on bg thread
  PAIR inproc://monitor.chatz.req
  PAIR inproc://monitor.chatz.sub
  PAIR inproc://monitor.tickz.sub
  PAIR inproc://monitor.valuz.push
  PAIR inproc://monitor.valuz.pull
  PAIR inproc://monitor.valuz.pub
</pre>

=====

#### Document

__TODO: ???__


=====

#### Concepts

* __Some names are misleading__
  * ZeroMQ is NOT middleware (i.e. it's not a message queue)
  * ZeroMQ can be used anytime logic needs to be split (threads, processes, machines, networks)
  * "Sockets" aren't really like actual (network) sockets... even if they seem familiar


* Build in layers
  * Wire protocol sits just above transport protocol (ZMTP)
  * Socket behavior sits just above wire protocol (libzmq)
  * App code sits just above socket behavior
* Transport Unification
  * Unicast
    * TCP
    * IPC (not available on Windows)
    * INPROC
    * TIPC (coming soon)
  * Multicast
    * PGM
    * EPGM
* Message Passing
  * No shared state
  * Binary data (0 or more bytes) -- app provides meaning
  * Framing for structure (1 or more frames)
  * Must fit in memory
  * Guarenteed to be delivered whole or not at all
* Message Exchange Pattern
  * Determined by connecting certain Socket Types
  * Client-Server
  * Remote procedure call
  * Workflow (parallel, repeat, et cetera)
  * Data distribution
* Socket
  * Lightweight (not thread-safe)
  * Sends or Receives messages according to socket type
  * Connects or Binds to and endpoint according to topology
  * Manages inbound and outbound message queues (not accessible in app)
  * Has monitoring system (for diagnostics)
  * Socket Type
    * Determines message-exchange behavior
      * send/recv pattern
      * routing strategy
      * compatible peers
      * "mute state" action
    * REQ -- strictly synchronous send/recv, zmq handles tracking
    * REP -- strictly synchronous recv/send, zmq handles tracking
    * DEALER -- interleaved send/recv, app must do tracking)
    * ROUTER -- interleaved recv/send, app must do tracking)
    * PUSH -- send messages downstream
    * PULL -- recv upstream messages
    * PUB -- send topical data
    * SUB -- recv topical data, filtered by topic
    * XPUB -- like PUB, but subscription info is shared
    * XSUB -- like XSUB, but subscription info is shared
    * PAIR -- exclusive channel between two INPROC nodes
    * STREAM -- connect to non-ZeroMQ peer (via TCP)
* Context
  * Heavywieght (thread-safe)
  * "Owns" Sockets
  *  IS TRANSPORT for INPROC channels
  * Mulitple contexts in one process is like mulitple processes
* Proxy
  * In-the-box abstraction for shuttling messages between sockets
  * Also called: device, streamer, queue, forwarder
  * Not useful for custom routing logic
* Security
  * Available from ZMTP v3
  * SASL conformant
  * peers, arbitrarily, become "clients" or "servers"
  * Extensible mechansim with 3 defaults
    * NULL - no security at all (same as older versions of ZMTP)
    * PLAIN - username/password over plaintext, useful for internal networks or with transport-level encryption (eg: VPN)
    * CURVE - CurveCP handshake, adapted for TCP (eliptic curve crypto + perfect-forward secrecy)
  * Pluggable authenticators (INPROC nodes) via well-defined protocol
