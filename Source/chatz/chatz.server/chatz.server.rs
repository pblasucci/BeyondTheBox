extern crate time;
extern crate zmq;

use std::collections::{HashMap};
use zmq::{Context, REP, PUB};


// helper to convert incoming request (opaque string) into some usable data
fn validate_msg(message : &str) -> Result<(String,Option<String>),&'static str> {
  // decomposes opaque string into data
  let parts : Vec<&str> = message.split_str("\u0037").collect();
  match parts.len() {
    1 => {
      let key = parts.get(0).to_string();
      // no messaage, return "keep-alive" data
      Ok ((key, None))
    },
    2 => {
      let key = parts.get(0).to_string();
      let msg = parts.get(1).to_string();
      // return client name and message
      Ok ((key, Some(msg)))
    },
    // bad message!
    _ => Err("invalid message"),
  }
}


// helpler to convert list of connected clients into list of data for reply
fn build_reply<'a>(clients : &'a HashMap<String, f64>) -> Vec<(&'a String, int)> {
  let final  = clients.len() - 1;
  // pair client name with correct ZMQ flag (required for mutlipart messages)
  clients.keys      ()
         .enumerate ()
         .map       (|(i,key)| (key, if i == final { 0 } else { zmq::SNDMORE }))
         .collect   ()
}


// # keeps track of connected (expiring) users
//   # returns list of connected users
//   REP tcp://*:9001
//     -> [ "(?usr:\w+)(\037(?msg:\w+))?" ]
//     <- [ "(?usr:\w+)" ]+
// # broadcasts one user's message to entire group
//   PUB tcp://*:9002
//     << [ "(?usr:\w+)\037(?msg:\w+)" ]
fn main() {
  // set up sockets
  let mut context = Context::new();
  let mut server  = context.socket(REP).unwrap(); // socket for client requests
  let mut dialog  = context.socket(PUB).unwrap(); // broadcasts messages

  assert!(server.bind("tcp://*:9001").is_ok());
  assert!(dialog.bind("tcp://*:9002").is_ok());

  // set up main loop and list of connected clients
  let mut clients : HashMap<String,f64> = HashMap::new();
  loop {
    // a client is expired if it hasn't contacted the server in > 5 seconds
    let cutoff  = time::precise_time_s ();
    // purge expired clients
    clients = clients.move_iter    ()
                     .filter       (|&(_,v)| v > cutoff)
                     .collect      ();

    // get next client request, blocks if no clients connected
    let message = server.recv_str(0)
                        .unwrap();
    //NOTE: _technically_ we should check that there are no more frames

    match validate_msg(message.as_slice()) {
      // request is just a "keep-alive" (ie: no message)
      // ... update client expiry
      Ok((client, None)) => {
        clients.insert(client, time::precise_time_s() + 5.0);
      },
      // request contains data (ie: a message for the group)
      // ... update client expiry
      // ... broadcast message to group
      Ok((client, Some(_))) => {
        clients.insert(client, time::precise_time_s() + 5.0);
        //NOTE: per protocol, just publish message as originally received
        //NOTE: more robust implementation would broadcast more complex message
        dialog.send_str(message.as_slice(), 0).unwrap();
      },
      // invalid request
      // ... log it and move on
      Err(error) => println!("ERR: {}", error),
    }

    // send reply consisting of actively connected clients
    for &(ref client, flags) in build_reply(&clients).iter() {
      // ... each client name is sent as a seperate ZMQ frame
      server.send_str(client.as_slice(), flags)
            .unwrap();
    }
  }
}
