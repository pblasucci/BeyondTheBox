$(function () {
  "use strict";

  var USERSLABEL = "[Connected Users]\r----------------------------\r";

  function getChoice(min, max) { return Math.floor(Math.random() * (max - min)) + min; }
  function getRandom(min, max) { return Math.random() * (max - min) + min; }

  // constructs message to start following a stock
  function followStock(stock) {
    return { Case: "Follow", Fields: [ stock ] };
  }
  // constructs message to stop following a stock
  function forgetStock(stock) {
    return { Case: "Forget", Fields: [ stock ] };
  }
  // builds a randomly valued order for the given stock
  function buildOrder(stock,price) {
    var action = (getChoice(0, 2) === 0) ?  "Sell" : "Buy";
    return { Stock  : stock
           , Action : { Case: action, Fields: [] }
           , Price  : price + getRandom(0, 100) };
  }
  // builds a trade consisting of 500 randomly-valued orders
  function buildTrade() {
    var orders = [];
    for (var i = 0, j = 1, k = 2; k < 500; i += 3, j += 3, k += 3) {
      orders[i] = buildOrder("MSFT",  19.997);
      orders[j] = buildOrder("APPL",  35.765);
      orders[k] = buildOrder("GOOG", 256.004);
    }

    return { Case: "Reckon", Fields: [ orders ] }; 
  }

  // handle messages from server
  var c = $.connection('signalr');
  c.received(function (msg) {
    switch (msg.Case) {
      
      case 'Ticked': // of Tick
        var field = "#tickz_" + msg.Fields[0].Stock;
        $(field).val(msg.Fields[0].Price.toFixed(5));
        break;
      
      case 'Memoed': // of Memo
        var note = msg.Fields[0].Sender 
                 + ": " 
                 + msg.Fields[0].Message; 
        
        var chat = $('#chatz_out').val(); 
        if (chat.length > 0) { chat += "\r"; }

        $('#chatz_out').val(chat + note);
        break;
      
      case 'Joined': // of users:string[]
        var users = msg.Fields[0]
                       .sort()
                       .join("\r");
      
        $('#users').val(USERSLABEL + users);
        break;
      
      case 'Solved': // of Calc
        $('#valuz_out').val(msg.Fields[0].Value.toFixed(5));
        break;
      
      case 'Disconnect':
        console.log(msg.Case);
        c.stop();
        break;
      
      default:
        var info = msg.Case;
        if (msg.Fields.length > 0) { info += (": " + msg.Fields[0]); }
        console.log(note);
        break;
    }
  })
  // connect to server
  .start({ transport: 'longPolling' }, function () {
    console.log("Starting SignalR");
  })
  .done(function () {
    // wire-up tickz tasks
    $("#follow_AAPL").click(function () { c.send(window.JSON.stringify(followStock("AAPL"))); });   
    $("#forget_AAPL").click(function () { c.send(window.JSON.stringify(forgetStock("AAPL"))); });   

    $("#follow_GOOG").click(function () { c.send(window.JSON.stringify(followStock("GOOG"))); });   
    $("#forget_GOOG").click(function () { c.send(window.JSON.stringify(forgetStock("GOOG"))); });   

    $("#follow_MSFT").click(function () { c.send(window.JSON.stringify(followStock("MSFT"))); });   
    $("#forget_MSFT").click(function () { c.send(window.JSON.stringify(forgetStock("MSFT"))); });   

    // wire-up chatz tasks
    $("#jabber").click(function () {
      var inp = $("#jabber_words");
      var txt = inp.val();
      var msg = { Case: "Jabber", Fields: [txt] };
      c.send(window.JSON.stringify(msg));
      inp.val("");
    });

    // wire-up valuz tasks
    $("#reckon").click(function () { 
      var trade = buildTrade();
      c.send(window.JSON.stringify(trade)); 
    });
  });

  // initialize UI
  $("#users").val(USERSLABEL);
});
