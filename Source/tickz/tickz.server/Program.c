#include <time.h>
#include <stdlib.h>
#include <stdio.h>
#include "zmq.h"
#include "czmq.h"

typedef struct 
{
  char*   symbol;
  time_t  timestamp;
  double  value;
} tick_t;

typedef struct
{
  zlist_t*  stocks;
  void*     socket;
} zloop_data_t;

static tick_t
tick_new (char *symbol, double value)
{
  tick_t tick;
  tick.symbol     = symbol;    
  tick.timestamp  = time(NULL);
  tick.value      = value;
  return tick;
}

static int 
randsign () 
{ 
  return (rand() % 2 - 1 == 0) ? +1 : -1; 
}

static double 
randf () 
{ 
  return (double) rand() / (double) RAND_MAX; 
}

static double 
revalue (const double last) 
{ 
  return last + randsign() * randf(); 
}

static void 
showStocks (zlist_t* stocks)
{
  tick_t *stock = (tick_t *)zlist_first(stocks);
  while (stock != NULL)
  {
    printf( "%s = { value = %f; timestamp = %i }\n"
          , stock->symbol
          , stock->value
          , stock->timestamp );
    stock = zlist_next(stocks);
  }
}

static int 
onloop (zloop_t *loop, int timer, void *arg)
{
  zloop_data_t *loopdata = (zloop_data_t *)arg;

  zframe_t *frame = zframe_new_empty();
  tick_t *stock = (tick_t *)zlist_first(loopdata->stocks);
  while (stock != NULL)
  {
    stock->timestamp  = time(NULL);
    stock->value      = revalue(stock->value);

    frame = zframe_new(stock->symbol,strlen(stock->symbol));
    zframe_send(&frame,loopdata->socket,ZFRAME_MORE);
    frame = zframe_new(&(stock->timestamp),sizeof(stock->timestamp));
    zframe_send(&frame,loopdata->socket,ZFRAME_MORE);
    frame = zframe_new(&(stock->value),sizeof(stock->value));
    zframe_send(&frame,loopdata->socket,0);

    stock = zlist_next(loopdata->stocks);
  }
  zframe_destroy(&frame);
  
  return 0;
}

/*
PUB tcp://*:9003
<<  [ "(?stock:[A-Z][A-Z0-9]+)" ]
    [ timestamp :f64            ]
    [ price     :f64            ]
*/
int main(int argc, const char* argv[])
{
  // initialize random number generator
  srand( (unsigned int)time(NULL) );

  // initialize stock data
  zlist_t *stocks = zlist_new();
  tick_t msft = tick_new("MSFT",41.78);
  zlist_append(stocks,&msft);
  tick_t aapl = tick_new("AAPL",95.35);
  zlist_append(stocks,&aapl);
  tick_t goog = tick_new("GOOG",571.09);
  zlist_append(stocks,&goog);
  tick_t yhoo = tick_new("YHOO",34.53);
  zlist_append(stocks,&yhoo);
  tick_t bbry = tick_new("BBRY",10.90);
  zlist_append(stocks,&bbry);
  
  // set up publisher
  zctx_t *ctx = zctx_new();
  void *pub = zsocket_new(ctx,ZMQ_PUB);
  zsocket_bind(pub,"tcp://*:9003");

  // set up main loop
  zloop_t *loop = zloop_new();
  zloop_data_t loopdata;
  loopdata.stocks = stocks;
  loopdata.socket = pub;
  // every 1000 ms, update the stocks and publish the new data
  int timer = zloop_timer(loop,1000,0,onloop,&loopdata);
  zloop_start(loop);
  
  // clean up
  zctx_destroy(&ctx);
  zlist_destroy(&stocks);

  return 0;
}
