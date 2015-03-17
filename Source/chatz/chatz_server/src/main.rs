/*******************************************************************************
 *                                                                             *
 * NOTE                                                                        *
 *   On Windows:                                                               *
 *     - place libzmq.dll, libzmq.rlib in the same folder as chatz.server.rs   *
 *     - compile with: rustc chatz.server.rs -L . -o chatz.server.exe          *
 *                                                                             *
 ******************************************************************************/

#[macro_use]
extern crate log;
extern crate env_logger;
extern crate time;
extern crate zmq;

use std::collections::{HashMap};
use zmq::{Context, REP, PUB};


// helper to convert incoming request (opaque string) into some usable data
fn validate_msg(message : &str) -> Result<(String,Option<String>),&'static str> {
  // decomposes opaque string into data
  let parts : Vec<&str> = message.split("\u{221E}").collect();
  debug!("Received message with {} parts", parts.len());
  match parts.len() {
    1 => {
      let key = parts.get(0).unwrap();
      // no message, return "keep-alive" data
      Ok ((key.to_string(), None))
    },
    2 => {
      let key = parts.get(0).unwrap();
      let msg = parts.get(1).unwrap();
      // return client name and message
      Ok ((key.to_string(), Some(msg.to_string())))
    },
    // bad message!
    _ => Err("Invalid message"),
  }
}


// helper to track or extend active an active client's expiry
fn extend_client(clients : &mut HashMap<String, f64>, client : String) {
  // give active client 5 seconds to ping us again
  let expiry = time::precise_time_s() + 5.0;
  debug!("Set {} expiry to {}", client, expiry);
  clients.insert(client, expiry);
}


// helper to convert list of connected clients into list of data for reply
fn build_reply<'a>(clients : &'a HashMap<String, f64>) -> Vec<(&'a String, isize)> {
  let bounds = clients.len() - 1;
  // pair client name with correct ZMQ flag (required for mutlipart messages)
  clients.keys      ()
         .enumerate ()
         .map       (|(i,key)| (key, if i == bounds { 0 } else { zmq::SNDMORE }))
         .collect   ()
}


// # keeps track of connected (expiring) users
// # returns list of connected users
//   REP tcp://*:9001
//     -> [ "(?usr:\w+)(\037(?msg:\w+))?" ]
//     <- [ "(?usr:\w+)" ]+
// # broadcasts one user's message to entire group
//   PUB tcp://*:9002
//     << [ "(?usr:\w+)\037(?msg:\w+)" ]
fn main() {
  env_logger::init().unwrap();

  // set up sockets
  let mut context = Context::new();
  let mut server  = context.socket(REP).unwrap(); // socket for client requests
  let mut dialog  = context.socket(PUB).unwrap(); // broadcasts messages

  assert!(server.bind("tcp://*:9001").is_ok());
  assert!(dialog.bind("tcp://*:9002").is_ok());
  println!("Listening on port 9001");
  println!("Broadcasting on port 9002");

  // set up main loop and list of connected clients
  let mut clients : HashMap<String,f64> = HashMap::new();
  loop {
    // a client is expired if it hasn't contacted the server in > 5 seconds
    let cutoff  = time::precise_time_s ();
    // purge expired clients
    clients = clients.into_iter    ()
                     .filter       (|&(_,v)| v > cutoff)
                     .collect      ();

    // get next client request, blocks if no clients connected
    let message = server.recv_str(0)
                        .unwrap();
    //NOTE: _technically_ we should check that there are no more frames
    match validate_msg(&message) {
      // request is just a "keep-alive" (ie: no message)
      // ... update client expiry
      Ok((client, None)) => {
        extend_client(&mut clients, client);
      },
      // request contains data (ie: a message for the group)
      // ... update client expiry
      // ... broadcast message to group
      Ok((client, Some(_))) => {
        extend_client(&mut clients, client);
        //NOTE: per protocol, just publish message as originally received
        //NOTE: more robust implementation would broadcast more complex message
        dialog.send_str(&message, 0).unwrap();
        debug!("Broadcast '{}'", message.to_string());
      },
      // invalid request
      // ... log it and move on
      Err(error) => error!("{}", error),
    }

    // send reply consisting of actively connected clients
    for &(ref client, flags) in build_reply(&clients).iter() {
      // ... each client name is sent as a seperate ZMQ frame
      server.send_str(client, flags)
            .unwrap();
    }
  }
}
