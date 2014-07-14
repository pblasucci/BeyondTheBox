extern crate zmq;

use zmq::{Context, Message, REP, PUB};

// # keeps track of connected (expiring) users
//   # returns list of connected users
//   REP tcp://*:9001
//     -> [ "(?usr:\w+)(\037(?msg:\w+))?" ]
//     <- [ "(?usr:\w+)" ]+
// # broadcasts one user's message to entire group
//   PUB tcp://*:9002
//     << [ "(?usr:\w+)\037(?msg:\w+)" ]
fn main() {
  let mut context = Context::new();
  let mut server = context.socket(REP).unwrap();
  let mut dialog = context.socket(PUB).unwrap();

  assert!(server.bind("tcp://*:9001").is_ok());
  assert!(dialog.bind("tcp://*:9002").is_ok());

  let mut message = Message::new();
  loop {
    server.recv(&mut message, 0).unwrap();
    if server.get_rcvmore().unwrap() { fail!("invalid framing"); }

    message.with_str(|f|{
      let mut parts : Vec<& str> = f.split_str("\u0037").collect();
      let client = parts.shift().expect("malformed message");
      // TOOD: track clients
      for msg in parts.shift().iter() { dialog.send_str(*msg, 0).unwrap(); }
    });
    //TODO: replace ackowledgment with list of tracked clients
    server.send_str("", 0).unwrap();
  }
}
/*
tracking
========

map : id,expiry

loop {
  now
  for each id -> if expired, remove // expired : < now
  onrecv -> set/update expiry for id // expiry : now + TTL
  for each id -> send as frame
}
*/
