$(function () {
  "use strict";

  var c = $.connection('signalr');
  c.received(function (msg) {
    switch (msg.Case) {
      case 'Ticked': // of Tick
        //TODO: ???
        break;
      case 'Memoed': // of Memo
        //TODO: ???
        break;
      case 'Joined': // of users:string[]
        //TODO: ???
        break;
      case 'Solved': // of Calc
        //TODO: ???
        break;
      case 'Disconnect':
        console.log(msg.Case);
        c.stop();
        break
      default:
        var note = msg.Case;
        if (msg.Fields.length > 0) {
          note += ": " + msg.Fields[0];
        }
        console.log(note);
        break;
    }
  })
  .start()
  .done(function () {
    //TODO: ???
  });
});
