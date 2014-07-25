import random
import time
import zmq

# ======================================================================
# # sends batch data to worker(s) for processing
#   PUSH tcp://*:9004
#   <- [ stock  : UTF8([A-Z][A-Z0-9]+)       ]
#      [ action : UTF8(BUY = +1 | SELL = -1) ]
#      [ price  : UTF8(f64)                  ]
# # receives start signal from reducer
#   SUB  tcp://localhost:9006
#   ?> [ UTF8('batch.start') ]
# ======================================================================

## setup a simple portfolio
stocks = [ (b"MSFT",  41.78)
         , (b"AAPL",  95.35)
         , (b"GOOG", 571.09)
         , (b"YHOO",  34.53)
         , (b"BBRY",  10.90) ]

random.seed()
# helper function to create a randomly values trade (given the portfolio)
# NOTE: all orders are assumed to have a size of 1 lot
def order ():
  # pick a security
  (stock,base) = stocks[random.randint(0, 4)]
  # pick an action
  buySell = random.choice([1, -1])
  # set desired share price
  price = base + random.uniform(-3.0,3.0)
  # package order for transmission
  return  [ stock
          , str(buySell).encode('UTF8')
          , str(price  ).encode('UTF8') ]

context = zmq.Context()
# socket by which orders are sent to worker(s)
distrib = context.socket(zmq.PUSH)
distrib.bind("tcp://*:9004")
# socket by control signals are received from the aggregator
collect = context.socket(zmq.SUB)
collect.connect("tcp://localhost:9006")

# wait for worker coordination (ie: start signal from aggregator)
collect.set(zmq.SUBSCRIBE, b'')
_ = collect.recv()
print("Distributing orders for valuation...")

# send out a batch of orders for pricing
for _ in range(100):
  distrib.send_multipart(order())

# give ZMQ a chance to deliver
time.sleep(1)
print("100 orders sent.")

# give user a chance to review the resutls
_ = input("Press <RETURN> to exit")
