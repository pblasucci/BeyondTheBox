$(function () {
  "use strict";
  var orders = new Array();
  for (var i = 0, j = 1, k = 2; k < 500; i += 3, j += 3, k += 3) {
    orders[i] = { Stock: "MSFT", Action: { Case: "Buy", Fields: [] }, Price: 19.997 };
    orders[j] = { Stock: "APPL", Action: { Case: "Buy", Fields: [] }, Price: 35.765 };
    orders[k] = { Stock: "GOOG", Action: { Case: "Sell", Fields: [] }, Price: 256.004 };
  }
  var trades = { Case: "Reckon", Fields: [orders] };
  var SMALLTRADE = window.JSON.stringify(trades);
  var FOLLOWMSFT = window.JSON.stringify({ Case: "Follow", Fields: ["MSFT"] });
  var FORGETMSFT = window.JSON.stringify({ Case: "Forget", Fields: ["MSFT"] });

  var c = $.connection('signalr');
  c.received(function (msg) {
    switch (msg.Case) {
      case 'Ticked': // of Tick
        $('#tickz_out').val(msg.Fields[0].Price);
        break;
      case 'Memoed': // of Memo
        var chat = $('#chatz_out').val(); 
        if (chat.length > 0) { chat += "\r"; }
        $('#chatz_out').val(chat + msg.Fields[0].Message);
        break;
      case 'Joined': // of users:string[]
        $('#users').val(msg.Fields[0]);
        break;
      case 'Solved': // of Calc
        $('#valuz_out').val(msg.Fields[0].Value);
        break;
      case 'Disconnect':
        console.log(msg.Case);
        c.stop();
        break
      default:
        var note = msg.Case;
        if (msg.Fields.length > 0) { note += (": " + msg.Fields[0]); }
        console.log(note);
        break;
    }
  })
  .start({ transport: 'longPolling' }, function () {
    console.log("Starting SignalR");
  })
  .done(function () {
    $("#follow").click(function () { c.send(FOLLOWMSFT); });
    $("#forget").click(function () { c.send(FORGETMSFT); });
    $("#jabber").click(function () {
      var inp = $("#jabber_words");
      var txt = inp.val();
      var msg = { Case: "Jabber", Fields: [txt] };
      c.send(window.JSON.stringify(msg));
      inp.val("");
    });
    $("#reckon").click(function () { c.send(SMALLTRADE); });
  });
});
