extern crate time;
extern crate zmq;

use std::collections::{HashMap};
use zmq::{Context, Socket, REP, PUB};

fn purge_expired(clients : &mut HashMap<String,f64>, cutoff : f64) {
  let mut expired : Vec<String> = Vec::new();
  for (k,v) in clients.iter() {
    if *v < cutoff { expired.push(k.clone()); }
  }
  for k in expired.iter() { clients.remove(k); }
  expired.clear();
}

fn process_msg(clients : &mut HashMap<String,f64>
              ,parts   : &Vec<&str>
              ,socket  : &mut Socket) {
  if  parts.len() > 0 && parts.len() < 3 {
    let client = parts.get(0).to_string();
    let expiry = time::precise_time_s() + 5.0;
    clients.insert(client, expiry);

    if parts.len() > 1 {
      let msg = parts.get(1);
      socket.send_str(*msg, 0).unwrap();
    }
  } else {
    fail!("malformed message");
  }
}

fn send_reply(clients : &HashMap<String,f64>, socket : &mut Socket) {
  for i in range(0, clients.len()) {
    let key   = clients.keys().nth(i).unwrap();
    let more  = if i < clients.len() - 1 { zmq::SNDMORE } else { 0 };
    println!("({}) {}", i, key);
    socket.send_str(key.as_slice(), more).unwrap();
  }
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
  let mut context = Context::new();
  let mut server  = context.socket(REP).unwrap();
  let mut dialog  = context.socket(PUB).unwrap();

  assert!(server.bind("tcp://*:9001").is_ok());
  assert!(dialog.bind("tcp://*:9002").is_ok());

  let mut clients : HashMap<String,f64> = HashMap::new();
  loop {
    purge_expired(&mut clients, time::precise_time_s());

    let message = server.recv_str(0).unwrap();
    if server.get_rcvmore().unwrap() { fail!("invalid framing"); }

    let parts : Vec<&str> = message.as_slice().split_str("\u0037").collect();
    process_msg(&mut clients, &parts, &mut dialog);

    send_reply(&clients, &mut server);
  }
}
