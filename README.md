### Beyond the Box: Distributed Computing with ZeroMQ

As fast and powerful as the PC has become, it's still not enough. Modern computing demands moving code up and out -- from threads to processes to machines to clusters. To do so intelligently requires solid tools and flexible techniques. In this example-driven talk, you'll learn how ZeroMQ, a library for distributed computing, provides the means to rise up to the challenges of modern computing. In particular, we will explore how a language-agnostic, pattern-based approach to message exchange may be used to deliver sophisticated and compelling distributed programming solutions.

_(Duration: 60,75,120 minutes)_

=====

#### 60-Minute Outline

1.  Title
    1.  About the presenter
*   Introduction ZeroMQ
    1.  ???
    *   ???
*   Example 1
*   Basic ZeroMQ Concepts
    1.  ???
    *   ???
*   Example 2
*   ZeroMQ in detail
    1.  ???
    *   ??? 
*   Example 3
*   Things we didn't cover
    1.  ???
    *   ??? 
*   Conclusion
    1.  Question & Answer
    *   Links about ZeroMQ
    *   Links about the presenter    

=====

#### 75-Minute Outline

1.  Title
    1.  About the presenter
*   Introduction ZeroMQ
    1.  ???
    *   ???
*   Example 1
*   Basic ZeroMQ Concepts
    1.  ???
    *   ???
*   Example 2
*   ZeroMQ in detail
    1.  ???
    *   ??? 
*   Example 3
*   More important concepts
    1.  ???
    *   ???
*   Example 4
*   Things we didn't cover
    1.  ???
    *   ??? 
*   Conclusion
    1.  Question & Answer
    *   Links about ZeroMQ
    *   Links about the presenter    

=====

#### 120-Minute Outline

1.  Title
    1.  About the presenter
*   Introduction ZeroMQ
    1.  ???
    *   ???
*   Example 1
*   Basic ZeroMQ Concepts
    1.  ???
    *   ???
*   Example 2
*   ZeroMQ in detail
    1.  ???
    *   ??? 
*   Example 3
*   More important concepts
    1.  ???
    *   ???
*   Example 4
*   Some things worth knowing
    1.  ???
    *   ???
*   Example 5
*   Things we didn't cover
    1.  ???
    *   ??? 
*   Conclusion
    1.  Question & Answer
    *   Links about ZeroMQ
    *   Links about the presenter    

=====

#### Examples: 120-Minute Talk

1.  Chat server (w/ clients)   ... Client/Server
    *   Server:  tcl (fallback: Python)
    *   Client:  tcl (fallback: Python)
*   Stock ticker (w/ clients)  ... Data distribution
    *   Ticker:  Rust (fallback: C)
    *   Reader:  Rust (fallback: C)
*   "Big Data" calculation     ... Workflow
    *   Joiner:  VB
    *   Venter:  C#
    *   Worker:  C 
*   Query interpreter          ... Combining MEPs (Client/Server, Data distirbution)
    *   Facade:  Racket (fallback: F#)
    *   Engine:  Racket (fallback: F#)
*   Trading desk               ... Multi-threading (+ examples 1-4)
    *   GUI/UX:  ? WPF + F#
                 ? web + F# 
    *   Client:  F#
    *   Server:  from Example #1
    *   Reader:  F#
    *   Ticker:  from Example #2
    *   Joiner:  F#
    *   Venter:  F#
    *   Worker:  from Example #3
    *   Facade:  F#
    *   Engine:  from Example #4

#### Examples: 75-Minute Talk

1.  Chat server (w/ clients)   ... Client/Server
    *   Server:  tcl (fallback: Python)
    *   Client:  tcl (fallback: Python)
*   Stock ticker (w/ clients)  ... Data distribution
    *   Ticker:  Rust (fallback: C)
    *   Reader:  Rust (fallback: C)
*   "Big Data" calculation     ... Workflow
    *   Joiner:  VB
    *   Venter:  C#
    *   Worker:  C 
*   Trading desk               ... Multi-threading (+ examples 1-4)
    *   GUI/UX:  ? WPF + F#
                 ? web + F# 
    *   Client:  F#
    *   Server:  from Example #1
    *   Reader:  F#
    *   Ticker:  from Example #2
    *   Joiner:  F#
    *   Venter:  F#
    *   Worker:  from Example #3

#### Examples: 60-Minute Talk

1.  Chat server (w/ clients)   ... Client/Server
    *   Server:  F#
    *   Client:  F#
*   Stock ticker (w/ clients)  ... Data distribution
    *   Ticker:  F#
    *   Reader:  F#
*   "Big Data" calculation     ... Workflow
    *   Joiner:  VB
    *   Venter:  C#
    *   Worker:  C 

=====

#### Concepts

* __Some names are misleading__
  * ZeroMQ is NOT middleware (i.e. it's not a message queue)
  * ZeroMQ can be used anytime logic needs to be split (threads, processes, machines, networks)
  * "Sockets" aren't really like actual (network) sockets... even if they seem familiar


1. Build in layers
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
  * Determined by connecting cetain Socket Types
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

=====

#### Solution

* BeyondTheBox.sln
  + docs
    + content
    + tools
      - generate.fsx
      - template.cshtml
  + project
    - build.fsx
    - README.md
    - RELEASE_NOTES.md
  + 1_Chat
    * ???
  + 2_Tick
    * ???
  + 3_Calc
    * BeyondTheBox.Calc.Joiner.vbproj
    * BeyondTheBox.Calc.Venter.csporj
    * BeyondTheBox.Calc.Worker.cpproj
  + 4_Eval
    * ???
  + 5_Quix
    * BeyondTheBox.Quixotic.fsproj
  - 1_Chat.ps
  - 2_Tick.ps
  - 3_Calc.ps
  - 4_Eval.ps
  - 5_Quix.ps
  - Index.lnk
