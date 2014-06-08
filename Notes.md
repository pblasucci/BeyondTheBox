### Beyond the Box: Distributed Computing with ZeroMQ

As fast and powerful as the PC has become, it's still not enough. Modern computing demands moving code up and out -- from threads to processes to machines to clusters. To do so intelligently requires solid tools and flexible techniques. In this example-driven talk, you'll learn how ZeroMQ, a library for distributed computing, provides the means to rise up to the challenges of modern computing. In particular, we will explore how a language-agnostic, pattern-based approach to message exchange may be used to deliver sophisticated and compelling distributed programming solutions.

_(Duration: 60,75,120 minutes)_

=====

#### Outline: 120-minute Talk

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
            *   Well-defined protocols
            *   Vibrant community
            *   4 languge "ports"
            *   45+ language bindings
*   Example: Stock ticker
*   Basic ZeroMQ Concepts
    1.  Sockets passing (sending/receiving) messages
        1.  Sends or Receives messages according to socket type
        *   Connects or Binds to and endpoint according to topology
        *   No shared state
        *   Binary data (0 or more bytes) -- app provides meaning
    *   Transport Unification
        1.  Change transport by changing address (string)
        *   Popular transports include
            1.  TCP
            *   IPC -- doesn't work on Windows!
            *   InProc (actor-based multi-threading)
*   Example: "Big Data" calculation
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
*   Example: Chat server
    1.  Multi-part messages
        1.  Framing for structure (1 or more frames)
        *   Must fit in memory
        *   Guarenteed to be delivered whole or not at all
    *   Advanced socket roles
        1.  DEALER -- interleaved send/recv, app must do tracking)
        *   ROUTER -- interleaved recv/send, app must do tracking)
        *   XPUB -- like PUB, but subscription info is shared
        *   XSUB -- like XSUB, but subscription info is shared
        *   PAIR -- exclusive channel between two INPROC nodes
*   Example: Query engine
*   More important concepts
    1.  Context
        1.  Heavywieght (thread-safe)
            1. Sockets are lightweigh (NOT thread-safe)
        *   "Owns" Sockets
        *   IS TRANSPORT for INPROC channels
        *   Mulitple contexts in one process is like mulitple processes
    *   Proxy
        1.  In-the-box abstraction for shuttling messages between sockets
        *   Also called: device, streamer, queue, forwarder
        *   Not useful for custom routing logic
    *   Diagnostics
        1.  Exposed at the Socket-level
        *   Stream of events delivered via PAIR socket
*   Example: Trading Desk
*   Things we didn't cover
    1.  Multicast protocols
    *   Security framework
    *   Non-ZMQ peers
*   Conclusion
    1.  Question & Answer
    *   Links about ZeroMQ
    *   Links about the presenter

#### Outline: 75-minute Talk

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
            *   Well-defined protocols
            *   Vibrant community
            *   4 languge "ports"
            *   45+ language bindings
*   Example: Stock ticker
*   Basic ZeroMQ Concepts
    1.  Sockets passing (sending/receiving) messages
        1.  Sends or Receives messages according to socket type
        *   Connects or Binds to and endpoint according to topology
        *   No shared state
        *   Binary data (0 or more bytes) -- app provides meaning
    *   Transport Unification
        1.  Change transport by changing address (string)
        *   Popular transports include
            1.  TCP
            *   IPC -- doesn't work on Windows!
            *   InProc (actor-based multi-threading)
    *   Context
        1.  Heavywieght (thread-safe)
            1. Sockets are lightweigh (NOT thread-safe)
        *   "Owns" Sockets
        *   IS TRANSPORT for INPROC channels
        *   Mulitple contexts in one process is like mulitple processes
*   Example: "Big Data" calculation
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
*   Example: Chat server
    1.  Multi-part messages
        1.  Framing for structure (1 or more frames)
        *   Must fit in memory
        *   Guarenteed to be delivered whole or not at all
    *   Advanced socket roles
        1.  DEALER -- interleaved send/recv, app must do tracking)
        *   ROUTER -- interleaved recv/send, app must do tracking)
        *   XPUB -- like PUB, but subscription info is shared
        *   XSUB -- like XSUB, but subscription info is shared
        *   PAIR -- exclusive channel between two INPROC nodes
*   Example: Trading desk
*   Things we didn't cover
    1.  Multicast protocols
    *   Proxies
    *   Diagnostics
    *   Security framework
    *   Non-ZMQ peers
*   Conclusion
    1.  Question & Answer
    *   Links about ZeroMQ
    *   Links about the presenter

#### Outline: 60-minute Talk

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
            *   Well-defined protocols
            *   Vibrant community
            *   4 languge "ports"
            *   45+ language bindings
*   Basic ZeroMQ Concepts
    1.  Sockets passing (sending/receiving) messages
        1.  Sends or Receives messages according to socket type
        *   Connects or Binds to and endpoint according to topology
        *   No shared state
        *   Binary data (0 or more bytes) -- app provides meaning
    *   Transport Unification
        1.  Change transport by changing address (string)
        *   Popular transports include
            1.  TCP
            *   IPC -- doesn't work on Windows!
            *   InProc (actor-based multi-threading)
    *   Context
        1.  Heavywieght (thread-safe)
            1. Sockets are lightweigh (NOT thread-safe)
        *   "Owns" Sockets
        *   IS TRANSPORT for INPROC channels
        *   Mulitple contexts in one process is like mulitple processes
*   Example: Stock ticker
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
*   Example: "Big Data" Calculation
    1.  Multi-part messages
        1.  Framing for structure (1 or more frames)
        *   Must fit in memory
        *   Guarenteed to be delivered whole or not at all
    *   Advanced socket roles
        1.  DEALER -- interleaved send/recv, app must do tracking)
        *   ROUTER -- interleaved recv/send, app must do tracking)
        *   XPUB -- like PUB, but subscription info is shared
        *   XSUB -- like XSUB, but subscription info is shared
        *   PAIR -- exclusive channel between two INPROC nodes
*   Example: Chat server
*   Things we didn't cover
    1.  Multicast protocols
    *   Proxies
    *   Diagnostics
    *   Security framework
    *   Non-ZMQ peers
*   Conclusion
    1.  Question & Answer
    *   Links about ZeroMQ
    *   Links about the presenter

=====

#### Examples: 120-Minute Talk

1.  Stock ticker            ... Data distribution
*   "Big Data" calculation  ... Workflow
*   Chat app                ... Client/Server
*   Query engine            ... Combining MEPs (Client/Server, Data distirbution)
*   Trading desk            ... Multi-threading (+ examples 1-4)

#### Examples: 75-Minute Talk

1.  Stock ticker            ... Data distribution
*   "Big Data" calculation  ... Workflow
*   Chat app                ... Client/Server
*   Trading desk            ... Multi-threading (+ examples 1-4)

#### Examples: 60-Minute Talk

1.  Stock ticker            ... Data distribution
*   "Big Data" calculation  ... Workflow
*   Chat app                ... Client/Server

#### Example languages

*   ???

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
