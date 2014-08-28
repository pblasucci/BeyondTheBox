function buildMessage (tag, fields) {
  var msg = { Case: tag, Fields: fields };
  return window.JSON.stringify(msg);
}

function ticked (tick) {
  $('#tickz_out_' + tick.Stock).val(tick.Price);
  //TODO: update line chart
}

$(function () {
  "use strict";
  //var orders = [];
  //for (var i = 0, j = 1, k = 2; k < 500; i += 3, j += 3, k += 3) {
  //  orders[i] = { Stock: "MSFT", Action: { Case: "Buy" , Fields: [] }, Price:  19.997 };
  //  orders[j] = { Stock: "APPL", Action: { Case: "Buy" , Fields: [] }, Price:  35.765 };
  //  orders[k] = { Stock: "GOOG", Action: { Case: "Sell", Fields: [] }, Price: 256.004 };
  //}
  //var SMALLTRADE = buildMessage("Reckon", [ orders ]);
  
  var c = $.connection('signalr');
  c.received(function (msg) {
    switch (msg.Case) {
      case 'Ticked': // of Tick
        ticked (msg.Fields[0]);
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
        break;

      default:
        var info = msg.Case; 
        if (msg.Fields.length > 0) {
          info += (": " + msg.Fields[0]);  
        } 
        console.log();
        break;
    }
  })
  .start({ transport: 'longPolling' })
  .done(function () {
    $('#followStock').submit(function () {
      //TODO: send topic to server
      //TODO: add stock field to gui
      //TODO: reset form
      //TODO: add stock to line chart
      return false;
    });
  });
});
